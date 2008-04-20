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
    /// 4) Every method will be of 20 lines max.
    /// 5) Variables, including iteration variables, will be named with at least three characters, including iteration variables.
    /// </summary>
    public class Checker {

        // TODO: character recognition needs to change with unicode support.
        public Results CheckFile(string filename) {
            Results results = new Results(filename);
            try {
                using (FileStream file = File.Open(filename, FileMode.Open)) {
                    HeightRule lineChecker = new HeightRule(filename);
                    WidthRule widthChecker = new WidthRule();
                    while (file.Position < file.Length) {
                        char currentCharacter = (char)file.ReadByte();
                        lineChecker.Check(currentCharacter);
                        widthChecker.Check(currentCharacter);
                    }
                    lineChecker.ReportViolations(results);
                    widthChecker.ReportViolations(results);
                }
            } catch (FileNotFoundException fne) {
                results.Add(Violation.FileNotFoundViolation(fne, filename));
            }
            return results;
        }

        public abstract class Rule {

            protected const char ASCII_LF = (char)0x0A;

            protected const char ASCII_CR = (char)0x0D;

            public abstract void Check(char currentCharacter);

            protected int myLines = 0;
            public int Lines {
                get {
                    return myLines;
                }
            }

            public abstract void ReportViolations(Results toResults);

            protected bool iAmInComment = false;

            protected bool iAmInMultilineComment = false;
            // TODO Remove
            public bool IsInMultilineComment {
                get {
                    return iAmInMultilineComment;
                }
            }

            protected char myPreviousCharacter = '\0';

            protected char myPreviousMeaningfulCharacter = ' ';

            protected void HandleComment(char currentCharacter) {
                if ('/' == currentCharacter) {
                    if ('/' == myPreviousCharacter) {
                        iAmInComment = true;
                    } else if ('*' == currentCharacter && iAmInMultilineComment) {
                        iAmInMultilineComment = false;
                        iAmInComment = false;
                    }
                } else if ('*' == currentCharacter) {
                    if ('/' == myPreviousCharacter) {
                        iAmInMultilineComment = true;
                    }
                }
            }

            protected bool NotInComment() {
                return (!iAmInMultilineComment && !iAmInComment);
            }

        }

        public class HeightRule : Rule {

            private string fileName;

            public HeightRule(string fileName) {
                this.fileName = fileName;
            }

            public override void Check(char currentCharacter) {
                if (ASCII_LF == currentCharacter) {
                    NewLine();
                } else if ('/' == currentCharacter || '*' == currentCharacter) {
                    HandleComment(currentCharacter);
                } else if (Char.IsLetterOrDigit(currentCharacter) && !iAmInComment) {
                    iHaveCode = true;
                }

                myPreviousCharacter = currentCharacter;
            }

            private void NewLine() {
                bool countIt = iHaveCode || NotInComment();

                if (countIt) {
                    myLines++;
                }
                iAmInComment = iAmInMultilineComment;
                iHaveCode = false;
            }


            private bool iHaveCode = false;
            // TODO Remove
            public bool HasCode {
                get {
                    return iHaveCode;
                }
            }

            public override void ReportViolations(Results toResults) {
                if (myLines > 700) {
                    toResults.Add(Violation.CompilationUnitTooLong(fileName, myLines));
                }
            }
        }

        public class WidthRule : Rule {

            private int startingWhitespaces = 0;

            private int reentranceLevel = 0;

            private int charactersInLine = 0;

            private string currentLine = "";

            private List<Violation> violations = new List<Violation>();

            public WidthRule() {
                myLines = 1;
            }

            // IDEA: hit the deck and merge the check
            public override void Check(char currentCharacter) {
                if (currentCharacter == ASCII_LF) {
                    NewLine();
                } else if ('/' == currentCharacter || '*' == currentCharacter) {
                    HandleComment(currentCharacter);
                }
                if (NotInComment() && currentCharacter != ASCII_CR && currentCharacter != ASCII_LF) {
                    if (Char.IsWhiteSpace(currentCharacter)) {
                        if (' ' == myPreviousMeaningfulCharacter) {
                            if ('\t' == currentCharacter) {
                                startingWhitespaces += 4;
                            } else {
                                startingWhitespaces += 1;
                            }
                        }
                    } else {
                        if (' ' == myPreviousMeaningfulCharacter) {
                            charactersInLine = (reentranceLevel > startingWhitespaces ? reentranceLevel : startingWhitespaces);
                        }
                        myPreviousMeaningfulCharacter = currentCharacter;
                    }
                    charactersInLine++;
                    currentLine += currentCharacter;
                }
                myPreviousCharacter = currentCharacter;
            }

            private const int checkCharacters = 128;

            private void NewLine() {
                if (charactersInLine > checkCharacters) {
                    violations.Add(Violation.LineTooWide(currentLine, myLines, charactersInLine, checkCharacters));
                }
                if ('{' == myPreviousMeaningfulCharacter) {
                    reentranceLevel += 4;
                } else if ('}' == myPreviousMeaningfulCharacter) {
                    reentranceLevel -= 4;
                }
                currentLine = "";
                charactersInLine = 0;
                startingWhitespaces = 0;
                iAmInComment = iAmInMultilineComment;
                myPreviousMeaningfulCharacter = ' ';
                myLines++;
            }


            public override void ReportViolations(Results toResults) {
                NewLine();
                foreach (Violation violation in violations) {
                    toResults.Add(violation);
                }
            }

        }

    }

    public class Results {

        public Results(string fileName) {
            myFileName = fileName;
        }

        string myFileName;
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
        private Violation(ViolationType type, string message, int lineNumber, string line) {
            myType = type;
            myMessage = message;
            myLineNumber = lineNumber;
            myLine = line;
        }

        public static Violation FileNotFoundViolation(FileNotFoundException fne, string fileName) {
            return new Violation(ViolationType.FileNotFound, fne.Message, NO_LINE, fileName);
        }

        public static Violation CompilationUnitTooLong(string fileName, int recordLine) {
            return new Violation(ViolationType.FileTooLong, "The compilation unit is too long", recordLine, fileName);
        }

        public static Violation LineTooWide(string currentLine, int violationLine, int currentCharacters, int checkCharacters) {
            string viewLine = currentLine.Replace(' ', '.');
            string message = string.Format("This line is too wide: {0} instead of {1}", currentCharacters, checkCharacters);
            return new Violation(ViolationType.LineTooWide, message, violationLine, currentLine);
        }

        public override string ToString() {
            return myMessage + " on line " + myLineNumber + " : " + myLine;
        }

        public static int NO_LINE = 0;

    }

}