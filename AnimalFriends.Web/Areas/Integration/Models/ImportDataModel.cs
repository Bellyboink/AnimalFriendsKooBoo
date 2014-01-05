using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kooboo_CMS.Areas.Integration.Models
{
    public class ImportDataModel
    {
        public int ItemsToImport { get; set; }
        public List<ImportDataItemModel> Items { get; set; }
    }

    public class ImportDataItemModel
    {
    }
}