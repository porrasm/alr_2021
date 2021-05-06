import sys
from pysat.formula import CNF
from pysat.solvers import Glucose4
import copy

iteration = 0

instance = sys.argv[1]
all = bool(int(sys.argv[2]))

clauses = CNF(from_file=instance).clauses
model_clauses = list()
disabled_models = set()
oracle = Glucose4()

minimal_models = list()

def disable_model(clauselist, model, global_disable=False):
    #print("disabling found model")
    l = list()
    for lit in model:
        if lit > 0:
            l.append(-lit)
    clauselist.append(l)

    if global_disable:
        disabled_models.add(tuple(i for i in model))

def disable_model_in_solver(clauselist, model, global_disable=False):
    #print("disabling found model")
    clause = list()
    for lit in model:
        if lit > 0:
            clause.append(-lit)
    oracle.add_clause(clause)

    if global_disable:
        disabled_models.add(tuple(i for i in model))

def get_smaller_models(model):
    newClauses = list()
    found_models = list()

    # keep negative assignments
    for lit in model:
        if lit < 0:
            newClauses.append([lit])
    disable_model(newClauses, model, True)

    solver = Glucose4(bootstrap_with=clauses + newClauses)
    while True:
        sat = solver.solve()
        smaller_model = solver.get_model()
        
        if not sat:
            break
        # append new model and disable it for future calls

        found_models.append(smaller_model)
        #disable_model(newClauses, smaller_model)
        newClause = list()
        for lit in smaller_model:
            if lit > 0:
                newClause.append(-lit)
        solver.add_clause(newClause)
        disable_model_in_solver(model_clauses, smaller_model)
    
    solver.delete()
    return found_models

def get_model_size(model):
    size = 0
    for lit in model:
        if lit > 0:
            size += 1
    return size

def get_minimal_models2(min_models, model, depth=0):
    # Disable model and avoid duplicates
    if tuple(i for i in model) in disabled_models:
        #print("returning cause of duplicate")
        return

    #print(f"Handling model of size {get_model_size(model)} at depth={depth}")
    disable_model_in_solver(model_clauses, model)

    smaller_models = get_smaller_models(model)

    #print(f"Found {len(smaller_models)} smaller models at depth={depth} minimal={len(minimal_models)}")

    if len(smaller_models) == 0:
        if len(minimal_models) == 0:
            pModel = [lit for lit in model if lit > 0]
            print(f"One minimal model of size: {len(pModel)}")
        minimal_models.append(copy.deepcopy(model))
        if len(minimal_models) % 25 == 0:
            print(f"Found {len(minimal_models)} models")

        #print(f"Added minimal model at depth={depth} minimal={len(minimal_models)}")
        return
    
    for s in smaller_models:
        get_minimal_models2(min_models, s, depth + 1)
    

def find_new_minimal_models():
    global iteration
    iteration += 1
    #if iteration > 200:
    #    return (False, [])
    
    #print(f"Solving with {len(clauses)} clauses, minimal model count so far: {len(minimal_models)} iteration={iteration}")
    if not oracle.solve():
        return (False, None)
    min_models = list()
    #print("Solved instance, finding minimal models...")
    get_minimal_models2(min_models, oracle.get_model())
    
    return (True, min_models)
    

oracle = Glucose4(bootstrap_with=clauses)

while True:
    res = find_new_minimal_models()
    if not res[0]:
        break

    minimal_models.extend(res[1])

    if not all:
        break

oracle.delete()

print(f"Found {len(minimal_models)} minimal models:")

used = set()

for m in minimal_models:
    model_clauses.clear()
    t = tuple(i for i in m)
    if t in used:
        print("Duplicate found")
    used.add(t)

    min = get_smaller_models(m)
    if len(min) > 0:
        print(f"Unminimal in set: {len(min)}")
        model_clauses.clear()
        min2 = get_smaller_models(min[0])
        print(f"min2 count: {len(min2)}")