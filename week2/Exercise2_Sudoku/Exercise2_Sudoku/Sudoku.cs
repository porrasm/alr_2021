using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2_Sudoku {
    public class Sudoku {
        public int N2 { get; private set; }
        public int N { get; private set; }
        public int[,] Table { get; private set; }

        public Sudoku(int size) {
            N2 = size;
            N = (int)Math.Sqrt(size);
            if (N * N != N2) {
                throw new Exception("Not squarable to integer");
            }
            Table = new int[N2, N2];
        }

        public static Sudoku FromFile(string path) {
            int n2 = int.Parse(Path.GetFileNameWithoutExtension(path).Split('x')[0]);
            Sudoku sudoku = new Sudoku(n2);
            string[] lines = File.ReadAllLines(path);
            for (int x = 0; x < n2; x++) {
                int[] row = lines[x].Split(' ').Select(s => int.Parse(s)).ToArray();
                for (int y = 0; y < n2; y++) {
                    sudoku.Table[x, y] = row[y];
                }
            }

            return sudoku;
        }

        public static Sudoku[] FromDirectory(string path) {
            string[] files = Directory.GetFiles(path);

            Sudoku[] sudokus = new Sudoku[files.Length];
            for (int i = 0; i < files.Length; i++) {
                sudokus[i] = Sudoku.FromFile(files[i]);
            }

            return sudokus;
        }

        public bool IsValid() {
            HashSet<int> used = new HashSet<int>();
            // Row
            for (int x = 0; x < N2; x++) {
                used.Clear();
                for (int y = 0; y < N2; y++) {
                    if (Table[x, y] == 0) {
                        Console.WriteLine($"ROW CHECK FAIL: " + x);
                        return false;
                    }
                    if (!used.Add(Table[x, y])) {
                        return false;
                    }
                }
            }

            // Col
            for (int y = 0; y < N2; y++) {
                used.Clear();
                for (int x = 0; x < N2; x++) {

                    if (!used.Add(Table[x, y])) {
                        Console.WriteLine($"COL CHECK FAIL: " + y);
                        return false;
                    }
                }
            }

            // Grid
            for (int grid = 0; grid < N; grid++) {
                used.Clear();

                for (int gridX = 0; gridX < N; gridX++) {
                    for (int gridY = 0; gridY < N; gridY++) {
                        Console.WriteLine($"GRID CHECK FAIL: " + grid);
                        FromGridIndex(grid, gridX, gridY, out int x, out int y);
                        if (!used.Add(Table[x, y])) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public override string ToString() {
            StringBuilder b = new StringBuilder();

            for (int x = 0; x < N2; x++) {

                if (x % N == 0) {
                    b.Append("\n");
                }
                for (int y = 0; y < N2; y++) {
                    if (y % N == 0) {
                        b.Append("  ");
                    }

                    b.Append($"{ValueToString(Table[x, y])} ");
                }

                b.Append("\n\n");
            }

            b.Append("Is valid: " + IsValid());
            return b.ToString();
        }

        private void FromGridIndex(int grid, int x, int y, out int newX, out int newY) {
            int gridX = grid / N;
            int gridY = grid % N;

            newX = gridX * N + x;
            newY = gridY * N + y;
        }

        private string ValueToString(int val) {
            if (val < 10) {
                return val + " ";
            } else {
                return val + "";
            }
        }
    }
}
