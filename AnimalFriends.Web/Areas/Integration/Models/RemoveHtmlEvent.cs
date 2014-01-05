using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class RemoveHtmlEvent : IIntegrationEvent
    {
        public int ID
        {
            get
            {
                return 10001;
            }
        }

        public string Name
        {
            get { return "Remove HTML tags"; }
        }

        public RemoveHtmlEvent() 
        {
            string hej = "sadasdasd";
            string he2 = "sadasdasd";
        }

        public string DoEvent(string text)
        {
            string HTML_TAG_PATTERN = "<.*?>";
            return Regex.Replace(text, HTML_TAG_PATTERN, string.Empty);
        }
       
    }
}