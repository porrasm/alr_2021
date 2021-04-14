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
        #endregion

        public void LoadProblem(string path) {
            clauseList = new List<List<int>>();

            string[] lines = File.ReadAllLines(path);

            Console.WriteLine("Read " + lines.Length + " lines");

            int i;
            for (i = 0; i < lines.Length; i++) {
                if (lines[i].StartsWith("p ")) {
                    break;
                }
            }

            string[] opts = lines[i].Split(' ');
            VariableCount = int.Parse(opts[2]);
            int clauseCount = int.Parse(opts[3]);
            i++;

            List<int> currentClause = new List<int>();
            for (; i < lines.Length; i++) {
                foreach (int val in lines[i].Split(' ').Select(o => int.Parse(o))) {
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

        public virtual void Clear() {
            VariableCount = 0;
            clauseList = null;
        }

        protected int GetVar(int value) {
            return value > 0 ? value : -value;
        }

        public abstract bool Solve();
    }
}
