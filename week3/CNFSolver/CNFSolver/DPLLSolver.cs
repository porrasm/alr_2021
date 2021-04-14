using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {

    public class UnitClauseAppearances {
        public int Variable;
        public bool IsUnit = false;
        public int ClauseValue = 0;
        public List<int> Clauses = new List<int>();

        public UnitClauseAppearances(int variable) {
            Variable = variable;
        }
    }

    public class DPLLSolver : SatSolver {

        #region fields
        private RList<int> variables;
        private Clauses clauses;

        public override List<int> GetVariableAssignments => variables.List;

        private List<int>[] variableAppearances;
        #endregion

        public void PrintState() {
            StringBuilder b = new StringBuilder();
            b.AppendLine("-----------------------------------");
            b.AppendLine("Clauses:");
            foreach (var clause in clauseList) {
                foreach (int var in clause) {
                    b.Append(var + " ");
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

        public override bool Solve() {
            variables = new RList<int>(new List<int>(new int[VariableCount + 1]));
            clauses = new Clauses(clauseList);

            return DPLL();
        }
        private void SetVariableAppearances() {

        }

        private bool DPLL() {

            Console.WriteLine("DPLL");
            PrintState();

            Checkpoint();
            RunUnitPropagation();

            if (CheckUnsatisfiability()) {
                Console.WriteLine("UNSATISFIABLE, returning");
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
                if (variables[index] == assignment) {
                    return true;
                }
            }

            return false;
        }

        private bool CheckUnsatisfiability() {
            for (int i = 0; i < clauses.List.Count; i++) {
                // Contains emtpy clause
                if (clauses.List[i].Count == 0) {
                    return true;
                }
            }
            if (SelectLiteral() == 0) {
                return true;
            }
            return false;
        }

        #region new unit
        private void RunUnitPropagation() {
            while (GetVariableToPropagate() is var unit && unit != null) {

            }
            //while (true) {
            //    var varIndices = GetVariableIndices();
            //    UnitClauseAppearances unitClause = null;
            //    foreach (var pair in varIndices) {
            //        if (pair.Value.IsUnit && variables[pair.Value.Variable] == 0) {
            //            unitClause = pair.Value;
            //        }
            //    }

            //    if (unitClause == null) {
            //        break;
            //    }

            //    UnitPropagateOnNew(unitClause);
            //}
        }

        private void UnitPropagateOnNew(UnitClauseAppearances unit) {

            Console.WriteLine("Unit propagating on: " + unit.Variable);

            int varIndex = unit.Variable > 0 ? unit.Variable : -unit.Variable;
            // save result
            variables[varIndex] = unit.Variable > 0 ? 1 : -1;

            for (int i = 0; i < clauses.List.Count; i++) {
                for (int j = 0; j < clauses.List[i].Count; j++) {

                    if (unit.ClauseValue == clauses[i, j] && clauses.List[i].Count > 1) {
                        // Remove clauses containing the literal
                        clauses.NullifyClause(i);
                        i--;
                        break;
                    } else if (clauses[i, j] == -unit.Variable) {
                        // Remove negation of literal from clauses
                        clauses.RemoveVarAt(i, j);
                        j--;
                    }
                }
            }

            Console.WriteLine("FINISH PROPAGATE ON: " + unit.Variable);
            PrintState();
        }

        private UnitClauseAppearances GetVariableToPropagate() {
            int iMax = clauses.List.Count;
            for (int cl = 0; cl < iMax; cl++) {
                int jMax = clauses.List[cl].Count;
                for (int li = 0; li < jMax; li++) {
                    int value = clauses[cl, li];
                    int index = GetVar(value);

                    if (variables[index] == 0 && clauses.List[cl].Count == 1) {
                        UnitClauseAppearances u = new UnitClauseAppearances(index);
                        u.ClauseValue = value;
                        return u;
                    }
                }
            }
            return null;
        }

        private Dictionary<int, UnitClauseAppearances> GetVariableIndices() {
            Dictionary<int, UnitClauseAppearances> variableIndices = new Dictionary<int, UnitClauseAppearances>();
            int iMax = clauses.List.Count;
            for (int cl = 0; cl < iMax; cl++) {
                int jMax = clauses.List[cl].Count;

                for (int li = 0; li < jMax; li++) {
                    int literal = clauses[cl, li];
                    literal = literal > 0 ? literal : -literal;

                    if (!variableIndices.ContainsKey(literal)) {
                        variableIndices.Add(literal, new UnitClauseAppearances(literal));
                    }
                    variableIndices[literal].Clauses.Add(cl);
                    if (clauses.List[cl].Count == 1) {
                        variableIndices[literal].IsUnit = true;
                        variableIndices[literal].ClauseValue = clauses[cl, li];
                    }
                }
            }
            return variableIndices;
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

        #region old unit
        //private void RunUnitPropagationOld() {
        //    var variableIndices = GetVariableIndices();

        //    while (variableIndices.Count > 0) {
        //        int key = 0;
        //        foreach (var varPairs in variableIndices) {
        //            if (UnitPropagateOn(varPairs.Key, varPairs.Value)) {
        //                key = varPairs.Key;
        //                break;
        //            }
        //        }
        //        if (key == 0) {
        //            break;
        //        }
        //        variableIndices.Remove(key);
        //    }
        //}

        //private class UnitClauseAppearances {
        //    public int Variable;
        //    public bool IsUnit = false;
        //    public List<int> Clauses = new List<int>();

        //    public UnitClauseAppearances(int variable) {
        //        Variable = variable;
        //    }
        //}

        //private Dictionary<int, UnitClauseAppearances> GetVariableIndices() {
        //    Dictionary<int, UnitClauseAppearances> variableIndices = new Dictionary<int, UnitClauseAppearances>();
        //    int iMax = clauses.List.Count;
        //    for (int i = 0; i < iMax; i++) {
        //        int jMax = clauses.List[i].Count;

        //        for (int j = 0; j < jMax; j++) {
        //            int l = clauses[i, j];
        //            l = l > 0 ? l : -l;

        //            if (!variableIndices.ContainsKey(l)) {
        //                variableIndices.Add(l, new UnitClauseAppearances(l));
        //            }
        //            variableIndices[l].Clauses.Add(i);
        //            if (clauses.List[l].Count == 1) {
        //                variableIndices[l].IsUnit = true;
        //            }
        //        }
        //    }
        //    return variableIndices;
        //}


        //private bool UnitPropagateOn(int variable, List<int> clauses) {

        //    if (!IsUnitClause(variable, clauses)) {
        //        return false;
        //    }

        //    foreach (int clause in clauses) {

        //    }

        //    return true;
        //}

        //private bool IsUnitClause(int var, List<int> clauseList) {
        //    foreach (int clause in clauseList) {
        //        if (ClauseVarCount(clauses.List[clause]) == 1) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private int ClauseVarCount(List<int> clause) {
        //    int count = 0;
        //    foreach (int var in clause) {
        //        if (variables[var] == 0) {
        //            count++;
        //        }
        //    }
        //    return count;
        //}
        #endregion
    }
}
