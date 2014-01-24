using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Cryptography;
using Kooboo.CMS.Content.Models;
using Kooboo.CMS.Content.Query;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Sites.View;
using Kooboo_CMS.Areas.Integration.Models;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class ImportSettingsService
    {
        public TextFolder GetImportSettingsFolder()
        {
            var dataService = new DataService();
            return dataService.GetFolder("ImportSetting");
        }

        public List<ImportSetting> GetAll()
        {
            var list = new List<ImportSetting>();

            // Get KooBooFolders
            var folder = GetImportSettingsFolder();
            if (folder != null)
            {
                foreach (var importSetting in folder.CreateQuery())
                {
                    var setting = new ImportSetting();
                    setting.Populate(importSetting);
                    list.Add(setting);
                }
            }
            return list;
        }

        public ImportSetting GetActive()
        {
            return GetAll().FirstOrDefault(a => a.Active);
        }

        public ImportSetting Get(string uuid)
        {
            return GetAll().FirstOrDefault(a => a.UUID == uuid);
        }

        public List<MappedFieldModel> GetMappedFields(string uuid)
        {
            var service = new XmlService();
            var folder = GetImportSettingsFolder();
            var content = folder.CreateQuery().FirstOrDefault(a => a.UUID == uuid);
            var mappedFields = content.GetValue<string>("MappedFields");
            if (string.IsNullOrEmpty(mappedFields))
                return new List<MappedFieldModel>();

            var data = service.Deserialize<List<MappedFieldModel>>(mappedFields);
            return data;
        }

        public void SetMappedFields(string uuid, List<MappedFieldModel> data)
        {
            var service = new XmlService();
            var dataAsString = service.Serialize<List<MappedFieldModel>>(data);
            this.UpdateImportSettings(uuid, new NameValueCollection { { "MappedFields", dataAsString } });
        }

        public List<MappedFieldModel> AddMappedField(string uuid, MappedFieldModel data)
        {
            var service = new XmlService();
            var mappedFields = GetMappedFields(uuid);
            mappedFields.Add(data);
            var dataAsString = service.Serialize<List<MappedFieldModel>>(mappedFields);
            this.UpdateImportSettings(uuid, new NameValueCollection { { "MappedFields", dataAsString } });
            return mappedFields;
        }

        public List<MappedFieldModel> RemoveMappedField(string uuid, MappedFieldModel data)
        {
            var service = new XmlService();
            var mappedFields = GetMappedFields(uuid);
            var elementToRemove = mappedFields.FirstOrDefault(a => a.KoobooField == data.KoobooField);
            if (elementToRemove != null)
            {
                mappedFields.Remove(elementToRemove);
                var dataAsString = service.Serialize<List<MappedFieldModel>>(mappedFields);
                UpdateImportSettings(uuid, new NameValueCollection {{"MappedFields", dataAsString}});
            }
            return mappedFields;
        }

        public ContentBase CreateSetting()
        {
            var dataService = new DataService();
            var textfolder = GetImportSettingsFolder();
            return dataService.CreateContent(textfolder);
        }

        public void DeleteSetting(string uuid)
        {
            var dataService = new DataService();
            var textfolder = GetImportSettingsFolder();
            dataService.DeleteContent(uuid, textfolder);
        }

        public void UpdateImportSettings(string uuid, NameValueCollection settings)
        {
            var dataService = new DataService();
            dataService.GetRepository();
            var folder = GetImportSettingsFolder();
            ServiceFactory.TextContentManager.Update(Repository.Current, folder, uuid, settings);
        }
    }
}