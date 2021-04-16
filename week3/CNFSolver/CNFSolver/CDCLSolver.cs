using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class CDCLSolver : SatSolver {
        #region fields
        public override List<int> GetVariableAssignments => throw new NotImplementedException();

        private int iterations;

        private RList<TrailNode> trail = new RList<TrailNode>(new List<TrailNode>());
        private TrailNode PopTrail {
            get {
                TrailNode last = trail.List[trail.List.Count - 1];
                trail.RemoveAt(trail.List.Count - 1);
                return last;
            }
        }

        private struct TrailNode {
            public int Variable;
            public int Reason;

            public TrailNode(int variable, int reason) {
                Variable = variable;
                Reason = reason;
            }
        }

        private int DecisionLevel => variables.CheckPointLevel;
        #endregion

        public override bool SolveImplementation() {
            bool res = CDCL();

            return res;
        }

        private bool CDCL() {

            RunInitialUnitPropagation();
            UnitClause decision;

            while (true) {

                // if count > 0 -> get decision, prepare
                if (trail.Count > 0) {
                    TrailNode last = PopTrail;
                    PrepareBranch(new UnitClause(last.Variable));
                }
                //

                if (unsatisfiable) {
                    if (variables.CheckPointLevel == 0) {
                        return false;
                    }

                    // analyze conflict
                    // addclause
                    // bacjump
                    continue;
                }
                if (satisfiable) {
                    return true;
                }

                // make decision
                decision = new UnitClause(heuristic.GetVariableToBranchOn());
                // false first
                decision.Value *= -1;


            }
        }

        protected override void PrepareBranch(UnitClause unit) {

            TrailNode node = new TrailNode(unit.Index, 0);
            trail.Add(node);

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

        private int RunConflictAnalysis() {

        }
    }
}
