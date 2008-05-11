/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System.Collections.Generic;
using System.Text;

namespace Cluefultoys.Streams {
    
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
            offsets[Encoding.Default] = 0;
            offsets[Encoding.ASCII] = 0;
            offsets[Encoding.UTF8] = 3;
            offsets[Encoding.UTF7] = 3;
            offsets[Encoding.UTF32] = 4;
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

            return encoding;
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
    
    public delegate void Read(byte[] streamData, int readBytes, char[] characters);

    public delegate void Callback(char[] characters, int decoded);
    
    public class Reader {
        
        public Reader(Callback callback) {
            this.currentRead = FirstRead;
            this.myCallbackDelegate = callback;
            
            selector = new EncodingSelector();
        }
        
        private Read currentRead;
        
        public Read DoRead {
            get {
                return currentRead;
            }
        }
        
        private EncodingSelector selector;

        private Callback myCallbackDelegate;
        
        private Decoder decoder;
        
        private void FirstRead(byte[] streamData, int readBytes, char[] characters) {
            Encoding encoding = selector.GetEncoding(streamData);
            decoder = encoding.GetDecoder();
            int theOffset = selector.ByteCount(encoding);

            int decoded = decoder.GetChars(streamData, theOffset, readBytes - theOffset , characters, 0);
            myCallbackDelegate(characters, decoded);
            currentRead = ReadFurther;
        }
        
        private void ReadFurther(byte[] streamData, int readBytes, char[] characters) {
            int charDecoded = decoder.GetChars(streamData, 0, readBytes, characters, 0);
            myCallbackDelegate(characters, charDecoded);
        }
        
    }

}
