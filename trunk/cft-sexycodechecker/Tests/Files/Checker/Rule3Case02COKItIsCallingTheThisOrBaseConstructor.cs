using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class WhateverElse02C {

        // It is calling the this constructor
        public WhateverElse02C()
            : this("shrewt") {
        }

        // It is calling the base constructor
        public WhateverElse02C(string message)
            : base() {
        }

        public Results CheckFile(string filename, object blam) {
            Results results = new Results(filename);
            return results;
        }
    }

}