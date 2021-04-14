using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    class Program {
        private static DPLLSolver solver;
        static void Main(string[] args) {
            //TestClauses();

            //Console.ReadLine();
            //return;
            if (args.Length == 0) {
                solver = new DPLLSolver();
                SolveSatInstance("P:/Stuff/School/alr_2021/week3/CNFSolver/CNFSolver/example.cnf");
            } else if (args.Length == 3) {
                if (args[1] == "dpll" || true) {
                    solver = new DPLLSolver();
                }
            }

            Console.ReadLine();
        }

        public static void PrintList(List<int> list) {
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

            bool res = solver.Solve();

            Console.WriteLine("Solve status: " + res);
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
