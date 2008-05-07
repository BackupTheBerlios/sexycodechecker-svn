/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License 
 * See Copying.txt for the full details.
 */
using NUnit.Framework;
using Cluefultoys.Nunit;

namespace Cluefultoys.Sexycodechecker {

    [TestFixture]
    public class HotProject : CheckerParent {

        private const string myFilesPath = "Src/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        [Test]
        public void Checker() {
            CallCheck(GetFilename("Checker.cs"));
            IsHot();
        }

        [Test]
        public void Xml() {
            CallCheck(GetFilename("Xml.cs"));
            IsHot();
        }

        [Test]
        public void Handler() {
            CallCheck(GetFilename("Handler.cs"));
            IsHot();
        }

        [Test]
        public void Rules() {
            CallCheck(GetFilename("Rules.cs"));
            IsHot();
        }

    }


}
