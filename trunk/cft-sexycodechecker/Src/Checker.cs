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
    /// 5) Non-Keyword Identifiers will be named with at least three characters.
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
            rules.Add(new VariableLenghtRule());
            return rules;
        }
    }

    public class Results {

        public Results(string fileName) {
            myFileName = fileName;
        }

        private string myFileName;
        public string Filename {
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
            VariableTooShort,       // rule 5
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