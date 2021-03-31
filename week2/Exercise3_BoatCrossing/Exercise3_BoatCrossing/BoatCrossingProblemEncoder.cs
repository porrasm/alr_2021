using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise3_BoatCrossing {
    public class BoatCrossingProblemEncoder : CNFEncoder {
        #region fields
        private const int OBJ_COUNT = 4;
        private int stepCount;
        private int k;

        public override int VariableCount { get; }

        public enum Object {
            Rabbit = 0,
            Wolf = 1,
            Carrots = 2,
            Boat = 3
        }
        #endregion

        public BoatCrossingProblemEncoder(int k) {
            this.k = k;
            stepCount = k + 1;
            VariableCount = stepCount * OBJ_COUNT;
            AddConstraints();
        }

        #region constraints
        private void AddConstraints() {
            AddFixedConstraints();
            AddStateConstraints();
            AllowMovementOnlyFromBoatSide();
            AddBoatConstraints();
            AddTransitionConstraints();
        }

        private void AddFixedConstraints() {
            // Init state
            AddClause(-GetVariable(Object.Rabbit, 0));
            AddClause(-GetVariable(Object.Wolf, 0));
            AddClause(-GetVariable(Object.Carrots, 0));
            AddClause(-GetVariable(Object.Boat, 0));

            // Goal state
            AddClause(GetVariable(Object.Rabbit, k));
            AddClause(GetVariable(Object.Wolf, k));
            AddClause(GetVariable(Object.Carrots, k));
            AddClause(GetVariable(Object.Boat, k));
        }

        private void AddStateConstraints() {
            for (int t = 0; t < stepCount; t++) {

                int r = GetVariable(Object.Rabbit, t);
                int w = GetVariable(Object.Wolf, t);
                int c = GetVariable(Object.Carrots, t);
                int b = GetVariable(Object.Boat, t);

                // Wolf cannot eat rabbit
                AddClause(-r, -w, c, b);
                AddClause(r, w, -c, -b);

                // Rabbit cannot eat carrots
                AddClause(-r, -c, w, b);
                AddClause(r, c, -w, -b);
            }
        }

        private void AllowMovementOnlyFromBoatSide() {
            for (int t = 0; t < k; t++) {
                for (int o = 0; o < (int)Object.Boat; o++) {

                    int xot = GetVariable(o, t);
                    int xot1 = GetVariable(o, t + 1);
                    int xbt = GetVariable(Object.Boat, t);

                    AddClause(xot, -xot1, -xbt);
                    AddClause(-xot, xot1, xbt);
                }
            }
        }

        private void AddBoatConstraints() {
            for (int t = 0; t < k; t++) {

                int b0 = GetVariable(Object.Boat, t);
                int b1 = GetVariable(Object.Boat, t + 1);

                // If boat on land then go to island
                AddClause(-b0, -b1);

                // If boat on island then go to land
                AddClause(b0, b1);
            }
        }

        private void AddTransitionConstraints() {
            for (int t = 0; t < k; t++) {

                int r0 = GetVariable(Object.Rabbit, t);
                int r1 = GetVariable(Object.Rabbit, t + 1);

                int w0 = GetVariable(Object.Wolf, t);
                int w1 = GetVariable(Object.Wolf, t + 1);

                int c0 = GetVariable(Object.Carrots, t);
                int c1 = GetVariable(Object.Carrots, t + 1);

                AddClause(-r1, r0, -w1, w0);
                AddClause(-r0, r1, -w1, w0);
                AddClause(-w1, w0, -c1, c0);
                AddClause(-w1, w0, -c0, c1);
                AddClause(-r1, r0, -w0, w1);
                AddClause(-r0, r1, -w0, w1);
                AddClause(-w0, w1, -c1, c0);
                AddClause(-w0, w1, -c0, c1);
                AddClause(-r1, r0, -c1, c0);
                AddClause(-r0, r1, -c1, c0);
                AddClause(-r1, r0, -c0, c1);
                AddClause(-r0, r1, -c0, c1);
            }
        }
        #endregion

        #region utility
        private int GetVariable(Object o, int timeStep) {
            return GetVariable((int)o, timeStep);
        }
        private int GetVariable(int o, int timeStep) {
            return timeStep * OBJ_COUNT + o + 1;
        }
        #endregion
    }
}
