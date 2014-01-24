using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class MappedFieldModel
    {
        public bool IsDefault { get; set; }
        public string DefaultValue { get; set; }
        public string SourceField { get; set; }
        public string KoobooField { get; set; }
        public bool Key { get; set; }
    }
}