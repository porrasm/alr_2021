import sys
import copy
from pysat.formula import WCNF
from pysat.card import *
from pysat.solvers import Solver

class MSU3:

    k = 0
    selectors = list()
    smap = {}
    hard = list()
    soft = list()
    oracle = Solver(name='mc')
    topv = 0
    R = set()

    def __init__(self, wcnf):
        self.topv = wcnf.nv
        self.hard = copy.deepcopy(wcnf.hard)
        self.soft = copy.deepcopy(wcnf.soft)
        self.initOracle(False)

    def initOracle(self, withSoft=True):
        if self.oracle == None:
            oracle.delete()

        self.oracle = Solver(name='mc', bootstrap_with=self.hard)
        if withSoft:
            oracle.append_formula(self.soft)


    def solve(self):
        # unsolvable hards
        if not self.oracle.solve():
            return False

        self.selectors = list()
        self.smap = {}

        for i in range(len(self.soft)):
            self.topv += 1

            self.soft[i].append(self.topv)
            self.selectors.append(-self.topv)
            #self.oracle.add_clause(self.soft[i])

            self.smap[self.topv] = i 

        self.compute()
        self.oracle.delete()
        return self.k

    def compute(self):
        
        self.reinit_oracle()

        while True:
            print(f"SOLVING WITH K = {self.k}")
            if self.oracle.solve(assumptions=self.selectors):
                print(f"True with selectors: {self.selectors}")
                return
            print(f"FINISHED SOLVING WITH K = {self.k}")
            self.k += 1
            self.handle_core()
            
            #return
            
    def handle_core(self):
        core = self.oracle.get_core()
        print(f"\nCore: {core}")

        print(f"BEFORE: Ass size: {len(self.selectors)}")
        print(f"BEFORE: Car size: {len(self.R)}")
        for l in core:
            self.handle_core_literal(l)

        print(f"AFTER: Ass size: {len(self.selectors)}")
        print(f"AFTER: Car size: {len(self.R)}")

        self.reinit_oracle()

    def handle_core_literal(self, l):
        posl = l if l > 0 else -l
        negl = -l if l > 0 else l

        # add to cardinality
        #print(f"Adding {posl} to cardinality")
        self.R.add(posl)

        # remove from assumptions
        #print(f"Removing {negl} from assumptions")
        self.selectors.remove(negl)


    def reinit_oracle(self):
        if self.oracle == None:
            oracle.delete()

        self.oracle = Solver(name='mc', bootstrap_with=self.hard)
        self.oracle.append_formula(self.soft)

        rlist = self.r_to_list()

        print(f"Add atmost for R = {rlist} with k = {self.k}")
        self.oracle.add_atmost(lits=self.R, k = self.k)
        #self.oracle.append_formula(CardEnc.atmost(lits=rlist, bound=self.k, encoding=EncType.sortnetwrk).clauses)



    
    def r_to_list(self):
        l = list()
        for i in self.R:
            l.append(i)
        return l

path = sys.argv[1]
msu3 = MSU3(WCNF(from_file=path))
cost = msu3.solve()

print(f"Solved {path} with MSU3, k = {cost}")