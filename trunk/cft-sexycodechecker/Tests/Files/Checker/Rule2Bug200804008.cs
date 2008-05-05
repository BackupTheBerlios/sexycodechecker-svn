using System;
using System.Collections.Generic;
using System.IO;

namespace Cluefultoys.Sexycodechecker {

    // TODO: name
    /// <summary>
    /// THE 700 by 128 MANIFESTO
    /// Rules:
    /// 1) No compilation unit can be longer more than 700 lines.
    /// - lines consisting only of comments do not count in this limit.
    /// 2) No line of code can be wider more than 128 characters.
    /// - Comments will not be part of the characters count.
    /// - Re-entrances will be counted starting at 4 characters.
    /// 3) There will be only one statement for each line of code.
    /// - one statement will stay only on one line of code.
    /// 4) Every method will be of 20 lines max.
    /// 5) Variables, including iteration variables, will be named with at least three characters, including iteration variables.
    /// </summary>
    public class Checker {

        // TODO: character recognition needs to change with unicode support.
        public Results CheckFile(string filename) {
            List<Rule> rules = GetRules(filename);

            Results results = new Results(filename);
            try {
                using (FileStream file = File.Open(filename, FileMode.Open)) {
                    while (file.Position < file.Length) {
                        char currentCharacter = (char)file.ReadByte();
                        foreach (Rule rule in rules) {
                            rule.Check(currentCharacter);
                        }
                    }
                    foreach (Rule rule in rules) {
                        rule.ReportViolations(results);
                    }
                }
            } catch (FileNotFoundException fne) {
                results.Add(FileNotFoundViolation(fne, filename));
            }
            return results;
        }

        private Violation FileNotFoundViolation(FileNotFoundException fne, string fileName) {
            return new Violation(Violation.ViolationType.FileNotFound, fne.Message, Constants.NO_LINE, fileName);
        }

        private List<Rule> GetRules(string filename) {
            List<Rule> rules = new List<Rule>();
            rules.Add(new HeightRule(filename));
            rules.Add(new WidthRule());
            rules.Add(new OneStatementPerLineRule());
            rules.Add(new OneLinePerStatementRule());
            rules.Add(new MethodHeightRule());
            return rules;
        }
    }

    public class CharacterHandler {

        public delegate void Executor();

        public delegate bool CheckCharacter(char character);

        private char toHandle;

        private CheckCharacter Condition;

        private Executor Execute;

        private bool stop;

        public CharacterHandler(Executor DefaultExecute, bool stop) {
            this.Condition = IsDefault;
            this.Execute = DefaultExecute;
            this.stop = stop;
        }

        public CharacterHandler(CheckCharacter Condition, Executor Execute, bool stop) {
            this.Condition = Condition;
            this.Execute = Execute;
            this.stop = stop;
        }

        public CharacterHandler(CheckCharacter Condition) {
            this.Condition = Condition;
            this.Execute = DefaultExecute;
            this.stop = true;
        }

        public CharacterHandler(char toHandle, Executor Execute, bool stop) {
            this.toHandle = toHandle;
            this.Condition = SimpleCheck;
            this.Execute = Execute;
            this.stop = stop;

        }

        private bool SimpleCheck(char character) {
            return toHandle == character;
        }

        private bool IsDefault(char character) {
            return true;
        }

        public bool Handle(char character) {
            if (Condition(character)) {
                Execute();
                return stop;
            }
            return false;
        }

        private void DefaultExecute() {
            // No-operation executor, for stop-only conditions.
        }
    }

    public class CharacterChain {

        private List<CharacterHandler> Handlers = new List<CharacterHandler>();

        public void Add(CharacterHandler handler) {
            Handlers.Add(handler);
        }

        public void Execute(char character) {
            bool stop = false;
            IEnumerator<CharacterHandler> enumerator = Handlers.GetEnumerator();
            while (!stop && enumerator.MoveNext()) {
                stop = enumerator.Current.Handle(character);
            }
        }

    }

    public abstract class Rule {

        public Rule() {
            chain = new CharacterChain();
            chain.Add(new CharacterHandler(Constants.ASCII_LF, HandleNewline, true));
            chain.Add(new CharacterHandler('\'', HandleCharDefinition, false));
            chain.Add(new CharacterHandler('"', HandleStringDefinition, false));
            chain.Add(new CharacterHandler('/', HandleSlash, true));
            chain.Add(new CharacterHandler('*', HandleStar, true));
            chain.Add(new CharacterHandler(HandleDefault, true));
        }

        private CharacterChain chain;

        protected bool totallyEmptyLine = true;

        protected char currentCharacter;

        protected char myPreviousCharacter = Constants.ASCII_CR;

        private bool isEscaped;

        private int myFileLenght = 1;

        protected int FileLenght {
            get {
                return myFileLenght;
            }
        }

        protected bool iAmInComment = false;

        protected bool iAmInMultilineComment = false;

        protected List<Violation> violations = new List<Violation>();

        protected string currentLine = "";

        protected bool inCharDefinition = false;

        protected bool inStringDefinition = false;

        protected abstract void Record();

        protected abstract void DoNewLine();

        protected abstract bool ValidCharacter();

        public virtual void ReportViolations(Results toResults) {
            DoNewLine();
            foreach (Violation violation in violations) {
                toResults.Add(violation);
            }
        }

        private void HandleNewline() {
            DoNewLine();
            iAmInComment = iAmInMultilineComment;
            totallyEmptyLine = true;
            currentLine = "";
            myFileLenght++;
            inCharDefinition = false;
            inStringDefinition = false;
        }

        private void HandleSlash() {
            if ('/' == myPreviousCharacter && !(inCharDefinition || inStringDefinition)) {
                iAmInComment = true;
            } else if ('*' == myPreviousCharacter && !(inCharDefinition || inStringDefinition)) {
                iAmInMultilineComment = false;
                iAmInComment = false;
            } else if (!(Constants.ASCII_CR == myPreviousCharacter || Constants.ASCII_LF == myPreviousCharacter)) {
                HandleDefault();
            }
        }

        private void HandleStar() {
            if ('/' == myPreviousCharacter && !(inCharDefinition || inStringDefinition)) {
                iAmInMultilineComment = true;
            } else {
                HandleDefault();
            }
        }

        private void HandleDefault() {
            if (!iAmInMultilineComment && !iAmInComment && ValidCharacter()) {
                Record();
            }
        }

        public void Check(char currentCharacter) {
            this.currentCharacter = currentCharacter;
            this.currentLine += currentCharacter;
            if (!char.IsWhiteSpace(currentCharacter)) {
                totallyEmptyLine = false;
            }
            chain.Execute(currentCharacter);
            myPreviousCharacter = currentCharacter;
            isEscaped = !isEscaped && (myPreviousCharacter == '\\');
        }

        private void HandleCharDefinition() {
            if (!isEscaped && !inStringDefinition) {
                inCharDefinition = !inCharDefinition;
            }
        }

        private void HandleStringDefinition() {
            if (!inCharDefinition) {
                inStringDefinition = !inStringDefinition;
            }
        }
    }

    public class HeightRule : Rule {

        private string fileName;

        private int myCodeLenght = 0;

        public int CodeLenght {
            get {
                return myCodeLenght;
            }
        }

        private bool iHaveCode = false;
        // TODO Remove
        public bool HasCode {
            get {
                return iHaveCode;
            }
        }

        public HeightRule(string fileName) {
            this.fileName = fileName;
        }

        protected override void Record() {
            iHaveCode = true;
        }

        protected override bool ValidCharacter() {
            return !char.IsWhiteSpace(currentCharacter);
        }

        protected override void DoNewLine() {
            bool countIt = totallyEmptyLine || (iHaveCode && !iAmInComment);
            if (countIt) {
                myCodeLenght++;
            }
            iHaveCode = false;
        }

        public override void ReportViolations(Results toResults) {
            if (Constants.ALLOWABLE_LINES_PER_FILE < myCodeLenght) {
                toResults.Add(CompilationUnitTooLong());
            }
        }

        private Violation CompilationUnitTooLong() {
            string message = "The compilation unit is too long: there are {0} lines of code in this file";
            message = string.Format(message, myCodeLenght);
            return new Violation(Violation.ViolationType.FileTooLong, message, FileLenght, fileName);
        }

    }

    public class MethodHeightRule : Rule3 {

        private string originalMethodLine = "";

        private int myMethodLength = 0;

        private int myReentrance = 0;

        private int myMethodReentranceStart = 0;

        private bool methodEnter = false;

        private bool methodExit = false;

        private bool inMethod = false;

        private CharacterChain newChain = new CharacterChain();

        public MethodHeightRule() {
            newChain.Add(new CharacterHandler(StopIfInCharacterHandling));
            newChain.Add(new CharacterHandler(RecordLastCharacter, false));
            newChain.Add(new CharacterHandler('(', HandleMethodEnter, false));
            newChain.Add(new CharacterHandler('(', HandleParensLevelUp, true));
            newChain.Add(new CharacterHandler(')', HandleMethodExit, false));
            newChain.Add(new CharacterHandler(')', HandleParensLevelDown, true));
            newChain.Add(new CharacterHandler('{', HandleReentranceUp, true));
            newChain.Add(new CharacterHandler('}', HandleReentranceDown, true));
        }

        private void HandleMethodEnter() {
            if (!inMethod) {
                methodEnter = true;
            }
        }

        private void HandleMethodExit() {
            if (!inMethod) {
                methodExit = true;
            }
        }

        private void HandleReentranceUp() {
            Console.Out.WriteLine("HandleReentranceUp()");
            myReentrance++;
        }

        private void HandleReentranceDown() {
            Console.Out.WriteLine("HandleReentranceDown()");
            myReentrance--;
            Console.Out.WriteLine("inMethod {0}", inMethod);
            Console.Out.WriteLine("myReentrance {0}", myReentrance);
            Console.Out.WriteLine("myMethodReentranceStart {0}", myMethodReentranceStart);
            if (inMethod && myReentrance < myMethodReentranceStart) {
                Console.Out.WriteLine("myMethodLength {0}", myMethodLength);
                if (myMethodLength >= 20) {
                    MethodTooLong();
                }
                inMethod = false;
                myMethodLength = 0;
                myMethodReentranceStart = 0;
            }
        }

        protected override void Clear() {
            if (inMethod) {
                bool countIt = (totallyEmptyLine || (iHaveCode && !iAmInComment));
                if (countIt) {
                    myMethodLength++;
                }
            } else {
                if (LastCharacter == '{' && methodEnter && methodExit && parensLevel == 0) {
                    inMethod = true;
                    myMethodReentranceStart = myReentrance;
                    originalMethodLine = currentLine;
                }
            }
            methodEnter = false;
            methodExit = false;
            iHaveCode = false;
        }

        private bool iHaveCode = false;
        // TODO Remove
        public bool HasCode {
            get {
                return iHaveCode;
            }
        }

        private void MethodTooLong() {
            string message = "Method is {0} lines long";
            message = string.Format(message, myMethodLength);
            this.violations.Add(new Violation(Violation.ViolationType.MethodTooLong, message, FileLenght, originalMethodLine));
        }

        protected override void Record() {
            iHaveCode = true;
            newChain.Execute(currentCharacter);
        }

    }

    public class WidthRule : Rule {

        private CharacterChain chain;

        private int reentranceLevel = 0;

        private int charactersInLine = 0;

        private bool lineStart = true;

        public WidthRule() {
            chain = new CharacterChain();
            chain.Add(new CharacterHandler('\t', HandleTab, true));
            chain.Add(new CharacterHandler(Char.IsWhiteSpace, HandleWhitespace, true));
            chain.Add(new CharacterHandler(HandleDefault, true));
        }

        private void HandleTab() {
            charactersInLine += 4;
        }

        private void HandleWhitespace() {
            charactersInLine += 1;
        }

        private void HandleDefault() {
            if (lineStart) {
                charactersInLine = (reentranceLevel > charactersInLine ? reentranceLevel : charactersInLine);
                lineStart = false;
            }
            if ('{' == currentCharacter) {
                reentranceLevel += 4;
            } else if ('}' == currentCharacter) {
                reentranceLevel -= 4;
            }
            charactersInLine++;
        }

        protected override bool ValidCharacter() {
            return currentCharacter != Constants.ASCII_CR && currentCharacter != Constants.ASCII_EOF;
        }

        protected override void Record() {
            chain.Execute(currentCharacter);
        }

        protected override void DoNewLine() {
            if (Constants.ALLOWABLE_CHARACTERS_PER_LINE < charactersInLine) {
                violations.Add(LineTooWide());
            }
            charactersInLine = 0;
            lineStart = true;
        }

        private Violation LineTooWide() {
            string viewLine = currentLine.Replace(' ', '.');
            string message = "This line is too wide: {0} instead of {1}";
            message = string.Format(message, charactersInLine, Constants.ALLOWABLE_CHARACTERS_PER_LINE);
            return new Violation(Violation.ViolationType.LineTooWide, message, FileLenght, currentLine);
        }

    }

    public abstract class Rule3 : Rule {

        protected CharacterChain chain = new CharacterChain();

        private char characterBefore = Constants.ASCII_CR;

        private char myLastCharacter = Constants.ASCII_CR;

        protected int parensLevel = 0;

        protected abstract void Clear();

        protected void HandleParensLevelDown() {
            parensLevel--;
        }

        protected void HandleParensLevelUp() {
            parensLevel++;
        }

        protected bool StopIfInCharacterHandling(char shouldIt) {
            return inCharDefinition || inStringDefinition;
        }

        protected override void DoNewLine() {
            Clear();
            myLastCharacter = Constants.ASCII_CR;
            characterBefore = Constants.ASCII_CR;
            parensLevel = 0;
        }

        protected void RecordLastCharacter() {
            characterBefore = myLastCharacter;
            myLastCharacter = currentCharacter;
        }

        protected char LastCharacter {
            get {
                return ((iAmInComment) ? characterBefore : myLastCharacter);
            }
        }

        protected override bool ValidCharacter() {
            return !char.IsWhiteSpace(currentCharacter);
        }

    }

    public class OneLinePerStatementRule : Rule3 {

        private char firstCharacterInLine = Constants.ASCII_CR;

        private List<char> tailChars = new List<char>();

        private Violation delayViolation;

        public OneLinePerStatementRule() {
            tailChars.Add(';');
            tailChars.Add(',');
            tailChars.Add('{');
            tailChars.Add('}');
            tailChars.Add(Constants.ASCII_CR);

            chain.Add(new CharacterHandler(StopIfInCharacterHandling));
            chain.Add(new CharacterHandler('(', HandleParensLevelUp, false));
            chain.Add(new CharacterHandler(')', HandleParensLevelDown, false));
            chain.Add(new CharacterHandler(RecordFirstCharacter, false));
            chain.Add(new CharacterHandler(RecordLastCharacter, true));
        }


        private void RecordFirstCharacter() {
            if (firstCharacterInLine == Constants.ASCII_CR) {
                firstCharacterInLine = currentCharacter;
            }
        }

        protected override void Record() {
            chain.Execute(currentCharacter);
        }

        protected override void Clear() {
            if (delayViolation != null) {
                if (!IsInitializingTheBaseClass()) {
                    violations.Add(delayViolation);
                }
                delayViolation = null;
            }
            if (parensLevel > 0 || !tailChars.Contains(LastCharacter)) {
                delayViolation = OneLinePerStatement();
                if (')' != LastCharacter) {
                    violations.Add(delayViolation);
                    delayViolation = null;
                }
            }
            firstCharacterInLine = Constants.ASCII_CR;
        }

        private Violation OneLinePerStatement() {
            string message = "This statement does not end on the current line";
            return new Violation(Violation.ViolationType.OneLinePerStatement, message, FileLenght, currentLine);
        }

        private bool IsInitializingTheBaseClass() {
            bool callingBaseInitializer = (currentLine.Contains("base") || currentLine.Contains("this"));
            return (firstCharacterInLine == ':' && callingBaseInitializer && currentLine.Contains("("));
        }

    }

    public class OneStatementPerLineRule : Rule3 {

        private int semiColumns;

        private int commas;

        public OneStatementPerLineRule() {
            chain.Add(new CharacterHandler(StopIfInCharacterHandling));
            chain.Add(new CharacterHandler(RecordLastCharacter, false));
            chain.Add(new CharacterHandler(',', HandleCommas, true));
            chain.Add(new CharacterHandler(';', HandleSemiColumns, true));
            chain.Add(new CharacterHandler('(', HandleParensLevelUp, true));
            chain.Add(new CharacterHandler(')', HandleParensLevelDown, true));
        }

        protected override void Record() {
            chain.Execute(currentCharacter);
        }

        private void HandleCommas() {
            if (parensLevel < 1) {
                commas++;
            }
        }

        private void HandleSemiColumns() {
            if (parensLevel < 1) {
                semiColumns++;
            }
        }

        protected override void Clear() {
            int resultCommas = commas - (LastCharacter == ',' ? 1 : 0);
            if (resultCommas > 0 || semiColumns > 1) {
                violations.Add(OneStatementPerLine());
            }
            semiColumns = 0;
            commas = 0;
        }

        private Violation OneStatementPerLine() {
            string message = "This line has more than one statement";
            return new Violation(Violation.ViolationType.OneStatementPerLine, message, FileLenght, currentLine);
        }

    }

    public class Results {

        public Results(string fileName) {
            myFileName = fileName;
        }

        private string myFileName;
        public String Filename {
            get {
                return myFileName;
            }
        }

        private List<Violation> myViolations = new List<Violation>();
        public List<Violation> Violations {
            get {
                return myViolations;
            }
        }

        public void Add(Violation violation) {
            isHappy = false;
            myViolations.Add(violation);
        }

        private bool isHappy = true;
        public bool Happy {
            get {
                return isHappy;
            }
        }

    }

    public class Violation {

        public enum ViolationType {
            FileNotFound,
            FileTooLong,
            LineTooWide,
            OneStatementPerLine,    // rule 3 first part
            OneLinePerStatement,    // rule 3 second part
            MethodTooLong,          // rule 4
        }

        private ViolationType myType;
        public ViolationType KindOfViolation {
            get {
                return myType;
            }
        }

        private string myMessage;

        private string myLine;

        private int myLineNumber;

        // TODO: add a map<string,object> with reason and parameters
        public Violation(ViolationType type, string message, int lineNumber, string line) {
            myType = type;
            myMessage = message;
            myLineNumber = lineNumber;
            myLine = line;
        }

        public override string ToString() {
            return myMessage + " on line " + myLineNumber + " : " + myLine;
        }

    }

    internal class Constants {

        internal const int NO_LINE = 0;

        internal const int ALLOWABLE_CHARACTERS_PER_LINE = 128;

        internal const int ALLOWABLE_LINES_PER_FILE = 700;

        internal const char ASCII_LF = '\n';

        internal const char ASCII_CR = '\r';

        internal const char ASCII_EOF = '\0';

    }

}