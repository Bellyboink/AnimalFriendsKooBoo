using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public interface IIntegrationEvent
    {
        int ID { get; }
        string Name { get; }

        string DoEvent(string text);
    }
}
