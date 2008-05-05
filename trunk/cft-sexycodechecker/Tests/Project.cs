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
