using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2_Sudoku {
    class Program {

        private const string inDirectory = "P:\\Stuff\\School\\alr_2021\\week2\\sudokus";
        private const string cnfDirectory = "P:\\Stuff\\School\\alr_2021\\cnf\\week2_sudoku";
        private const string solutionDirectory = "P:\\Stuff\\School\\alr_2021\\week2\\cnf_benchmark\\results";

        private static string[] sudokuFileNames;
        private static SudokuCont[] sudokus;

        private class SudokuCont {
            public Sudoku Sudoku;
            public CNFEncoder Pairwise;
            public CNFEncoder Ladder;

            public SudokuCont(Sudoku s) {
                this.Sudoku = s;
                Pairwise = new SudokuEncoderPairwise(s);
                Ladder = new SudokuEncoderLadder(s);
            }
        }

        static void Main(string[] args) {
            Encode();
            Decode();
            Console.ReadLine();
        }

        private static void Encode() {
            sudokuFileNames = Directory.GetFiles(inDirectory).Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
            sudokus = Sudoku.FromDirectory(inDirectory).Select(s => new SudokuCont(s)).ToArray();
            Console.WriteLine($"Loaded {sudokus.Length} sudokus");

            if (!Directory.Exists(cnfDirectory)) {
                Directory.CreateDirectory(cnfDirectory);
            }

            for (int i = 0; i < sudokus.Length; i++) {
                Console.WriteLine($"Writing ${sudokuFileNames[i]} as cnf...");
                SudokuCont s = sudokus[i];

                string cnf_pairwise = s.Pairwise.ToDIMACS();
                string cnf_ladder = s.Ladder.ToDIMACS();

                Console.WriteLine($"    Pairwise size: ({s.Pairwise.VariableCount}, {s.Pairwise.Sentence.Count})");
                Console.WriteLine($"    Ladder size  : ({s.Ladder.VariableCount}, {s.Ladder.Sentence.Count})");

                string pairwiseFile = $"{cnfDirectory}\\{sudokuFileNames[i]}_pairwise.cnf";
                string ladderFile = $"{cnfDirectory}\\{sudokuFileNames[i]}_ladder.cnf";


                File.WriteAllText(pairwiseFile, cnf_pairwise);
                File.WriteAllText(ladderFile, cnf_ladder);
            }
        }

        private static void Decode() {
            string[] resultFiles = Directory.GetFiles(solutionDirectory);
            Console.WriteLine($"Found {resultFiles.Length} results");

            for (int i = 0; i < resultFiles.Length; i++) {
                Console.WriteLine("--------------------------------------------------------------");

                if (!resultFiles[i].Contains("9x9") || !resultFiles[i].Contains("ladder")) {
                    continue;
                }

                int sudokuIndex = i / 2;
                SudokuCont sudoku = sudokus[sudokuIndex];

                string[] lines = File.ReadAllLines(resultFiles[i]);
                int nanos = int.Parse(lines[0]);
                if (lines.Length == 2) {
                    Console.WriteLine($"No solution found for {sudokuFileNames[sudokuIndex]} : {Path.GetFileNameWithoutExtension(resultFiles[i])}");
                    continue;
                }

                Console.WriteLine($"Solved sudoku: {Path.GetFileNameWithoutExtension(resultFiles[i])}");

                Console.WriteLine($"    Pairwise encoding size : ({sudoku.Pairwise.VariableCount}, {sudoku.Pairwise.Sentence.Count})");
                Console.WriteLine($"    Ladder encoding size   : ({sudoku.Ladder.VariableCount}, {sudoku.Ladder.Sentence.Count})");
                Console.WriteLine($"    Sudoku solve time      : {nanos / 1000} milliseconds");

                Console.WriteLine("\nOriginal sudoku:");
                Console.WriteLine(sudoku.Sudoku);

                Sudoku solved = GetSudokuFromResult(sudoku.Sudoku, lines);
                Console.WriteLine("\nSolved sudoku:");
                Console.WriteLine(solved);

                Console.WriteLine("\n");

                Console.WriteLine("--------------------------------------------------------------");
                break;
            }
        }

        private static Sudoku GetSudokuFromResult(Sudoku original, string[] res) {

            Sudoku sudoku = new Sudoku(original.N2);
            int maxVar = sudoku.N2 * sudoku.N2 * sudoku.N2;

            for (int i = 2; i < res.Length; i++) {
                string line = res[i];
                //Console.WriteLine(line);
                string[] variables = line.Split(' ');

                for (int v = 1; v < variables.Length; v++) {
                    string var = variables[v];

                    if (int.TryParse(var, out int varInt)) {
                        if (varInt > 0 && varInt <= maxVar) {
                            SetSudokuValue(sudoku, varInt);
                        }
                    }
                }
            }

            return sudoku;
        }

        private static void SetSudokuValue(Sudoku sudoku, int var) {
            SetIndices(sudoku.N2, var, out int x, out int y, out int value);
            Console.WriteLine($"{var} -> ({x}, {y}) = {value+1}");
            sudoku.Table[x, y] = value + 1;
        }

        public static void SetIndices(int n2, int variable, out int x, out int y, out int value) {
            variable -= 1;
            x = variable / (n2 * n2);
            variable -= (x * n2 * n2);
            y = variable / n2;
            value = variable % n2;
        }
    }
}
