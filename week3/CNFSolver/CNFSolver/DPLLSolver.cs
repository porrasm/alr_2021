using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {

    public class VariableAppearance {
        public int Variable;
        public int Count;

        public VariableAppearance(int variable, int count) {
            Variable = variable;
            Count = count;
        }
    }

    public class DPLLSolver : SatSolver {

        #region fields
        private RList<int> variables;
        private Clauses clauses;
        private BranchingHeuristic heuristic;

        private int iterations;

        private bool unsatisfiable;
        private bool satisfiable;

        public override List<int> GetVariableAssignments => variables.List;

        private List<int>[] variableAppearances;
        #endregion

        #region solve
        public override bool Solve() {

            Console.WriteLine("Solving with: " + VariableCount);

            variables = new RList<int>(new List<int>(new int[VariableCount + 1]));
            clauses = new Clauses(clauseList);
            heuristic = new BranchingHeuristic(clauses, variables);

            SetVariableAppearances();

            bool res = DPLLIterative();
            Console.WriteLine("Completed in " + iterations + " iterations");
            return res;
            //return DPLL();
        }
        private void SetVariableAppearances() {
            variableAppearances = new List<int>[VariableCount + 1];
            for (int i = 1; i < VariableCount + 1; i++) {
                variableAppearances[i] = new List<int>();
            }

            for (int i = 0; i < clauses.List.Count; i++) {
                for (int j = 0; j < clauses.List[i].Count; j++) {
                    int val = GetVar(clauses[i, j]);
                    variableAppearances[val].Add(i);
                }
            }
        }

        private struct ClauseBranch {
            public int Clause;
            public int RevertLevel;

            public ClauseBranch(int clause, int revertLevel) {
                Clause = clause;
                RevertLevel = revertLevel;
            }
        }
        private bool DPLLIterative() {

            Stack<ClauseBranch> clauseStack = new Stack<ClauseBranch>();
            RunInitialUnitPropagation();

            while (true) {
                iterations++;

                Stopwatch iteration = Stopwatch.StartNew();

                PrintState();

                Log("Clause stack: " + clauseStack.Count);
                if (clauseStack.Count > 0) {
                    Log("Applying branch: " + clauseStack.Peek().Clause);
                    Checkpoint();
                    ClauseBranch clause = clauseStack.Pop();
                    clauses.AddClause(clause.Clause);
                    PrepareNewUnitClause(new UnitClause(clause.Clause));
                }
                Log("DPLL Iteration CLAUSE COUNT: " + clauses.ClauseCount);

                Stopwatch propagation = Stopwatch.StartNew();

                propagation.Stop();

                if (unsatisfiable) {
                    //if (unsatisfiable) {
                    Log("BRANCH UNSATISFIABLE, continuing");
                    PrintState();

                    if (clauseStack.Count == 0) {
                        PrintState(true);
                        Console.WriteLine("NO MORE CLAUSES LEFT; UNSATISFIABLE");
                        return false;
                    }

                    Backtrack(clauseStack.Peek().RevertLevel);
                    // ?
                    continue;
                }

                if (satisfiable) {
                //if (CheckSatisfiability()) {
                    PrintState(true);
                    Log("SATISFIABLE, returning");
                    // forget backtrack history and just return 
                    variables.Forget();
                    clauses.Forget();
                    return true;
                }

                int literal = heuristic.GetVariableToBranchOn();
                //int literal = SelectLiteral();
                Log("Branching on: " + literal);
                if (literal == 0) {
                    Log("--------------------------------------------No var found");
                    PrintState(false);
                    if (clauseStack.Count > 0) {
                        Backtrack(clauseStack.Peek().RevertLevel);
                    } else {
                        Backtrack();
                    }
                    continue;
                    throw new Exception("Cant branch on 0");
                }

                clauseStack.Push(new ClauseBranch(literal, variables.CheckPointLevel));
                clauseStack.Push(new ClauseBranch(-literal, variables.CheckPointLevel));
                iteration.Stop();
                //if (propagation.Elapsed.Ticks > 5) {
                    //Console.WriteLine("Iteration time: " + iteration.Elapsed.Ticks);
                    //Console.WriteLine("Propagation time: " + propagation.Elapsed.Ticks);
                    //Console.WriteLine("Percentage: " + (1.0 * propagation.Elapsed.Ticks / iteration.Elapsed.Ticks));
                //}

            }
        }
        #endregion

        #region satisfiability
        private bool CheckPolarity() {
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
        #endregion


        #region new unit
        private void RunInitialUnitPropagation() {
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

        bool areNonUnitClauses;
        private void PrepareNewUnitClause(UnitClause unit) {
            unsatisfiable = false;
            areNonUnitClauses = false;
            UnitPropagate(unit);
            satisfiable = !areNonUnitClauses && CheckPolarity();
        }
        private void UnitPropagate(UnitClause unit) {
            List<UnitClause> newUnitClauses = GetNewUnitClausesFromPropagate(unit);
            if (newUnitClauses == null) {
                return;
            }
            foreach (UnitClause clause in newUnitClauses) {
                UnitPropagate(clause);
            }
        }
        private List<UnitClause> GetNewUnitClausesFromPropagate(UnitClause unit) {
            if (variables[unit.Index] != 0) {
                //Console.WriteLine("Already propagated: " + unit.Value);
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
                    areNonUnitClauses = true;
                }
            }

            // Console.WriteLine($"Ran unit propagation on {unit.Value} and redundant: {redundant}, removed vars: {removedVar}");

            return newUnits;
        }
        #endregion

        #region utility
        private void Checkpoint() {
            Log("Current check level: " + variables.CheckPointLevel);
            Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> to " + (variables.CheckPointLevel + 1));
            //Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>> Checkpoint level: " + (variables.CheckPointLevel + 1));

            variables.CreateCheckpoint();
            clauses.CreateCheckpoint();
            Log("After check level: " + variables.CheckPointLevel);
        }
        private void Backtrack() {
            Backtrack(variables.CheckPointLevel - 1);
        }
        private void Backtrack(int targeLevel) {
            // targeLevel = variables.CheckPointLevel - 1;
            Log("Current check level: " + variables.CheckPointLevel);
            Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< to " + targeLevel);
            variables.RevertToLevel(targeLevel);
            clauses.RevertToLevel(targeLevel);
            //Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<< Checkpoint level: target: " + targeLevel + ", actual: " + variables.CheckPointLevel);
            Log("After check level: " + variables.CheckPointLevel);
        }

        public void PrintState(bool force = false) {
            if (!force) {
                //return;
            }

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

            if (force) {
                Console.WriteLine(b.ToString());
            } else {
                Log(b.ToString());
            }
        }
        #endregion

    }

    public class UnitClause {
        public int Index;
        public int Value = 0;
        public int Sign => Value > 0 ? 1 : -1;
        public UnitClause(int value) {
            this.Value = value;
            this.Index = SatSolver.GetVar(value);
        }
        public UnitClause(int variable, int value) {
            Index = variable;
            this.Value = value;
        }
    }

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

            //for (int i = 0; i < variables.List.Count; i++) {
            //    var app = appearances[i];
            //    if (variables[app.Variable] == 0) {
            //        return app.Variable;
            //    }
            //}

            //return 0;
            // select first unassigned
            //for (int i = 1; i < variables.List.Count; i++) {
            //    if (variables[i] == 0) {
            //        return i;
            //    }
            //}
            //return 0;

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
