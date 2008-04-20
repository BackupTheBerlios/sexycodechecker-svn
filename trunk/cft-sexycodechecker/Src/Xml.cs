using System;
using System.IO;
using System.Reflection;
using System.Xml;
using NUnit.Framework;

namespace Cluefultoys.Xml {

    public class Configuration {

        public static string GetConfigurationString(string resourceName, string xmlNamespace, string element) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                return Utilities.GetString(stream, xmlNamespace, element);
            }
        }

    }

    public class Utilities {

        private const string localNamespace = "ns1";

        private static string GetXPathQuery(string namespaceHandle, string element) {
            return "//" + namespaceHandle + ":" + element;
        }

        public static string GetString(Stream stream, string namespaceHandle, string element) {
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace(localNamespace, namespaceHandle);
            XmlNode node = document.SelectSingleNode(GetXPathQuery(localNamespace, element), namespaceManager);

            if (node == null) {
                node = document.SelectSingleNode("//" + element);
            }
            if (node == null) {
                return null;
            }
            return node.InnerText;
        }

    }

}