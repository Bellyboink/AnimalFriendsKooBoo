using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using Kooboo_CMS.Areas.Integration.Models;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class ImportProcessService
    {
        private string GetKey(string uuid)
        {
            return "Items" + uuid;
        }

        private string GetThreadKey()
        {
            return "Thread";
        }

        public ImportProcessModel GetProcess(string uuid)
        {
            var key = GetKey(uuid);
            var cache = MemoryCache.Default;
            object item = cache.Get(key);
            if (item != null)
                return (ImportProcessModel)item;
            return new ImportProcessModel(uuid);
        }

        public void SetProcess(ImportProcessModel process)
        {
            var key = GetKey(process.Uuid);
            ObjectCache cache = MemoryCache.Default;
            if (process.ItemsLeft != 0)
                cache.Set(key, process, new DateTimeOffset(DateTime.Now.AddMinutes(30)));
            else
                cache.Remove(key);
        }

        public Dictionary<string, Thread> GetThreads()
        {
            var key = GetThreadKey();
            var cache = MemoryCache.Default;
            object list = cache.Get(key);
            if (list != null)
                return (Dictionary<string, Thread>)list;
            return new Dictionary<string, Thread>();
        }

        public void SetThreads(Dictionary<string, Thread> threads)
        {
            var key = GetThreadKey();
            var cache = MemoryCache.Default;
            cache.Set(key, threads, new DateTimeOffset(DateTime.Now.AddMinutes(200)));
        }

        public void StartImport(ImportSetting importSetting)
        {
            var threads = GetThreads();
            if (!threads.ContainsKey(importSetting.UUID))
            {
                var service = new ImportService();
                var process = new ImportProcessModel(importSetting.UUID, 1, 2, true);
                SetProcess(process);
                var importThread = new Thread(() => service.Import(importSetting));
                importThread.Start();

                threads.Add(importSetting.UUID, importThread);
                SetThreads(threads);
            }
        }

        public void StartAllIntegrations()
        {
            var importSettingsService = new ImportSettingsService();
            var importProcessService = new ImportProcessService();
            var importSettings = importSettingsService.GetAll();

            foreach (var importSetting in importSettings)
            {
                if (importSetting.Enabled && importSetting.RunOnApplicationStartup)
                    importProcessService.StartImport(importSetting);
            }

            var importService = new ImportService();
            var mainIntegrationThread = new Thread(importService.MainThread);
            mainIntegrationThread.Start();
        }
    }

    public class ImportProcessModel
    {
        public string Uuid { get; set; }
        public int ItemsLeft { get; set; }
        public int ItemsTotal { get; set; }
        public int Procent { get; set; }
        public bool Running { get; set; }

        public ImportProcessModel(string uuid, int itemsleft = 1, int itemsTotal = 2, bool running = false)
        {
            Running = running;
            Uuid = uuid;
            ItemsLeft = itemsleft;
            ItemsTotal = itemsTotal;
            Procent = 100;
            if (ItemsLeft != 0)
            {
                var procent = ItemsLeft / ItemsTotal * 100;
                Procent = procent;
            }
        }
    }
}