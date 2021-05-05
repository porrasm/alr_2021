from pysat.formula import WCNF
from pysat.card import *
from pysat.solvers import Solver
from pysat.solvers import Glucose4
import math
import find_gcd
import sys

orders = list()
orders.append(None)

orders.append(["MF", 215])
orders.append(["FF", 275])
orders.append(["SS", 335])
orders.append(["HW", 355])
orders.append(["MS", 420])
orders.append(["SP", 580])
#orders.append(["BB", 655])

#orders.append(["MF", 5])
#orders.append(["FF", 10])
#orders.append(["SS", 15])

target = int(sys.argv[1])
print(f"Finding combinations with price of total {target}")

# greatest common divisor improvement
gcd = find_gcd.find_gcd(orders, target)

if gcd > 1:
    print(f"Found GCD of {gcd}")
    target = target // gcd
    for o in range(1, len(orders)):
        orders[o][1] = orders[o][1] // gcd

# prepare clauses
itemcount = len(orders) - 1
topv = itemcount

auxlist = list()
clauses = list()

print("Initializing clauses...")
for i in range(1, len(orders)):
    for j in range(0, orders[i][1]):
        topv += 1
        clauses.append([i, -topv])
        clauses.append([-i, topv])
        auxlist.append(topv)

clauses.extend(CardEnc.equals(lits=auxlist, bound=target, top_id=topv, encoding=EncType.seqcounter))

# Solve
def solve():
    solver = Glucose4(bootstrap_with=clauses)
    res = solver.solve()
    model = solver.get_model()
    solver.delete()
    return (res, model)

def disable_model(model):
    antimodel = list()
    for i in range(len(model)):
        antimodel.append(-model[i])
    clauses.append(antimodel)

solutions = list()

while True:
    print(f"Solving with {len(clauses)} clauses and {topv} variables...")
    result = solve()
    if not result[0]:
        break
    print("Found solution")
    model = result[1]
    disable_model(model)
    solutions.append(model[:itemcount])
print("\n----------------------------------------------------------------\n")
print(f"Solution count: {len(solutions)}")

for sol in solutions:
    solString = "Solution      : "
    for lit in sol:
        if lit > 0:
            solString += f"{orders[lit][0]} "
    print(solString)