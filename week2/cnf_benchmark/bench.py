import os
import subprocess
import time

targetPath = "../../cnf/week2_sudoku"
resPath = "./results"

files = os.listdir(targetPath)

print(files)

for file in files:
    command = f"../../kissat/build/kissat {targetPath}/{file} --quiet"
    output = None

    t0 = time.perf_counter_ns()
    try:
        output = subprocess.check_output(command, shell=True, stderr=subprocess.STDOUT)
    except subprocess.CalledProcessError as e:
        output = e.output
    
    t1 = time.perf_counter_ns()



    fileP = open(f"{resPath}/{file}_result.txt", "w")
    fileP.write(f"{t1 - t0}\n" + output.decode())
    fileP.close()
