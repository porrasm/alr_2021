using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class Clauses {
        #region fields
        public List<List<int>> List { get; private set; }
        private Stack<Action> counterOperations;
        private Stack<int> checkpoints;
        public int ClauseCount { get; private set; }
        public int CheckPointLevel => checkpoints.Count;
        #endregion

        public Clauses(List<List<int>> initialState) {
            this.List = initialState;
            counterOperations = new Stack<Action>();
            checkpoints = new Stack<int>();
            ClauseCount = initialState.Count;
        }

        public void Forget() {
            counterOperations.Clear();
            checkpoints.Clear();
        }
        public void CreateCheckpoint() {
            checkpoints.Push(counterOperations.Count);
        }
        public void Reset() {
            checkpoints.Clear();
            RevertToLastCheckpoint();
        }
        public void RevertToLevel(int level) {
            int cp = 0;
            if (level >= CheckPointLevel) {
                throw new Exception("Cant revert to current level or up");
            }
            while (CheckPointLevel > level && checkpoints.Count > 0) {
                cp = checkpoints.Pop();
            }
            int revertCount = counterOperations.Count - cp;
            for (int i = 0; i < revertCount; i++) {
                counterOperations.Pop()();
            }
        }
        public void RevertToLastCheckpoint() {
            RevertToLevel(CheckPointLevel - 1);
        }

        #region modifiers
        public int this[int i, int j] {
            get {
                return List[i][j];
            }
            set {
                CounterSetVariable(i, j, List[i][j]);
                List[i][j] = value;
            }
        }

        public void AddVar(int i, int var) {
            CounterAddVar(i);
            List[i].Add(var);
        }
        public void RemoveVarAt(int i, int j) {
            CounterRemoveVarAt(i, j, List[i][j]);
            List[i].RemoveAt(j);
        }

        public void AddClause(params int[] clause) {
            ClauseCount++;
            AddClause(clause.ToList());
        }
        public void AddClause(List<int> clause) {
            CounterAddClause();
            List.Add(clause);
        }
        //public void RemoveClause(int index) {
        //    CounterRemoveClause(index);
        //    List.RemoveAt(index);
        //}
        public void NullifyClause(int index) {
            CounterSetClause(index);
            ClauseCount--;
            List[index] = null;
        }
        #endregion

        public int GetClauseSize(int index) {
            return List[index].Count;
        }

        #region counters
        private void CounterAddClause() {
            counterOperations.Push(() => List.RemoveAt(List.Count - 1));
        }
        private void CounterRemoveClause(int i) {
            List<int> toRemove = List[i];
            counterOperations.Push(() => List.Insert(i, toRemove));
        }
        private void CounterSetClause(int index) {
            List<int> replacedVal = List[index];
            counterOperations.Push(() => {
                List[index] = replacedVal;
                ClauseCount++;
            });
        }

        private void CounterSetVariable(int i, int j, int v) {
            counterOperations.Push(() => List[i][j] = v);
        }
        private void CounterAddVar(int i) {
            counterOperations.Push(() => List[i].RemoveAt(List[i].Count - 1));
        }
        private void CounterRemoveVarAt(int i, int j, int value) {
            counterOperations.Push(() => List[i].Insert(j, value));
        }
        #endregion

        public override string ToString() {
            StringBuilder b = new StringBuilder();
            b.Append("----------------------------------------\n");
            foreach (var clause in List) {
                foreach (int val in clause) {
                    b.Append(val + ", ");
                }
                b.Append("\n");
            }
            b.Append("----------------------------------------\n");
            return b.ToString();
        }
    }
}
