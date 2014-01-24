using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Kooboo.CMS.Common.Persistence.Non_Relational;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Sites.Models;
using Kooboo.CMS.Sites.View;
using Kooboo_CMS.Areas.Integration.Models;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class DataService
    {
        public void GetRepository()
        {
            Site.Current = SiteHelper.Parse("AnimalFriends").AsActual();
            Repository.Current = Site.Current.GetRepository().AsActual();
        }
        
        public TextFolder GetFolder(string folderFullname)
        {
            GetRepository();
            return ServiceFactory.TextFolderManager.Get(Repository.Current, folderFullname);
        }

        public TextFolder CreateFolder(string folderFullname)
        {
            GetRepository();
            return ContentHelper.NewTextFolderObject(folderFullname);
        }

        public void DeleteContent(string uuid, TextFolder textfolder)
        {
            GetRepository();
            ServiceFactory.TextContentManager.Delete(Repository.Current, textfolder, uuid);
        }

        public ContentBase CreateContent(TextFolder textfolder)
        {
            GetRepository();
            var values = new NameValueCollection();
            return ServiceFactory.TextContentManager.Add(Repository.Current, textfolder, values, null, null);
        }

        //public ImportDataModel GetImportInfo(string uuid)
        //{
        //    var settingsService = new ImportSettingsService();
        //    var setting = settingsService.Get(uuid);
        //    var data = GetSqlData(setting);
        //    var model = new ImportDataModel();
        //    if (data != null)
        //    {
        //        model.ItemsToImport = data.Rows.Count;
        //    }
        //    return model;
        //}

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

        public List<string> GetDatabaseStructure(string connectionString, string command)
        {
            return new List<string>();
        }

        public NameValueCollection GetSourceStructure(ImportSetting setting)
        {
            var sqlService = new SqlService();
            var xmlService = new XmlService();
            switch (setting.SourceType)
            {
                case SourceTypeEnum.XML:
                    return xmlService.GetSourceStructure(setting);
                case SourceTypeEnum.SQL:
                case SourceTypeEnum.SQLQUERY:
                    return sqlService.GetSourceStructure(setting);
            }
            return new NameValueCollection();
        }
    }
}