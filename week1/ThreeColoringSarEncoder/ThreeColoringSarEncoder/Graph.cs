using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeColoringSatEncoder {
    public class Graph {
        public int NodeCount { get; private set; }
        public Edge[] Edges;

        public Graph(int nodeCount, params Edge[] edges) {
            NodeCount = nodeCount;
            this.Edges = edges;
        }

        public delegate void NodeIterator(int node);
        public void ForEachNode(NodeIterator it) {
            for (int i = 0; i < NodeCount; i++) {
                it(i);
            }
        }

        public delegate void EdgeIterator(Edge e);
        public void ForEachEdge(EdgeIterator it) {
            foreach (Edge e in Edges) {
                it(e);
            }
        }
    }

    public struct Edge {
        public int u, v;
        public Edge(int u, int v) {
            this.u = u;
            this.v = v;
        }
        public static Edge FromChars(char u, char v) {
            return new Edge(u - 'a', v - 'a');
        }
    }
}
