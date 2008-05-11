/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using NUnit.Framework;
using Cluefultoys.Xml;
using Cluefultoys.Nunit;
using Cluefultoys.Sexycodechecker;

namespace Cluefultoys.Sexycodechecker.Tests {

    // TODO make it easy for clients to embed SCC as Unit Tests
    public abstract class TestParent : BaseTest {

        private const string ParametersResource = "Cluefultoys.Tests.Resources.Parameters.xml";

        private string mySourceRootPath;
        
        protected override string SourceRootPath {
            get {
                return mySourceRootPath;
            }
        }
        
        private const string myFilesPath = "Tests/Files/Checker/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        protected Checker checker;

        protected Results results;

        protected string filename;

        protected TestParent() {
            mySourceRootPath = CftConfiguration.GetConfigurationString(ParametersResource, ParametersNamespace, SourceRoot, typeof(TestParent));
        }

        [SetUp]
        protected void SetUp() {
            checker = new Checker();
        }

        [TearDown]
        protected void TearDown() {
            checker = null;
            filename = null;
            results = null;
        }

        private const string noBut = "";

        protected void IsHot() {
            string whyNot = noBut;
            if (!results.Happy) {
                foreach (Violation violation in results.Violations) {
                    System.Console.Out.WriteLine(violation);
                }
                whyNot = string.Format("File {0} should be HOT but is not because of {1}", filename, results.Violations[0]);
            }

            Assert.IsTrue(results.Happy, whyNot);
        }

        protected void IsNot() {
            Assert.IsFalse(results.Happy, "File {0} should be NOT", filename);
        }

        protected void CallCheck(string filename) {
            this.filename = filename;
            results = checker.CheckFile(filename);
        }

    }

    [TestFixture]
    public class TestChecker : TestParent {

        [Test]
        public void FileNotFound() {
            CallCheck("C:/BLAH");
            IsNot();
            Assert.IsInstanceOfType(ViolationType.FileNotFound.GetType(), results.Violations[0].KindOfViolation, "File {1} should report violation {2}", filename, ViolationType.FileNotFound);
        }

        [Test]
        public void ValidSource() {
            CallCheck(GetFileName("ValidSource.cs"));
            IsHot();
        }

    }

    internal class TestCheckerMockLine : HeightRule {

        private IRule setup = new ContextSetup();
        private IRule teardown = new ContextTeardown();
                
        public override void Close(Context context) {
            setup.Close(context);
            base.Close(context);
            teardown.Close(context);
        }
        
        public override void Check(char currentCharacter, Context context) {
            setup.Check(currentCharacter, context);
            base.Check(currentCharacter, context);
            teardown.Check(currentCharacter, context);
        }
        
    }

    [TestFixture]
    public class TestHeightRule {

        private Context context;
        
        private TestCheckerMockLine lineChecker;

        [SetUp]
        protected void SetUp() {
            context = new Context();
            lineChecker = new TestCheckerMockLine();
        }

        [TearDown]
        protected void TearDown() {
            context = null;
            lineChecker = null;
        }

        [Test]
        public void Counting2Lines() {
            string twoLines = "/*\n*/";
            foreach (char c in twoLines.ToCharArray()) {
                lineChecker.Check(c, context);
            }
            Assert.AreEqual(0, lineChecker.CodeLenght, "there shouldn't be lines, but there are {0}", lineChecker.CodeLenght);
        }

        [Test]
        public void Rule1Bug200804004() {
            string oneLine = "/*\n";
            foreach (char c in oneLine.ToCharArray()) {
                lineChecker.Check(c, context);
            }
            Assert.AreEqual(0, lineChecker.CodeLenght, "there shouldn't be lines, but there are {0}", lineChecker.CodeLenght);
        }

        [Test]
        public void Rule1Bug200804007() {
            string oneLine = "\n";
            foreach (char c in oneLine.ToCharArray()) {
                lineChecker.Check(c, context);
            }
            Assert.AreEqual(1, lineChecker.CodeLenght, "there should be 1 line, but there are {0}", lineChecker.CodeLenght);
        }

        [Test]
        public void Counting2LinesDetailed() {
            lineChecker.Check('/', context);
            lineChecker.Check('*', context);
            lineChecker.Check('\n', context);
            Assert.AreEqual(true, context.IAmInMultilineComment);
            Assert.AreEqual(false, lineChecker.HasCode);
            lineChecker.Check('*', context);
            lineChecker.Check('/', context);
            lineChecker.Check('\n', context);
            Assert.AreEqual(false, context.IAmInMultilineComment);
            Assert.AreEqual(false, lineChecker.HasCode);
        }
    }

    [TestFixture]
    public class TestCheckerHeightRule : TestParent {

        [Test]
        public void Rule1OK() {
            CallCheck(GetFileName("Rule1OK.cs"));
            IsHot();
        }

        [Test]
        public void Rule1KO() {
            CallCheck(GetFileName("Rule1KO.cs"));
            IsNot();
        }

        [Test]
        public void Rule1OKComments() {
            CallCheck(GetFileName("Rule1OKComments.cs"));
            IsHot();
        }

        [Test]
        public void Rule1OKMultilineComments() {
            CallCheck(GetFileName("Rule1OKMultilineComments.cs"));
            IsHot();
        }

        [Test]
        public void Rule1Bug200804003() {
            // end of multiline comments should be not counted
            CallCheck(GetFileName("Rule1Bug200804003.cs"));
            IsHot();
        }

        [Test]
        public void Rule1Bug200804005() {
            // whitespace ONLY lines should count against total size
            CallCheck(GetFileName("Rule1Bug200804005.cs"));
            IsNot();
            Assert.IsTrue(results.Violations[0].ToString().Contains("705"));
        }

        [Test]
        public void Rule1Bug200804009() {
            // lines ending in a comment should count against total size
            CallCheck(GetFileName("Rule1Bug200804009.cs"));
            IsNot();
            Assert.IsTrue(results.Violations[0].ToString().Contains("703"));
        }

        [Test]
        public void Rule1Bug200804010() {
            // 200804009: regression whith single-line comments : //
            CallCheck(GetFileName("Rule1Bug200804010.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class TestWidthRule {

        private const string TEST_FILENAME = "blah";

        private const char TEST_CHARACTER = 'x';

        private const int TEST_LIMIT = 128;

        private WidthRule ruleChecker;

        private Results results;

        private Context context;
        
        [SetUp]
        protected void SetUp() {
            ruleChecker = new WidthRule();
            results = new Results(TEST_FILENAME);
            context = new Context();
        }

        [TearDown]
        protected void TearDown() {
            ruleChecker = null;
            results = null;
            context = null;
        }

        private void PunchLine() {
            for (int index = 0; index < 128; index++) {
                ruleChecker.Check(TEST_CHARACTER, context);
            }
        }

        private void PunchCharacter() {
            ruleChecker.Check(TEST_CHARACTER, context);
        }

        private void PunchCharacter(char c) {
            ruleChecker.Check(c, context);
        }

        private void PunchReturn() {
            ruleChecker.Check('\r', context);
            ruleChecker.Check('\n', context);
        }

        private void CollectResults() {
            ruleChecker.Close(context);
            context.ReportViolations(results);
        }

        private const string noBut = "";

        protected void IsHot() {
            string whyNot = noBut;
            if (!results.Happy) {
                whyNot = string.Format("File {0} should be HOT but is not because of {1}", TEST_FILENAME, results.Violations[0]);
            }

            Assert.IsTrue(results.Happy, whyNot);
        }

        private void IsNot() {
            Assert.IsFalse(results.Happy);
        }

        [Test]
        public void OneCharacter() {
            PunchCharacter();
            CollectResults();
            IsHot();
        }

        [Test]
        public void OneLineOk() {
            PunchLine();
            CollectResults();
            IsHot();
        }

        [Test]
        public void OneLineKo() {
            PunchKoLine();
            CollectResults();
            IsNot();
        }


        [Test]
        public void OneLine130Ko() {
            PunchLine();
            PunchCharacter();
            PunchCharacter();
            CollectResults();
            IsNot();
            //TODO: update when it gets new parameters
            Assert.IsTrue(results.Violations[0].ToString().Contains("130"), results.Violations[0].ToString());
        }

        [Test]
        public void ReentranceKo() {
            PunchCharacter('{');
            PunchReturn();
            PunchLine();
            CollectResults();
            IsNot();
            //TODO: update when it gets new parameters
            Assert.IsTrue(results.Violations[0].ToString().Contains("132"));
        }

        private void PunchKoLine() {
            PunchLine();
            PunchCharacter();
        }

        [Test]
        public void TwoLinesOk() {
            PunchLine();
            PunchReturn();
            PunchLine();
            CollectResults();
            IsHot();
        }

        [Test]
        public void TwoLinesOkKo() {
            PunchKoLine();
            PunchReturn();
            PunchLine();
            CollectResults();
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
        }

        [Test]
        public void TwoLinesKo() {
            PunchKoLine();
            PunchReturn();
            PunchKoLine();
            CollectResults();
            IsNot();
            Assert.AreEqual(2, results.Violations.Count);
        }

    }

    [TestFixture]
    public class TestCheckerWidthtRule : TestParent {

        [Test]
        public void Rule2OK() {
            CallCheck(GetFileName("Rule2OK.cs"));
            IsHot();
        }

        [Test]
        public void Rule2KO() {
            CallCheck(GetFileName("Rule2KO.cs"));
            IsNot();
        }

        [Test]
        //IDEA, property bug
        public void Rule2Bug200804001() {
            // a single-line comment should stop parsing for extra characters
            CallCheck(GetFileName("Rule2Bug200804001.cs"));
            IsHot();
        }

        [Test]
        public void Rule2Bug200804002() {
            // else and catch statements should count against re-entrances in code
            CallCheck(GetFileName("Rule2Bug200804002.cs"));
            // it shouldn't be hot only for a question of method length, if it gets more than one
            // violation then there is an error outside the test
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.MethodTooLong, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule2Bug200804008() {
            // string { } and character { } should not count against re-entrances in code
            CallCheck(GetFileName("Rule2Bug200804008.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class TestCheckerUniqueLine : TestParent {

        [Test]
        public void Rule3case01AOKOneStatementInARow() {
            CallCheck(GetFileName("Rule3Case01AOKOneStatementInARow.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case01BKOTwoStatementsInARow() {
            CallCheck(GetFileName("Rule3Case01BKOTwoStatementsInARow.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.OneStatementPerLine, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3case01COKForLoops() {
            CallCheck(GetFileName("Rule3Case01COKForLoops.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case01DOKForCharacterDefinitions() {
            CallCheck(GetFileName("Rule3Case01DOKForCharacterDefinitions.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case01EKOInStrings() {
            CallCheck(GetFileName("Rule3Case01EKOInStrings.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.OneStatementPerLine, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case01FKOInitializer() {
            CallCheck(GetFileName("Rule3Case01FKOInitializer.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.OneStatementPerLine, results.Violations[0].KindOfViolation);
        }
        
        [Test]
        public void Rule3Case01Bug200804011() {
            // TODO the self referential bug, straroftl. Regression agains 20080409+20080410
            CallCheck(GetFileName("Rule3Case01Bug200804011.cs"));
            IsHot();
        }

        [Test]
        public void Rule3Case01Bug200805001() {
            CallCheck(GetFileName("Rule3Case01Bug200805001.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case02AKOOneStatementInTwoRows() {
            CallCheck(GetFileName("Rule3Case02AKOOneStatementInTwoRows.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.OneLinePerStatement, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case02Bug200804006() {
            CallCheck(GetFileName("Rule3Case02Bug200804006.cs"));
            IsHot();
        }
        
        [Test]
        public void Rule3Case02BKOOneMethodDeclarationInTwoRows() {
            CallCheck(GetFileName("Rule3Case02BKOOneMethodDeclarationInTwoRows.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.OneLinePerStatement, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case02COKItIsCallingTheThisOrBaseConstructor() {
            CallCheck(GetFileName("Rule3Case02COKItIsCallingTheThisOrBaseConstructor.cs"));
            IsHot();
        }

        [Test]
        public void Rule3Case02DKOIHateMicrosoftFormatting() {
            CallCheck(GetFileName("Rule3Case02DKOIHateMicrosoftFormatting.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.OneLinePerStatement, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case02EOKGenericDefinition() {
            CallCheck(GetFileName("Rule3Case02EOKGenericDefinition.cs"));
            IsHot();
        }

        [Test]
        public void Rule3Case02FOKArrayDefinitions() {
            CallCheck(GetFileName("Rule3Case02FOKArrayDefinitions.cs"));
            IsHot();
        }

        public void Rule3Case02GOKRegionDefinitions() {
            CallCheck(GetFileName("Rule3Case02GOKRegionDefinitions.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class TestCheckerMethodLenght : TestParent {

        [Test]
        public void Rule4Case01OKBase() {
            CallCheck(GetFileName("Rule4Case01OKBase.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case02KOBase() {
            CallCheck(GetFileName("Rule4Case02KOBase.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.MethodTooLong, results.Violations[0].KindOfViolation);
            Assert.IsTrue(results.Violations[0].ToString().Contains("base()"));
        }

        [Test]
        public void Rule4Case03OKSimple() {
            CallCheck(GetFileName("Rule4Case03OKSimple.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case04KOSimple() {
            CallCheck(GetFileName("Rule4Case04KOSimple.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(ViolationType.MethodTooLong, results.Violations[0].KindOfViolation);
            Assert.IsTrue(results.Violations[0].ToString().Contains("CheckFile("));
        }

        [Test]
        public void Rule4Case05OKLimitSimple() {
            CallCheck(GetFileName("Rule4Case05OKLimitSimple.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case06OKLimitBase() {
            CallCheck(GetFileName("Rule4Case06OKLimitBase.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case07OKLimitSimpleWithComments() {
            CallCheck(GetFileName("Rule4Case07OKLimitSimpleWithComments.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class TestCheckerVariableLenghtRule : TestParent {

        [Test]
        public void Rule5KO() {
            CallCheck(GetFileName("Rule5KO.cs"));
            IsNot();
            Assert.AreEqual(5, results.Violations.Count);
        }


        [Test]
        public void Rule5OK() {
            CallCheck(GetFileName("Rule5OK.cs"));
            IsHot();
        }


        [Test]
        public void Rule5OKStopWords() {
            CallCheck(GetFileName("Rule5OKStopWords.cs"));
            IsHot();
        }

        [Test]
        public void Rule5Bug200804012() {
            CallCheck(GetFileName("Rule5Bug200804012.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
        }

        [Test]
        public void Rule5Bug200805002() {
            CallCheck(GetFileName("Rule5Bug200805002.cs"));
            IsHot();
        }

    }

}
