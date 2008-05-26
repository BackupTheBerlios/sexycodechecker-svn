/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Globalization;

using Cluefultoys.Streams;
using Cluefultoys.Handlers;
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
    /// 5) Non-Keyword Identifiers will be named with at least three characters.
    /// </summary>
    public class Checker {

        private Context context;

        public Checker() {
            InitializeRuleset();
        }

        private void InitializeRuleset() {
            context = new Context();
        }

        public Results CheckFile(string fileName) {
            Results results;
            try {
                using (Stream file = File.Open(fileName, FileMode.Open)) {
                    results = Check(file, fileName);
                }
            } catch (FileNotFoundException exception) {
                results = new Results(fileName);
                Violation violation = new Violation(ViolationType.FileNotFound, exception.Message, Constants.NO_LINE, fileName);
                results.Add(violation);
            }
            return results;
        }

        public Results Check(Stream stream, string fileName) {
            byte[] buffer = new byte[1024];
            char[] charBuffer = new char[1024];

            Reader reader = new Reader(AnalyzeCharacters);

            while (stream.Position < stream.Length) {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                reader.DoRead(buffer, bytesRead, charBuffer);
            }

            return context.DoClose(fileName);
        }

        private void AnalyzeCharacters(char[] charBuffer, int charDecoded) {
            for (int index = 0; index < charDecoded; index++) {
                context.AnalyzeCharacter(charBuffer[index]);
            }
        }
    }

    public interface IDocumentHolder {

        XmlDocument Document {
            get;
        }

        XmlNamespaceManager NamespaceManager {
            get;
        }

        XmlNodeList ExecuteSelectNodes(string query);

    }

    public class DocumentHolder : IDocumentHolder {

        private XmlDocument myDocument;

        public XmlDocument Document {
            get {
                return myDocument;
            }
        }

        private XmlNamespaceManager myNamespaceManager;

        public XmlNamespaceManager NamespaceManager {
            get {
                return myNamespaceManager;
            }
        }

        // TODO: namespaces should be also passable with a dictionary
        public DocumentHolder(Stream stream, string prefix, string xmlNamespace) {
            myDocument = new XmlDocument();
            myDocument.Load(stream);

            myNamespaceManager = new XmlNamespaceManager(myDocument.NameTable);
            myNamespaceManager.AddNamespace(prefix, xmlNamespace);
        }

        public XmlNodeList ExecuteSelectNodes(string query) {
            XmlNodeList result = myDocument.SelectNodes(query, myNamespaceManager);
            return result;
        }

    }

    public class NullDocumentHolder : IDocumentHolder {

        public NullDocumentHolder() {
            myDocument = new XmlDocument();
            myNamespaceManager = new XmlNamespaceManager(myDocument.NameTable);
        }

        private XmlDocument myDocument;

        public XmlDocument Document {
            get {
                return myDocument;
            }
        }


        private XmlNamespaceManager myNamespaceManager;

        public XmlNamespaceManager NamespaceManager {
            get {
                return myNamespaceManager;
            }
        }

        public XmlNodeList ExecuteSelectNodes(string query) {
            return new XmlNodeListImpl();
        }

    }

    public class XmlNodeListImpl : XmlNodeList {

        private static NullEnumerator noEnumerator = new NullEnumerator();
        
        public override int Count {
            get {
                return 0;
            }
        }

        public override IEnumerator GetEnumerator() {
            return noEnumerator;
        }

        public override XmlNode Item(int index) {
            return null;
        }
    }

    public class NullEnumerator : IEnumerator {
        public object Current {
            get {
                throw new InvalidOperationException("this iterator has no elements");
            }
        }

        public bool MoveNext() {
            return false;
        }

        public void Reset() {
        }
    }

    public class MSBuildReader {

        private static string msBuildPrefix = Cluefultoys.Resources.XmlProperties.MsBuildPrefix;

        private static string msBuildNamespace = Cluefultoys.Resources.XmlProperties.MsBuildNamespace;

        private static string sccBuildPrefix = Cluefultoys.Resources.XmlProperties.SccBuildPrefix;

        private static string sccBuildNamespace = Cluefultoys.Resources.XmlProperties.SccBuildNamespace;

        private static string readCompileTags = Cluefultoys.Resources.XmlProperties.XPathMsBuildCompile;

        private static string readSccExcludeCompileTags = Cluefultoys.Resources.XmlProperties.XPathSccBuildCompileExclude;

        private static string readSccIncludeCompileTags = Cluefultoys.Resources.XmlProperties.XPathSccBuildCompileInclude;

        private static string readImports = Cluefultoys.Resources.XmlProperties.XPathSccBuildImportInclude;

        private IDocumentHolder project;

        private IDocumentHolder overlay;

        private string configurationDir;

        public MSBuildReader(string configurationFile) {
            configurationDir = configurationFile.Substring(0, configurationFile.LastIndexOf('/'));
            using (Stream stream = File.Open(configurationFile, FileMode.Open)) {
                project = new DocumentHolder(stream, msBuildPrefix, msBuildNamespace);
            }

            string overlayFile = string.Format("{0}.overlay", configurationFile);
            try {
                using (Stream stream = File.Open(overlayFile, FileMode.Open)) {
                    overlay = new DocumentHolder(stream, sccBuildPrefix, sccBuildNamespace);
                }
            } catch (FileNotFoundException) {
                overlay = new NullDocumentHolder();
            }
        }

        public Collection<string> FilesToInclude() {
            Collection<string> result = FetchCompilationTargets();
            ResolveImports(result);
            return result;
        }

        private void ResolveImports(Collection<string> result) {
            XmlNodeList importsList = overlay.ExecuteSelectNodes(readImports);
            foreach (XmlNode import in importsList) {
                String newImport = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", configurationDir, import.Value);
                LoadFromImportedProject(result, newImport);
            }
        }

        private static void LoadFromImportedProject(Collection<string> result, String newImport) {
            MSBuildReader newReader = new MSBuildReader(newImport);
            Collection<string> importResult = newReader.FilesToInclude();
            foreach (string importedTarget in importResult) {
                result.Add(importedTarget);
            }
        }

        private void Include(Collection<string> tempHolder, IDocumentHolder holder, string query) {
            XmlNodeList nodes = holder.ExecuteSelectNodes(query);
            foreach (XmlNode node in nodes) {
                tempHolder.Add(node.Value);
            }
        }

        private void Exclude(Collection<string> tempHolder, IDocumentHolder holder, string query) {
            XmlNodeList nodes = holder.ExecuteSelectNodes(query);
            foreach (XmlNode node in nodes) {
                tempHolder.Remove(node.Value);
            }
        }

        private Collection<string> FetchCompilationTargets() {
            Collection<string> tempHolder = new Collection<string>();
            Include(tempHolder, project, readCompileTags);
            Include(tempHolder, overlay, readSccIncludeCompileTags);
            Exclude(tempHolder, overlay, readSccExcludeCompileTags);

            Collection<string> result = new Collection<string>();
            foreach (string tempResult in tempHolder) {
                result.Add(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", configurationDir, tempResult));
            }

            return result;
        }

    }

    public class Results {

        public Results(string fileName) {
            myFileName = fileName;
        }

        private string myFileName;
        public string FileName {
            get {
                return myFileName;
            }
        }

        private Collection<Violation> myViolations = new Collection<Violation>();
        public Collection<Violation> Violations {
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

    public class ProjectChecker {

        private string myProjectName;

        public string ProjectName {
            get {
                return myProjectName;
            }
        }

        private Collection<Results> myResults = new Collection<Results>();
        public Collection<Results> Results {
            get {
                return myResults;
            }
        }

        private bool isHappy = true;
        public bool Happy {
            get {
                return isHappy;
            }
        }

        private string configurationFile;

        public ProjectChecker(string configurationFile) {
            this.configurationFile = configurationFile;
            this.myProjectName = configurationFile;
        }

        public void Run() {
            myResults = new Collection<Results>();
            MSBuildReader reader = new MSBuildReader(configurationFile);
            Collection<string> allFiles = reader.FilesToInclude();
            foreach (string file in allFiles) {
                Checker checker = new Checker();
                Results results = checker.CheckFile(file);
                isHappy &= results.Happy;
                myResults.Add(results);
            }
        }

    }

    public class Violation {

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

    public enum ViolationType {
        FileNotFound,
        FileTooLong,
        LineTooWide,
        OneStatementPerLine,    // rule 3 first part
        OneLinePerStatement,    // rule 3 second part
        MethodTooLong,          // rule 4
        VariableTooShort,       // rule 5
    }

    internal sealed class Constants {

        private Constants() {
        }

        internal const int NO_LINE = 0;

        internal const int ALLOWABLE_CHARACTERS_PER_LINE = 128;

        internal const int ALLOWABLE_LINES_PER_FILE = 700;

        internal const char ASCII_LF = '\n';

        internal const char ASCII_CR = '\r';

        internal const char ASCII_EOF = '\0';

    }

    public class Context {

        // From ContextHandler
        private List<IRule> rules;

        private Collection<Violation> violations = new Collection<Violation>();

        public void AddViolation(Violation violation) {
            violations.Add(violation);
        }

        public void ReportViolations(Results toResults) {
            foreach (Violation violation in violations) {
                toResults.Add(violation);
            }
        }

        private char myPreviousCharacter = Constants.ASCII_CR;
        public char PreviousCharacter {
            get {
                return myPreviousCharacter;
            }
        }

        private bool myInCharDefinition;
        public bool InCharDefinition {
            get {
                return myInCharDefinition;
            }
        }

        private bool myInStringDefinition;
        public bool InStringDefinition {
            get {
                return myInStringDefinition;
            }
        }

        private bool myInDirectiveDefinition;
        public bool InDirectiveDefinition {
            get {
                return myInDirectiveDefinition;
            }
        }

        private bool myIsEscaped;
        public bool IsEscaped {
            get {
                return myIsEscaped;
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
        }

        private bool myIAmInComment;
        public bool IAmInComment {
            get {
                return myIAmInComment;
            }
        }

        private bool myIAmInMultilineComment;
        public bool IAmInMultilineComment {
            get {
                return myIAmInMultilineComment;
            }
        }

        private string myCurrentLine = "";
        public string CurrentLine {
            get {
                return myCurrentLine;
            }
        }

        private bool myTotallyEmptyLine = true;
        public bool TotallyEmptyLine {
            get {
                return myTotallyEmptyLine;
            }
        }

        private bool myBlock;
        public bool Block {
            get {
                return myBlock;
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

        private Chain<char> setupChain;

        private Chain<char> teardownChain;

        public Context() {
            rules = new List<IRule>();

            rules.Add(new HeightRule());
            rules.Add(new WidthRule());
            rules.Add(new OneStatementPerLineRule());
            rules.Add(new OneLinePerStatementRule());
            rules.Add(new MethodHeightRule());
            rules.Add(new VariableLenghtRule());

            setupChain = new Chain<char>();
            setupChain.Add(new Handler<char>('\'', HandleCharDefinition, true));
            setupChain.Add(new Handler<char>('"', HandleStringDefinition, true));
            setupChain.Add(new Handler<char>('#', HandleDirectiveDefinition, true));
            setupChain.Add(new Handler<char>('/', HandleSlash, true));
            setupChain.Add(new Handler<char>('*', HandleStar, true));

            teardownChain = new Chain<char>();
            teardownChain.Add(new Handler<char>(Constants.ASCII_LF, HandleNewLine, true));
        }

        // TODO public must go away
        public void DoSetup(char character) {
            myCurrentLine += character;
            if (!char.IsWhiteSpace(character)) {
                myTotallyEmptyLine = false;
            }

            setupChain.Execute(character);
        }

        // TODO public must go away
        public void DoTeardown(char character) {
            teardownChain.Execute(character);
            myPreviousCharacter = character;
            myIsEscaped = !myIsEscaped && (myPreviousCharacter == '\\');
            myBlock = false;

        }

        // From ContextHandler
        public void AnalyzeCharacter(char character) {
            DoSetup(character);
            foreach (IRule rule in rules) {
                rule.Check(character, this);
            }
            DoTeardown(character);
        }

        // From ContextHandler
        public Results DoClose(string fileName) {
            Results results = new Results(fileName);

            foreach (IRule rule in rules) {
                rule.Close(this);
            }

            ReportViolations(results);
            return results;
        }

        private void HandleCharDefinition(char target) {
            if (!myIsEscaped && !myInStringDefinition) {
                myInCharDefinition = !myInCharDefinition;
            }
        }

        private void HandleStringDefinition(char target) {
            if (!myInCharDefinition) {
                myInStringDefinition = !myInStringDefinition;
            }
        }

        private void HandleDirectiveDefinition(char target) {
            if (!myInCharDefinition && !myInStringDefinition) {
                myInDirectiveDefinition = true;
            }
        }

        private void HandleSlash(char target) {
            if ('/' == myPreviousCharacter && !HandlingString()) {
                myIAmInComment = true;
            } else if ('*' == myPreviousCharacter && !HandlingString()) {
                myIAmInMultilineComment = false;
                myIAmInComment = false;
            }
            myBlock = true;
        }

        private void HandleStar(char target) {
            if ('/' == myPreviousCharacter && !HandlingString()) {
                myIAmInMultilineComment = true;
            }
            myBlock = true;
        }

        private void HandleNewLine(char target) {
            myIAmInComment = myIAmInMultilineComment;
            myTotallyEmptyLine = true;
            myCurrentLine = "";
            myFileLenght++;

            myInCharDefinition = false;
            myInStringDefinition = false;
            myInDirectiveDefinition = false;

            myLastCharacter = Constants.ASCII_CR;
        }

    }


}
