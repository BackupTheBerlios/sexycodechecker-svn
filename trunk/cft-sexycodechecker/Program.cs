using System;

namespace Cluefultoys.Sexycodechecker {
    class Program {
        static void Main(string[] arguments) {
            int iParam = 0;
            foreach (string argument in arguments) {
                Console.Out.WriteLine("param " + iParam + ": " + argument);
                iParam++;
            }
        }
    }
}