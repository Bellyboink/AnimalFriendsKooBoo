using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Cryptography;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Sites.Models;
using Kooboo_CMS.Areas.Integration.Models;
using Kooboo_CMS.Areas.Integration.SignalR;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class DataService
    {
        public void GetRepository()
        {
            Site.Current = SiteHelper.Parse("AnimalFriends").AsActual();
            Repository.Current = Site.Current.GetRepository().AsActual();
        }
        
        public TextFolder GetImportSettingsFolder()
        {
            GetRepository();
            return ServiceFactory.TextFolderManager.Get(Repository.Current, "ImportSetting");
        }

        public void Delete(string uuid, TextFolder textfolder)
        {
            GetRepository();
            ServiceFactory.TextContentManager.Delete(Repository.Current, textfolder, uuid);
        }

        public ContentBase Create(TextFolder textfolder)
        {
            GetRepository();
            var values = new NameValueCollection();
            return ServiceFactory.TextContentManager.Add(Repository.Current, textfolder, values, null, null, "");
        }

        public string Serialize<T>(T value)
        {
            if (value == null)
                return null;

            var serializer = new XmlSerializer(typeof(T));

            var settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        public void MainThread()
        {
            // Every minute.
            Thread.Sleep(60000);

            // Loop all import tasks.
            var importSettingsService = new ImportSettingsService();
            var importProcessService = new ImportProcessService();
            var importSettings = importSettingsService.GetAll();
            
            foreach (var importSetting in importSettings)
            {
                if (importSetting.Enabled && !importSetting.IsRunning() && 
                    importSetting.LastStartedAt.AddMinutes(importSetting.RepeatIntervalInMinutes) < DateTime.Now)
                    importProcessService.StartImport(importSetting.UUID);
            }
            this.MainThread();
        }

        public void Import(string uuid)
        {
            var stared = DateTime.Now;
            var importSettingsService = new ImportSettingsService();
            var setting = importSettingsService.Get(uuid);
            setting.SetLastStartedAt(DateTime.Now);
            var folder = setting.ContentTypeFolder;
            var textfolder = ServiceFactory.TextFolderManager.Get(Repository.Current, folder);
            var dataToImport = GetDataToImport(uuid);
            var totalCountStart = dataToImport.Count;
            var elapsedCount = dataToImport.Count;
            var importProcessService = new ImportProcessService();
            foreach (var data in dataToImport)
            {
                var settingNow = importSettingsService.Get(uuid);
                if(!settingNow.Enabled)
                    break;
                importProcessService.SetProcess(new ImportProcessModel(uuid, elapsedCount, totalCountStart, true));
                IntegrationProgress.Send(new Progress { Uuid = uuid, TotalCount = totalCountStart, ElapsedCount = elapsedCount, StartDate = stared.ToString("HH:mm:ss"), ElapsedTime = (DateTime.Now - stared).ToString(@"hh\:mm\:ss"), Active = true});
                ServiceFactory.TextContentManager.Add(Repository.Current, textfolder, data, null, null);
                elapsedCount--;
            }
            
            importProcessService.SetProcess(new ImportProcessModel(uuid));
            IntegrationProgress.Send(new Progress { Uuid = uuid, TotalCount = totalCountStart, ElapsedCount = elapsedCount, StartDate = stared.ToString("HH:mm:ss"), ElapsedTime = (DateTime.Now - stared).ToString(@"hh\:mm\:ss"), Active = false });

            // Remove the thread.
            importProcessService.GetThreads().Remove(uuid);
        }

        public T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default(T);


            var serializer = new XmlSerializer(typeof(T));

            var settings = new XmlReaderSettings();
            // No settings need modifying here

            using (var textReader = new StringReader(xml))
            {
                using (var xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        public ImportDataModel GetImportInfo(string uuid)
        {
            var settingsService = new ImportSettingsService();
            var setting = settingsService.Get(uuid);
            var data = GetSqlData(setting);
            var model = new ImportDataModel();
            if (data != null)
            {
                model.ItemsToImport = data.Rows.Count;
            }
            return model;
        }

        public List<string> GetSchemaStructure(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                return new List<string>();

            GetRepository();

            var f = ServiceFactory.TextFolderManager.Get(Repository.Current, folderName);
            if (f != null)
            {
                var schema = ServiceFactory.SchemaManager.Get(Repository.Current, f.SchemaName);
                return schema.Columns.Select(a => a.Name).ToList();
            }
            return new List<string>();
        }

        

        public List<NameValueCollection> GetDataToImport(string uuid)
        {
            var settingsService = new ImportSettingsService();
            var objectsToImport = new List<NameValueCollection>();
            var setting = settingsService.Get(uuid);

            var dataFromSql = GetSQL(uuid);
            foreach (var row in dataFromSql)
            {
                var objectToImport = new NameValueCollection();
                foreach (var mappedField in setting.MappedFields)
                {
                    if (!string.IsNullOrEmpty(row[mappedField.SourceField]))
                    {
                        if (!mappedField.IsDefault)
                            objectToImport.Add(mappedField.KoobooField, row[mappedField.SourceField]);
                        else
                            objectToImport.Add(mappedField.KoobooField, mappedField.DefaultValue);
                    }
                }
                if(objectToImport.Count > 0)
                    objectsToImport.Add(objectToImport);
            }

            return objectsToImport;
        }

        public NameValueCollection GetStructureSQL(string uuid)
        {
            var list = GetSQL(uuid, true);
            if (list == null || list.Count == 0)
                return null;
            return list.First();
        }

        private DataTable GetSqlData(ImportSetting setting, bool onlyStructure = false)
        {
            string command = "";
            SqlConnection con = null;
            con = new SqlConnection(setting.ConnectionString);
            switch (setting.SourceType)
            {
                case SourceTypeEnum.SQL:
                    command = string.Format(onlyStructure ? "SELECT TOP 1 * FROM {0} WHERE Deleted = 0" : "SELECT * FROM {0} WHERE Deleted = 0", setting.DatabaseTable);
                    break;
                case SourceTypeEnum.SQLQUERY:
                    command = setting.Query;
                    break;
            }

            var data = new DataTable();
            var da = new SqlDataAdapter(command, con);
            da.Fill(data);
            return data;
        }

        public List<NameValueCollection> GetSQL(string uuid, bool onlyStructure = false)
        {
            var objects = new List<NameValueCollection>();
            var importService = new ImportSettingsService();
            try
            {
                var data = GetSqlData(importService.Get(uuid), onlyStructure);

                foreach (DataRow item in data.Rows)
                {
                    var o = new NameValueCollection();
                    foreach (DataColumn col in data.Columns)
                        o.Add(col.ColumnName, item[col.ColumnName].ToString());

                    objects.Add(o);
                }
            }
            catch (Exception ex)
            {
                return objects;
            }

            return objects;
        }

        public List<string> GetDatabaseStructure(string connectionString, string command)
        {
            return new List<string>();
        }

    }
}