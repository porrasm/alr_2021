using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class RList<T> {

        #region fields
        public List<T> List { get; private set; }
        private Stack<Action> counterOperations;
        private Stack<int> checkpoints;
        #endregion

        public RList(List<T> initialState) {
            this.List = initialState;
            counterOperations = new Stack<Action>();
            checkpoints = new Stack<int>();
        }

        public void Forget() {
            counterOperations.Clear();
            checkpoints.Clear();
        }
        public void CreateCheckpoint() {
            if (counterOperations.Count == 0) {
                return;
            }
            checkpoints.Push(counterOperations.Count);
        }
        public void Reset() {
            checkpoints.Clear();
            RevertToLastCheckpoint();
        }
        public void RevertToLastCheckpoint() {
            int cp = checkpoints.Count == 0 ? 0 : checkpoints.Pop();
            int revertCount = counterOperations.Count - cp;
            for (int i = 0; i < revertCount; i++) {
                counterOperations.Pop()();
            }
        }

        #region modifiers
        public T this[int i] {
            get {
                return List[i];
            }
            set {
                CounterSet(i, List[i]);
                this.List[i] = value;
            }
        }

        public void Add(T value) {
            CounterAdd();
            List.Add(value);
        }
        #endregion

        #region counters
        private void CounterAdd() {
            counterOperations.Push(() => List.RemoveAt(List.Count - 1));
        }
        private void CounterSet(int i, T v) {
            counterOperations.Push(() => List[i] = v);
        }
        #endregion
    }
}
