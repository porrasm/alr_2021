using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeColoringSatEncoder {
    public class ThreeColoringCNFSentence {
        private Graph graph;
        public int VariableCount { get; private set; }
        public List<int[]> Sentence { get; private set; } = new List<int[]>();

        public ThreeColoringCNFSentence(Graph g) {
            this.graph = g;
            this.VariableCount = g.NodeCount * 3;
            AddConstraintsForNodes();
            AddConstraintsForEdges();
        }

        #region constraints
        private void AddConstraintsForNodes() {
            graph.ForEachNode(AddNodeConstraints);
        }

        private void AddNodeConstraints(int v) {
            // At least 1 color
            AddClause(GetR(v), GetG(v), GetB(v));
            // Not red and green
            AddClause(-GetR(v), -GetG(v));
            // Not red and blue
            AddClause(-GetR(v), -GetB(v));
            // Not green and blue
            AddClause(-GetG(v), -GetB(v));
        }

        private void AddConstraintsForEdges() {
            graph.ForEachEdge(AddEdgeConstraints);
        }

        private void AddEdgeConstraints(Edge e) {
            // Not edge from red to red
            AddClause(-GetR(e.u), -GetR(e.v));
            // Not edge from green to green
            AddClause(-GetG(e.u), -GetG(e.v));
            // Not edge from blue to blue
            AddClause(-GetB(e.u), -GetB(e.v));
        }

        public void AddClause(params int[] variables) {
            Sentence.Add(variables);
        }
        #endregion

        #region color variables
        private int GetR(int v) {
            return v * 3 + 1;
        }
        private int GetG(int v) {
            return v * 3 + 2;
        }
        private int GetB(int v) {
            return v * 3 + 3;
        }
        #endregion

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
