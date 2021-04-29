shopt -s extglob
for file in /mnt/p/Stuff/School/alr_2021/week5/wcnf/*
do
  echo "########################################## ${file//+(*\/|.*)}"
  echo
  echo "----------------- FM"
  time timeout 1s python3 fm.py "$file"
  echo
  echo "----------------- LSU"
  time timeout 1s python3 lsu.py "$file"
  echo
  echo "----------------- MSU3"
  time timeout 120s python3 msu3.py "$file"
  echo
done