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
            for (int i = 0; i < sudoku.N2; i++) {
                for (int j = 0; j < sudoku.N2; j++) {
                    // from 1 based to 0 based
                    int val = sudoku.Table[i, j] - 1;
                    if (val >= 0) {
                        // from 0 based back to 1 based
                        AddClause(VarIndex(i, j, val));
                    }
                }
            }
        }
        #endregion

        #region row and column
        private void AddRowAndColumnConstraints() {
            // Methods separated for clarity, could combine loops for optimal performance
            AtLeastOneInRowAndColumn();
            AtMostInOneRow_Ladder();
            AtMostInOneCol_Ladder();
        }

        private void AtLeastOneInRowAndColumn() {
            for (int i = 0; i < sudoku.N2; i++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    int[] rowClause = new int[sudoku.N2];
                    int[] colClause = new int[sudoku.N2];
                    for (int j = 0; j < sudoku.N2; j++) {
                        rowClause[j] = VarIndex(i, j, val);
                        colClause[j] = VarIndex(j, i, val);
                    }
                }
            }
        }
        private void AtMostInOneRow_Ladder() {

            for (int i = 0; i < sudoku.N2; i++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int j = 1; j < sudoku.N2; j++) {
                        int row_xi = VarIndex(i, j, val);
                        int row_yi = trueVarCount + ladderVarCount + j;

                        // y[i] -> y[i-1]
                        AddClause(-row_yi, row_yi - 1);

                        // x[k] <=> (y[i-1] && !y[i])
                        AddClause(row_xi, -(row_yi - 1), row_yi);
                        AddClause(-row_xi, row_yi - 1);
                        AddClause(-row_xi, -row_yi);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                }
            }
        }

        private void AtMostInOneCol_Ladder() {

            for (int j = 0; j < sudoku.N2; j++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int i = 1; i < sudoku.N2; i++) {
                        int col_xi = VarIndex(i, j, val);
                        int col_yi = trueVarCount + ladderVarCount + j;

                        // y[i] -> y[i-1]
                        AddClause(-col_yi, col_yi - 1);

                        // x[k] <=> (y[i-1] && !y[i])
                        AddClause(col_xi, -(col_yi - 1), col_yi);
                        AddClause(-col_xi, col_yi - 1);
                        AddClause(-col_xi, -col_yi);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                }
            }
        }
        #endregion

        #region grid
        private void AddGridConstraints() {
            // Methods separated for clarity, could combine loops for optimal performance
            AtLeastOneInGrid();
            AtMostOneInGrid_Ladder();
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

        private void AtMostOneInGrid_Ladder() {
            for (int grid = 0; grid < sudoku.N2; grid++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int gridPos = 1; gridPos < sudoku.N2; gridPos++) {
                        int gridX = gridPos / sudoku.N;
                        int gridY = gridPos % sudoku.N;

                        int xi = VarGridIndex(grid, gridX, gridY, val);
                        int yi = trueVarCount + ladderVarCount + gridPos;

                        // y[i] -> y[i-1]
                        AddClause(-yi, yi - 1);

                        // x[k] <=> (y[i-1] && !y[i])
                        AddClause(xi, -(yi - 1), yi);
                        AddClause(-xi, yi - 1);
                        AddClause(-xi, -yi);
                    }

                    ladderVarCount += sudoku.N2 - 1;
                }
            }
        }
        #endregion

        #region utility
        private int VarIndex(int x, int y, int value) {
            return x * sudoku.N2 * sudoku.N2 + y * sudoku.N2 + value + 1;
        }
        private int VarGridIndex(int grid, int x, int y, int val) {
            int gridX = grid / sudoku.N;
            int gridY = grid % sudoku.N;

            int trueX = gridX * sudoku.N + x;
            int trueY = gridY * sudoku.N + y;

            return VarIndex(trueX, trueY, val);
        }
        #endregion
    }
}
