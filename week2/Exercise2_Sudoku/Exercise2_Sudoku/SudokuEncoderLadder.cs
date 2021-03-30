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
            // AddGridConstraints();
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
            //AtMostInOneRow_Ladder();
            //AtMostInOneCol_Ladder();

            AtLeastOnePerCell();
            AtMostOnePerCell();
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

        private void AtMostInOneCol_Ladder() {

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
            return x * sudoku.N2 * sudoku.N2 + y * sudoku.N2 + value + 1;
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
