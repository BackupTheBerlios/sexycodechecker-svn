using System;
using System.Collections.Generic;

namespace Cluefultoys.Sexycodechecker {

    public class Handler<Type> where Type : IComparable<Type> {

        public delegate void Executor();

        public delegate bool Check(Type target);

        private Type toHandle;

        private Check Condition;

        private Executor Execute;

        private bool stop;

        public Handler(Executor DefaultExecute, bool stop) {
            this.Condition = IsDefault;
            this.Execute = DefaultExecute;
            this.stop = stop;
        }

        public Handler(Check Condition, Executor Execute, bool stop) {
            this.Condition = Condition;
            this.Execute = Execute;
            this.stop = stop;
        }

        public Handler(Check Condition) {
            this.Condition = Condition;
            this.Execute = DefaultExecute;
            this.stop = true;
        }

        public Handler(Type targetToHandle, Executor Execute, bool stop) {
            this.toHandle = targetToHandle;
            this.Condition = SimpleCheck;
            this.Execute = Execute;
            this.stop = stop;
        }

        private bool SimpleCheck(Type target) {
            return toHandle.CompareTo(target) == 0;
        }

        private bool IsDefault(Type target) {
            return true;
        }

        public bool Handle(Type target) {
            if (Condition(target)) {
                Execute();
                return stop;
            }
            return false;
        }

        private void DefaultExecute() {
            // No-operation executor, for stop-only conditions.
        }
    }

    public class Chain<Type> where Type : IComparable<Type> {

        private List<Handler<Type>> Handlers = new List<Handler<Type>>();

        public void Add(Handler<Type> handler) {
            Handlers.Add(handler);
        }

        public void Execute(Type target) {
            bool stop = false;
            IEnumerator<Handler<Type>> enumerator = Handlers.GetEnumerator();
            while (!stop && enumerator.MoveNext()) {
                stop = enumerator.Current.Handle(target);
            }
        }

    }

}