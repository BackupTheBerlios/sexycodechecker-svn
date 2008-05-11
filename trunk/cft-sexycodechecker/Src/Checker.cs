/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

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

        private EncodingSelector selector;
        
        private ContextHandler contextHandler;

        public Checker() {
            InitializeRuleset();
        }
        
        private void InitializeRuleset() {
            contextHandler = new ContextHandler();
            
            selector = new EncodingSelector();
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
            stream.Read(buffer, 0, 4);
            Encoding encoding = selector.GetEncoding(buffer);
            
            return Check(stream, buffer, encoding, fileName);
        }
        
        private Results Check(Stream stream, byte[] buffer, Encoding encoding, string fileName) {
            int theOffset = selector.ByteCount(encoding);
            Decoder decoder = encoding.GetDecoder();
            char[] charBuffer = new char[1024];
            while (stream.Position < stream.Length) {
                int bytesRead = stream.Read(buffer, theOffset, buffer.Length - theOffset);
                int charDecoded = decoder.GetChars(buffer, 0, bytesRead + theOffset, charBuffer, 0);
                AnalyzeCharacters(charBuffer, charDecoded);
                theOffset = 0;
            }

            return contextHandler.CloseStream(fileName);
        }
        
        private void AnalyzeCharacters(char[] charBuffer, int charDecoded) {
            for (int index = 0; index < charDecoded; index++) {
                char currentCharacter = charBuffer[index];
                contextHandler.AnalyzeCharacter(currentCharacter);
            }
        }
    }
    
    public class ContextHandler {
        
        private Context context = new Context();
        
        private IRule contextSetup;
        
        private IRule contextTeardown;
        
        private List<IRule> rules;

        public ContextHandler() {
            rules = new List<IRule>();

            rules.Add(new HeightRule());
            rules.Add(new WidthRule());
            rules.Add(new OneStatementPerLineRule());
            rules.Add(new OneLinePerStatementRule());
            rules.Add(new MethodHeightRule());
            rules.Add(new VariableLenghtRule());
            
            contextSetup = new ContextSetup();
            contextTeardown = new ContextTeardown();
        }
        
        public void AnalyzeCharacter(char character) {
            contextSetup.Check(character, context);
            foreach (IRule rule in rules) {
                rule.Check(character, context);
            }
            contextTeardown.Check(character, context);
        }
        
        public Results CloseStream(string fileName) {
            Results results = new Results(fileName);

            contextSetup.Close(context);
            foreach (IRule rule in rules) {
                rule.Close(context);
            }
            
            context.ReportViolations(results);
            return results;
        }
        
    }
    
    public class EncodingSelector {
        
        private Dictionary<uint, Encoding> decoders;

        private Dictionary<Encoding, int> offsets;
        
        public EncodingSelector() {
            InitializeDictionaries();
        }
        
        private void InitializeDictionaries() {
            decoders = new Dictionary<uint, Encoding>();
            decoders[BOMCount(Encoding.BigEndianUnicode.GetPreamble(), 2)] = Encoding.BigEndianUnicode;
            decoders[BOMCount(Encoding.Unicode.GetPreamble(), 2)] =  Encoding.Unicode;
            decoders[BOMCount(Encoding.UTF32.GetPreamble(), 4)] =  Encoding.UTF32;
            decoders[BOMCount(Encoding.UTF7.GetPreamble(), 3)] =  Encoding.UTF7;
            decoders[BOMCount(Encoding.UTF8.GetPreamble(), 3)] =  Encoding.UTF8;

            offsets = new Dictionary<Encoding, int>();
            offsets[Encoding.Default] = 4;
            offsets[Encoding.ASCII] = 4;
            offsets[Encoding.UTF8] = 1;
            offsets[Encoding.UTF7] = 1;
            offsets[Encoding.UTF32] = 0;
            offsets[Encoding.Unicode] = 2;
            offsets[Encoding.BigEndianUnicode] = 2;
        }

        public Encoding GetEncoding(byte[] buffer) {
            Encoding encoding = Encoding.Default;

            for (int bytes = 4; bytes >= 2 && encoding == Encoding.Default; bytes--) {
                uint byteOrderModel = BOMCount(buffer, bytes);
                try {
                    encoding = decoders[byteOrderModel];
                } catch (KeyNotFoundException) {
                }
            }

            CorrectArray(buffer, encoding);
            return encoding;
        }
        
        private void CorrectArray(byte[] buffer, Encoding encoding) {
            int count = ByteCount(encoding);
            for (int index = 0; index < count; index++) {
                buffer[index] = buffer[4 - count + index];
            }
        }
        
        public int ByteCount (Encoding encoding) {
            return offsets[encoding];
        }
        
        private static uint BOMCount(byte[] input, int howMany) {
            uint result = 0;
            for(int index = 0; index < howMany && index < input.Length; index++) {
                result = result * 256;
                result += input[index];
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
