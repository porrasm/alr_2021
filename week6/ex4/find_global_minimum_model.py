import sys
from pysat.card import *
from pysat.formula import CNF
from pysat.solvers import Glucose4

k = 0

instance = sys.argv[1]

cnf = CNF(from_file=instance)
clauses = cnf.clauses
oracle = Glucose4()
topv = cnf.nv
variables = list()
for i in range(1, topv + 1):
    variables.append(i)
cnf = None

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
    oracle = Glucose4(bootstrap_with=clauses)

add = 1

reset_oracle()
res = oracle.solve()

if not res:
    print("UNSAT")
    exit(0)

minimal = oracle.get_model()

maxK = get_model_size(minimal)
k = maxK // 2
minK = 0

while True:
    if k == maxK:
        break
    print(f"Trying to find solution of size: {k} / {maxK}")
    reset_oracle()
    oracle.append_formula(CardEnc.atmost(lits=variables, bound=k, top_id=topv,encoding=EncType.seqcounter).clauses)

    if oracle.solve():
        minimal = oracle.get_model()[:topv]
        maxK = get_model_size(minimal)
        k = minK + ((maxK - k) // 2)
        print(f"Found solution of size: {maxK}")
    else:
        minK = k
        k = maxK - ((maxK - k) // 2)

print(f"Found global minimum of size: {maxK}")
pModel = [lit for lit in minimal if lit > 0]
print(pModel)