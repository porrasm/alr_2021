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
        private const string solutionDirectory = "P:\\Stuff\\School\\alr_2021\\week2\\cnf_benchmark\\solutions";

        private static string[] sudokuFileNames;
        private static Sudoku[] sudokus;

        static void Main(string[] args) {
            Encode();
            Decode();
            Console.ReadLine();
        }

        private static void Encode() {
            sudokuFileNames = Directory.GetFiles(inDirectory).Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
            sudokus = Sudoku.FromDirectory(inDirectory);
            Console.WriteLine($"Loaded {sudokus.Length} sudokus");

            if (!Directory.Exists(cnfDirectory)) {
                Directory.CreateDirectory(cnfDirectory);
            }

            for (int i = 0; i < sudokus.Length; i++) {
                Console.WriteLine($"Writing ${sudokuFileNames[i]} as cnf...");

                string cnf_pairwise = new SudokuEncoderPairwise(sudokus[i]).ToDIMACS();
                string cnf_ladder = new SudokuEncoderLadder(sudokus[i]).ToDIMACS();

                string pairwiseFile = $"{cnfDirectory}\\{sudokuFileNames[i]}_pairwise.cnf";
                string ladderFile = $"{cnfDirectory}\\{sudokuFileNames[i]}_ladder.cnf";

                File.WriteAllText(pairwiseFile, cnf_pairwise);
                File.WriteAllText(ladderFile, cnf_ladder);
            }
        }

        private static void Decode() {

        }
    }
}
