import sys
import copy
from pysat.formula import WCNF
from pysat.card import *
from pysat.solvers import Solver
from pysat.solvers import Glucose4

class MSU3:

    k = 0
    selectors = list()
    hard = list()
    soft = list()
    oracle = Glucose4()
    topv = 0
    R = set()

    def __init__(self, wcnf):
        self.topv = wcnf.nv
        self.hard = copy.deepcopy(wcnf.hard)
        self.soft = copy.deepcopy(wcnf.soft)

    def solve(self):
        self.oracle.append_formula(self.hard)
        if not self.oracle.solve():
            self.oracle.delete()
            return False

        self.selectors = list()

        for i in range(len(self.soft)):
            self.topv += 1
            self.soft[i].append(self.topv)
            self.selectors.append(-self.topv)

        self.compute()
        self.oracle.delete()
        return self.k

    def compute(self):
        self.reinit_oracle()
        while True:
            if self.oracle.solve(assumptions=self.selectors):
                return
            self.k += 1
            self.handle_core()
            
    def handle_core(self):
        core = self.oracle.get_core()
        
        if core != None:
            print(len(core))
            for l in core:
                self.handle_core_literal(l)
        self.reinit_oracle()

    def handle_core_literal(self, l):
        posl = abs(l)
        negl = -abs(l)
        self.R.add(posl)
        self.selectors.remove(negl)

    def reinit_oracle(self):
        self.oracle.delete()
        self.oracle = Glucose4(bootstrap_with=self.hard)
        self.oracle.append_formula(self.soft)
        rlist = self.r_to_list()
        self.oracle.append_formula(CardEnc.atmost(lits=rlist, bound=self.k, top_id=self.topv).clauses)
        
    
    def r_to_list(self):
        l = list()
        for i in self.R:
            l.append(i)
        return l

path = sys.argv[1]
msu3 = MSU3(WCNF(from_file=path))
cost = msu3.solve()

if cost == False:
    print("s UNSATISFIABLE")
else:
    print(f"s OPTIMUM FOUND")
    print(f"o {cost}")