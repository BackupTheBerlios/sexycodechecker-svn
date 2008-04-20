using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class WhateverElse01B {

        public Results CheckFile(string filename) {
            // this will fail as it has two ";"
            Results results = new Results(filename); return results;
        }

    }

}