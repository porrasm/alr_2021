for file in /mnt/p/Stuff/School/alr_2021/week4/cnf/*
do
  echo "---------------------------------------------------------------------------------"
  time timeout 120s python3 max_sat.py del "$file" 
  echo 
  time timeout 120s python3 max_sat.py ins "$file" 
  echo 
done