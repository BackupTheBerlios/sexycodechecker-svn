using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class Rule04 {

        // This is ok;
        public Rule04()
            : base() {
        }

        // This is ok too;
        public Results CheckFile(string filename, object blam) {
            Results results = new Results(filename);
            return results;
        }
    }

}