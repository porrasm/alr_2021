using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class BranchingHeuristic {
        private struct Key : IEquatable<Key> {
            public int ClauseLength;
            public int Variable;

            public Key(int clauseLength, int variable) {
                ClauseLength = clauseLength;
                Variable = variable;
            }

            public override bool Equals(object obj) {
                return obj is Key key && Equals(key);
            }

            public bool Equals(Key other) {
                return Variable == other.Variable;
            }

            public override int GetHashCode() {
                return 410573293 + Variable.GetHashCode();
            }

            public static bool operator ==(Key left, Key right) {
                return left.Equals(right);
            }

            public static bool operator !=(Key left, Key right) {
                return !(left == right);
            }
        }
        private List<VariableAppearance> appearances;

        private Clauses clauses;
        private RList<int> variables;

        Dictionary<Key, int> varCount = new Dictionary<Key, int>();

        public BranchingHeuristic(Clauses clauses, RList<int> variables) {
            this.clauses = clauses;
            this.variables = variables;
            this.appearances = new List<VariableAppearance>();
            CalculateAppearances();
        }

        private void CalculateAppearances() {

            for (int i = 0; i < variables.List.Count; i++) {
                appearances.Add(new VariableAppearance(i, 0));
            }

            foreach (var clause in clauses.List) {
                foreach (int var in clause) {
                    int index = SatSolver.GetVar(var);
                    appearances[index].Count++;
                }
            }

            appearances = appearances.OrderBy(o => -o.Variable).ToList();
        }

        public List<int> GetBranchingOrder() {
            return appearances.Select(o => o.Variable).ToList();
        }

        public int GetVariableToBranchOn() {
            // Random
            //var rnd = new System.Random();
            //while (true) {
            //    int index = rnd.Next(1, variables.List.Count);
            //    if (variables[index] == 0) {
            //        return index;
            //    }
            //}

            // Order at start and use that
            //for (int i = 0; i < variables.List.Count; i++) {
            //    var app = appearances[i];
            //    if (variables[app.Variable] == 0) {
            //        return app.Variable;
            //    }
            //}

            //return 0;

            // Select first unassigned
            //for (int i = 1; i < variables.List.Count; i++) {
            //    if (variables[i] == 0) {
            //        return i;
            //    }
            //}
            //return 0;

            // Select most common
            //int[] counts = new int[variables.List.Count];

            //foreach (var clause in clauses.List) {
            //    if (clause == null) {
            //        continue;
            //    }
            //    if (clause.Count > 1) {
            //        foreach (int varValue in clause) {
            //            int ind = SatSolver.GetVar(varValue);
            //            counts[ind]++;
            //        }
            //    }
            //}

            //int maxIndex = 0;
            //int maxVal = 0;

            //for (int i = 1; i < counts.Length; i++) {
            //    if (counts[i] > maxVal) {
            //        maxVal = counts[i];
            //        maxIndex = i;
            //    }
            //}

            //return maxIndex;

            InitVarCounts();

            int var = 0;
            int maxCount = 0;

            foreach (var pair in varCount) {
                if (pair.Value > maxCount) {
                    maxCount = pair.Value;
                    var = pair.Key.Variable;
                }
            }

            return var;
        }

        private void InitVarCounts() {
            varCount.Clear();
            for (int c = 0; c < clauses.List.Count; c++) {
                if (clauses.List[c] == null) {
                    continue;
                }

                int len = clauses.List[c].Count;
                if (len < 2) {
                    continue;
                }

                foreach (int variable in clauses.List[c]) {

                    Key key = new Key(len, SatSolver.GetVar(variable));

                    if (variables[key.Variable] != 0) {
                        continue;
                    }

                    if (varCount.ContainsKey(key)) {
                        varCount[key]++;
                    } else {
                        varCount.Add(key, 1);
                    }
                }
            }
        }
    }
}
