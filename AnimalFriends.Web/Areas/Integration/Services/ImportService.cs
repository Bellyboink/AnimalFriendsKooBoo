using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Web;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Services;
using Kooboo_CMS.Areas.Integration.Models;
using Kooboo_CMS.Areas.Integration.SignalR;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class ImportService
    {
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
                    importProcessService.StartImport(importSetting);
            }
            this.MainThread();
        }

        public void Import(ImportSetting importSetting)
        {
            var stared = DateTime.Now;
            var importSettingsService = new ImportSettingsService();
            importSetting.SetLastStartedAt(DateTime.Now);
            var folder = importSetting.ContentTypeFolder;
            var textfolder = ServiceFactory.TextFolderManager.Get(Repository.Current, folder);
            var dataToImport = GetDataToImport(importSetting);
            var totalCountStart = dataToImport.Count;
            var elapsedCount = dataToImport.Count;
            var importProcessService = new ImportProcessService();
            foreach (var data in dataToImport)
            {
                var settingNow = importSettingsService.Get(importSetting.UUID);
                if (!settingNow.Enabled)
                    break;
                importProcessService.SetProcess(new ImportProcessModel(importSetting.UUID, elapsedCount, totalCountStart, true));
                IntegrationProgress.Send(new Progress { Uuid = importSetting.UUID, TotalCount = totalCountStart, ElapsedCount = elapsedCount, StartDate = stared.ToString("HH:mm:ss"), ElapsedTime = (DateTime.Now - stared).ToString(@"hh\:mm\:ss"), Active = true });
                ServiceFactory.TextContentManager.Add(Repository.Current, textfolder, data, null, null);
                elapsedCount--;
            }

            importProcessService.SetProcess(new ImportProcessModel(importSetting.UUID));
            IntegrationProgress.Send(new Progress { Uuid = importSetting.UUID, TotalCount = totalCountStart, ElapsedCount = elapsedCount, StartDate = stared.ToString("HH:mm:ss"), ElapsedTime = (DateTime.Now - stared).ToString(@"hh\:mm\:ss"), Active = false });

            // Remove the thread.
            importProcessService.GetThreads().Remove(importSetting.UUID);
        }
        public List<NameValueCollection> GetDataToImport(ImportSetting importSetting)
        {
            var objectsToImport = new List<NameValueCollection>();
            var sqlService = new SqlService();
            var xmlService = new XmlService();
            var dataToImport = new List<NameValueCollection>();
            switch (importSetting.SourceType)
            {
                case SourceTypeEnum.XML:
                    dataToImport = xmlService.GetData(importSetting);
                    break;
                case SourceTypeEnum.SQL:
                case SourceTypeEnum.SQLQUERY:
                    dataToImport = sqlService.GetData(importSetting);
                    break;
            }

            foreach (var row in dataToImport)
            {
                var objectToImport = new NameValueCollection();
                foreach (var mappedField in importSetting.MappedFields)
                {
                    if (!string.IsNullOrEmpty(row[mappedField.SourceField]))
                    {
                        if (!mappedField.IsDefault)
                            objectToImport.Add(mappedField.KoobooField, row[mappedField.SourceField]);
                        else
                            objectToImport.Add(mappedField.KoobooField, mappedField.DefaultValue);
                    }
                }
                if (objectToImport.Count > 0)
                    objectsToImport.Add(objectToImport);
            }

            return objectsToImport;
        }
    }

    public class ImportRow
    {
        
    }

    public class ImportConnection
    {

    }
}