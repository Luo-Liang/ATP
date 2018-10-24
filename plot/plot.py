import numpy as np
import matplotlib.pyplot as plt
import os
import sys
import csv

# Fixing random state for reproducibility
probeFolder = "../benchmarks"
graphType = "time"
if len(sys.argv) > 1:
    probeFolder = sys.argv[1]
    if len(sys.argv) > 2:
        graphType = sys.argv[2]

folders = [d for d in os.listdir(probeFolder) if d.endswith("FLDR")]

#foreach folder, generate a mini plot.
CNTR = 1
for folder in folders:
    folderPath = probeFolder + "/" + folder
    print("entering " + folderPath)
    #retrieve metadata file.
    optDict = {}
    with open(folderPath + "/metadata", 'r') as md:
        metadataContent = md.read()
        options = metadataContent.split(';')
        for opt in options:
            kv = opt.split(':')
            k = kv[0]
            v = kv[1]
            optDict[k] = v
            pass
        pass


    #t1 = np.arange(0.0, 5.0, 0.1)
    #t2 = np.arange(0.0, 5.0, 0.02)

    plt.figure(CNTR)
    #plt.subplot(211)
    #plt.plot(t1, f(t1), 'bo', t2, f(t2), 'k')

    #plt.subplot(212)
    #plt.plot(t2, np.cos(2*np.pi*t2), 'r--')
    #now foreach file, read in the matrix.
    #OPTIONS MAX_X, FILES
    dim = int(optDict['DIM'])
    max_x = int(optDict['MAX_X'])
    max_y = float(optDict['MAX_Y'])
    min_y = float(optDict['MIN_Y'])
    #x_skip = int(optDict['FREQ_X'])
    for sndr in range(0, dim):
        for recv in range(0, dim):
            file = folderPath + "/" + str(sndr) + "-" + str(recv)
            with open(file) as csvfile:
                fileContent = csv.reader(csvfile, delimiter=',')
                x = []
                y = []
                area = []
                for line in fileContent:
                    x = x + [int(line[0])]
                    y = y + [float(line[1])]
                    area = area + [0.2]
                    pass
           
                highlightYElement = 0 if len(y) == 0 else np.mean(y)
                plt.subplot(dim,dim,dim * sndr + recv + 1)
                if graphType == "histogram":
                    plt.xlim(min_y,max_y)
                    weights = np.ones_like(y) / float(len(y))
                    n, bins, patches = plt.hist(y,20,weights=weights)
                #plt.scatter(x,y,s=area)
                #for histogram, swap x and y
                    plt.plot([highlightYElement,highlightYElement],[0,1] , color='r')
                    pass
                elif graphType == "time":
                    starting = 0 if len(x) == 0 else x[-1] + 1
                    pad = [max_x] #range(starting + 1, starting + max_x - 0 if len(x) == 0 else x[-1])
                    #if len(pad) + len(x) > 2000:

                    x += pad
                    y += [0] * len(pad)
                    highlightY = [highlightYElement] * len(y)
                    plt.ylim(0,max_y)
                    plt.xlim(0,max_x)
                    plt.scatter(x,y,s=area)
                    plt.plot(x, highlightY, color='r')
                    pass

    #plt.tight_layout()
    plt.suptitle(folder)
    plt.subplots_adjust(left=0, right=1, bottom=0, top=0.95, wspace=None, hspace=None)
    plt.show()
    CNTR+=1


#N = 50
#x = np.random.rand(N)
#y = np.random.rand(N)
#area = np.pi * (15 * np.random.rand(N)) ** 2 # 0 to 15 point radii#plt.scatter(x, y, s=area, c=colors, alpha=0.5)
#plt.show()
