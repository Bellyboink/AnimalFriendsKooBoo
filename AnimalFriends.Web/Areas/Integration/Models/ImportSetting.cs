using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Kooboo.CMS.Content.Models;
using Kooboo_CMS.Areas.Integration.Services;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class ImportSetting
    {
        private ImportSettingsService _importService { get; set; }

        public SourceTypeEnum SourceType { get; set; }
        public string UUID { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseTable { get; set; }
        public string Query { get; set; }

        public string ContentTypeFolder { get; set; }
        public List<MappedFieldModel> MappedFields { get; set; }

        public bool Active { get; set; }
        public bool Enabled { get; set; }
        public bool RunOnApplicationStartup { get; set; }
        public DateTime LastStartedAt { get; set; }
        public int RepeatIntervalInMinutes { get; set; }

        public ImportSetting()
        {
            _importService = new ImportSettingsService();
        }

        public ImportSetting Populate(TextContent setting)
        {
            var sourceType = setting.GetValue<string>("SourceType");
            int sourceTypeInt;
            if (int.TryParse(sourceType, out sourceTypeInt))
                SourceType = (SourceTypeEnum) sourceTypeInt;
            else
                SourceType = SourceTypeEnum.None;

            UUID = setting.UUID;
            Name = setting.GetValue<string>("Name");
            ConnectionString = setting.GetValue<string>("ConnectionString");
            DatabaseTable = setting.GetValue<string>("DatabaseTable");
            ContentTypeFolder = setting.GetValue<string>("ContentTypeFolder");
            Query = setting.GetValue<string>("Query");
            RunOnApplicationStartup = setting.GetValue<bool>("RunOnApplicationStartup");
            Enabled = setting.GetValue<bool>("Enabled");
            LastStartedAt = setting.GetValue<DateTime>("LastStartedAt");
            RepeatIntervalInMinutes = setting.GetValue<int>("RepeatIntervalInMinutes");

            // Get mapped fields.
            MappedFields = _importService.GetMappedFields(setting.UUID);

            return this;
        }

        public void SetLastStartedAt(DateTime dateTime)
        {
            var collection = new NameValueCollection();
            collection.Add("LastStartedAt", dateTime.ToString());
            _importService.UpdateImportSettings(this.UUID, collection);
        }

        public bool IsRunning()
        {
            var service = new ImportProcessService();
            var model = service.GetProcess(this.UUID);
            return model.Running;
        }
    }

    public enum SourceTypeEnum
    {
        None = 0,
        SQL = 1,
        SQLQUERY = 2,
        XML = 3
    }

    public static class TextContentExtension
    {
        public static T GetValue<T>(this TextContent content, string key)
        {
            object o;
            if (content.TryGetValue(key, out o) && o != null)
                return (T) o;
            return default(T);
        }
    }
}