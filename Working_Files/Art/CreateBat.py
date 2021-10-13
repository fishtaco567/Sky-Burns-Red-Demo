import os
import subprocess

NAME = "CastleDecorTiles"

files = [f for f in os.listdir('.') if os.path.isfile(f)]

goodFiles = []

for file in files:
	if file.endswith(".aseprite"):
		output = "Aseprite {0} --sheet {1}.png -b --sheet-pack".format(file, os.path.splitext(file)[0])
		outputFile = open("createsheet.bat", "w+")
		outputFile.write(output)
		outputFile.close()
		subprocess.call('createsheet.bat')