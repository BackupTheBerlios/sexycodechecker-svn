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

    public abstract class BaseTest {

        protected const string ParametersResource = "Cluefultoys.Tests.Resources.Parameters.xml";

        protected const string ParametersNamespace = "http://limacat.googlepages.com/Cluefultoys/Tests/Parameters.xsd";

        protected const string SourceRoot = "SourceRoot";

        // private Assembly assembly = Assembly.GetExecutingAssembly();

        protected BaseTest() {
            mySourceRootPath = CftConfiguration.GetConfigurationString(ParametersResource, ParametersNamespace, SourceRoot);
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

        protected Stream OpenFile(string fileName) {
            Stream stream = File.Open(GetFileName(fileName), FileMode.Open);
            return stream;
        }

        protected string GetFileName(string fileName) {
            return SourceRootPath + TestFilesPath + fileName;
        }

    }

}
