import sys
import order_solver

maxval = int(sys.argv[1])
gcd = 1

if len(sys.argv) > 2:
    gcd = int(sys.argv[2])

target = gcd

max_target = 0
max_sol = (0, 0)

while target < maxval:
    print(f"Solving with {target} / {maxval}, maximum so far: {max_sol[0]}")
    sol = order_solver.solve_with(target)
    if sol[0] > max_sol[0]:
        max_sol = sol
        max_target = target
    target += gcd

print(f"Most solutions with target {max_target}")
print(max_sol)