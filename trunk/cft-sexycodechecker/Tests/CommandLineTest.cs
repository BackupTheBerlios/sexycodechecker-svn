using NUnit.Framework;
using Cluefultoys.Xml;
using Cluefultoys.Nunit;

namespace Cluefultoys.CommandLine {

    [TestFixture]
    public class CommandLineTest : TestBase {

        private const string myFilesPath = "Tests/Files/Configuration/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        private static string[] strings = new string[] {
            "a.txt", //a* //*.txt //*.* //?.* // ?.???
            "b.txta", //b* //*.txta //*.* // ?.*
            "ib.txta", //i?.txta //*.txta //ib.txt?a
        };

        // vedere prima se file e directory accettano asterischi e interroghini
        //a*
 
        //*.txt



    }

}