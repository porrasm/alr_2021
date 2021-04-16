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
        public UnitClause(int value) {
            this.Value = value;
            this.Index = SatSolver.GetVar(value);
        }
        public UnitClause(int variable, int value) {
            Index = variable;
            this.Value = value;
        }
    }
}
