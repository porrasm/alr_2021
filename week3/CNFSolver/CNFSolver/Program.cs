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
        private static string solverType;
        static void Main(string[] args) {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            //TestClauses();

            Console.WriteLine("Arg count: " + args.Length);
            //Console.ReadLine();
            //return;
            if (args.Length != 2) {
                solverType = "dpll";
                SolveSatInstance("P:/Stuff/School/alr_2021/week3/CNFSolver/CNFSolver/example.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week2_boat/k7.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/uf20-0100.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/bmc-ibm-6.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/uf100-0102.cnf");
                //SolveSatInstance("P:/Stuff/School/alr_2021/cnf/week3/hole6.cnf");
                Console.ReadLine();
            } else if (args.Length == 2) {
                solverType = args[0];
                if (Directory.Exists(args[1])) {
                    SolveDir(args[1]);
                } else {
                    SolveSatInstance(args[1]);
                }
            }
        }

        private static SatSolver GetSolver() {
            if (solverType == "dpll") {
                return new DPLLSolver();
            }
            return null;
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

            //if (instance.Contains("bmc")) {
            //    Console.WriteLine("Skippped: " + instance);
            //    return; 
            //}

            string instanceName = Path.GetFileName(instance);
            SatSolver solver = GetSolver();
            solver.LoadProblem(instance);
            //solver.PrintState();

            bool res = false;
            bool finished = false;

            Stopwatch watch = new Stopwatch();
            long passed = 0;

            var cancel = new CancellationTokenSource();
            Thread solveThread = new Thread(() => {
                res = solver.SolveImplementation();
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
                Console.WriteLine($"Cancelled by timeout: \"{instanceName}\"");
                solveThread.Abort();
                Console.WriteLine();
                return;
            }

            string sat = res ? "SATISFIABLE" : "UNSATISFIABLE";
            Console.WriteLine($"Solved problem \"{instanceName}\" in {passed} ms, result: {sat}");

            PrintList(solver.GetVariableAssignments);

            solver.Clear();
            Console.WriteLine();
        }
    }
}
