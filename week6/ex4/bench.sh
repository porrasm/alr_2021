shopt -s extglob
for file in /mnt/p/Stuff/School/alr_2021/week6/ex4/cnf/*
do
  echo "############################## ${file//+(*\/|.*)} ############################## "
  echo

  time timeout 120s python3 minimal_model.py "$file" 1
done