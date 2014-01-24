using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Kooboo_CMS.Areas.Integration.Models;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class SqlService
    {
        public List<NameValueCollection> GetData(ImportSetting importSetting, bool onlyStructure = false)
        {
            var objects = new List<NameValueCollection>();
            try
            {
                var data = GetSqlData(importSetting, onlyStructure);

                foreach (DataRow item in data.Rows)
                {
                    var o = new NameValueCollection();
                    foreach (DataColumn col in data.Columns)
                        o.Add(col.ColumnName, item[col.ColumnName].ToString());

                    objects.Add(o);
                }
            }
            catch (Exception ex)
            {
                return objects;
            }

            return objects;
        }

        private DataTable GetSqlData(ImportSetting setting, bool onlyStructure = false)
        {
            string command = "";
            var con = new SqlConnection(setting.ConnectionString);
            switch (setting.SourceType)
            {
                case SourceTypeEnum.SQL:
                    command = string.Format(onlyStructure ? "SELECT TOP 1 * FROM {0} WHERE Deleted = 0" : "SELECT * FROM {0} WHERE Deleted = 0", setting.DatabaseTable);
                    break;
                case SourceTypeEnum.SQLQUERY:
                    command = setting.Query;
                    break;
            }

            var data = new DataTable();
            var da = new SqlDataAdapter(command, con);
            da.Fill(data);
            return data;
        }

        public NameValueCollection GetSourceStructure(ImportSetting importSetting)
        {
            var list = GetData(importSetting, true);
            if (list == null || list.Count == 0)
                return null;
            return list.First();
        }
    }
}