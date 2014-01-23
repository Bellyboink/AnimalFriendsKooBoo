#region License
// 
// Copyright (c) 2013, Kooboo team
// 
// Licensed under the BSD License
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Threading;
using System.Web.Mvc;
using AnimalFriends.Integration.Models;
using Kooboo.CMS.Common;
using Kooboo.CMS.Content.Services;
using Kooboo.CMS.Content.Models;
using System.Linq;
using System.Collections.Specialized;
using Kooboo.CMS.Sites.Models;
using Kooboo_CMS.Areas.Integration.Services;
using Kooboo_CMS.Areas.Integration.Models;

namespace AnimalFriends.Integration.Controllers
{
    public class IntegrationAdminController : AdminControllerBase
    {
        private DataService _dataService;
        private ImportSettingsService _importSettingsService;

        public IntegrationAdminController()
        {
            _dataService = new DataService();
            _importSettingsService = new ImportSettingsService();
        }
        public ActionResult Index()
        {
            _dataService.GetRepository();
            ViewBag.Table = CookieService.GetTable();
            ViewBag.Folder = CookieService.GetFolder();
            ViewBag.Fields = _dataService.GetSchemaStructure(CookieService.GetFolder());

            // Get KooBooFolders
            var folders = ServiceFactory.TextFolderManager.All(Repository.Current, "").Cast<TextFolder>().ToList();
            var schema = folders.First().GetSchema();
            ViewBag.Folders = folders;

            ViewBag.ActiveSetting = _importSettingsService.GetActive();

            return View();
        }

        public ActionResult ImportSetting(string id)
        {
            var model = _importSettingsService.Get(id);
            var structure = _dataService.GetStructureSQL(id);
            if (structure != null)
                ViewBag.DatabaseTableStructure = structure.AllKeys.ToList();

            return View(model);
        }

        public ActionResult EditImportSetting(string id)
        {
            var model = _importSettingsService.Get(id);
            var structure = _dataService.GetStructureSQL(id);
            if (structure != null)
                ViewBag.DatabaseTableStructure = structure.AllKeys.ToList();

            ViewBag.Folders = ServiceFactory.TextFolderManager.All(Repository.Current, "").Cast<TextFolder>().ToList();

            return View(model);
        }

        public ActionResult Import(string uuid)
        {
            var service = new ImportProcessService();
            service.StartImport(uuid);
            
            return RedirectToAction("Index");
        }
        public ActionResult StopImport(string uuid)
        {
            var importProcessService = new ImportProcessService();
            var threads = importProcessService.GetThreads();
            if (threads[uuid] != null)
            {
                threads[uuid].Abort();
                var list = importProcessService.GetThreads();
                list.Remove(uuid);
                importProcessService.SetThreads(list);
            }

            importProcessService.SetProcess(new ImportProcessModel(uuid));
            return RedirectToAction("Index");
        }

        public ActionResult GetImportProcess(string uuid)
        {
            var importProcessService = new ImportProcessService();
            var model = importProcessService.GetProcess(uuid);
            return PartialView("_ImportProcess", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetProcessInfo(string uuid)
        {
            var importProcessService = new ImportProcessService();
            var model = importProcessService.GetProcess(uuid);
            return Json(model);
        }

        public ActionResult SourceSettings()
        {
            _dataService.GetRepository();
            var structure =_dataService.GetStructureSQL("ds");
            if (structure != null)
                ViewBag.DatabaseTableStructure = structure.AllKeys.ToList();
            ViewBag.ActiveImportSetting = _importSettingsService.GetActive();

            // Get KooBooFolders
            ViewBag.Folders = ServiceFactory.TextFolderManager.All(Repository.Current, "").Cast<TextFolder>().ToList();

            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(News news, string @return)
        {
            JsonResultData resultEntry = new JsonResultData(ViewData.ModelState);
            try
            {
                if (ModelState.IsValid)
                {
                    //repository.Add(news);
                    resultEntry.RedirectUrl = @return;
                }

            }
            catch (Exception e)
            {
                resultEntry.AddException(e);
            }

            return Json(resultEntry);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CreateSetting()
        {
            var newSetting = _importSettingsService.CreateSetting();
            return RedirectToAction("EditImportSetting", new { id = newSetting.UUID, siteName = Site.Name});
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateSetting(ImportSetting importSetting)
        {
            return RedirectToAction("Index");
        }

        public ActionResult UpdateSetting(ImportSetting importSetting)
        {
            if (Request.Form["action"] == "delete")
            {
                _importSettingsService.DeleteSetting(importSetting.UUID);
                return RedirectToAction("Index", new {siteName = Site.Name});
            }

            if (importSetting != null)
            {
                var setting = new NameValueCollection();
                
                if (importSetting.ConnectionString != null)
                    setting.Add("ConnectionString", importSetting.ConnectionString);
                if (importSetting.ContentTypeFolder != null)
                    setting.Add("ContentTypeFolder", importSetting.ContentTypeFolder);
                if (importSetting.DatabaseTable != null)
                    setting.Add("DatabaseTable", importSetting.DatabaseTable);
                if (importSetting.Name != null)
                    setting.Add("Name", importSetting.Name);
                if (importSetting.Query != null)
                    setting.Add("Query", importSetting.Query);
                setting.Add("Enabled", importSetting.Enabled.ToString());
                setting.Add("RunOnApplicationStartup", importSetting.RunOnApplicationStartup.ToString());
                setting.Add("SourceType", ((int)importSetting.SourceType).ToString());
                setting.Add("RepeatIntervalInMinutes", ((int)importSetting.RepeatIntervalInMinutes).ToString());

                _importSettingsService.UpdateImportSettings(importSetting.UUID, setting);
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult GetAllImportSettings()
        {
            var list = _importSettingsService.GetAll();
            return PartialView("_AllImportSettings", list);
        }

        public ActionResult AddMappedField(string uuid, MappedFieldModel mappedField)
        {
            var importSettingsService = new ImportSettingsService();
            var data = importSettingsService.AddMappedField(uuid, mappedField);
            ViewBag.UUID = uuid;
            return PartialView("_Connected", data);
        }

        public ActionResult RemoveMappedField(string uuid, MappedFieldModel mappedField)
        {
            var importSettingsService = new ImportSettingsService();
            var data = importSettingsService.RemoveMappedField(uuid, mappedField);
            ViewBag.UUID = uuid;
            return PartialView("_Connected", data);
        }
        public ActionResult GetImportData(string uuid)
        {
            var service = new DataService();
            var model = service.GetImportInfo(uuid);

            return PartialView("_ImportData", model);
        }

        public ActionResult GetFieldDefaults()
        {
            var defaults = CookieService.GetFieldDefaults();

            return PartialView("_Defaults", defaults);
        }

        public ActionResult AddDefaultField(string uuid, MappedFieldModel mappedField)
        {
            if (mappedField != null)
                mappedField.IsDefault = true;
            var importSettingsService = new ImportSettingsService();
            var data = importSettingsService.AddMappedField(uuid, mappedField);

            ViewBag.UUID = uuid;
            return PartialView("_Connected", data);
        }

        public ActionResult RemoveFieldDefaults(string field)
        {
            var defaults = CookieService.GetFieldDefaults();
            defaults.Remove(field);

            CookieService.SetFieldDefaults(defaults);
            return PartialView("_Defaults", defaults);
        }
    }
}