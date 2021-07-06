Remove-Item xml.osm
$file = Get-ChildItem -Path . -Filter *.osm
Move-Item $file xml.osm