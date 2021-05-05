def find_gcd(orders, target):

    l = list()
    l.append(target)
    for o in range(1, len(orders)):
        l.append(orders[o][1])

    def gcd(a, b):
        while b:
            a, b = b, a%b
        return a
    n =1
    f = l[0]
    while n != len(l):
        f = gcd(f,l[n])
        if  f == 1:
            return 1
        else:
            n = n + 1          
    return f