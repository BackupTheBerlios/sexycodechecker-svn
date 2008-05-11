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

    public class Context {

        private Collection<Violation> violations = new Collection<Violation>();
        
        public void AddViolation(Violation violation) {
            violations.Add(violation);
        }
        
        public virtual void ReportViolations(Results toResults) {
            foreach (Violation violation in violations) {
                toResults.Add(violation);
            }
        }

        private char myPreviousCharacter = Constants.ASCII_CR;
        public char PreviousCharacter {
            get {
                return myPreviousCharacter;
            }
            set {
                myPreviousCharacter = value;
            }
        }

        private bool myInCharDefinition;
        public bool InCharDefinition {
            get {
                return myInCharDefinition;
            }
            set {
                myInCharDefinition = value;
            }
        }

        private bool myInStringDefinition;
        public bool InStringDefinition {
            get {
                return myInStringDefinition;
            }
            set {
                myInStringDefinition = value;
            }
        }

        private bool myInDirectiveDefinition;
        public bool InDirectiveDefinition {
            get {
                return myInDirectiveDefinition;
            }
            set {
                myInDirectiveDefinition = value;
            }
        }
        
        private bool myIsEscaped;
        public bool IsEscaped {
            get {
                return myIsEscaped;
            }
            set {
                myIsEscaped = value;
            }
        }

        public bool HandlingString() {
            return myInCharDefinition || myInStringDefinition || myInDirectiveDefinition;
        }
        
        private int myFileLenght = 1;
        public int FileLenght {
            get {
                return myFileLenght;
            }
            set {
                myFileLenght = value;
            }
        }

        private bool myIAmInComment;
        public bool IAmInComment {
            get {
                return myIAmInComment;
            }
            set {
                myIAmInComment = value;
            }
        }

        private bool myIAmInMultilineComment;
        public bool IAmInMultilineComment {
            get {
                return myIAmInMultilineComment;
            }
            set {
                myIAmInMultilineComment = value;
            }
        }

        private string myCurrentLine = "";
        public string CurrentLine {
            get {
                return myCurrentLine;
            }
            set {
                myCurrentLine = value;
            }
        }

        private bool myTotallyEmptyLine = true;
        public bool TotallyEmptyLine {
            get {
                return myTotallyEmptyLine;
            }
            set {
                myTotallyEmptyLine = value;
            }
        }

        private bool myBlock;
        public bool Block {
            get {
                return myBlock;
            }
            set {
                myBlock = value;
            }
        }

        public bool IsInitializingTheBaseClass(char firstCharacterInLine) {
            bool callingBaseInitializer = (myCurrentLine.Contains("base") || myCurrentLine.Contains("this"));
            return (firstCharacterInLine == ':' && callingBaseInitializer && myCurrentLine.Contains("("));
        }

        private char myLastCharacter = Constants.ASCII_CR;

        public char LastCharacter {
            get {
                return myLastCharacter;
            }
            set {
                myLastCharacter = value;
            }
        }
        
    }

    public interface IRule {
        
        void Close(Context context);
        
        void Check(char currentCharacter, Context context);
        
    }
    
    public class ContextSetup : IRule {

        private Chain<char> chain;
        
        private Context context;
        
        public ContextSetup() {
            chain = new Chain<char>();
            chain.Add(new Handler<char>('\'', HandleCharDefinition, true));
            chain.Add(new Handler<char>('"', HandleStringDefinition, true));
            chain.Add(new Handler<char>('#', HandleDirectiveDefinition, true));
            chain.Add(new Handler<char>('/', HandleSlash, true));
            chain.Add(new Handler<char>('*', HandleStar, true));
        }
        
        public void Close(Context context) {
        }

        public void Check(char currentCharacter, Context context) {
            this.context = context;
            context.CurrentLine += currentCharacter;
            if (!char.IsWhiteSpace(currentCharacter)) {
                context.TotallyEmptyLine = false;
            }
            
            chain.Execute(currentCharacter);
        }
        
        private void HandleCharDefinition() {
            if (!context.IsEscaped && !context.InStringDefinition) {
                context.InCharDefinition = !context.InCharDefinition;
            }
        }

        private void HandleStringDefinition() {
            if (!context.InCharDefinition) {
                context.InStringDefinition = !context.InStringDefinition;
            }
        }
        
        private void HandleDirectiveDefinition() {
            if (!context.InCharDefinition && !context.InStringDefinition) {
                context.InDirectiveDefinition = true;
            }
        }
        
        private void HandleSlash() {
            if ('/' == context.PreviousCharacter && !context.HandlingString()) {
                context.IAmInComment = true;

            } else if ('*' == context.PreviousCharacter && !context.HandlingString()) {
                context.IAmInMultilineComment = false;
                context.IAmInComment = false;
            }
            context.Block = true;
        }

        private void HandleStar() {
            if ('/' == context.PreviousCharacter && !context.HandlingString()) {
                context.IAmInMultilineComment = true;
            }
            context.Block = true;
        }
        
    }
    
    public class ContextTeardown : IRule {
        
        private Chain<char> chain;
        
        private Context context;
        
        public ContextTeardown() {
            chain = new Chain<char>();
            chain.Add(new Handler<char>(Constants.ASCII_LF, HandleNewLine, true));
        }
        
        public void Check(char currentCharacter, Context context) {
            this.context = context;
            chain.Execute(currentCharacter);
            context.PreviousCharacter = currentCharacter;
            context.IsEscaped = !context.IsEscaped && (context.PreviousCharacter == '\\');
            context.Block = false;
        }

        private void HandleNewLine() {
            context.IAmInComment = context.IAmInMultilineComment;
            context.TotallyEmptyLine = true;
            context.CurrentLine = "";
            context.FileLenght++;

            context.InCharDefinition = false;
            context.InStringDefinition = false;
            context.InDirectiveDefinition = false;

            context.LastCharacter = Constants.ASCII_CR;
        }
       
        public void Close(Context context) {
        }
    }
    
    public abstract class AbstractRule : IRule {

        protected AbstractRule() {
            myChain = new Chain<char>();
            myChain.Add(new Handler<char>(Constants.ASCII_LF, HandleNewline, true));
            myChain.Add(new Handler<char>(StopIfBozo));
        }

        private Chain<char> myChain;
        protected Chain<char> Chain {
            get {
                return myChain;
            }
            set {
                myChain = value;
            }
        }
        
        private Context myContext;
        protected Context Context {
            get {
                return myContext;
            }
        }

        private char myCurrentCharacter;
        protected char CurrentCharacter {
            get {
                return myCurrentCharacter;
            }
        }

        protected abstract void DoNewLine(Context context);

        protected virtual bool ValidCharacter() {
            return !char.IsWhiteSpace(myCurrentCharacter);
        }

        public virtual void Close(Context context) {
            DoNewLine(context);
        }

        private void HandleNewline() {
            DoNewLine(myContext);
            myParensLevel = 0;
        }
        
        private bool StopIfBozo(char character) {
            return myContext.Block || myContext.IAmInMultilineComment || myContext.IAmInComment || !ValidCharacter();
        }

        public virtual void Check(char currentCharacter, Context context) {
            this.myContext = context;
            this.myCurrentCharacter = currentCharacter;
            myChain.Execute(currentCharacter);
        }

        protected bool StopIfInCharacterHandling(char shouldIt) {
            return myContext.InCharDefinition || myContext.InStringDefinition || myContext.InDirectiveDefinition;
        }

        private int myParensLevel;
        protected int ParensLevel {
            get {
                return myParensLevel;
            }
        }

        protected  void HandleParensLevelDown() {
            myParensLevel--;
        }

        protected void HandleParensLevelUp() {
            myParensLevel++;
        }

        protected void RecordLastCharacter() {
            Context.LastCharacter = CurrentCharacter;
        }
        
    }

    public class HeightRule : AbstractRule {

        public HeightRule() {
            Chain.Add(new Handler<char>(Record, true));
        }
        
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

        protected void Record() {
            iHaveCode = true;
        }

        protected override bool ValidCharacter() {
            return !Context.IAmInComment && !char.IsWhiteSpace(CurrentCharacter);
        }

        protected override void DoNewLine(Context context) {
            bool countIt = context.TotallyEmptyLine || iHaveCode;
            if (countIt) {
                myCodeLenght++;
            }
            iHaveCode = false;
        }

        public override void Close(Context context) {
            if (Constants.ALLOWABLE_LINES_PER_FILE < myCodeLenght) {
                context.AddViolation(CompilationUnitTooLong());
            }
        }

        private Violation CompilationUnitTooLong() {
            string message = "The compilation unit is too long: there are {0} lines of code in this file";
            message = string.Format(CultureInfo.InvariantCulture, message, myCodeLenght);
            return new Violation(ViolationType.FileTooLong, message, Context.FileLenght, "");
        }

    }

    public class WidthRule : AbstractRule {

        private int reentranceLevel;

        private int charactersInLine;

        private const int maxCharacters = Constants.ALLOWABLE_CHARACTERS_PER_LINE;

        private bool lineStart = true;

        public WidthRule() {
            Chain.Add(new Handler<char>('\t', HandleTab, true));
            Chain.Add(new Handler<char>(char.IsWhiteSpace, HandleWhitespace, true));
            Chain.Add(new Handler<char>(HandleDefault, true));
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
            if (!(StopIfInCharacterHandling(CurrentCharacter))) {
                if ('{' == CurrentCharacter) {
                    reentranceLevel += 4;
                } else if ('}' == CurrentCharacter) {
                    reentranceLevel -= 4;
                }
            }
            charactersInLine++;
        }

        protected override bool ValidCharacter() {
            return CurrentCharacter != Constants.ASCII_CR && CurrentCharacter != Constants.ASCII_EOF;
        }

        protected override void DoNewLine(Context context) {
            if (maxCharacters < charactersInLine) {
                LineTooWide(context);
            }
            charactersInLine = 0;
            lineStart = true;
        }
        
        private void LineTooWide(Context context) {
            string viewLine = context.CurrentLine.Replace(' ', '.');
            string message = "This line is too wide: {0} instead of {1}";
            message = string.Format(CultureInfo.InvariantCulture, message, charactersInLine, maxCharacters);
            context.AddViolation(new Violation(ViolationType.LineTooWide, message, context.FileLenght, viewLine));
        }

    }

    public class OneLinePerStatementRule : AbstractRule {

        private char firstCharacterInLine = Constants.ASCII_CR;

        private List<char> tailChars = new List<char>();

        private Violation delayViolation;

        public OneLinePerStatementRule() {
            tailChars.Add(';');
            tailChars.Add(',');
            tailChars.Add('{');
            tailChars.Add('}');
            tailChars.Add(Constants.ASCII_CR);

            Chain.Add(new Handler<char>(StopIfInCharacterHandling));
            Chain.Add(new Handler<char>(RecordLastCharacter, false));            
            Chain.Add(new Handler<char>('(', HandleParensLevelUp, false));
            Chain.Add(new Handler<char>(')', HandleParensLevelDown, false));
            Chain.Add(new Handler<char>(RecordFirstCharacter, false));
        }

        private void RecordFirstCharacter() {
            if (firstCharacterInLine == Constants.ASCII_CR) {
                firstCharacterInLine = CurrentCharacter;
            }
        }

        protected override void DoNewLine(Context context) {
            if (delayViolation != null) {
                if (!context.IsInitializingTheBaseClass(firstCharacterInLine)) {
                    context.AddViolation(delayViolation);
                }
                delayViolation = null;
            }
            if ((ParensLevel > 0 || !tailChars.Contains(context.LastCharacter))) {
                delayViolation = OneLinePerStatement();
                if (')' != context.LastCharacter) {
                    context.AddViolation(delayViolation);
                    delayViolation = null;
                }
            }
            firstCharacterInLine = Constants.ASCII_CR;
        }

        private Violation OneLinePerStatement() {
            string message = "This statement does not end on the current line";
            return new Violation(ViolationType.OneLinePerStatement, message, Context.FileLenght, Context.CurrentLine);
            
        }
    }

    public class OneStatementPerLineRule : AbstractRule {

        private int semiColumns;

        private int commas;

        public OneStatementPerLineRule() {
            Chain.Add(new Handler<char>(StopIfInCharacterHandling));
            Chain.Add(new Handler<char>(RecordLastCharacter, false));
            Chain.Add(new Handler<char>(',', HandleCommas, true));
            Chain.Add(new Handler<char>(';', HandleSemiColumns, true));
            Chain.Add(new Handler<char>('{', HandleParensLevelUp, true));
            Chain.Add(new Handler<char>('<', HandleParensLevelUp, true));
            Chain.Add(new Handler<char>('(', HandleParensLevelUp, true));
            Chain.Add(new Handler<char>('>', HandleParensLevelDown, true));
            Chain.Add(new Handler<char>(')', HandleParensLevelDown, true));
            Chain.Add(new Handler<char>('}', HandleParensLevelDown, true));
        }

        private void HandleCommas() {
            if (ParensLevel < 1) {
                commas++;
            }
        }

        private void HandleSemiColumns() {
            if (ParensLevel < 1) {
                semiColumns++;
            }
        }

        protected override void DoNewLine(Context context) {
            int resultCommas = commas - (context.LastCharacter == ',' ? 1 : 0);
            if (resultCommas > 0 || semiColumns > 1) {
                context.AddViolation(OneStatementPerLine());
            }
            semiColumns = 0;
            commas = 0;
        }
        
        private Violation OneStatementPerLine() {
            string message = "This line has more than one statement";
            return new Violation(ViolationType.OneStatementPerLine, message, Context.FileLenght, Context.CurrentLine);
        }

    }

    public class MethodHeightRule : AbstractRule {

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
            Chain.Add(new Handler<char>(Record, false));
            Chain.Add(new Handler<char>(StopIfInCharacterHandling));
            Chain.Add(new Handler<char>(RecordLastCharacter, false));
            Chain.Add(new Handler<char>('(', HandleMethodEnter, false));
            Chain.Add(new Handler<char>('(', HandleParensLevelUp, true));
            Chain.Add(new Handler<char>(')', HandleMethodExit, false));
            Chain.Add(new Handler<char>(')', HandleParensLevelDown, true));
            Chain.Add(new Handler<char>('{', HandleReentranceUp, true));
            Chain.Add(new Handler<char>('}', HandleReentranceDown, true));
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

        protected override void DoNewLine(Context context) {
            if (inMethod) {
                bool countIt = (context.TotallyEmptyLine || (iHaveCode && !context.IAmInComment));
                if (countIt) {
                    myMethodLength++;
                }
            } else {
                if (context.LastCharacter == '{' && methodEnter && methodExit && ParensLevel == 0) {
                    inMethod = true;
                    myMethodReentranceStart = myReentrance;
                    originalMethodLine = Context.CurrentLine;
                }
            }
            methodEnter = false;
            methodExit = false;
            iHaveCode = false;
        }

        private void MethodTooLong() {
            string message = "Method is {0} lines long";
            message = string.Format(CultureInfo.InvariantCulture, message, myMethodLength);
            Context.AddViolation(new Violation(ViolationType.MethodTooLong, message, Context.FileLenght, originalMethodLine));
        }

        protected void Record() {
            iHaveCode = true;
        }

    }

    public class VariableLenghtRule : AbstractRule {

        private string currentWord = "";

        private bool isCurrentNumericConstant;

        private int wordLenght;

        public VariableLenghtRule() {
            Chain.Add(new Handler<char>(StopIfInCharacterHandling));
            Chain.Add(new Handler<char>(char.IsWhiteSpace, HandleNewWord, true));
            Chain.Add(new Handler<char>(char.IsLetterOrDigit, RecordWord, true));
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
            if (char.IsWhiteSpace(CurrentCharacter)) {
                return;
            }
            if (wordLenght == 0 && (char.IsDigit(CurrentCharacter) || !char.IsLetterOrDigit(CurrentCharacter))) {
                isCurrentNumericConstant = true;
            }
            wordLenght++;
            currentWord += CurrentCharacter;
        }

        private void VariableTooShort() {
            string message = "Identifier '{0}' is too short: {1} characters";
            message = string.Format(CultureInfo.InvariantCulture, message, currentWord, wordLenght);
            string currentLine = Context.CurrentLine;
            Violation violation = new Violation(ViolationType.VariableTooShort, message, Context.FileLenght, currentLine);
            Context.AddViolation(violation);
        }

        protected override void DoNewLine(Context context) {
            HandleNewWord();
        }
        
        protected override bool ValidCharacter() {
            return true;
        }
    }
    
}
