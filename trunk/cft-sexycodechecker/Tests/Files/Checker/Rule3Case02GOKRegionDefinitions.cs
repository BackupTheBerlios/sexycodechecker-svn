using System;
using System.Collections.Generic;
using System.IO;

namespace Cluefultoys.Sexycodechecker {

    public class Violation {
        
        private int value;
        
        public Violation() {
            value = 0;
        }
        
        #region guibuilder
        public TellGui(object graphicWhizzo) {
            // "640 k ought to be enough for anyone"
            System.Console.Out("value is {0}", value);
        }
        #endregion
              
    }

}
