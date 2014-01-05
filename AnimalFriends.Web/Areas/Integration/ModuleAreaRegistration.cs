#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion

using System.Threading;
using System.Web.Mvc;
using System.IO;
using Kooboo;
using Kooboo.Web.Mvc;
using Kooboo_CMS.Areas.Integration.Services;

namespace AnimalFriends.Integration
{
    public class SampleAreaRegistration : AreaRegistration
    {
        public const string ModuleName = "Integration";
        public override string AreaName
        {
            get
            {
                return ModuleName;
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               ModuleName + "_default",
                ModuleName + "/{controller}/{action}",
                new { controller = "Admin", action = "Index" }
                , null
                , new[] { "AnimalFriends.Integration.Controllers", "AnimalFriends.Integration.Areas.Integration.Controllers", "Kooboo.Web.Mvc", "Kooboo.Web.Mvc.WebResourceLoader" }
            );

            var menuFile = AreaHelpers.CombineAreaFilePhysicalPath(AreaName, "Menu.config");
            if (File.Exists(menuFile))
            {
                Kooboo.Web.Mvc.Menu.MenuFactory.RegisterAreaMenu(AreaName, menuFile);
            }
            var resourceFile = Path.Combine(Settings.BaseDirectory, "Areas", AreaName, "WebResources.config");
            if (File.Exists(resourceFile))
            {
                Kooboo.Web.Mvc.WebResourceLoader.ConfigurationManager.RegisterSection(AreaName, resourceFile);
            }
            var importProcessService = new ImportProcessService();
            importProcessService.StartAllIntegrations();
        }
    }
}
