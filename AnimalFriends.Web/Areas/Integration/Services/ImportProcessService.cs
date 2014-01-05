using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using System.Web.Caching;


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
            var key = GetKey(process.UUID);
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
    }

    public class ImportProcessModel
    {
        public string UUID { get; set; }
        public int ItemsLeft { get; set; }
        public int ItemsTotal { get; set; }
        public int Procent { get; set; }
        public bool Running { get; set; }

        public ImportProcessModel(string uuid, int itemsleft = 1, int itemsTotal = 2, bool running = false)
        {
            Running = running;
            UUID = uuid;
            ItemsLeft = itemsleft;
            ItemsTotal = itemsTotal;
            Procent = 100;
            if (ItemsLeft != 0)
            {
                var procent = (decimal) ItemsLeft/(decimal) ItemsTotal*100;
                Procent = (int) procent;
            }
        }
    }
}