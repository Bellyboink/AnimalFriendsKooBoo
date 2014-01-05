using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class EventModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
    }
}