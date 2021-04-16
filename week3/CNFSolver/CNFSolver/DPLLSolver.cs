using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class DPLLSolver : SatSolver {

        #region fields
        private int iterations;

        public override List<int> GetVariableAssignments => variables.List;
        #endregion

        #region solve
        public override bool SolveImplementation() {
            bool res = DPLLIterative();
            //Console.WriteLine("Completed in " + iterations + " iterations");
            return res;
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

                if (clauseStack.Count > 0) {
                    Checkpoint();
                    ClauseBranch clause = clauseStack.Pop();
                    clauses.AddClause(clause.Clause);
                    PrepareBranch(new UnitClause(clause.Clause));
                }

                if (unsatisfiable) {
                    if (clauseStack.Count == 0) {
                        return false;
                    }

                    Backtrack(clauseStack.Peek().RevertLevel);
                    continue;
                }
                if (satisfiable) {
                    // Forget backtrack history and just return 
                    variables.Forget();
                    clauses.Forget();
                    return true;
                }

                Stopwatch heur = Stopwatch.StartNew();
                int literal = heuristic.GetVariableToBranchOn();
                heur.Stop();
                //int literal = SelectLiteral();
                if (literal == 0) {
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
            }
        }
        #endregion

        protected override void PrepareBranch(UnitClause unit) {
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
    }
 }
