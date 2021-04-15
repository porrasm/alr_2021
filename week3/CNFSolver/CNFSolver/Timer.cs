using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNFSolver {
    public class Timer {
        public static long Milliseconds => DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public static long PassedFrom(long millis) {
            return Milliseconds - millis;
        }
    }
}
