/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using NUnit.Framework;
using Cluefultoys.Nunit;
using System.Collections.ObjectModel;
using Cluefultoys.Sexycodechecker;

namespace Cluefultoys.Sexycodechecker.Tests {

    [TestFixture]
    public class TestHotProject : TestParent {

        private const string myFilesPath = "Src/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        [Test]
        // Identifier 'Cluefultoys.Sexycodechecker.Tests.TestHotProject.Checker()'
        // differing only in case is not CLS-compliant (CS3005)
        public void Checkr() {
            CallCheck(GetFileName("Checker.cs"));
            IsHot();
        }

        [Test]
        public void Handler() {
            CallCheck(GetFileName("Handler.cs"));
            IsHot();
        }

        [Test]
        public void Nunit() {
            CallCheck(GetFileName("Nunit.cs"));
            IsHot();
        }

        [Test]
        public void Streams() {
            CallCheck(GetFileName("Streams.cs"));
            IsHot();
        }

        [Test]
        public void Rules() {
            CallCheck(GetFileName("Rules.cs"));
            IsHot();
        }

        [Test]
        public void Xml() {
            CallCheck(GetFileName("Xml.cs"));
            IsHot();
        }
        
    }

    [TestFixture]
    public class TestHotProjectByMsBuild : TestParent {

        private const string myFilesPath = "";
        
        private const string configurationFile = "cft-sexycodechecker.csproj";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        private void DoTheBuilderCheck() {
            bool isHot = true;
            ProjectChecker projectChecker = new ProjectChecker(GetFileName(configurationFile));
            projectChecker.Run();
            foreach (Results result in projectChecker.Results) {
                isHot &= result.Happy;
                foreach (Violation violation in result.Violations) {
                    System.Console.Out.WriteLine(violation);
                }
            }
            Assert.IsTrue(isHot);
        }
        
        [Test]
        public void TestWholeProject() {
            DoTheBuilderCheck();
        }
        
    }
    
}
