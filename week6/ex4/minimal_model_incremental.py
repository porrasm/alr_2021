import sys
from pysat.card import *
from pysat.formula import CNF
from pysat.solvers import Glucose4
import copy

k = 0

instance = sys.argv[1]

cnf = CNF(from_file=instance)
clauses = cnf.clauses
topv = cnf.nv
variables = list()
for i in range(1, topv + 1):
    variables.append(i)
cnf = None

forbidden = list()


minimal_models = list()

oracle = Glucose4()

def get_model_size(model):
    size = 0
    for lit in model:
        if lit > 0:
            #print(lit)
            size += 1
    return size

def is_minimal_model(model):
    newClauses = list()
    found_models = list()

    # keep negative assignments
    disableClause = list()
    for lit in model:
        disableClause.append(-lit)
        if lit < 0:
            newClauses.append([lit])
    newClauses.append(disableClause)

    solver = Glucose4(bootstrap_with=clauses + newClauses)
    res = solver.solve()
    

    if res:
        print(f"Found smaller model: {get_model_size(model)} -> {get_model_size(solver.get_model())}")

    solver.delete()
    return not res

def verify():
    global forbidden
    used = set()

    for m in minimal_models:
        t = tuple(i for i in m)
        if t in used:
            print("Duplicate found")
        used.add(t)

        if not is_minimal_model(model):
            print("Not minimal model")

def reset_oracle():
    global oracle
    oracle.delete()
    oracle = Glucose4(bootstrap_with=clauses + forbidden)

add = 1

def find_next_k(prevK):
    print(f"Finding next k after prevK={prevK}")
    global oracle
    k = 0
    while True:
        print(f"Try with k={k}")
        reset_oracle()
        oracle.append_formula(CardEnc.atleast(lits=variables, bound=prevK, top_id=topv).clauses)
        if k > 0:
            oracle.append_formula(CardEnc.atmost(lits=variables, bound=k - 1, top_id=topv).clauses)
        if oracle.solve():
            k = get_model_size(oracle.get_model()[:topv])
        else:
            oracle.delete()
            return k

reset_oracle()
while k <= topv:
    print(f"Solving with k={k} minimal={len(minimal_models)} forbidden={len(forbidden)}")
    
    oracle.append_formula(CardEnc.equals(lits=variables, bound=k, top_id=topv).clauses)

    if oracle.solve():
        print(f"Found solvable k={k}")
        model = oracle.get_model()[:topv]

        if get_model_size(model) != k:
            print(f"Invalid model size: {get_model_size(model)}")
            break

        minimal_models.append(copy.deepcopy(model))
        newClause = list()
        for lit in model:
            if lit > 0:
                newClause.append(-lit)
        oracle.add_clause(newClause)
        forbidden.append(newClause)
    else:
        k += 1
        reset_oracle()
        #k = find_next_k(k)

verify()