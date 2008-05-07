/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License 
 * See Copying.txt for the full details.
 */
using System.IO;
using Cluefultoys.Xml;

namespace Cluefultoys.Nunit {

    public abstract class TestBase {

        protected const string TEST_PARAMETERS_RESOURCE = "Cluefultoys.Tests.Resources.Parameters.xml";

        protected const string TEST_PARAMETERS_NAMESPACE = "http://limacat.googlepages.com/Cluefultoys/Tests/Parameters.xsd";

        protected const string SOURCE_ROOT = "SourceRoot";

        // private Assembly assembly = Assembly.GetExecutingAssembly();

        public TestBase() {
            mySourceRootPath = Configuration.GetConfigurationString(TEST_PARAMETERS_RESOURCE, TEST_PARAMETERS_NAMESPACE, SOURCE_ROOT);
        }

        private string mySourceRootPath;

        protected string SourceRootPath {
            get {
                return mySourceRootPath;
            }
        }

        /// <summary>
        /// Must end in "/" eg "Tests/Xml/"
        /// </summary>
        protected abstract string TestFilesPath {
            get;
        }

        protected Stream OpenFile(string filename) {
            Stream stream = File.Open(GetFilename(filename), FileMode.Open);
            return stream;
        }

        protected string GetFilename(string filename) {
            return SourceRootPath + TestFilesPath + filename;
        }

    }

}
