using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class WhateverElse02C {

        public Results CheckFile(string filename, object blam) {
            // This comment shouldn't count against the limit of 20 lines
            Results results = new Results(filename);
            /*
             * Neither do this
             */
            if (true) { // nor this
                if (true) {
                    if (true) {
                        if (true) {
                            if (true) {
                                if (true) {
                                    if (true) {
                                        Console.Out.Write("I am going to say...");
                                        Console.Out.Write("WHOPEEE!!!");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }
    }

}