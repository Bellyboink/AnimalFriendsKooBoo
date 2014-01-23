using Kooboo.CMS.Common.Runtime;
using Kooboo.CMS.Common.Runtime.Dependency;
using Kooboo.Collections;
using Kooboo.Web.Mvc;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kooboo_CMS.Areas.Integration.SignalR
{
    [Dependency(typeof(Kooboo.CMS.Common.IHttpApplicationEvents), Key = "SignalRConfig", Order = -1)]
    public class SignalRConfig : Kooboo.CMS.Common.HttpApplicationEvents
    {
        public override void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.MapHubs();

            base.Application_Start(sender, e);
        }
    }
}