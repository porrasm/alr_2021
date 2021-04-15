using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    class Program {
        private static DPLLSolver solver;
        static void Main(string[] args) {
            //TestClauses();

            Console.WriteLine("Arg count: " + args.Length);
            //Console.ReadLine();
            //return;
            if (args.Length == 0) {
                solver = new DPLLSolver();
                //SolveSatInstance("P:/Stuff/School/alr_2021/week3/CNFSolver/CNFSolver/example.cnf");
                SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week2_boat/k7.cnf");
            } else if (args.Length == 2) {
                if (args[0] == "dpll" || true) {
                    solver = new DPLLSolver();
                }
                if (Directory.Exists(args[1])) {
                    foreach (string file in Directory.GetFiles(args[1])) {
                        if (file.Contains("bmc") || file.Contains("hole")) {
                            Console.WriteLine("Skipping: " + file);
                            continue;
                        }
                        SolveSatInstance(file);
                    }
                }
            }

            Console.ReadLine();
        }

        public static void PrintList(List<int> list) {
            return;
            Console.WriteLine("-----------------------------------");
            foreach (int val in list) {
                Console.WriteLine(val);
            }
            Console.WriteLine("-----------------------------------");
        }

        private static void SolveSatInstance(string instance) {
            Console.WriteLine("Loading problem: " + instance);
            solver.LoadProblem(instance);
            Console.WriteLine("Loaded problem");
            Console.WriteLine("Printing problem");
            solver.PrintState();

            Console.WriteLine("Starting solver...");

            long time = Timer.Milliseconds;
            bool res = solver.Solve();
            time = Timer.PassedFrom(time);

            Console.WriteLine("Solve status: " + res);
            Console.WriteLine("Solved in " + time + "ms");
            PrintList(solver.GetVariableAssignments);

            solver.Clear();
        }

        private static void TestClauses() {

            Clauses clauses = new Clauses(new List<List<int>>() { 
            new List<int>() {1, 2, 3},
            new List<int>() {4, 5, 6},
            new List<int>() {7, 8, 9}
            });

            Console.WriteLine(clauses);

            clauses.AddClause(new List<int>() { 10, 11, 12 });
            clauses.CreateCheckpoint();
            Console.WriteLine(clauses);

            clauses[0, 0] = 0;
            clauses.CreateCheckpoint();
            Console.WriteLine(clauses);

            clauses.AddVar(0, 4);
            Console.WriteLine(clauses);

            clauses.RevertToLastCheckpoint();
            Console.WriteLine(clauses);

            clauses.RevertToLastCheckpoint();
            Console.WriteLine(clauses);

            clauses.RevertToLastCheckpoint();
            Console.WriteLine(clauses);
        }

    }
}
