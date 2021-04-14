using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {

    public class UnitClauseAppearances {
        public int Index;
        public int Value = 0;
        public int Sign => Value > 0 ? 1 : -1;
        public UnitClauseAppearances(int variable, int value) {
            Index = variable;
            this.Value = value;
        }
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

        private bool DPLL() {

            Console.WriteLine("DPLL");
            PrintState();

            Checkpoint();
            RunUnitPropagation();

            if (CheckUnsatisfiability()) {
                Console.WriteLine("UNSATISFIABLE, returning");
                PrintState();
                Backtrack();
                return false;
            }
            if (CheckSatisfiability()) {
                Console.WriteLine("SATISFIABLE, returning");
                PrintState();
                // forget backtrack history and just return 
                variables.Forget();
                return true;
            }
            

            int literal = SelectLiteral();
            Console.WriteLine("Branching on: " + literal);

            // Assign literal = 1

            bool result = ContinueDPLL(literal, 1) || ContinueDPLL(literal, -1);

            // Finish
            Backtrack();
            return result;
        }
        private bool ContinueDPLL(int literal, int assignment) {
            Checkpoint();
            clauses.AddClause(literal * assignment);
            Console.WriteLine($"Branching on {literal} with value = {assignment}");
            bool result = DPLL();
            Backtrack();
            return result;
        }
        #endregion
  
        #region satisfiability
        private bool CheckSatisfiability() {
            if (clauses.ClauseCount == 0) {
                Console.WriteLine("NO CLAUSES");
                return true;
            }
            for (int i = 0; i < clauses.List.Count; i++) {
                var clause = clauses.List[i];
                if (clause == null) {
                    continue;
                }
                Console.WriteLine("Checking clause:");
                Program.PrintList(clause);

                // Check if any clause is UNSAT
                if (!ClauseIsSatisfied(clause)) {
                    Console.WriteLine("Was not satisfied");
                    PrintState();
                    return false;
                } else {
                    Console.WriteLine("Was satisfied");
                }
            }

            return true;
            // Contains no clauses
            Console.WriteLine("Check SAT, clauses: " + clauses.List.Count);
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

        private void UnitPropagateOnNew(UnitClauseAppearances unit) {

            Console.WriteLine("Unit propagating on: " + unit.Index);
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
            Console.WriteLine("FINISH PROPAGATE ON: " + unit.Index);
            PrintState();
        }

        private UnitClauseAppearances GetVariableToPropagate() {
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
                        UnitClauseAppearances u = new UnitClauseAppearances(index, value);
                        return u;
                    }
                }
            }
            return null;
        }

        #endregion

        #region heuristic
        private int SelectLiteral() {
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
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            variables.CreateCheckpoint();
            clauses.CreateCheckpoint();
        }
        private void Backtrack() {
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            variables.RevertToLastCheckpoint();
            clauses.RevertToLastCheckpoint();
        }

        public void PrintState() {
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
                for (int i = 1; i < VariableCount + 1; i++) {
                    b.AppendLine($"Literal {i} = {variables[i]}");
                }
            }
            b.AppendLine("-----------------------------------");

            Console.WriteLine(b.ToString());
        }
        #endregion

    }
}
