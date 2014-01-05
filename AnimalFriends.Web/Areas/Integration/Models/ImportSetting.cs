using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kooboo.CMS.Content.Models;
using Kooboo_CMS.Areas.Integration.Services;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class ImportSetting
    {
        private ImportSettingsService _importService { get; set; }

        public ImportSetting()
        {
            _importService = new ImportSettingsService();
        }

        public SourceTypeEnum SourceType { get; set; }
        public string UUID { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseTable { get; set; }
        public string Query { get; set; }

        public string ContentTypeFolder { get; set; }
        public List<MappedFieldModel> MappedFields { get; set; }

        public bool IsActive { get; set; }

        public ImportSetting Populate(TextContent setting)
        {
            var sourceType = setting.GetValue("SourceType");
            int sourceTypeInt;
            if (int.TryParse(sourceType, out sourceTypeInt))
                SourceType = (SourceTypeEnum) sourceTypeInt;
            else
                SourceType = SourceTypeEnum.None;

            UUID = setting.UUID;
            Name = setting.GetValue("Name");
            ConnectionString = setting.GetValue("ConnectionString");
            DatabaseTable = setting.GetValue("DatabaseTable");
            ContentTypeFolder = setting.GetValue("ContentTypeFolder");
            Query = setting.GetValue("Query");

            // Get mapped fields.
            MappedFields = _importService.GetMappedFields(setting.UUID);

            return this;
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
        public static string GetValue(this TextContent content, string key)
        {
            object o;
            if(content.TryGetValue(key, out o) && o != null)
                return o.ToString();
            return string.Empty;
        }
    }
}