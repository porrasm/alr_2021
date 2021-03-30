using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2_Sudoku {
    public class SudokuEncoderLadder : CNFEncoder {
        private Sudoku sudoku;
        private int trueVarCount;
        private int ladderVarCount;
        public override int VariableCount => trueVarCount + ladderVarCount;

        public SudokuEncoderLadder(Sudoku sudoku) {
            this.sudoku = sudoku;
            trueVarCount = sudoku.N2 * sudoku.N2 * sudoku.N2;

            AddFixedConstraints();
            AddRowAndColumnConstraints();
            AddGridConstraints();
        }

        #region fixed
        private void AddFixedConstraints() {
            for (int row = 0; row < sudoku.N2; row++) {
                for (int col = 0; col < sudoku.N2; col++) {

                    // from 1 based to 0 based
                    int val = sudoku.Table[row, col] - 1;
                    if (val >= 0) {
                        // from 0 based to 1 based variable index
                        AddClause(VarIndex(row, col, val));
                    }

                }
            }
        }
        #endregion

        #region row and column
        private void AddRowAndColumnConstraints() {
            // Methods separated for clarity, could combine loops for optimal performance
            AtLeastOneInRowAndColumn();

            //AtMostOneInRowAndCol_Pairwise();
            AtMostOneInRow_Ladder();
            //AtMostOneInCol_Ladder();

            ExactlyOnePerCell_Pairwise();
        }


        private void AtLeastOneInRowAndColumn() {
            for (int row = 0; row < sudoku.N2; row++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    int[] rowClause = new int[sudoku.N2];
                    int[] colClause = new int[sudoku.N2];
                    for (int col = 0; col < sudoku.N2; col++) {
                        rowClause[col] = VarIndex(row, col, val);
                        colClause[col] = VarIndex(col, row, val);
                    }
                }
            }
        }

        private void AtMostOneInRowAndCol_Pairwise() {

            for (int row = 0; row < sudoku.N2; row++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int col1 = 0; col1 < sudoku.N2; col1++) {
                        for (int col2 = 0; col2 < sudoku.N2; col2++) {
                            if (col1 == col2) {
                                continue;
                            }

                            AddClause(-VarIndex(row, col1, val), -VarIndex(row, col2, val));
                            AddClause(-VarIndex(col1, row, val), -VarIndex(col2, row, val));
                        }
                    }
                }
            }
        }
        private void AtMostOneInRow_Ladder() {

            for (int row = 0; row < sudoku.N2; row++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int col = 1; col < sudoku.N2 - 1; col++) {
                        int row_xi = VarIndex(row, col, val);
                        int row_yi = trueVarCount + ladderVarCount + col;

                        AddLadderConstraint(row_xi, row_yi, row_yi - 1);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                }
            }
        }
        private void AtMostOneInCol_Ladder() {

            for (int j = 0; j < sudoku.N2; j++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int i = 1; i < sudoku.N2 - 1; i++) {
                        int col_xi = VarIndex(i, j, val);
                        int col_yi = trueVarCount + ladderVarCount + j;

                        AddLadderConstraint(col_xi, col_yi, col_yi - 1);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                }
            }
        }


        private void ExactlyOnePerCell_Pairwise() {
            for (int row = 0; row < sudoku.N2; row++) {
                for (int col = 0; col < sudoku.N2; col++) {

                    List<int> clause = new List<int>();

                    for (int val1 = 0; val1 < sudoku.N2; val1++) {
                        clause.Add(VarIndex(row, col, val1));
                        for (int val2 = 0; val2 < sudoku.N2; val2++) {
                            if (val1 == val2) {
                                continue;
                            }

                            AddClause(-VarIndex(row, col, val1), -VarIndex(row, col, val2));
                        }
                    }

                    AddClause(clause.ToArray());
                }
            }
        }

        private void AtLeastOnePerCell() {
            for (int row = 0; row < sudoku.N2; row++) {
                for (int col = 0; col < sudoku.N2; col++) {
                    List<int> clause = new List<int>();
                    for (int val = 0; val < sudoku.N2; val++) {
                        clause.Add(VarIndex(row, col, val));
                    }
                    AddClause(clause.ToArray());
                }
            }
        }

        private void AtMostOnePerCell() {
            for (int row = 0; row < sudoku.N2; row++) {
                for (int col = 0; col < sudoku.N2; col++) {

                    List<int> clause = new List<int>();

                    for (int val = 1; val < sudoku.N2 - 1; val++) {
                        clause.Add(VarIndex(row, col, val));

                        int xi = VarIndex(row, col, val);
                        int yi = trueVarCount + ladderVarCount + val;

                        AddLadderConstraint(xi, yi, yi - 1);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                    AddClause(clause.ToArray());
                }
            }
        }    
        #endregion

        #region grid
        private void AddGridConstraints() {
            // Methods separated for clarity, could combine loops for optimal performance
            AtLeastOneInGrid();
            AtMostOneInGrid_Pairwise();
        }

        private void AtLeastOneInGrid() {
            for (int grid = 0; grid < sudoku.N2; grid++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    int[] clause = new int[sudoku.N2];

                    for (int i = 0; i < sudoku.N; i++) {
                        for (int j = 0; j < sudoku.N; j++) {
                            clause[i * sudoku.N + j] = VarGridIndex(grid, i, j, val);
                        }
                    }

                    AddClause(clause);
                }
            }
        }

        private void AtMostOneInGrid_Pairwise() {
            for (int grid = 0; grid < sudoku.N2; grid++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    // Pairwise coordinates
                    for (int i1 = 0; i1 < sudoku.N; i1++) {
                        for (int j1 = 0; j1 < sudoku.N; j1++) {
                            for (int i2 = 0; i2 < sudoku.N; i2++) {
                                for (int j2 = 0; j2 < sudoku.N; j2++) {
                                    if (i1 == i2 && j1 == j2) {
                                        continue;
                                    }

                                    AddClause(-VarGridIndex(grid, i1, j1, val), -VarGridIndex(grid, i2, j2, val));
                                }
                            }
                        }
                    }
                }
            }
        }
        private void AtMostOneInGrid_Ladder() {
            for (int grid = 0; grid < sudoku.N2; grid++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int gridPos = 1; gridPos < sudoku.N2 - 1; gridPos++) {
                        int gridX = gridPos / sudoku.N;
                        int gridY = gridPos % sudoku.N;

                        int xi = VarGridIndex(grid, gridX, gridY, val);
                        int yi = trueVarCount + ladderVarCount + gridPos;

                        AddLadderConstraint(xi, yi, yi - 1);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                }
            }
        }
        #endregion

        #region utility
        private int VarIndex(int x, int y, int value) {
            int var = x * sudoku.N2 * sudoku.N2 + y * sudoku.N2 + value + 1;

            Program.SetIndices(sudoku.N2, var, out int backX, out int backY, out int backValue);

            if (x != backX || y != backY || value != backValue) {
                Console.WriteLine($"Original: ({x}, {y}) = {value}");
                Console.WriteLine($"Variable: {var}");
                Console.WriteLine($"Back    : ({backX}, {backY}) = {backValue}");
                throw new Exception("Invalid conversion");
            }

            return var;
        }
        private int VarGridIndex(int grid, int x, int y, int val) {
            int gridX = grid / sudoku.N;
            int gridY = grid % sudoku.N;

            int trueX = gridX * sudoku.N + x;
            int trueY = gridY * sudoku.N + y;

            return VarIndex(trueX, trueY, val);
        }
        private void AddLadderConstraint(int xi, int yi, int yiPrev) {
            // y[i] -> y[i-1]
            AddClause(-yi, yi - 1);

            // x[k] <=> (y[i-1] && !y[i])
            AddClause(xi, -yiPrev, yi);
            AddClause(-xi, yiPrev);
            AddClause(-xi, -yi);
        }
        #endregion
    }
}
