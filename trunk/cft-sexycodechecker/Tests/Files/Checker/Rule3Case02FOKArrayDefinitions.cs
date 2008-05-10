using System;
using System.Collections.Generic;
using System.IO;

namespace Cluefultoys.Sexycodechecker {

    public class Violation {
        byte[] bytes2 =  new byte[] { 0x98, 0xA3 }; // this goes bang unrightully, it must be left alone!!!
        byte[] bytes1 = new byte[] { 0x20, 0x23, 0xE2 }; // this is correctly left alone
    }

}
