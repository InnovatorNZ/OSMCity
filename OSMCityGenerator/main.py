import bedrock
import os

minX = -3000
maxX = 7000
minZ = -3000
maxZ = 7000
KeyErrorOccured = 0
NotImplementedErrorOccured = 0

print("Begin generating...")
f = open("block.txt")
with bedrock.World("OSMCustomed") as world:
    while True:
        str = f.readline()
        if str == '': break
        cblock = str.split(' ')
        try:
            x = int(cblock[0])
            y = int(cblock[1])
            z = int(cblock[2])
            id = cblock[3]
            data = int(cblock[4])
            if x < minX or x > maxX or y < 0 or y > 256 or z < minZ or z > maxZ: continue
            block = bedrock.Block(id, data)
            world.setBlock(x, y, z, block)
        except KeyError:
            KeyErrorOccured += 1
        except NotImplementedError:
            NotImplementedErrorOccured += 1
        except IndexError:
            print("ERROR: Format of block.txt not correct, maybe error occured when generating it")

if KeyErrorOccured > 0:
    print("ERROR: Range exceeded, to large for", KeyErrorOccured, "times")
if NotImplementedErrorOccured > 0:
    print("ERROR: Chunk version not supported, empty saves is not correct for", NotImplementedErrorOccured, "times")

print("Finished generating!")
