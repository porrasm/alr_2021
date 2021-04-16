using System;
using System.Collections.Generic;
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
}
