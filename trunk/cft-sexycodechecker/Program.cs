/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System;

namespace Cluefultoys.Sexycodechecker {
    class Program {
        static void Main(string[] arguments) {
            int iParam = 0;
            foreach (string argument in arguments) {
                Console.Out.WriteLine("param " + iParam + ": " + argument);
                iParam++;
            }
            
            
            Checker checker = new Checker();
            checker.CheckFile("C:/Program.cs");
        }
    }
}
