using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeColoringSatEncoder {
    class Program {
        public const string CNF_PATH = "P:\\Stuff\\School\\alr_2021\\cnf";
        static void Main(string[] args) {

            Graph graph3b = new Graph(7,
                Edge.FromChars('a', 'b'),
                Edge.FromChars('a', 'c'),
                Edge.FromChars('a', 'g'),
                Edge.FromChars('b', 'c'),
                Edge.FromChars('b', 'd'),
                Edge.FromChars('c', 'd'),
                Edge.FromChars('d', 'e'),
                Edge.FromChars('d', 'f'),
                Edge.FromChars('e', 'f'),
                Edge.FromChars('e', 'g'),
                Edge.FromChars('f', 'g'));

            Graph graph3c = new Graph(7,
                Edge.FromChars('a', 'b'),
                // Edge.FromChars('a', 'c'),
                Edge.FromChars('a', 'g'),
                Edge.FromChars('b', 'c'),
                Edge.FromChars('b', 'd'),
                Edge.FromChars('c', 'd'),
                Edge.FromChars('d', 'e'),
                Edge.FromChars('d', 'f'),
                Edge.FromChars('e', 'f'),
                Edge.FromChars('e', 'g'),
                Edge.FromChars('f', 'g'));

            ThreeColoringCNFSentence sentence3b = new ThreeColoringCNFSentence(graph3b);

            ThreeColoringCNFSentence sentence3c = new ThreeColoringCNFSentence(graph3c);

            SaveCNF("week1_3b", sentence3b.ToDIMACS());
            SaveCNF("week1_3c", sentence3c.ToDIMACS());
        }

        private static void Test2() {

            Graph g = new Graph(3,
                new Edge(0, 2),
                new Edge(0, 1),
                new Edge(1, 2));

            ThreeColoringCNFSentence cnf = new ThreeColoringCNFSentence(g);
            KColoringCNFSentence cnfK = new KColoringCNFSentence(g, 3);

            Console.WriteLine(cnf.ToDIMACS());
            Console.WriteLine();
            Console.WriteLine(cnfK.ToDIMACS());

            Console.ReadLine();
        }

        private static void SaveCNF(string filename, string cnf) {
            string path = $"{CNF_PATH}\\{filename}.cnf";
            File.WriteAllText(path, cnf);
        }
    }
}
