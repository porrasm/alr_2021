import sys
import subprocess

targetCnf = sys.argv[1]

print(f"Target CNF: {targetCnf}")

command = f"../kissat/build/kissat {targetCnf} --quiet"
output = None

try:
    output = subprocess.check_output(command, shell=True, stderr=subprocess.STDOUT)
except subprocess.CalledProcessError as e:
    output = e.output.decode()



states = list()

if "UNSATISFIABLE" in output:
    print("Unsatisfiable")
else:
    print(output)

    lines = output.splitlines()

    varlist = list()
    for i in range(1, len(lines)):
        vars = lines[i].split(" ")
        for var in vars:
            if var == "v":
                continue
            varlist.append(int(var))
            if len(varlist) == 4:
                states.append(varlist)
                varlist = list()

print(states)

for state in states:
    print("------------------------------------------------------------------")
    if state[0] > 0:
        print("                r")
    else:
        print("    r")

    if state[1] > 0:
        print("                w")
    else:
        print("    w")

    if state[2] > 0:
        print("                c")
    else:
        print("    c")

    if state[3] > 0:
        print("                b")
    else:
        print("    b")
    print("------------------------------------------------------------------")