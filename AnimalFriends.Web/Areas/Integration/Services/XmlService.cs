using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Kooboo_CMS.Areas.Integration.Models;

namespace Kooboo_CMS.Areas.Integration.Services
{
    public class XmlService
    {
        public T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default(T);

            var serializer = new XmlSerializer(typeof(T));

            var settings = new XmlReaderSettings();
            // No settings need modifying here

            using (var textReader = new StringReader(xml))
            {
                using (var xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }
        public string Serialize<T>(T value)
        {
            if (value == null)
                return null;

            var serializer = new XmlSerializer(typeof(T));

            var settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        public XDocument ReadXml(string directoryPath, string fileSearchPattern)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            var files = directoryInfo.GetFiles(fileSearchPattern).OrderBy(a => a.CreationTime).ToList();
            var firstFile = files.First();

            return XDocument.Load(firstFile.FullName);
        }

        public NameValueCollection GetSourceStructure(ImportSetting importSetting)
        {
            var xml = ReadXml(importSetting.XmlReadFolder, "*.*");
            var element = Recurse(xml.Root);
            var e = element.FirstOrDefault(a => a.Name.LocalName == importSetting.XmlItemName.ToLower());
            var nameValueCollection = new NameValueCollection();
            foreach (var xElement in e.Elements())
                nameValueCollection.Add(xElement.Name.LocalName, xElement.Name.LocalName);
            return nameValueCollection;
        }

        private List<XElement> Recurse(XElement element)
        {
            return Recurse(element, new List<XElement>());
        }

        private List<XElement> Recurse(XElement element, List<XElement> list)
        {
            list.Add(element);
            if (element.HasElements){
                Recurse(element.Elements().FirstOrDefault(), list);
            }
            return list;
        }

        public List<NameValueCollection> GetData(ImportSetting importSetting)
        {
            return new List<NameValueCollection>();
        }
    }
}