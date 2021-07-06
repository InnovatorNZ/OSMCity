$osmfile = Get-ChildItem -Path . -Filter *.osm
Copy-Item $osmfile .\OSMCityReader\OSMReader4\out\production\OSMReader4\xml.osm -Force
cd .\OSMCityReader\OSMReader4\out\production\OSMReader4\
echo "Preprocessing OSM data..."
java com.osmreader4.Main > output.log
cd ..\..\..\..\..
Move-Item .\OSMCityReader\OSMReader4\out\production\OSMReader4\osm.txt .\OSMCityCoder\OSMCityCoder\bin\x64\Release\osm.txt -Force
cd .\OSMCityCoder\OSMCityCoder\bin\x64\Release\
.\OSMCityCoder.exe
Move-Item block.txt ..\..\..\..\..\OSMCityGenerator\ -Force
cd ..\..\..\..\..\OSMCityGenerator\
Remove-Item -Path .\OSMCustomed -Recurse -Force
Expand-Archive -Path .\OSMCustomed.zip -DestinationPath .\OSMCustomed
echo "Generating Minecraft saves..."
python main.py
Remove-Item -Path $env:APPDATA\..\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\OSMCustomed -Recurse -Force
Copy-Item -Path .\OSMCustomed -Destination $env:APPDATA\..\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds\ -Recurse -Force
echo "Export to Minecraft Done! Open your MinecraftBE and enjoy."
cd ..