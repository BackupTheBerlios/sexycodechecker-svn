/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Cluefultoys.Xml {

    // TODO: Study Microsoft Configuration
    // TODO: Or Try to supplant singleton with DI
    public sealed class CftConfiguration {

        private CftConfiguration() {
        }
        
        public static string GetConfigurationString(string resourceName, string xmlNamespace, string element, Type caller) {
            Assembly assembly = Assembly.GetAssembly(caller);
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                return Utilities.GetString(stream, xmlNamespace, element);
            }
        }

    }

    // TODO: Try to supplant singleton with DI
    public sealed class Utilities {

        private Utilities() {
        }

        private const string namespacePrefix = "ns1";

        private static string GetXPathQuery(string namespaceUri, string element) {
            return "//" + namespaceUri + ":" + element;
        }

        public static string GetString(Stream stream, string namespaceUri, string element) {
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace(namespacePrefix, namespaceUri);
            XmlNode node = document.SelectSingleNode(GetXPathQuery(namespacePrefix, element), namespaceManager);

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
