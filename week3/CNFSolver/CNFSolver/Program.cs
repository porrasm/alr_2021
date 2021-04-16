using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CNFSolver {
    class Program {
        private static DPLLSolver solver;
        static void Main(string[] args) {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            //TestClauses();

            Console.WriteLine("Arg count: " + args.Length);
            //Console.ReadLine();
            //return;
            if (args.Length != 2) {
                solver = new DPLLSolver();
                SolveSatInstance("P:/Stuff/School/alr_2021/week3/CNFSolver/CNFSolver/example.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week2_boat/k7.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/uf20-0100.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/bmc-ibm-6.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/uf100-0102.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/hole6.cnf");
                Console.ReadLine();
            } else if (args.Length == 2) {
                if (args[0] == "dpll" || true) {
                    solver = new DPLLSolver();
                }
                if (Directory.Exists(args[1])) {
                    SolveDir(args[1]);
                } else {
                    SolveSatInstance(args[1]);
                }
            }
        }

        public static void PrintList(List<int> list, bool force = false) {
            if (!force) {
                return;
            }
            Console.WriteLine("-----------------------------------");
            foreach (int val in list) {
                Console.WriteLine(val);
            }
            Console.WriteLine("-----------------------------------");
        }

        private static async void SolveDir(string dir) {
            foreach (string file in Directory.GetFiles(dir)) {
                //if (file.Contains("bmc") || file.Contains("hole")) {
                //    Console.WriteLine("Skipping: " + file);
                //    continue;
                //}
                SolveSatInstance(file);
            }
        }
        private static void SolveSatInstance(string instance) {

            if (instance.Contains("bmc")) {
                Console.WriteLine("Skippped: " + instance);
                return; 
            }

            Console.WriteLine("\nSolving problem: " + Path.GetFileName(instance));
            solver.LoadProblem(instance);
            //solver.PrintState();

            bool res = false;
            bool finished = false;

            Stopwatch watch = new Stopwatch();
            long passed = 0;

            var cancel = new CancellationTokenSource();
            Thread solveThread = new Thread(() => {
                Console.WriteLine("Started solve thread");
                res = solver.Solve();
                finished = true;
                cancel.Cancel();
                passed = watch.ElapsedMilliseconds;
            });
            solveThread.Priority = ThreadPriority.Highest;

            watch.Start();
            solveThread.Start();

            while (watch.ElapsedMilliseconds < 120000) {
                Thread.Sleep(250);
                if (finished) {
                    break;
                }
            }
            watch.Stop();

            solveThread.Abort();

            if (!finished) {
                Console.WriteLine("Cancelled by timeout");
                solveThread.Abort();
                Console.WriteLine();
                return;
            }

            Console.WriteLine("Solve status: " + res);
            Console.WriteLine("Solved in " + passed + "ms");
            PrintList(solver.GetVariableAssignments);

            solver.Clear();
            Console.WriteLine();
        }

        private static async Task<bool> Solve(SatSolver solver) {
            await Task.Yield();
            return solver.Solve();
        }

        private Task WrapSolve(Action act) {
            return Task.Run(() => {
                Task.Yield();
                act();
            });
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
