using System;
using System.Collections.Generic;
using System.IO;

namespace Cluefultoys.Sexycodechecker {

    public class Violation {

        // Recursive Bug
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

}