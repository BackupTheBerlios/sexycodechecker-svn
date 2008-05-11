/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System.Collections.ObjectModel;
using System.IO;

using Cluefultoys.Streams;
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
        public  Collection<Violation> Violations {
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

}
