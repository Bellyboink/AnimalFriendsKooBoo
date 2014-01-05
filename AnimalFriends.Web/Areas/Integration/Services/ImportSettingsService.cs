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

        public List<ImportSetting> GetAll()
        {
            var dataService = new DataService();

            dataService.GetRepository();
            var list = new List<ImportSetting>();

            // Get KooBooFolders
            var folder = dataService.GetImportSettingsFolder();
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
            return GetAll().FirstOrDefault(a => a.IsActive);
        }

        public ImportSetting Get(string uuid)
        {
            return GetAll().FirstOrDefault(a => a.UUID == uuid);
        }

        public List<MappedFieldModel> GetMappedFields(string uuid)
        {
            var dataService = new DataService();
            var folder = dataService.GetImportSettingsFolder();
            var content = folder.CreateQuery().FirstOrDefault(a => a.UUID == uuid);
            var mappedFields = content.GetValue("MappedFields");
            if (string.IsNullOrEmpty(mappedFields))
                return new List<MappedFieldModel>();

            var data = dataService.Deserialize<List<MappedFieldModel>>(mappedFields);
            return data;
        }

        public void SetMappedFields(string uuid, List<MappedFieldModel> data)
        {
            var dataService = new DataService();
            var dataAsString = dataService.Serialize<List<MappedFieldModel>>(data);
            this.UpdateImportSettings(uuid, new NameValueCollection { { "MappedFields", dataAsString } });
        }

        public List<MappedFieldModel> AddMappedField(string uuid, MappedFieldModel data)
        {
            var dataService = new DataService();
            var mappedFields = GetMappedFields(uuid);
            mappedFields.Add(data);
            var dataAsString = dataService.Serialize<List<MappedFieldModel>>(mappedFields);
            this.UpdateImportSettings(uuid, new NameValueCollection { { "MappedFields", dataAsString } });
            return mappedFields;
        }

        public List<MappedFieldModel> RemoveMappedField(string uuid, MappedFieldModel data)
        {
            var dataService = new DataService();
            var mappedFields = GetMappedFields(uuid);
            var elementToRemove = mappedFields.FirstOrDefault(a => a.KoobooField == data.KoobooField);
            if (elementToRemove != null)
            {
                mappedFields.Remove(elementToRemove);
                var dataAsString = dataService.Serialize<List<MappedFieldModel>>(mappedFields);
                UpdateImportSettings(uuid, new NameValueCollection {{"MappedFields", dataAsString}});
            }
            return mappedFields;
        }

        public void UpdateImportSettings(string uuid, NameValueCollection settings)
        {
            var dataService = new DataService();
            dataService.GetRepository();
            var folder = ServiceFactory.TextFolderManager.Get(Repository.Current, "ImportSetting");
            ServiceFactory.TextContentManager.Update(Repository.Current, folder, uuid, settings);
        }
    }
}