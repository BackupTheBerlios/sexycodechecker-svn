﻿using NUnit.Framework;
using Cluefultoys.Nunit;

namespace Cluefultoys.Sexycodechecker {

    public abstract class CheckerParent : TestBase {

        private const string myFilesPath = "Tests/Files/Checker/";

        protected override string TestFilesPath {
            get {
                return myFilesPath;
            }
        }

        protected Checker checker;

        protected Results results;

        protected string filename;

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
    public class CheckerTest : CheckerParent {

        [Test]
        public void FileNotFound() {
            CallCheck("C:/BLAH");
            IsNot();
            Assert.IsInstanceOfType(Violation.ViolationType.FileNotFound.GetType(), results.Violations[0].KindOfViolation, "File {1} should report violation {2}", filename, Violation.ViolationType.FileNotFound);
        }

        [Test]
        public void ValidSource() {
            CallCheck(GetFilename("ValidSource.cs"));
            IsHot();
        }

    }

    internal class MockLineChecker : HeightRule {

        public MockLineChecker(string filename)
            : base(filename) {
        }

        public bool IsInMultilineComment {
            get {
                return iAmInMultilineComment;
            }
        }

        public bool IsInComment {
            get {
                return iAmInComment;
            }
        }

    }

    [TestFixture]
    public class HeightRuleTest {

        private MockLineChecker lineChecker;

        [SetUp]
        protected void SetUp() {
            lineChecker = new MockLineChecker("test");
        }

        [TearDown]
        protected void TearDown() {
            lineChecker = null;
        }

        [Test]
        public void Counting2Lines() {
            string twoLines = "/*\n*/";
            foreach (char c in twoLines.ToCharArray()) {
                lineChecker.Check(c);
            }
            Assert.AreEqual(0, lineChecker.CodeLenght, "there shouldn't be lines, but there are {0}", lineChecker.CodeLenght);
        }

        [Test]
        public void Rule1Bug200804004() {
            string oneLine = "/*\n";
            foreach (char c in oneLine.ToCharArray()) {
                lineChecker.Check(c);
            }
            Assert.AreEqual(0, lineChecker.CodeLenght, "there shouldn't be lines, but there are {0}", lineChecker.CodeLenght);
        }

        [Test]
        public void Rule1Bug200804007() {
            string oneLine = "\n";
            foreach (char c in oneLine.ToCharArray()) {
                lineChecker.Check(c);
            }
            Assert.AreEqual(1, lineChecker.CodeLenght, "there should be 1 line, but there are {0}", lineChecker.CodeLenght);
        }

        [Test]
        public void Counting2LinesDetailed() {
            lineChecker.Check('/');
            lineChecker.Check('*');
            lineChecker.Check('\n');
            Assert.AreEqual(true, lineChecker.IsInMultilineComment);
            Assert.AreEqual(false, lineChecker.HasCode);
            lineChecker.Check('*');
            lineChecker.Check('/');
            lineChecker.Check('\n');
            Assert.AreEqual(false, lineChecker.IsInMultilineComment);
            Assert.AreEqual(false, lineChecker.HasCode);
        }
    }

    [TestFixture]
    public class HeightRuleCheckerTest : CheckerParent {

        [Test]
        public void Rule1OK() {
            CallCheck(GetFilename("Rule1OK.cs"));
            IsHot();
        }

        [Test]
        public void Rule1KO() {
            CallCheck(GetFilename("Rule1KO.cs"));
            IsNot();
        }

        [Test]
        public void Rule1OKComments() {
            CallCheck(GetFilename("Rule1OKComments.cs"));
            IsHot();
        }

        [Test]
        public void Rule1OKMultilineComments() {
            CallCheck(GetFilename("Rule1OKMultilineComments.cs"));
            IsHot();
        }

        [Test]
        public void Rule1Bug200804003() {
            // end of multiline comments should be not counted
            CallCheck(GetFilename("Rule1Bug200804003.cs"));
            IsHot();
        }

        [Test]
        public void Rule1Bug200804005() {
            // whitespace ONLY lines should count against total size
            CallCheck(GetFilename("Rule1Bug200804005.cs"));
            IsNot();
            Assert.IsTrue(results.Violations[0].ToString().Contains("705"));
        }

        [Test]
        public void Rule1Bug200804009() {
            // lines ending in a comment should count against total size
            CallCheck(GetFilename("Rule1Bug200804009.cs"));
            IsNot();
            Assert.IsTrue(results.Violations[0].ToString().Contains("703"));
        }

        [Test]
        public void Rule1Bug200804010() {
            // 200804009: regression whith single-line comments : //
            CallCheck(GetFilename("Rule1Bug200804010.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class WidthRuleTest {

        private const string TEST_FILENAME = "blah";

        private const char TEST_CHARACTER = 'x';

        private const int TEST_LIMIT = 128;

        private WidthRule ruleChecker;

        private Results results;

        [SetUp]
        protected void SetUp() {
            ruleChecker = new WidthRule();
            results = new Results(TEST_FILENAME);
        }

        [TearDown]
        protected void TearDown() {
            ruleChecker = null;
            results = null;
        }

        private void PunchLine() {
            for (int index = 0; index < 128; index++) {
                ruleChecker.Check(TEST_CHARACTER);
            }
        }

        private void PunchCharacter() {
            ruleChecker.Check(TEST_CHARACTER);
        }

        private void PunchCharacter(char c) {
            ruleChecker.Check(c);
        }

        private void PunchReturn() {
            ruleChecker.Check('\r');
            ruleChecker.Check('\n');
        }

        private void CollectResults() {
            ruleChecker.ReportViolations(results);
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
    public class WidthtRuleCheckerTest : CheckerParent {

        [Test]
        public void Rule2OK() {
            CallCheck(GetFilename("Rule2OK.cs"));
            IsHot();
        }

        [Test]
        public void Rule2KO() {
            CallCheck(GetFilename("Rule2KO.cs"));
            IsNot();
        }

        [Test]
        //IDEA, property bug
        public void Rule2Bug200804001() {
            // a single-line comment should stop parsing for extra characters
            CallCheck(GetFilename("Rule2Bug200804001.cs"));
            IsHot();
        }

        [Test]
        public void Rule2Bug200804002() {
            // else and catch statements should count against re-entrances in code
            CallCheck(GetFilename("Rule2Bug200804002.cs"));
            // it shouldn't be hot only for a question of method length, if it gets more than one
            // violation then there is an error outside the test
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.MethodTooLong, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule2Bug200804008() {
            // string { } and character { } should not count against re-entrances in code
            CallCheck(GetFilename("Rule2Bug200804008.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class UniqueLineCheckerTest : CheckerParent {

        [Test]
        public void Rule3case01AOKOneStatementInARow() {
            CallCheck(GetFilename("Rule3Case01AOKOneStatementInARow.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case01BKOTwoStatementsInARow() {
            CallCheck(GetFilename("Rule3Case01BKOTwoStatementsInARow.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.OneStatementPerLine, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3case01COKForLoops() {
            CallCheck(GetFilename("Rule3Case01COKForLoops.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case01DOKForCharacterDefinitions() {
            CallCheck(GetFilename("Rule3Case01DOKForCharacterDefinitions.cs"));
            IsHot();
        }

        [Test]
        public void Rule3case01EKOInStrings() {
            CallCheck(GetFilename("Rule3Case01EKOInStrings.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.OneStatementPerLine, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case01FKOInitializer() {
            CallCheck(GetFilename("Rule3Case01FKOInitializer.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.OneStatementPerLine, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case01Bug200804011() {
            // TODO the self referential bug, straroftl. Regression agains 20080409+20080410
            CallCheck(GetFilename("Rule3Case01Bug200804011.cs"));
            IsHot();
        }


        [Test]
        public void Rule3case02AKOOneStatementInTwoRows() {
            CallCheck(GetFilename("Rule3Case02AKOOneStatementInTwoRows.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.OneLinePerStatement, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case02Bug200804006() {
            CallCheck(GetFilename("Rule3Case02Bug200804006.cs"));
            IsHot();
        }

        [Test]
        public void Rule3Case02BKOOneMethodDeclarationInTwoRows() {
            CallCheck(GetFilename("Rule3Case02BKOOneMethodDeclarationInTwoRows.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.OneLinePerStatement, results.Violations[0].KindOfViolation);
        }

        [Test]
        public void Rule3Case02COKItIsCallingTheThisOrBaseConstructor() {
            CallCheck(GetFilename("Rule3Case02COKItIsCallingTheThisOrBaseConstructor.cs"));
            IsHot();
        }

        [Test]
        public void Rule3Case02DKOIHateMicrosoftFormatting() {
            CallCheck(GetFilename("Rule3Case02DKOIHateMicrosoftFormatting.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.OneLinePerStatement, results.Violations[0].KindOfViolation);
        }

    }

    [TestFixture]
    public class MethodLenghtCheckerTest : CheckerParent {

        [Test]
        public void Rule4Case01OKBase() {
            CallCheck(GetFilename("Rule4Case01OKBase.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case02KOBase() {
            CallCheck(GetFilename("Rule4Case02KOBase.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.MethodTooLong, results.Violations[0].KindOfViolation);
            Assert.IsTrue(results.Violations[0].ToString().Contains("base()"));
        }

        [Test]
        public void Rule4Case03OKSimple() {
            CallCheck(GetFilename("Rule4Case03OKSimple.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case04KOSimple() {
            CallCheck(GetFilename("Rule4Case04KOSimple.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
            Assert.AreEqual(Violation.ViolationType.MethodTooLong, results.Violations[0].KindOfViolation);
            Assert.IsTrue(results.Violations[0].ToString().Contains("CheckFile("));
        }

        [Test]
        public void Rule4Case05OKLimitSimple() {
            CallCheck(GetFilename("Rule4Case05OKLimitSimple.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case06OKLimitBase() {
            CallCheck(GetFilename("Rule4Case06OKLimitBase.cs"));
            IsHot();
        }

        [Test]
        public void Rule4Case07OKLimitSimpleWithComments() {
            CallCheck(GetFilename("Rule4Case07OKLimitSimpleWithComments.cs"));
            IsHot();
        }

    }

    [TestFixture]
    public class VariableCheckerTest : CheckerParent {

        [Test]
        public void Rule5KO() {
            CallCheck(GetFilename("Rule5KO.cs"));
            IsNot();
            Assert.AreEqual(5, results.Violations.Count);
        }


        [Test]
        public void Rule5OK() {
            CallCheck(GetFilename("Rule5OK.cs"));
            IsHot();
        }


        [Test]
        public void Rule5OKStopWords() {
            CallCheck(GetFilename("Rule5OKStopWords.cs"));
            IsHot();
        }

        [Test]
        public void Rule5Bug200804012() {
            CallCheck(GetFilename("Rule5Bug200804012.cs"));
            IsNot();
            Assert.AreEqual(1, results.Violations.Count);
        }

    }

}