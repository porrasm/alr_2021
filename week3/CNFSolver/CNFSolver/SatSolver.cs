using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public abstract class SatSolver {

        #region fields
        protected int VariableCount { get; private set; }
        protected List<List<int>> clauseList;

        public abstract List<int> GetVariableAssignments { get; }

        protected RList<int> variables;
        protected Clauses clauses;
        protected BranchingHeuristic heuristic;

        protected List<int>[] variableAppearances;

        protected bool unsatisfiable;
        protected bool satisfiable;
        protected bool existsNonUnitClauses;
        #endregion

        public void LoadProblem(string path) {
            clauseList = new List<List<int>>();

            string[] lines = File.ReadAllLines(path);

            int i;
            for (i = 0; i < lines.Length; i++) {
                if (lines[i].StartsWith("p ")) {
                    break;
                }
            }

            string[] opts = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            VariableCount = int.Parse(opts[2]);
            int clauseCount = int.Parse(opts[3]);
            i++;

            List<int> currentClause = new List<int>();
            for (; i < lines.Length; i++) {
                foreach (int val in lines[i]
                    .Trim()
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => int.Parse(o))) {
                    if (val == 0) {
                        clauseList.Add(currentClause);
                        currentClause = new List<int>();
                    } else {
                        currentClause.Add(val);
                        if (clauseList.Count == clauseCount) {
                            break;
                        }
                    }
                }
            }
        }

        protected void SetVariableAppearances() {
            variableAppearances = new List<int>[VariableCount + 1];
            for (int i = 1; i < VariableCount + 1; i++) {
                variableAppearances[i] = new List<int>();
            }

            for (int i = 0; i < clauseList.Count; i++) {
                for (int j = 0; j < clauseList[i].Count; j++) {
                    int val = GetVar(clauseList[i][j]);
                    variableAppearances[val].Add(i);
                }
            }
        }

        public virtual void Clear() {
            VariableCount = 0;
            clauseList = null;
        }

        public static int GetVar(int value) {
            return value > 0 ? value : -value;
        }

        public bool Solve() {
            variables = new RList<int>(new List<int>(new int[VariableCount + 1]));
            clauses = new Clauses(clauseList);
            heuristic = new BranchingHeuristic(clauses, variables);

            SetVariableAppearances();

            return SolveImplementation();
        }

        public abstract bool SolveImplementation();

        #region new unit
        protected void RunInitialUnitPropagation() {
            List<UnitClause> units = new List<UnitClause>();
            foreach (var clause in clauses.List) {
                if (clause == null) {
                    continue;
                }
                if (clause.Count == 1) {
                    units.Add(new UnitClause(clause[0]));
                }
            }
            foreach (UnitClause unitClause in units) {
                UnitPropagate(unitClause);
            }
        }

        protected void PrepareBranch(UnitClause unit) {
            unsatisfiable = false;
            existsNonUnitClauses = false;

            UnitPropagate(unit);

            if (unsatisfiable) {
                return;
            }
            satisfiable = !existsNonUnitClauses && CheckPolarity();
            if (satisfiable) {
                return;
            }
            unsatisfiable = AllAssigned();
        }
        protected void UnitPropagate(UnitClause unit) {
            List<UnitClause> newUnitClauses = GetNewUnitClausesFromPropagate(unit);
            if (newUnitClauses == null) {
                return;
            }
            foreach (UnitClause clause in newUnitClauses) {
                UnitPropagate(clause);
            }
        }
        protected List<UnitClause> GetNewUnitClausesFromPropagate(UnitClause unit) {
            if (variables[unit.Index] != 0) {
                return null;
            }

            // Get clauses in which variable appears in
            var clausesToScan = variableAppearances[unit.Index];
            // Assign necessary variable value
            variables[unit.Index] = unit.Sign;
            List<UnitClause> newUnits = new List<UnitClause>();

            int redundant = 0;
            int removedVar = 0;

            foreach (var clauseIndex in clausesToScan) {
                var clause = clauses.List[clauseIndex];
                if (clause == null) {
                    continue;
                }

                // Loop clause variables
                for (int j = 0; j < clause.Count; j++) {
                    if (unit.Value == clause[j] && clause.Count > 1) {

                        clauses.NullifyClause(clauseIndex);
                        redundant++;
                        break;
                    }
                    if (clauses[clauseIndex, j] == -unit.Value) {
                        clauses.RemoveVarAt(clauseIndex, j);
                        removedVar++;
                        j--;
                    }
                }

                clause = clauses.List[clauseIndex];
                if (clause == null) {
                    continue;
                }

                int newSize = clauses.GetClauseSize(clauseIndex);
                // Created new unit clause
                if (newSize == 1) {
                    newUnits.Add(new UnitClause(clauses[clauseIndex, 0]));
                } else if (newSize == 0) {
                    // We have propagated an empty clause
                    unsatisfiable = true;
                } else {
                    // Some clauses have more than 1 variable, uncertain satisfiability -> must search deeper
                    existsNonUnitClauses = true;
                }
            }

            // Console.WriteLine($"Ran unit propagation on {unit.Value} and redundant: {redundant}, removed vars: {removedVar}");

            return newUnits;
        }
        #endregion


        #region satisfiability
        protected bool CheckPolarity() {
            int[] polarities = new int[VariableCount + 1];

            bool polarityCheck = true;
            for (int i = 0; i < clauses.List.Count; i++) {
                if (clauses.List[i] == null) {
                    continue;
                }
                foreach (var literal in clauses.List[i]) {
                    int var = GetVar(literal);
                    int polarity = literal > 0 ? 1 : -1;
                    if (polarities[var] == 0) {
                        polarities[var] = polarity;
                        continue;
                    }
                    if (polarities[var] != polarity) {
                        polarityCheck = false;
                    }
                }
            }

            return polarityCheck;
        }

        protected bool AllAssigned() {
            for (int i = 1; i < variables.List.Count; i++) {
                if (variables[i] == 0) {
                    return false;
                }
            }
            return true;
        }
        #endregion


        #region utility
        protected void Checkpoint() {
            variables.CreateCheckpoint();
            clauses.CreateCheckpoint();
        }
        protected void Backtrack() {
            Backtrack(variables.CheckPointLevel - 1);
        }
        protected void Backtrack(int targeLevel) {
            variables.RevertToLevel(targeLevel);
            clauses.RevertToLevel(targeLevel);
        }

        public void PrintState() {

            StringBuilder b = new StringBuilder();
            b.AppendLine("-----------------------------------");
            b.AppendLine("Clauses:");
            foreach (var clause in clauseList) {
                if (clause == null) {
                    b.Append("REDUNDANT");
                } else if (clause.Count == 0) {
                    b.Append("CONFLICT");
                } else {
                    foreach (int var in clause) {
                        b.Append(var + " ");
                    }
                }

                b.Append("\n");
            }
            b.AppendLine();
            if (variables != null) {
                b.AppendLine("Literals:");
                for (int i = 1; i < variables.List.Count; i++) {
                    b.AppendLine($"Literal {i} = {variables[i]}");
                }
            }
            b.AppendLine("-----------------------------------");

            Console.WriteLine(b.ToString());
        }
        #endregion
    }
}
