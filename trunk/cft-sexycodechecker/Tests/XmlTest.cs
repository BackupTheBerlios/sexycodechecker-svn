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
using System.Collections.ObjectModel;

using NUnit.Framework;

using Cluefultoys.Sexycodechecker;
using Cluefultoys.Sexycodechecker.Tests;
namespace Cluefultoys.Xml.Tests {

    [TestFixture]
    public class LoadingConfigurationSpike : TestParent {

        private const string ParametersResource = "Cluefultoys.Tests.Resources.Parameters.xml";

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
            using (Stream stream = assembly.GetManifestResourceStream(ParametersResource)) {
                XmlDocument document = new XmlDocument();
                document.Load(stream);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
                namespaceManager.AddNamespace("ns1", "http://limacat.googlepages.com/cluefultoys/tests/parameters.xsd");
                XmlNode node = document.SelectSingleNode("//ns1:Ping", namespaceManager);

                Assert.IsNotNull(node);
                Assert.AreEqual("ping", node.InnerText);
            }
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
            using (Stream stream = assembly.GetManifestResourceStream(ParametersResource)) {
                XmlDocument document = new XmlDocument();
                document.Load(stream);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
                namespaceManager.AddNamespace("ns1", "http://limacat.googlepages.com/cluefultoys/tests/parameters.xsd");
                XmlNode node = document.SelectSingleNode("//ns1:ThisElementDoesNotExistAndNeverWill", namespaceManager);

                Assert.IsNull(node);
            }
        }
    }

    [TestFixture]
    public class MsBuildReaderTest : TestParent {

        private MSBuildReader reader;

        private string[] expected;

        private string[] doNotExpect;

        private string configurationFile;

        private Collection<string> actual;

        [SetUp]
        protected new void SetUp() {
            doNotExpect = new string[] { };
            expected = new string[] { };
        }

        [TearDown]
        protected new void TearDown() {
            reader = null;
            doNotExpect = null;
            expected = null;
            configurationFile = null;
            actual = null;
        }

        private const string myFilesPath = "Tests/Files/Schemas/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        private string FoundAllFiles() {
            string reason = "";
            foreach (string check in expected) {
                string completeFile = GetFileName(check);
                if (!actual.Contains(completeFile)) {
                    reason += string.Format("[{0} was not found inside the result collection]", check);
                }
            }

            foreach (string check in doNotExpect) {
                string completeFile = GetFileName(check);
                if (actual.Contains(completeFile)) {
                    reason += string.Format("[{0} was found inside the result collection]", check);
                }
            }

            return reason;
        }

        private void DoTheBuilderCheck() {
            reader = new MSBuildReader(configurationFile);
            actual = reader.FilesToInclude();
            string reason = FoundAllFiles();
            Assert.AreEqual("", reason);
        }

        private const string I1 = "I1.cs";

        private const string I2 = "I2.cs";

        private const string I3 = "I3.cs";

        private const string I4 = "I4.cs";

        private const string II5 = "I/I5.cs";

        private const string II6 = "I/I6.cs";

        private const string IA = "IA.vb";

        private const string IDDES = "ID.Designer.cs";

        [Test]
        public void MsBuildAllFiles() {
            expected = new string[] { I1, I2, I3, I4 };
            doNotExpect = new string[] { IA };
            configurationFile = GetFileName("MsBuildAllFiles.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildNoFiles() {
            doNotExpect = new string[] { I1, I2, I3, I4, IA };
            configurationFile = GetFileName("MsBuildNoFiles.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildExcludeDesigner() {
            doNotExpect = new string[] { IDDES };
            configurationFile = GetFileName("MsBuildExcludeDesigner.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildForceIncludeDesigner() {
            expected = new string[] { IDDES };
            configurationFile = GetFileName("MsBuildForceIncludeDesigner.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildNothingGroupsExcluded() {
            expected = new string[] { IDDES };
            doNotExpect = new string[] { I1, I2, I3, I4, IA };
            configurationFile = GetFileName("MsBuildNothingGroupsExcluded.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildPartial() {
            expected = new string[] { I2, I3 };
            doNotExpect = new string[] { I1, I4 };
            configurationFile = GetFileName("MsBuildPartial.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildImportChildAll() {
            expected = new string[] { II5, II6 };
            configurationFile = GetFileName("MsBuildImportChildAll.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildImportChildNothing() {
            doNotExpect = new string[] { II5, II6 };
            configurationFile = GetFileName("MsBuildImportChildNothing.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildAllAndAllChildren() {
            expected = new string[] { I1, I2, I3, I4, II5, II6 };
            doNotExpect = new string[] { IA };
            configurationFile = GetFileName("MsBuildAllAndAllChildren.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildDoNotImportChild() {
            doNotExpect = new string[] { II5, II6 };
            configurationFile = GetFileName("MsBuildDoNotImportChild.csproj");

            DoTheBuilderCheck();
        }

        [Test]
        public void MsBuildAllFilesNoOverlay() {
            expected = new string[] { I1, I2, I3, I4 };
            doNotExpect = new string[] { IA };
            configurationFile = GetFileName("MsBuildAllFilesNoOverlay.csproj");

            DoTheBuilderCheck();
            throw new Exception("Nothing to do!");
        }

        [Test]
        public void MsBuildNoFilesNoOverlay() {
            // change the test!
            doNotExpect = new string[] { I1, I2, I3, I4, IA };
            configurationFile = GetFileName("MsBuildNoFilesNoOverlay.csproj");

            DoTheBuilderCheck();
            throw new Exception("Nothing to do!");
        }

    }

    [TestFixture]
    public class CftConfigurationTest : TestParent {

        private const string ParametersResource = "Cluefultoys.Tests.Resources.Parameters.xml";

        [Test]
        public void GetConfiguration() {
            string result = CftConfiguration.GetConfigurationString(ParametersResource, ParametersNamespace, "Ping", typeof(LoadingConfigurationSpike));

            Assert.AreEqual("ping", result);
        }

    }

    [TestFixture]
    public class UtilitiesTest : TestParent {

        private const string myFilesPath = "Tests/Files/Configuration/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        [Test]
        public void ReadConfigurationWithDefaultNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithDefaultNamespace.xml")) {
                string result = Utilities.GetString(stream, ParametersNamespace, "Ping");
                Assert.AreEqual("ping", result);
            }
        }

        [Test]
        public void ReadConfigurationWithAliasedNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithAliasedNamespace.xml")) {
                string result = Utilities.GetString(stream, ParametersNamespace, "Ping");
                Assert.AreEqual("ping", result);
            }
        }

        [Test]
        public void ReadConfigurationWithNoNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithNoNamespace.xml")) {
                string result = Utilities.GetString(stream, ParametersNamespace, "Ping");
                Assert.AreEqual("ping", result);
            }
        }

        [Test]
        public void ReadConfigurationWithDifferentNamespace() {
            using (Stream stream = OpenFile("ReadConfigurationWithDifferentNamespace.xml")) {
                string result = Utilities.GetString(stream, ParametersNamespace, "Ping");
                Assert.IsNull(result);
            }
        }
    }

}
