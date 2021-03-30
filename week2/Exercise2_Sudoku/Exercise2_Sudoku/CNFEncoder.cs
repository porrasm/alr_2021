using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2_Sudoku {
    public abstract class CNFEncoder {
        public abstract int VariableCount { get; }
        public List<int[]> Sentence { get; protected set; } = new List<int[]>();

        public void AddClause(params int[] variables) {
            foreach (int var in variables) {
                if (var == 0) {
                    throw new Exception("Invalid variable");
                }
            }
            Sentence.Add(variables);
        }

        public override string ToString() {
            StringBuilder b = new StringBuilder();
            foreach (int[] clause in Sentence) {
                b.Append("(");
                foreach (int var in clause) {
                    b.Append($"{var}, ");
                }
                b.Append(") &\n");
            }
            return b.ToString();
        }

        public string ToDIMACS() {
            StringBuilder b = new StringBuilder();
            b.Append($"p cnf {VariableCount} {Sentence.Count}\n");

            foreach (int[] clause in Sentence) {
                foreach (int var in clause) {
                    b.Append($"{var} ");
                }
                b.Append("0\n");
            }

            return b.ToString();
        }
    }
}
