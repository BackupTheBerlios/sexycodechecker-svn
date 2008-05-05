using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class WhateverElse01C {

        public Results CheckFile(string filename) {
            Results results = new Results(filename);
            // this will be ok, since the ";" are enclosed in parens
            for (int index = 0; index < 2; index++) {
                Console.Out.WriteLine("hello!");
            }
            return results;
        }

    }

}