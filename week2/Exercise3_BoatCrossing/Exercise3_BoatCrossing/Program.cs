using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise3_BoatCrossing {
    class Program {

        private const string cnfDirectory = "P:\\Stuff\\School\\alr_2021\\cnf\\week2_boat";

        static void Main(string[] args) {

            if (!Directory.Exists(cnfDirectory)) {
                Directory.CreateDirectory(cnfDirectory);
            }

            for (int i = 1; i <= 10; i++) {
                Console.WriteLine("Writing i: " + i);
                CNFEncoder problem = new BoatCrossingProblemEncoder(i);
                string cnf = problem.ToDIMACS();

                File.WriteAllText($"{cnfDirectory}\\k{i}.cnf", cnf);
            }
        }
    }
}
