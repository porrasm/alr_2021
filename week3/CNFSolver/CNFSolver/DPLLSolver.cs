using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {

    public class UnitClause {
        public int Index;
        public int Value = 0;
        public int Sign => Value > 0 ? 1 : -1;
        public UnitClause(int variable, int value) {
            Index = variable;
            this.Value = value;
        }
    }

    public class BranchingHeuristic {

    }

    public class DPLLSolver : SatSolver {

        #region fields
        private RList<int> variables;
        private Clauses clauses;

        public override List<int> GetVariableAssignments => variables.List;

        private List<int>[] variableAppearances;
        #endregion

        #region solve
        public override bool Solve() {
            variables = new RList<int>(new List<int>(new int[VariableCount + 1]));
            clauses = new Clauses(clauseList);
            SetVariableAppearances();

            return DPLLIterative();
            return DPLL();
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

            while (true) {

                PrintState();

                Log("Clause stack: " + clauseStack.Count);
                if (clauseStack.Count > 0) {
                    Log("Applying branch: " + clauseStack.Peek());
                    Checkpoint();
                    clauses.AddClause(clauseStack.Pop().Clause);
                }
                Log("DPLL Iteration CLAUSE COUNT: " + clauses.ClauseCount);

                RunUnitPropagation();

                if (CheckUnsatisfiability()) {
                    Log("UNSATISFIABLE, returning");
                    PrintState();

                    if (clauseStack.Count == 0) {
                        return false;
                    }

                    Backtrack(clauseStack.Peek().RevertLevel);

                    

                    // ?
                    continue;
                }
                if (CheckSatisfiability()) {
                    Log("SATISFIABLE, returning");
                    PrintState();
                    // forget backtrack history and just return 
                    variables.Forget();
                    clauses.Forget();
                    return true;
                }

                int literal = SelectLiteral();
                Log("Branching on: " + literal);
                if (literal == 0) {
                    throw new Exception("Literal was 0");
                }
                clauseStack.Push(new ClauseBranch(-literal, variables.CheckPointLevel));
                clauseStack.Push(new ClauseBranch(literal, variables.CheckPointLevel));
            }
        }

        private bool DPLL() {

            Log("DPLL Iteration CLAUSE COUNT: " + clauses.ClauseCount);
            PrintState();

            Checkpoint();
            RunUnitPropagation();

            if (CheckUnsatisfiability()) {
                Log("UNSATISFIABLE, returning");
                PrintState();
                Backtrack();
                return false;
            }
            if (CheckSatisfiability()) {
                Log("SATISFIABLE, returning");
                PrintState();
                // forget backtrack history and just return 
                variables.Forget();
                return true;
            }


            int literal = SelectLiteral();
            Log("Branching on: " + literal);

            // Assign literal = 1

            bool result = ContinueDPLL(literal, 1) || ContinueDPLL(literal, -1);

            // Finish
            Backtrack();
            return result;
        }
        private bool ContinueDPLL(int literal, int assignment) {
            Checkpoint();
            clauses.AddClause(literal * assignment);
            Log($"Branching on {literal} with value = {assignment}");
            bool result = DPLL();
            Backtrack();
            return result;
        }
        #endregion

        #region satisfiability
        private bool CheckSatisfiability() {
            if (clauses.ClauseCount == 0) {
                Log("NO CLAUSES");
                return true;
            }
            for (int i = 0; i < clauses.List.Count; i++) {
                var clause = clauses.List[i];
                if (clause == null) {
                    continue;
                }
                Log("Checking clause:");
                Program.PrintList(clause);

                // Check if any clause is UNSAT
                if (!ClauseIsSatisfied(clause)) {
                    Log("Was not satisfied");
                    PrintState();
                    return false;
                } else {
                    Log("Was satisfied");
                }
            }

            return true;
            // Contains no clauses
            Log("Check SAT, clauses: " + clauses.List.Count);
            return clauses.List.Count == 0;
        }
        private bool ClauseIsSatisfied(List<int> clause) {
            foreach (var l in clause) {
                int index = l > 0 ? l : -l;
                if (variables[index] == 0) {
                    return false;
                }

                int assignment = l > 0 ? 1 : -1;

                // Check if some variable assignment is true
                if (variables[index] == assignment) {
                    return true;
                }
            }

            return false;
        }

        private bool CheckUnsatisfiability() {
            for (int i = 0; i < clauses.List.Count; i++) {
                if (clauses.List[i] == null) {
                    continue;
                }
                // Contains emtpy clause
                if (clauses.List[i].Count == 0) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region new unit
        private void RunUnitPropagation() {
            while (GetVariableToPropagate() is var unit && unit != null) {
                UnitPropagateOnNew(unit);
            }
        }

        private void UnitPropagateOnNew(UnitClause unit) {

            Log("Unit propagating on: " + unit.Index);
            var clausesWithLit = variableAppearances[unit.Index];
            variables[unit.Index] = unit.Sign;

            foreach (int clauseIndex in variableAppearances[unit.Index]) {
                var clause = clauses.List[clauseIndex];
                if (clause == null) {
                    continue;
                }

                for (int j = 0; j < clause.Count; j++) {
                    if (unit.Value == clause[j] && clause.Count > 1) {
                        clauses.NullifyClause(clauseIndex);
                        break;
                    }
                    if (clauses[clauseIndex, j] == -unit.Value) {
                        clauses.RemoveVarAt(clauseIndex, j);
                        j--;
                    }
                }
            }
            Log("FINISH PROPAGATE ON: " + unit.Index);
            PrintState();
        }

        private UnitClause GetVariableToPropagate() {
            int iMax = clauses.List.Count;
            for (int cl = 0; cl < iMax; cl++) {
                if (clauses.List[cl] == null) {
                    continue;
                }
                int jMax = clauses.List[cl].Count;
                for (int li = 0; li < jMax; li++) {
                    int value = clauses[cl, li];
                    int index = GetVar(value);

                    if (variables[index] == 0 && clauses.List[cl].Count == 1) {
                        UnitClause u = new UnitClause(index, value);
                        return u;
                    }
                }
            }
            return null;
        }

        #endregion

        #region heuristic
        private int SelectLiteral() {

            //Dictionary<int, List<int>> clausesByLen = new Dictionary<int, List<int>>();

            //int minClauseLength = int.MaxValue;
            //for (int i = 0; i < clauses.List.Count; i++) {
            //    if (clauses.List[i] == null) {
            //        continue;
            //    }
            //    int count = clauses.List[i].Count;
            //    if (count > 1 && count < minClauseLength) {
            //        minClauseLength = count;
            //    }
            //}
            //Console.WriteLine("Min length: " + minClauseLength);



            // most common literal
            Dictionary<int, int> literalCounts = new Dictionary<int, int>();
            for (int i = 0; i < clauses.List.Count; i++) {
                if (clauses.List[i] == null) {
                    continue;
                }
                //if (clauses.List[i].Count != minClauseLength) {
                //    continue;
                //}
                foreach (int literal in clauses.List[i]) {
                    int var = GetVar(literal);
                    if (!literalCounts.ContainsKey(var)) {
                        literalCounts.Add(var, 1);
                    } else {
                        literalCounts[var]++;
                    }
                }
            }

            KeyValuePair<int, int> max = default;
            foreach (var pair in literalCounts) {
                if (pair.Value > max.Value) {
                    max = pair;
                }
            }
            if (max.Key == 0) {
                throw new Exception("No literal found");
            }
            return max.Key;

            // select first unassigned
            for (int i = 1; i < VariableCount + 1; i++) {
                if (variables[i] == 0) {
                    return i;
                }
            }
            return 0;
        }
        #endregion

        #region utility
        private void Checkpoint() {
            Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            variables.CreateCheckpoint();
            clauses.CreateCheckpoint();
        }
        private void Backtrack() {
            Backtrack(variables.CheckPointLevel - 1);
        }
        private void Backtrack(int targeLevel) {
            Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            variables.RevertToLevel(targeLevel);
            clauses.RevertToLevel(targeLevel);
        }

        public void PrintState() {
            return;
            StringBuilder b = new StringBuilder();
            b.AppendLine("-----------------------------------");
            b.AppendLine("Clauses:");
            foreach (var clause in clauseList) {
                if (clause == null) {
                    b.Append("EMPTY");
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

            Log(b.ToString());
        }
        #endregion

    }
}
