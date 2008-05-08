/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License 
 * See Copying.txt for the full details.
 */
using NUnit.Framework;
using Cluefultoys.Nunit;

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
        public void Xml() {
            CallCheck(GetFileName("Xml.cs"));
            IsHot();
        }

        [Test]
        public void Handler() {
            CallCheck(GetFileName("Handler.cs"));
            IsHot();
        }

        [Test]
        public void Rules() {
            CallCheck(GetFileName("Rules.cs"));
            IsHot();
        }

        [Test]
        public void Nunit() {
            CallCheck(GetFileName("Nunit.cs"));
            IsHot();
        }

        
    }


}
