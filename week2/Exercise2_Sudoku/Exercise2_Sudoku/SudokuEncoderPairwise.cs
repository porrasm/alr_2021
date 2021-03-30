﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2_Sudoku {
    public class SudokuEncoderPairwise : CNFEncoder {
        private Sudoku sudoku;
        public override int VariableCount { get; }

        public SudokuEncoderPairwise(Sudoku sudoku) {
            this.sudoku = sudoku;
            VariableCount = sudoku.N2 * sudoku.N2 * sudoku.N2;

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
            AtMostOneInRowAndCol_Pairwise();
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

        private void AtMostOneInRowAndCol_Pairwise() {

            for (int i = 0; i < sudoku.N2; i++) {
                for (int val = 0; val < sudoku.N2; val++) {

                    for (int j1 = 0; j1 < sudoku.N2; j1++) {
                        for (int j2 = 0; j2 < sudoku.N2; j2++) {
                            if (j1 == j2) {
                                continue;
                            }

                            AddClause(-VarIndex(i, j1, val), -VarIndex(i, j2, val));
                            AddClause(-VarIndex(j1, i, val), -VarIndex(j2, i, val));
                        }
                    }
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