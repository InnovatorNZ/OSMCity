using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_Trial {
	class Program {
		static void Main(string[] args) {
			Console.WriteLine(LatLonToCoordinate(48.87, 2.27, 48.85, 2.25, 1));
		}

		static Tuple<double, double> LatLonToCoordinate(double clat, double clon, double minlat, double minlon, double ratio) {
			//转换为相对经纬度
			double dlat = clat - minlat;
			double dlon = clon - minlon;
			//转换为相对坐标
			double currentz = dlat * 111195 / ratio;
			double currentx = dlon * Math.Cos(minlat * Math.PI / 180) * 111195 / ratio;

			Tuple<double, double> currentCoordinate = new Tuple<double, double>(currentx, currentz);
			return currentCoordinate;
		}
	}
}
