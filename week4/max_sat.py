import sys
import algorithms

method = sys.argv[1]
path = sys.argv[2]

def print_sol(sol):
    print(f"Got MUS of size {len(sol[1])} with {sol[0]} solver calls")

if method == "del" or method == "all":
    print_sol(algorithms.solve_del(path))
if method == "ins" or method == "all":
    print_sol(algorithms.solve_ins(path))
if method == "csr" or method == "all":
    print_sol(algorithms.solve_ins_csr(path))
