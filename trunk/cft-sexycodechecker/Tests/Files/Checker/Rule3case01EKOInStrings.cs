using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class WhateverElse01C {

        public Results CheckFile(string filename) {
            Results results = new Results(filename);
            // parens are in string comments
            string theScc = "for (int i = 0"; string sucks = "i < 2; i++)";
            Console.Out.WriteLine("hello!");
            return results;
        }

    }

}