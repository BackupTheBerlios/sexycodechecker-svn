using System;

namespace Cluefultoys.Sexycodechecker {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
    public class Author : System.Attribute {
        private string name;
        public double version;

        public Author(string name) {
            this.name = name;
            version = 1.0;
        }
    }


    public class Something {

        [Author("limaCAT")] private int value;

        public Something() {
            value = 0;
        }

    }

}
