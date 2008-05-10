/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Cluefultoys.Sexycodechecker {

    public abstract class Rule {

        protected Rule() {
            chain = new Chain<char>();
            chain.Add(new Handler<char>(Constants.ASCII_LF, HandleNewline, true));
            chain.Add(new Handler<char>('\'', HandleCharDefinition, false));
            chain.Add(new Handler<char>('"', HandleStringDefinition, false));
            chain.Add(new Handler<char>('/', HandleSlash, true));
            chain.Add(new Handler<char>('*', HandleStar, true));
            chain.Add(new Handler<char>(HandleDefault, true));
        }

        private Chain<char> chain;

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

        protected bool iAmInComment;

        protected bool iAmInMultilineComment;

        protected Collection<Violation> violations = new Collection<Violation>();

        protected string currentLine = "";

        protected bool inCharDefinition;

        protected bool inStringDefinition;

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
            } else if (!char.IsWhiteSpace(myPreviousCharacter)) {
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

        private int myCodeLenght;

        public int CodeLenght {
            get {
                return myCodeLenght;
            }
        }

        private bool iHaveCode;

        // TODO Remove
        public bool HasCode {
            get {
                return iHaveCode;
            }
        }

        protected override void Record() {
            iHaveCode = true;
        }

        protected override bool ValidCharacter() {
            return !iAmInComment && !char.IsWhiteSpace(currentCharacter);
        }

        protected override void DoNewLine() {
            bool countIt = totallyEmptyLine || iHaveCode;
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
            message = string.Format(CultureInfo.InvariantCulture, message, myCodeLenght);
            return new Violation(ViolationType.FileTooLong, message, FileLenght, "");
        }

    }

    public class WidthRule : Rule {

        private Chain<char> chain;

        private int reentranceLevel;

        private int charactersInLine;

        private const int maxCharacters = Constants.ALLOWABLE_CHARACTERS_PER_LINE;

        private bool lineStart = true;

        public WidthRule() {
            chain = new Chain<char>();
            chain.Add(new Handler<char>('\t', HandleTab, true));
            chain.Add(new Handler<char>(char.IsWhiteSpace, HandleWhitespace, true));
            chain.Add(new Handler<char>(HandleDefault, true));
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
            if (!(inCharDefinition || inStringDefinition)) {
                if ('{' == currentCharacter) {
                    reentranceLevel += 4;
                } else if ('}' == currentCharacter) {
                    reentranceLevel -= 4;
                }
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
            if (maxCharacters < charactersInLine) {
                LineTooWide();
            }
            charactersInLine = 0;
            lineStart = true;
        }
        
        private void LineTooWide() {
            string viewLine = currentLine.Replace(' ', '.');
            string message = "This line is too wide: {0} instead of {1}";
            message = string.Format(CultureInfo.InvariantCulture, message, charactersInLine, maxCharacters);
            violations.Add(new Violation(ViolationType.LineTooWide, message, FileLenght, viewLine));
        }

    }

    public abstract class Rule3 : Rule {

        protected Chain<char> chain = new Chain<char>();

        private char myLastCharacter = Constants.ASCII_CR;

        protected int parensLevel;

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
            parensLevel = 0;
        }

        protected void RecordLastCharacter() {
            myLastCharacter = currentCharacter;
        }

        protected char LastCharacter {
            get {
                return myLastCharacter;
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

            chain.Add(new Handler<char>(StopIfInCharacterHandling));
            chain.Add(new Handler<char>('(', HandleParensLevelUp, false));
            chain.Add(new Handler<char>(')', HandleParensLevelDown, false));
            chain.Add(new Handler<char>(RecordFirstCharacter, false));
            chain.Add(new Handler<char>(RecordLastCharacter, true));
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
            if ((parensLevel > 0 || !tailChars.Contains(LastCharacter))) {
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
            return new Violation(ViolationType.OneLinePerStatement, message, FileLenght, currentLine);
            
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
            chain.Add(new Handler<char>(StopIfInCharacterHandling));
            chain.Add(new Handler<char>(RecordLastCharacter, false));
            chain.Add(new Handler<char>(',', HandleCommas, true));
            chain.Add(new Handler<char>(';', HandleSemiColumns, true));
            chain.Add(new Handler<char>('{', HandleParensLevelUp, true));
            chain.Add(new Handler<char>('<', HandleParensLevelUp, true));
            chain.Add(new Handler<char>('(', HandleParensLevelUp, true));
            chain.Add(new Handler<char>('>', HandleParensLevelDown, true));
            chain.Add(new Handler<char>(')', HandleParensLevelDown, true));
            chain.Add(new Handler<char>('}', HandleParensLevelDown, true));
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
            return new Violation(ViolationType.OneStatementPerLine, message, FileLenght, currentLine);
        }

    }

    public class MethodHeightRule : Rule3 {

        private string originalMethodLine = "";

        private int myMethodLength;

        private int myReentrance;

        private int myMethodReentranceStart;

        private bool methodEnter;

        private bool methodExit;

        private bool inMethod;

        private bool iHaveCode;

        // TODO Remove
        public bool HasCode {
            get {
                return iHaveCode;
            }
        }

        public MethodHeightRule() {
            chain.Add(new Handler<char>(StopIfInCharacterHandling));
            chain.Add(new Handler<char>(RecordLastCharacter, false));
            chain.Add(new Handler<char>('(', HandleMethodEnter, false));
            chain.Add(new Handler<char>('(', HandleParensLevelUp, true));
            chain.Add(new Handler<char>(')', HandleMethodExit, false));
            chain.Add(new Handler<char>(')', HandleParensLevelDown, true));
            chain.Add(new Handler<char>('{', HandleReentranceUp, true));
            chain.Add(new Handler<char>('}', HandleReentranceDown, true));
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
            myReentrance++;
        }

        private void HandleReentranceDown() {
            myReentrance--;
            if (inMethod && myReentrance < myMethodReentranceStart) {
                if (myMethodLength > 20) {
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

        private void MethodTooLong() {
            string message = "Method is {0} lines long";
            message = string.Format(CultureInfo.InvariantCulture, message, myMethodLength);
            this.violations.Add(new Violation(ViolationType.MethodTooLong, message, FileLenght, originalMethodLine));
        }

        protected override void Record() {
            iHaveCode = true;
            chain.Execute(currentCharacter);
        }

    }

    public class VariableLenghtRule : Rule3 {

        private string currentWord = "";

        private bool isCurrentNumericConstant;

        private int wordLenght;

        public VariableLenghtRule() {
            chain.Add(new Handler<char>(StopIfInCharacterHandling));
            chain.Add(new Handler<char>(char.IsWhiteSpace, HandleNewWord, true));
            chain.Add(new Handler<char>(char.IsLetterOrDigit, RecordWord, true));
            stopWords.Add("if");
            stopWords.Add("in");
            stopWords.Add("as");
            stopWords.Add("T");
        }

        private List<string> stopWords = new List<string>();

        private void HandleNewWord() {
            if (wordLenght == 0) {
                return;
            }

            if (!isCurrentNumericConstant && wordLenght < 3 && !stopWords.Contains(currentWord.Trim())) {
                VariableTooShort();
            }
            wordLenght = 0;
            currentWord = "";
            isCurrentNumericConstant = false;
        }

        private void RecordWord() {
            if (char.IsWhiteSpace(currentCharacter)) {
                return;
            }
            if (wordLenght == 0 && (char.IsDigit(currentCharacter) || !char.IsLetterOrDigit(currentCharacter))) {
                isCurrentNumericConstant = true;
            }
            wordLenght++;
            currentWord += currentCharacter;
        }

        private void VariableTooShort() {
            string message = "Identifier '{0}' is too short: {1} characters";
            message = string.Format(CultureInfo.InvariantCulture, message, currentWord, wordLenght);
            this.violations.Add(new Violation(ViolationType.VariableTooShort, message, FileLenght, currentLine));
        }

        protected override void Clear() {
            HandleNewWord();
        }

        protected override void Record() {
            chain.Execute(currentCharacter);
        }
      
        protected override bool ValidCharacter() {
             return true;
        }
    }
}
