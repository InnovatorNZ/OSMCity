About these projects:
OSMCityPreview: Preview the reconstruction effect written in Unity. It's a Unity project, you should open it in unity. Newest version of OSMCity will show here in the first place.
OSMCityCoder: Encode the reconstruction part. It turn OSM file to an encoded block.txt file. This project is updated after a main version of OSMCityPreview is finished. It's a C# console application.
OSMCityReader: Read the osm file and turn it into osm.txt that OSMCityPreview or OSMCityCoder use. It's a Java console application.
OSMCityGenerator: Turn the block.txt generated in OSMCityCoder to a Minecraft world. Unzip the OSMCustomed.zip which is an empty world, and the program will turn it to generatored world. You can load this generated world in Minecraft BE above 1.14. It's a Python console application.
OSMCityPolygonalCylinder: This project is an experiment on mesh style instead of voxel/Minecraft style. Don't care this if you are only interested in Minecraft.
Use order:
0. Prepare an osm file and rename it to xml.osm and then put it into OSMCityReader/OSMReader4
1. Open OSMCityReader in Java IDE and compile and run OSMReader4.
2. Copy the generated osm.txt to OSMCityPreview/samples or OSMCityCoder\OSMCityCoder\bin\Release
3. To preview in Unity: Open OSMCityPreview in Unity, and then find the function getFileName(), change the file name to "osm.txt" (return "osm.txt")
To generate in Minecraft:
(1). Run OSMCityCoder.exe or open OSMCityCoder in Visual Studio and then compile and run it. The block.txt generated would be automatically moved into OSMCityGenerator.
(2). Run OSMCityGenerator in Python by executing "python main.py" or open it in Python IDE. Make sure you have unzipped the empty world in OSMCustomed.zip.