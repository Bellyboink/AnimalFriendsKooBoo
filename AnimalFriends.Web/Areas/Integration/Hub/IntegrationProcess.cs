using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace Kooboo_CMS.Areas.Integration.SignalR
{
    public class IntegrationProgress : Hub
    {
        public static void Send(Progress data)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<IntegrationProgress>();
            hub.Clients.All.broadcastMessage(data);
        }
    }
    public class Progress
    {
        public string Uuid { get; set; }
        public int ElapsedCount { get; set; }
        public int TotalCount { get; set; }
        public string StartDate { get; set; }
        public string ElapsedTime { get; set; }
        public bool Active { get; set; }
    }
}