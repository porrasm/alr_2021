import os
from pysat.formula import CNF
from pysat.solvers import Glucose3
from pysat.solvers import Minisat22
from pysat.solvers import Solver

calls = 0

def solve_CNF(clauses):
    global calls
    calls += 1
    solver = Glucose3(bootstrap_with=clauses)
    res = solver.solve()
    solver.delete()
    return res

def solve_CNF_core(clauses):
    global calls
    calls += 1
    solver = Minisat22(bootstrap_with=clauses)
    res = solver.solve()
    core = solver.get_core()
    solver.delete()
    return (res, core)


def solve_del(instance):
    print(f"Solving using deletion: {instance}", flush=True)
    mus = CNF(from_file=instance).clauses

    i = 0
    global calls
    calls = 0
    while i < len(mus):
        clause = mus.pop(i)

        if solve_CNF(mus):
            mus.insert(i, clause)
            i += 1
    
    return (calls, mus)

def solve_ins(instance):
    print(f"Solving using insertion: {instance}", flush=True)
    f = CNF(from_file=instance).clauses

    global calls
    calls = 0

    mus = list()

    while len(f) > 0:
        s = mus.copy()

        ci = 0
        clause = None
        while solve_CNF(s):
            clause = f[ci]
            s.append(clause)
            ci += 1
        
        if clause == None:
            break

        mus.append(clause)
        s.pop(-1)
        f = s

    return (calls, mus)

def solve_ins_csr(instance):
    print(f"Solving using insertion and clause-set refinement: {instance}", flush=True)
    f = CNF(from_file=instance).clauses

    global calls
    calls = 0

    mus = list()

    while len(f) > 0:
        solve_with = mus.copy()
        solve_with.extend(f)

        if len(solve_with) == 0:
            break

        c = solve_with.pop(-1)
        res = solve_CNF_core(solve_with)
        print(res)
        if res[0]:
            mus.append(c)
            f.pop(-1)
        else:
            f = res[1]
            for m in mus:
                f.remove(m)
    
    return (calls, mus)