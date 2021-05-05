from pysat.formula import WCNF
from pysat.card import *
from pysat.solvers import Solver
from pysat.solvers import Glucose4
import math
import find_gcd
import sys

# Solve
def solve(clauses):
    solver = Glucose4(bootstrap_with=clauses)
    res = solver.solve()
    model = solver.get_model()
    solver.delete()
    return (res, model)

def disable_model(clauses, model):
    antimodel = list()
    for i in range(len(model)):
        antimodel.append(-model[i])
    clauses.append(antimodel)

def solve_with(target):
    print(f"Finding combinations with price of total {target}")

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
    

    # greatest common divisor improvement
    gcd = find_gcd.find_gcd(orders, target)
    #gcd = 20

    if gcd < 1:
        gcd = 1

    # prepare clauses
    itemcount = len(orders) - 1
    topv = itemcount

    auxlist = list()
    clauses = list()

    print("Initializing clauses...")
    for i in range(1, len(orders)):
        for j in range(0, orders[i][1] // gcd):
            topv += 1
            clauses.append([i, -topv])
            clauses.append([-i, topv])
            auxlist.append(topv)

    clauses.extend(CardEnc.equals(lits=auxlist, bound=target // gcd, top_id=topv, encoding=EncType.seqcounter))

    solutions = list()

    while True:
        print(f"Solving with {len(clauses)} clauses and {topv} variables...")
        result = solve(clauses)
        if not result[0]:
            break
        print("Found solution")
        model = result[1]
        disable_model(clauses, model)
        solutions.append(model[:itemcount])
    print("\n----------------------------------------------------------------\n")
    print(f"Solution count: {len(solutions)}, target: {target}")

    for sol in solutions:
        solString = "Solution      : "
        cost = 0
        for lit in sol:
            if lit > 0:
                solString += f"{orders[lit][0]} "
                cost += orders[lit][1]
        print(f"{solString}, cost: {cost}")
    
    return (len(solutions), solutions)

if __name__ == "__main__":
    target = int(sys.argv[1])
    solve_with(target)