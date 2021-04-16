using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class RList<T> {

        #region fields
        private struct CounterOp {
            public byte Type;
            public int Index;
            public T Value;

            public CounterOp(byte type, int index, T value) {
                Type = type;
                Index = index;
                Value = value;
            }
        }

        public List<T> List { get; private set; }
        //private Stack<Action> counterOperations;
        private Stack<CounterOp> counterOperations;

        private Stack<int> checkpoints;
        public int CheckPointLevel => checkpoints.Count;
        #endregion

        public RList(List<T> initialState) {
            this.List = initialState;
            //counterOperations = new Stack<Action>();
            counterOperations = new Stack<CounterOp>();
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
        public void RevertToLevel(int level) {
            int cp = 0;
            if (level != 0 && level >= CheckPointLevel) {
                throw new Exception("Cant revert to current level or up, current level: " + CheckPointLevel + ", target level: " + level);
            }
            while (CheckPointLevel > level && checkpoints.Count > 0) {
                cp = checkpoints.Pop();
            }
            int revertCount = counterOperations.Count - cp;
            for (int i = 0; i < revertCount; i++) {
                //counterOperations.Pop()();
                CounterOperation(counterOperations.Pop());
            }
        }
        public void RevertToLastCheckpoint() {
            RevertToLevel(CheckPointLevel - 1);
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
        public void RemoveAt(int i) {
            CounterRemoveAt(i, List[i]);
            List.RemoveAt(i);
        }
        #endregion

        #region counters
        private void CounterAdd() {
            // counterOperations.Push(() => List.RemoveAt(List.Count - 1));
            counterOperations.Push(new CounterOp(0, List.Count - 1, default));
        }
        private void CounterSet(int i, T v) {
            // counterOperations.Push(() => List[i] = v);
            counterOperations.Push(new CounterOp(1, i, v));
        }
        private void CounterRemoveAt(int i, T v) {
            // counterOperations.Push(() => List[i] = v);
            counterOperations.Push(new CounterOp(2, i, v));
        }

        private void CounterOperation(CounterOp op) {
            if (op.Type == 0) {
                List.RemoveAt(op.Index);
            } else if (op.Type == 1) {
                List[op.Index] = op.Value;
            } else {
                List.Insert(op.Index, op.Value);
            }
        }
        #endregion
    }
}
