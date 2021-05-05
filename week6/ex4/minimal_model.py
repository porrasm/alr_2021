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

def check_model(model):
    for clause in clauses:
        one_true = False
        for lit in clause:
            mIndex = abs(lit) - 1
            if lit == model[mIndex]:
                one_true = True
                break
        if not one_true:
            return False
    return True

def get_smaller_models(model):
    newClauses = list()
    found_models = list()

    # keep negative assignments
    for lit in model:
        if lit < 0:
            newClauses.append([lit])
    disable_model(newClauses, model, True)

    while True:
        solver = Glucose4(bootstrap_with=clauses + newClauses)
        sat = solver.solve()
        smaller_model = solver.get_model()
        solver.delete()
        if not sat:
            break
        # append new model and disable it for future calls

        found_models.append(smaller_model)
        disable_model(newClauses, smaller_model)
        #disable_model(model_clauses, smaller_model)
    
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
        print("returning cause of duplicate")
        return

    print(f"Handling model of size {get_model_size(model)} at depth={depth}")
    disable_model(model_clauses, model)

    smaller_models = get_smaller_models(model)

    print(f"Found {len(smaller_models)} smaller models at depth={depth} minimal={len(minimal_models)}")

    if len(smaller_models) == 0:
        minimal_models.append(copy.deepcopy(model))
        print(f"Added minimal model at depth={depth} minimal={len(minimal_models)}")
        return
    
    for s in smaller_models:
        get_minimal_models2(min_models, s, depth + 1)
    

def find_new_minimal_models():
    global iteration
    iteration += 1
    if iteration > 200:
        return (False, [])
    solver = Glucose4(bootstrap_with=clauses + model_clauses)
    print(f"Solving with {len(clauses)} clauses, minimal model count so far: {len(minimal_models)} iteration={iteration}")
    if not solver.solve():
        return (False, None)
    min_models = list()
    print("Solved instance, finding minimal models...")
    get_minimal_models2(min_models, solver.get_model())
    solver.delete()
    return (True, min_models)
    



while True:
    res = find_new_minimal_models()
    if not res[0]:
        break

    minimal_models.extend(res[1])

    if not all:
        break

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