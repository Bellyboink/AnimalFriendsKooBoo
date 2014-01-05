using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class ConditionModel
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public ConditionType Condition { get; set; }
    }
    public enum ConditionType
    {
        Same, Bigger, Smaller
    }
}