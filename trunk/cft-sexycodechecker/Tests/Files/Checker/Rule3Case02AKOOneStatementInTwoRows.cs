﻿using System;
using System.Collections.Generic;
using System.IO;

using Cluefultoys.Sexycodechecker;
namespace Cluefultoys.Whatever {

    public class WhateverElse01A {

        public Results CheckFile(string filename) {
            // Line does not end in ";", "{", "," or "}"
            Results results = 
                new Results(filename);
            return results;
        }
    }

}