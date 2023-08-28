import os
extension = '.cs'
result = [os.path.join(dp, f) for dp, dn, filenames in os.walk('.') for f in filenames if os.path.splitext(f)[1] == extension]
suma = 0
sumy = {}
for i in result:
    print(i)
    lenn = 0
    with open(i, 'r') as file:
        lenn = 0
        for lineee in file.readlines():
            lenn += len(lineee)
    print(lenn)
    suma += lenn

    x = i.split('\\')
    if not x[1] in sumy:
        sumy[x[1]] = lenn
    else:
        sumy[x[1]] += lenn
    
    

print(suma)
print(sumy)
