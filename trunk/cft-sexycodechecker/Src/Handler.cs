/* 
 * Sexy Code Checker: An Implementation of the 700x128 Manifesto
 * By Davide Inglima, 2008.
 * 
 * This source code is released under the MIT License
 * See Copying.txt for the full details.
 */
using System;
using System.Collections.Generic;

namespace Cluefultoys.Handlers {

    public delegate void Executor<T>(T target);

    public delegate bool Check<T>(T target);

    public class Handler<T> where T : IComparable<T> {

        private T toHandle;

        private Check<T> condition;

        private Executor<T> toExecute;

        private bool stop;

        public Handler(Executor<T> executeAsDefault, bool stop) {
            this.condition = DefaultCondition;
            this.toExecute = executeAsDefault;
            this.stop = stop;
        }

        public Handler(Check<T> condition, Executor<T> toExecute, bool stop) {
            this.condition = condition;
            this.toExecute = toExecute;
            this.stop = stop;
        }

        public Handler(Check<T> condition) {
            this.condition = condition;
            this.toExecute = DefaultExecution;
            this.stop = true;
        }

        public Handler(T targetToHandle, Executor<T> toExecute, bool stop) {
            this.toHandle = targetToHandle;
            this.condition = SimpleCheck;
            this.toExecute = toExecute;
            this.stop = stop;
        }

        private bool SimpleCheck(T target) {
            return toHandle.CompareTo(target) == 0;
        }

        private bool DefaultCondition(T target) {
            return true;
        }

        public bool Handle(T target) {
            if (condition(target)) {
                toExecute(target);
                return stop;
            }
            return false;
        }

        private void DefaultExecution(T target) {
            // No-operation executor, for stop-only conditions.
        }
    }

    public class Chain<T> where T : IComparable<T> {

        private List<Handler<T>> Handlers = new List<Handler<T>>();

        public void Add(Handler<T> handler) {
            Handlers.Add(handler);
        }

        public void Execute(T target) {
            bool stop = false;
            IEnumerator<Handler<T>> enumerator = Handlers.GetEnumerator();
            while (!stop && enumerator.MoveNext()) {
                stop = enumerator.Current.Handle(target);
            }
        }

    }

}
