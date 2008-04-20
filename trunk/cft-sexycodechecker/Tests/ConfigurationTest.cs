using System;
using System.IO;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using Cluefultoys.Xml;
using Cluefultoys.Nunit;

namespace Cluefultoys.Xml {

    [TestFixture]
    public class LoadingTest : TestBase {

        private const string myFilesPath = "Tests/Files/Configuration/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        private Assembly assembly = Assembly.GetExecutingAssembly();

        [Test]
        public void LoadFileFromResource() {
            using (Stream stream = assembly.GetManifestResourceStream("Cluefultoys.Tests.Resources.FileToEmbed.txt")) {
                StreamReader streamReader = new StreamReader(stream);
                String myText = streamReader.ReadToEnd();

                Assert.AreEqual("crimson", myText);
            }
        }

        [Test]
        public void LoadXmlFromResource() {
            using (Stream stream = assembly.GetManifestResourceStream("Cluefultoys.Tests.Resources.Parameters.xml")) {

                XmlDocument document = new XmlDocument();
                document.Load(stream);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
                namespaceManager.AddNamespace("ns1", "http://limacat.googlepages.com/Cluefultoys/Tests/Parameters.xsd");
                XmlNode node = document.SelectSingleNode("//ns1:Ping", namespaceManager);

                Assert.IsNotNull(node);
                Assert.AreEqual("ping", node.InnerText);
            }
        }

        [Test]
        public void GetConfiguration() {
            string result = Configuration.GetConfigurationString(TEST_PARAMETERS_RESOURCE, TEST_PARAMETERS_NAMESPACE, "Ping");

            Assert.AreEqual("ping", result);
        }

        [Test]
        public void ApplyConfiguration() {
            using (Stream stream = OpenFile("FileToReadOnly.txt")) {
                StreamReader streamReader = new StreamReader(stream);
                String result = streamReader.ReadToEnd();

                Assert.AreEqual("crimson", result);
            }
        }

        [Test]
        public void NonExistingElement() {
            using (Stream stream = assembly.GetManifestResourceStream("Cluefultoys.Tests.Resources.Parameters.xml")) {

                XmlDocument document = new XmlDocument();
                document.Load(stream);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
                namespaceManager.AddNamespace("ns1", "http://limacat.googlepages.com/Cluefultoys/Tests/Parameters.xsd");
                XmlNode node = document.SelectSingleNode("//ns1:ThisElementDoesNotExistAndNeverWill", namespaceManager);

                Assert.IsNull(node);
            }
        }

        [Test]
        public void ReadConfigurationWithDefaultNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithDefaultNamespace.xml")) {
                string result = Utilities.GetString(stream, TEST_PARAMETERS_NAMESPACE, "Ping");
                Assert.AreEqual("ping", result);
            }
        }

        [Test]
        public void ReadConfigurationWithAliasedNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithAliasedNamespace.xml")) {
                string result = Utilities.GetString(stream, TEST_PARAMETERS_NAMESPACE, "Ping");
                Assert.AreEqual("ping", result);
            }
        }

        [Test]
        public void ReadConfigurationWithNoNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithNoNamespace.xml")) {
                string result = Utilities.GetString(stream, TEST_PARAMETERS_NAMESPACE, "Ping");
                Assert.AreEqual("ping", result);
            }
        }

        [Test]
        public void ReadConfigurationWithDifferentNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithDifferentNamespace.xml")) {
                string result = Utilities.GetString(stream, TEST_PARAMETERS_NAMESPACE, "Ping");
                Assert.IsNull(result);
            }
        }

    }
}
