import matplotlib.pyplot as plt
import os
print(os.getcwd())

density = []
generation = []
with open('Life/data.txt', 'r') as file:
    next(file)  
    for line in file:
        parts = line.strip().split()
        density.append(float(parts[0].replace(',', '.')))  
        generation.append(int(parts[1]))

plt.figure(figsize=(10, 6))
plt.plot(density, generation , marker='o', linestyle='-', color='b')  
plt.xlabel('Density')  
plt.ylabel('Generation')     
plt.title('Dependence of Generation on Density')  
plt.grid(True)

plt.savefig('Life/plot.png', dpi=300, bbox_inches='tight')

plt.show()