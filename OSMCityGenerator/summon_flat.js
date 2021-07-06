for(var x=-1000; x<=5000; x++){
	for(var z=-1000; z<=5000; z++){
		setTile(x, 3, z, 1, 6);
		setTile(x, 2, z, 1, 6);
		setTile(x, 1, z, 3, 0);
		setTile(x, 0, z, 7, 0);
	}
}