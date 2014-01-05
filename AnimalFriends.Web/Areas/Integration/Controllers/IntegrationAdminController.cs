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
using Kooboo_CMS.Areas.Integration.Services;
using Kooboo_CMS.Areas.Integration.Models;

namespace AnimalFriends.Integration.Controllers
{
    public class IntegrationAdminController : AdminControllerBase
    {
        private DataService _dataService;
        private ImportSettingsService _importService;

        public IntegrationAdminController()
        {
            _dataService = new DataService();
            _importService = new ImportSettingsService();
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

            ViewBag.ActiveSetting = _importService.GetActive();

            return View();
        }

        public ActionResult ImportSetting(string id)
        {
            var model = _importService.Get(id);
            var structure = _dataService.GetStructureSQL(id);
            if (structure != null)
                ViewBag.DatabaseTableStructure = structure.AllKeys.ToList();

            return View(model);
        }

        public ActionResult EditImportSetting(string id)
        {
            var model = _importService.Get(id);
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
            ViewBag.ActiveImportSetting = _importService.GetActive();

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

        public ActionResult UpdateSettings(ImportSetting importSetting)
        {
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
                setting.Add("SourceType", ((int)importSetting.SourceType).ToString());

                _importService.UpdateImportSettings(importSetting.UUID, setting);
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetProperties(string folder)
        {
            ViewBag.Fields = _dataService.GetSchemaStructure(folder);
            return PartialView("_Fields");
        }

        //#region Connections

        //public ActionResult AddConnectionEvent(int eventToField, string field)
        //{
        //    var data = CookieService.GetFieldConnections();
        //    var fieldToEdit = data.First(a => a.KoobooField == field);
        //    if(fieldToEdit.Events == null)
        //        fieldToEdit.Events = new List<EventModel>();

        //    var eventModel = GetEvent(eventToField);
        //    if(eventModel != null && fieldToEdit.Events.Count(a=>a.ID == eventToField) == 0)
        //        fieldToEdit.Events.Add(eventModel);

        //    CookieService.SetFieldConnections(data);

        //    return PartialView("_Connected", data);
        //}

        //private EventModel GetEvent(int id)
        //{
        //    var list = GetAllEvents();
        //    if (list == null)
        //        return null;

        //    var item = list.FirstOrDefault(a=>a.ID == id);

        //    return (item != null) ? item : null;
        //}

        //private List<EventModel> GetAllEvents() 
        //{
        //    var list = new List<EventModel>();
        //    var t = typeof(IIntegrationEvent);
        //    var types = AppDomain.CurrentDomain.GetAssemblies()
        //        .SelectMany(s => s.GetTypes())
        //        .Where(p => t.IsAssignableFrom(p)).Where(a=>a.IsInterface == false);

        //    list.Add(new EventModel(){ ID = 0, Name = "None"});
        //    foreach (var type in types)
        //    {
        //        var tw = Activator.CreateInstance(type);
        //        var h = tw as IIntegrationEvent;
        //        list.Add(new EventModel() { ID = h.ID, Name = h.Name, AssemblyName = type.Assembly.FullName, TypeName = type.FullName });
        //    }

        //    return list;
        //}

        //public ActionResult GetEvents() 
        //{
        //    var model = GetAllEvents(); 
        //    return PartialView("_Events", model);
        //}

        //public ActionResult RemoveConnectionEvent(int eventToField, string field)
        //{
        //    var data = CookieService.GetFieldConnections();
        //    var fieldToEdit = data.First(a => a.KoobooField == field);
        //    fieldToEdit.Events = fieldToEdit.Events.Where(a => a.ID != eventToField).ToList();

        //    CookieService.SetFieldConnections(data);

        //    return PartialView("_Connected", data);
        //}

        public ActionResult GetAllImportSettings()
        {
            var list = _importService.GetAll();
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


        //public ActionResult RemoveFieldConnection(string databasecolumn, string field)
        //{
        //    var data = CookieService.GetFieldConnections();
        //    var first = data.FirstOrDefault(a => a.SourceField == databasecolumn);
        //    data.Remove(first);

        //    CookieService.SetFieldConnections(data);
        //    return PartialView("_Connected", data);
        //}

        //#endregion

        //#region Conditions

        //public ActionResult GetFieldConditions(string databasecolumn, string field)
        //{
        //    var conditions = CookieService.GetFieldConditions();

        //    return PartialView("_Conditions", conditions);
        //}

        //public ActionResult AddFieldConditions(string databasecolumn, string value, string condition)
        //{
        //    var conditions = CookieService.GetFieldConditions();

        //    conditions.Add(new ConditionModel(){ ColumnName = databasecolumn, Value = value, Condition = ConditionType.Same});

        //    CookieService.SetFieldConditions(conditions);
        //    return PartialView("_Conditions", conditions);
        //}

        //public ActionResult RemoveFieldConditions(string databasecolumn)
        //{
        //    var conditions = CookieService.GetFieldConditions();
        //    var condition = conditions.FirstOrDefault(a => a.ColumnName == databasecolumn);
        //    if (condition != null)
        //        conditions.Remove(condition);

        //    CookieService.SetFieldConditions(conditions);
        //    return PartialView("_Conditions", conditions);
        //}

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