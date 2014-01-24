using Cryptography;
using Kooboo_CMS.Areas.Integration.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class CookieService
    {
        #region Conditions

        public static List<ConditionModel> GetFieldConditions()
        {
            var decrypter = new RijndaelAlg();
            var dataAsString = GetCookie("FieldConditions");
            if (string.IsNullOrEmpty(dataAsString))
                return new List<ConditionModel>();

            var data = StringToConditions(decrypter.Decrypt(dataAsString));
            return data;
        }

        public static void SetFieldConditions(List<ConditionModel> conditions)
        {
            var dataAsString = ConditionsToString(conditions);
            var encrypter = new RijndaelAlg();
            SetCookie("FieldConditions", encrypter.Encrypt(dataAsString));
        }

        #endregion


        //public static Dictionary<string, string> GetFieldDefaults()
        //{
        //    var decrypter = new RijndaelAlg();
        //    var dataAsString = GetCookie("FieldDefaults");
        //    if (string.IsNullOrEmpty(dataAsString))
        //        return new Dictionary<string, string>();

        //    var data = StringToDictionary(decrypter.Decrypt(dataAsString));
        //    return data;
        //}

        //public static void SetFieldDefaults(Dictionary<string,string> defaults)
        //{
        //    var dataAsString = DictionaryToString(defaults);
        //    var encrypter = new RijndaelAlg();
        //    SetCookie("FieldDefaults", encrypter.Encrypt(dataAsString));
        //}


        //public static string GetTable()
        //{
        //    return GetCookie("Table");
        //}

        //public static void SetTable(string value)
        //{
        //    SetCookie("Table", value);
        //}

        //#endregion

        //public static string GetFolder()
        //{
        //    return GetCookie("Folder");
        //}

        //public static void SetFolder(string value)
        //{
        //    SetCookie("Folder", value);
        //}


        private static string GetCookie(string name)
        {
            HttpCookie myCookie = new HttpCookie(name);
            myCookie = HttpContext.Current.Request.Cookies[name];
            if (myCookie == null)
                return string.Empty;
            return myCookie.Value;
        }

        private static void SetCookie(string name, string value)
        {
            HttpCookie myCookie = new HttpCookie(name);
            DateTime now = DateTime.Now;

            // Set the cookie value.
            myCookie.Value = value;
            // Set the cookie expiration date.
            myCookie.Expires = now.AddYears(1);
            // Add the cookie.
            HttpContext.Current.Response.Cookies.Add(myCookie);
        }
   
        private static Dictionary<string, string> StringToDictionary(string data)
        {
            var list = new Dictionary<string, string>();

            var connections = data.Split('|');
            foreach (var item in connections)
            {
                if(item.Contains('='))
                {
                    var pair = item.Split('=');
                    list.Add(pair[0], pair[1]);
                }
            }
            return list;
        }

        private static string DictionaryToString(Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
                return string.Empty;
            string dataAsString = "";
            foreach (var item in data)
                dataAsString += item.Key + "=" + item.Value + "|";
            return dataAsString;
        }

        private static List<ConditionModel> StringToConditions(string conditions)
        {
            var list = new List<ConditionModel>();

            var connections = conditions.Split('|');
            foreach (var item in connections)
            {
                if (item.Contains(':'))
                {
                    var pair = item.Split(':');
                    list.Add(new ConditionModel() { ColumnName = pair[0], Value = pair[2], Condition = ConditionType.Same });
                }
            }
            return list;
        }

        private static string ConditionsToString(List<ConditionModel> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return string.Empty;
            string dataAsString = "";
            foreach (var item in conditions)
                dataAsString += item.ColumnName + ":" + item.Condition.ToString() + ":" + item.Value + "|";
            return dataAsString;
        }

    }
}