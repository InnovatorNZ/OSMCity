using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMBuildingGenerator__CSharp_ {
    public class OSMCity {
        private Random rd = new Random();
        // 以下为常量
        private const int undefined = -1073741824;
        private const int inf = 1073741824;
        private const int MAX_LEVEL = 256;
        private readonly int min_level = 3;                     // 最矮楼层数
        private readonly int max_level = 7;                     // 最高楼层数
        private readonly int max_legal_level = 36;              // 最高允许楼层
        private readonly int max_small_level = 3;               // 小建筑最高楼层
        private readonly int cmplx_building_nodenum = 30;       // 复杂建筑物顶点数
        private readonly float cos_th = 0.5f;                   // 使用在屋顶和内饰生成内的
        private readonly bool skipKCheck = true;                // 对于简单建筑物跳过斜率检查
        private readonly bool DEBUG = false;                    // 调试模式
        private readonly bool refalist = false;                 // 延伸是否加入nodelist
        private readonly Block[][,] WallConfig = new Block[][,]{
        new [,] {
            {new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 2, true) },
            {new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(24, 0, false), new Block(24, 2, true) },
            {new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(24, 0, false), new Block(24, 2, true) },
            {new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(24, 0, false), new Block(24, 2, true) },
            {new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(44, 0, true), new Block(44, 0, true), new Block(24, 0, false), new Block(24, 2, true) },
            {new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 2, true) },
            {new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true) }
        },
        new [,] {
            {new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15) },
            {new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159,2,false,1,0,15) },
            {new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159,2,false,1,0,15) },
            {new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159,2,false,1,0,15) },
            {new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159,2,false,1,0,15) },
            {new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15), new Block(159,2,false,1,0,15) },
            {new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true) }
        },
        new [,] {
            {new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(155, 2, true)},
            {new Block(159,1,false,1,0,15), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159,1,false,1,0,15), new Block(155, 2, true)},
            {new Block(159,1,false,1,0,15), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159,1,false,1,0,15), new Block(155, 2, true)},
            {new Block(159,1,false,1,0,15), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159,1,false,1,0,15), new Block(155, 2, true)},
            {new Block(159,1,false,1,0,15), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159,1,false,1,0,15), new Block(155, 2, true)},
            {new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(159,1,false,1,0,15), new Block(155, 2, true)},
            {new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 2, true)}
        },
        new [,] {
            {new Block(24, 2, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(159, 8, false), new Block(159, 8, false), new Block(159, 8, false), new Block(159, 8, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(159, 8, false), new Block(20, 0, false), new Block(20, 0, false), new Block(159, 8, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(159, 8, false), new Block(20, 0, false), new Block(20, 0, false), new Block(159, 8, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(159, 8, false), new Block(159, 8, false), new Block(159, 8, false), new Block(159, 8, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true) }
        },
        new [,] {
            {new Block(24, 2, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(45, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(45, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(45, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(45, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false), new Block(24, 0, false) },
            {new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true), new Block(24, 2, true) }
        },
        new [,] {
            {new Block(155, 2, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false) },
            {new Block(155, 2, false), new Block(155, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(155, 0, false) },
            {new Block(155, 2, false), new Block(155, 0, false), new Block(45, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(45, 0, false), new Block(155, 0, false) },
            {new Block(155, 2, false), new Block(155, 0, false), new Block(45, 0, false), new Block(20, 0, false), new Block(20, 0, false), new Block(45, 0, false), new Block(155, 0, false) },
            {new Block(155, 2, false), new Block(155, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(45, 0, false), new Block(155, 0, false) },
            {new Block(155, 2, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false), new Block(155, 0, false) },
            {new Block(155, 1, true), new Block(155, 1, true), new Block(155, 1, true), new Block(155, 1, true), new Block(155, 1, true), new Block(155, 1, true), new Block(155, 1, true) }
        },
        new [,] {
            {new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false) },
            {new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false) },
            {new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false) },
            {new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false) },
            {new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false) },
            {new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,9,false) },
            {new Block(155,0,false), new Block(155,0,false),  new Block(155,0,false), new Block(159,9,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(159,9,false) }
        },
        new [,] {
            {new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,8,true), new Block(159,8,true), new Block(159,8,true), new Block(159,8,true), new Block(159,0,false), new Block(159,0,false) },
            {new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false) },
            {new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false) },
            {new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false) },
            {new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false) }
        },
        new [,] {
            {new Block(159,11,false,1,0,15), new Block(155,1,false), new Block(155,1,false), new Block(159,11,false,1,0,15), new Block(155,1,false), new Block(155,1,false), new Block(159,11,false,1,0,15), new Block(155,2,false)},
            {new Block(159,11,false,1,0,15), new Block(20,0,false), new Block(20,0,false), new Block(159,11,false,1,0,15), new Block(20,0,false), new Block(20,0,false), new Block(159,11,false,1,0,15), new Block(155,2,false) },
            {new Block(159,11,false,1,0,15), new Block(20,0,false), new Block(20,0,false), new Block(159,11,false,1,0,15), new Block(20,0,false), new Block(20,0,false), new Block(159,11,false,1,0,15), new Block(155,2,false) },
            {new Block(159,11,false,1,0,15), new Block(20,0,false), new Block(20,0,false), new Block(159,11,false,1,0,15), new Block(20,0,false), new Block(20,0,false), new Block(159,11,false,1,0,15), new Block(155,2,false) },
            {new Block(159,11,false,1,0,15), new Block(155,1,false), new Block(155,1,false), new Block(159,11,false,1,0,15), new Block(155,1,false), new Block(155,1,false), new Block(159,11,false,1,0,15), new Block(155,2,false)},
            {new Block(159,11,false,1,0,15), new Block(159,11,false,1,0,15), new Block(159,11,false,1,0,15), new Block(159,11,false,1,0,15), new Block(159,11,false,1,0,15), new Block(159,11,false,1,0,15), new Block(159,11,false,1,0,15), new Block(155,2,false) }
        },
        new [,] {
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(159,8,false) },
            {new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false), new Block(159,8,false) },
            {new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false), new Block(159,8,false) },
            {new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false), new Block(159,8,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(159,8,false) },
            {new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true) }
        },
        new [,] {
            {new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(155,2,false) },
            {new Block(24,0,false), new Block(102,0,false), new Block(102,0,false), new Block(24,0,false), new Block(155,2,false) },
            {new Block(24,0,false), new Block(102,0,false), new Block(102,0,false), new Block(24,0,false), new Block(155,2,false) },
            {new Block(24,0,false), new Block(102,0,false), new Block(102,0,false), new Block(24,0,false), new Block(155,2,false) },
            {new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(155,2,false) },
            {new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true) }
        },
        new [,] {
            {new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false) },
            {new Block(24,0,false), new Block(102,0,false), new Block(102,0,false), new Block(24,0,false), new Block(45,0,false), new Block(45,0,false), new Block(24,0,false) },
            {new Block(24,0,false), new Block(102,0,false), new Block(102,0,false), new Block(24,0,false), new Block(45,0,false), new Block(45,0,false), new Block(24,0,false) },
            {new Block(24,0,false), new Block(102,0,false), new Block(102,0,false), new Block(24,0,false), new Block(45,0,false), new Block(45,0,false), new Block(24,0,false) },
            {new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false), new Block(24,0,false) }
        },
        new [,] {
            {new Block(155,0,false), new Block(45,0,false), new Block(155,0,false), new Block(45,0,false), new Block(45,0,false), new Block(155,0,false), new Block(45,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,true), new Block(155,0,true), new Block(155,0,true), new Block(155,0,true), new Block(155,0,false) },
            {new Block(155,0,false), new Block(45,0,false), new Block(155,0,false), new Block(102,0,false), new Block(102,0,false), new Block(155,0,false), new Block(45,0,false) },
            {new Block(155,0,false), new Block(45,0,false), new Block(155,0,false), new Block(102,0,false), new Block(102,0,false), new Block(155,0,false), new Block(45,0,false) },
            {new Block(155,0,false), new Block(45,0,false), new Block(155,0,false), new Block(102,0,false), new Block(102,0,false), new Block(155,0,false), new Block(45,0,false) },
            {new Block(155,0,false), new Block(45,0,false), new Block(155,0,false), new Block(102,0,false), new Block(102,0,false), new Block(155,0,false), new Block(45,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false) }
        },
        new [,] {
            {new Block(155,0,false), new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(160,0,false), new Block(160,0,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false) },
            {new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true) }
        },
        new [,] {
            {new Block(216,0,false), new Block(216,0,false), new Block(216,0,false), new Block(216,0,false), new Block(216,0,false), new Block(216,0,false) },
            {new Block(216,0,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(216,0,false) },
            {new Block(216,0,false), new Block(155,1,false), new Block(102,0,false), new Block(102,0,false), new Block(155,1,false), new Block(216,0,false) },
            {new Block(216,0,false), new Block(155,1,false), new Block(102,0,false), new Block(102,0,false), new Block(155,1,false), new Block(216,0,false) },
            {new Block(216,0,false), new Block(155,1,false), new Block(102,0,false), new Block(102,0,false), new Block(155,1,false), new Block(216,0,false) },
            {new Block(216,0,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(216,0,false) },
            {new Block(216,0,false), new Block(216,0,false), new Block(216,0,false), new Block(216,0,false), new Block(216,0,false), new Block(216,0,false) },
            {new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true) }
        },
        new [,] {
            {new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false) },
            {new Block(24,2,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(24,2,false) },
            {new Block(24,2,false), new Block(155,1,false), new Block(102,0,false), new Block(102,0,false), new Block(155,1,false), new Block(24,2,false) },
            {new Block(24,2,false), new Block(155,1,false), new Block(102,0,false), new Block(102,0,false), new Block(155,1,false), new Block(24,2,false) },
            {new Block(24,2,false), new Block(155,1,false), new Block(102,0,false), new Block(102,0,false), new Block(155,1,false), new Block(24,2,false) },
            {new Block(24,2,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(155,1,false), new Block(24,2,false) },
            {new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false) },
            {new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true), new Block(155,1,true) }
        },
        new [,] {
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(24,2,false), new Block(102,0,false), new Block(102,0,false), new Block(24,2,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(24,2,false), new Block(102,0,false), new Block(102,0,false), new Block(24,2,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(24,2,false), new Block(102,0,false), new Block(102,0,false), new Block(24,2,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(24,2,false), new Block(155,0,false) },
            {new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false), new Block(155,0,false) },
            {new Block(24,2,true), new Block(24,2,true), new Block(24,2,true), new Block(24,2,true), new Block(24,2,true), new Block(24,2,true) }
        },
        new [,] {
            {new Block(43,0,false), new Block(43,0,false), new Block(43,0,false), new Block(43,0,false), new Block(35,7,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,7,true) },
            {new Block(35,0,false), new Block(102,0,false), new Block(102,0,false), new Block(35,0,false), new Block(35,7,true) },
            {new Block(35,0,false), new Block(102,0,false), new Block(102,0,false), new Block(35,0,false), new Block(35,7,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,7,true) }
        },
        new[,] {
            {new Block(155,0,false), new Block(236,7,true) },
            {new Block(20,0,false), new Block(236,7,true) },
            {new Block(20,0,false), new Block(236,7,true) },
            {new Block(20,0,false), new Block(236,7,true) },
            {new Block(20,0,false), new Block(236,7,true) },
            {new Block(20,0,false), new Block(236,7,true) },
            {new Block(155,0,false), new Block(236,7,true) }
        },
        new[,] {
            {new Block(159,9,false), new Block(159,9,false), new Block(159,9,false), new Block(159,9,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(20,0,false), new Block(20,0,false), new Block(20,0,false) },
            {new Block(159,9,false), new Block(159,9,false), new Block(159,9,false), new Block(159,9,false) }
        },
        new[,] {
            {new Block(35,7,false), new Block(35,7,false), new Block(35,7,false), new Block(35,7,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,7,false), new Block(20,0,false), new Block(20,0,false), new Block(35,7,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,7,false), new Block(35,7,false), new Block(35,7,false), new Block(35,7,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) }
        },
        new[,] {
            {new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,11,false,1,1,15), new Block(20,0,false), new Block(20,0,false), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(20,0,false), new Block(20,0,false), new Block(35,11,false,1,1,15), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) },
            {new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,11,false,1,1,15), new Block(20,0,false), new Block(20,0,false), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(20,0,false), new Block(20,0,false), new Block(35,11,false,1,1,15), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,11,false,1,1,15), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) }
        }
    };
        private readonly Block[][,][] WallConfig_v2 = new Block[][,][] {
        new [,]{
            { new[]{new Block(155,0),new Block(155,0),new Block(155,0)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(44,14)}, new[]{new Block(155,0),new Block(155,0),new Block(155,2)} },
            { new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(85,2)}, new[]{new Block(155,0),new Block(0,0),new Block(155,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0), new Block(0,0), new Block(155,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0), new Block(0,0), new Block(155,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0), new Block(0,0), new Block(155,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0),new Block(0,0),new Block(155,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0),new Block(0,0),new Block(155,2)} }
        },
        new[,] {
            { new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(20,0)}, new[]{new Block(20,0)}, new[]{new Block(35,7)}, new[]{new Block(35,7), new Block(35,0)} },
            { new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(44,14)}, new[]{new Block(35,7), new Block(35,0)} }
        },
        new[,] {
            { new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(155,2)} },
            { new[]{new Block(24,0), new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(24,0), new Block(101,0)}, new[]{new Block(24,0), new Block(155,2)} },
            { new[]{new Block(24,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(155,2)} },
            { new[]{new Block(24,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(155,2)} },
            { new[]{new Block(24,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(155,2)} },
            { new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(155,2)} }
        },
        new[,] {
            { new[]{new Block(24,2), new Block(44,9)}, new[]{new Block(24,2), new Block(44,9)}, new[]{new Block(24,2), new Block(44,9)}, new[]{new Block(24,2), new Block(44,9)}, new[]{new Block(24,2), new Block(24,0)} },
            { new[]{new Block(24,2), new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(24,2), new Block(101,0)}, new[]{new Block(24,2), new Block(24,0)} },
            { new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(24,2),new Block(24,0)} },
            { new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(24,2),new Block(24,0)} },
            { new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(24,2),new Block(24,0)} },
            { new[]{new Block(24,2)}, new[]{new Block(24,2)}, new[]{new Block(24,2)}, new[]{new Block(24,2)}, new[]{new Block(24,2),new Block(24,0)} }
        },
        new[,] {
            { new[]{new Block(155,0), new Block(44,14)}, new[]{new Block(155,0), new Block(44,14)}, new[]{new Block(155,0), new Block(44,14)}, new[]{new Block(155,0), new Block(44,14)}, new[]{new Block(155,0), new Block(24,2)} },
            { new[]{new Block(155,0), new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(155,0), new Block(101,0)}, new[]{new Block(155,0), new Block(24,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0),new Block(24,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0),new Block(24,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0),new Block(24,2)} },
            { new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0),new Block(24,2)} }
        },
        new[,] {
            { new[]{new Block(24,0)}, new[]{new Block(24,0)},         new[]{new Block(24,0)},          new[]{new Block(24,0)},          new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(156,0) }, new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0),new Block(156,1) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},         new[]{new Block(24,0)},          new[]{new Block(24,0)},          new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(160,0,2,0,15)}, new[]{new Block(160,0,2,0,15) }, new[]{new Block(160,0,2,0,15) }, new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(128,5)},        new[]{new Block(160,0,2,0,15) }, new[]{new Block(128,4) },        new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(128,5)},        new[]{new Block(160,0,2,0,15) }, new[]{new Block(128,4) },        new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(24,0)},         new[]{new Block(24,0)},          new[]{new Block(24,0)},          new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0)},                   new[]{new Block(24,0)}, new[]{new Block(24,0)},         new[]{new Block(24,0)},          new[]{new Block(24,0)},          new[]{new Block(24,0)},  },
            { new[]{new Block(24,0)}, new[]{new Block(24,0)},         new[]{new Block(24,0)},          new[]{new Block(24,0)},          new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(156,4) }, new[]{new Block(24,0),new Block(155,2) }, new[]{new Block(24,0),new Block(156,5) }, new[]{new Block(24,0)}, new[]{new Block(24,0)},         new[]{new Block(24,0)},          new[]{new Block(24,0)},          new[]{new Block(24,0)},  },
            { new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)}, new[]{new Block(24,2), new Block(24,2)} }
        },
        new[,] {
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0),new Block(156,4)}, new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(155,0),new Block(156,5) }, new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(155,0),new Block(101,0) }, new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(156,7),new Block(44,14)}, new[]{new Block(156,7),new Block(44,14)}, new[]{new Block(155,0),new Block(156,7)},  new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(45,0),new Block(156,0)},  new[]{new Block(45,0),new Block(156,7)},  new[]{new Block(45,0),new Block(156,7)},  new[]{new Block(45,0),new Block(156,1)},   new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(45,0)},                   new[]{new Block(45,0)},                   new[]{new Block(45,0)},                   new[]{new Block(45,0)},                    new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(45,0)}, new[]{new Block(45,0)}, new[]{new Block(45,0)},                   new[]{new Block(45,0)},                   new[]{new Block(45,0)},                   new[]{new Block(45,0)},                    new[]{new Block(45,0)}, new[]{new Block(45,0) }, new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},new[]{new Block(155,0)},new[] {new Block(155,0)},                 new[]{new Block(155,0)},                  new[]{new Block(155,0)},                  new[]{new Block(155,0)},                   new[]{new Block(155,0)},new[]{new Block(155,0)}, new[]{new Block(155,0)} },
        },
        new[,] {
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0),new Block(156,4)}, new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(155,0),new Block(156,5) }, new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(155,0),new Block(101,0) }, new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0) },                 new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(155,0) },                  new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(156,7),new Block(44,14)}, new[]{new Block(156,7),new Block(44,14)}, new[]{new Block(155,0),new Block(156,7)},  new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0),new Block(156,0)},  new[]{new Block(24,0),new Block(156,7)},  new[]{new Block(24,0),new Block(156,7)},  new[]{new Block(24,0),new Block(156,1)},   new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0)},                   new[]{new Block(24,0)},                   new[]{new Block(24,0)},                    new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)}, new[]{new Block(24,0)},                   new[]{new Block(24,0)},                   new[]{new Block(24,0)},                   new[]{new Block(24,0)},                    new[]{new Block(24,0)}, new[]{new Block(24,0) } },
            { new[]{new Block(155,0)}, new[]{new Block(155,0)},new[]{new Block(155,0)},new[]{new Block(155,0)},                  new[]{new Block(155,0)},                  new[]{new Block(155,0)},                  new[]{new Block(155,0)},                   new[]{new Block(155,0)},new[]{new Block(155,0)} },
        },
        new[,] {
            { new[]{new Block(24,0),new Block(0,0),new Block(101,0)}, new[]{new Block(24,0),new Block(0,0),new Block(101,0)}, new[]{new Block(160,0),new Block(0,0),new Block(101,0)}, new[]{new Block(160,0),new Block(0,0),new Block(101,0)}, new[]{new Block(24,0),new Block(0,0),new Block(101,0)}, new[]{new Block(24,0),new Block(0,0),new Block(101,0)}, new[]{new Block(24,2),new Block(0,0),new Block(101,0)} },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0),new Block(128,7) },               new[]{new Block(160,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(24,0),new Block(128,7)},                new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0),new Block(128,0) },               new[]{new Block(24,0),new Block(128,7)},                 new[]{new Block(24,0),new Block(128,7)},                 new[]{new Block(24,0),new Block(128,1)},                new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,0),new Block(44,1)},                  new[]{new Block(24,0),new Block(44,1)},                  new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0),new Block(101,0)},                new[]{new Block(128,3),new Block(101,0)},                new[]{new Block(128,3),new Block(101,0)},                new[]{new Block(24,0),new Block(101,0)},                new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(160,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0)},                                 new[]{new Block(24,0),new Block(128,7)},                new[]{new Block(128,7)},                                 new[]{new Block(128,7)},                                 new[]{new Block(24,0),new Block(128,7)},                new[]{new Block(24,0)},                                 new[]{new Block(24,2)}                                 },
            { new[]{new Block(24,0),new Block(44,9),new Block(44,9)}, new[]{new Block(24,0),new Block(24,0),new Block(128,7)},new[]{new Block(24,0),new Block(128,7),new Block(44,9)}, new[]{new Block(24,0),new Block(128,7),new Block(44,9)}, new[]{new Block(24,0),new Block(24,0),new Block(128,7)},new[]{new Block(24,0),new Block(44,9),new Block(44,9) },new[]{new Block(24,0),new Block(44,9),new Block(44,9)} }
        },
        new[,] {
            { new[]{new Block(24,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(160,0),new Block(101,0)}, new[]{new Block(24,0),new Block(101,0)}, new[]{new Block(159,4) } },
            { new[]{new Block(24,0)},                  new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(24,0) },                 new[]{new Block(159,4) } },
            { new[]{new Block(24,0)},                  new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(24,0) },                 new[]{new Block(159,4) } },
            { new[]{new Block(24,0)},                  new[]{new Block(160,0) },                 new[]{new Block(160,0) },                 new[]{new Block(24,0) },                 new[]{new Block(159,4) } },
            { new[]{new Block(24,0)},                  new[]{new Block(24,2) },                  new[]{new Block(24,2) },                  new[]{new Block(24,0) },                 new[]{new Block(159,4) } },
            { new[]{new Block(24,0),new Block(128,7)}, new[]{new Block(24,0),new Block(128,7)},  new[]{new Block(24,0),new Block(128,7)},  new[]{new Block(24,0),new Block(128,7)}, new[]{new Block(159,4),new Block(128,7)} }
        },
        new[,] {
            { new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(155,0)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(155,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(155,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(155,0),new Block(156,7)},  new[]{new Block(155,0),new Block(156,7)},  new[]{new Block(155,0),new Block(156,7)}, new[]{new Block(155,0),new Block(156,7)} }
        },
        new[,] {
            { new[]{new Block(155,0), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(155,0), new Block(101,0)}, new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(128,7)},                   new[]{new Block(128,7)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0), new Block(128,4)}, new[]{new Block(155,0), new Block(128,7)}, new[]{new Block(155,0), new Block(128,7)}, new[]{new Block(155,0), new Block(128,5)}, new[]{new Block(155,0)} },
            { new[]{new Block(155,0), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(155,0), new Block(101,0)}, new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(128,7)},                   new[]{new Block(128,7)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)},                   new[]{new Block(155,0)} },
            { new[]{new Block(155,0), new Block(128,7)}, new[]{new Block(155,0), new Block(128,7)}, new[]{new Block(155,0), new Block(128,7)}, new[]{new Block(155,0), new Block(128,7)}, new[]{new Block(155,0), new Block(128,7)} }
        },
        new[,] {
            { new[]{new Block(159,0,1,0,15), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(159,0,1,0,15), new Block(101,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(128,7)},                   new[]{new Block(128,7)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15), new Block(128,4)}, new[]{new Block(159,0,1,0,15), new Block(128,7)}, new[]{new Block(159,0,1,0,15), new Block(128,7)}, new[]{new Block(159,0,1,0,15), new Block(128,5)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(160,0), new Block(101,0)}, new[]{new Block(159,0,1,0,15), new Block(101,0)}, new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(160,0)},                   new[]{new Block(160,0)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(128,7)},                   new[]{new Block(128,7)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)},                   new[]{new Block(159,0,1,0,15)} },
            { new[]{new Block(159,0,1,0,15), new Block(128,7)}, new[]{new Block(159,0,1,0,15), new Block(128,7)}, new[]{new Block(159,0,1,0,15), new Block(128,7)}, new[]{new Block(159,0,1,0,15), new Block(128,7)}, new[]{new Block(159,0,1,0,15), new Block(128,7)} }
        },
        new[,] {
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)},         new[]{new Block(128,7)},        new[]{new Block(128,7)},       new[]{new Block(24,0)},        new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)},         new[]{new Block(128,7)},        new[]{new Block(128,7)},       new[]{new Block(24,0)},        new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2), new Block(128,7)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2), new Block(128,7)}, new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,2), new Block(128,7)}, new[]{new Block(160,0)}, new[]{new Block(160,0)}, new[]{new Block(24,2), new Block(128,7)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15), new Block(128,0)}, new[]{new Block(159,0,1,0,15), new Block(24,0)}, new[]{new Block(159,0,1,0,15), new Block(24,0)}, new[]{new Block(159,0,1,0,15), new Block(128,1)}, new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15), new Block(128,0)}, new[]{new Block(159,0,1,0,15), new Block(24,0)}, new[]{new Block(159,0,1,0,15), new Block(24,0)}, new[]{new Block(159,0,1,0,15), new Block(128,1)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)},
                new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)},new[]{new Block(159,0,1,0,15)}, new[]{new Block(159,0,1,0,15)}, new[]{new Block(24,0)}
            },
            {
                new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(128,7)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(128,7)}, new[]{new Block(24,0), new Block(44,9)},
                new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(128,7)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(128,7)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(44,9)}, new[]{new Block(24,0), new Block(128,7)}
            }
        }
    };
        private readonly Block[] BaseBlock = {
        new Block(24,0,false), new Block(159,2,false,1,0,15), /*new Block(159,3,false), new Block(159,4,false), new Block(159,5,false),*/ new Block(159,1,false,1,0,15),
        new Block(24,0,false), new Block(24,0,false), new Block(155,0,false), new Block(155,0,false), new Block(159,0,false), /*new Block(159,0,false),*/ new Block(159,11,false,1,0,15),
        new Block(155,0,false), new Block(24,0,false), new Block(24,0,false), new Block(155,0,false), new Block(155,0,false), new Block(216,0,false), new Block(24,2,false), new Block(155,0,false), new Block(35,0,false),
        new Block(155,0,false), new Block(159,9,false), new Block(35,7,false), new Block(35,11,false,1,1,15), /*new Block(35,1,false), new Block(35,3,false), new Block(35,5,false), new Block(35,6,false)*/
    };
        private readonly Block[] BaseBlock_v2 = {
        new Block(155,0), new Block(35,7), new Block(24,0), new Block(24,2), new Block(155,0), new Block(24,0), new Block(45,0), new Block(24,0), new Block(24,0)
        , new Block(24,0), new Block(155,0), new Block(155,0), new Block(159,0,1,0,15), new Block(159,0,1,0,15)
    };
        private readonly Roof?[] RoofConfig = {
        null,
        null,
        new Roof(
            new Block(155,1), new Block(102,0), null,
            new Block[,,] {
                //方向 外->里然后下->上
                { { B, air, air, air, air }, { air, B, air, air, air }, { air, air, B, air, air }, { air, air, air, B, air}, { air, air, air, air, B} },
                { { B, air, air, air, air }, { air, B, air, air, air }, { air, air, B, air, air }, { air, air, air, B, air}, { air, air, air, air, B} },
                { { B, air, air, air, air }, { air, B, air, air, air }, { air, air, B, air, air }, { air, air, air, B, air}, { air, air, air, air, B} },
            }
        ),
        new Roof(
            new Block(155,1), new Block(102,0), null,
            new Block[,,] {
                { { B, air, air, air, air }, { air, B, air, air, air }, { air, air, B, air, air }, { air, air, air, B, air}, { air, air, air, air, B} },
                { { B, air, air, air, air }, { air, B, air, air, air }, { air, air, B, air, air }, { air, air, air, B, air}, { air, air, air, air, B} },
                { { B, air, air, air, air }, { air, B, air, air, air }, { air, air, B, air, air }, { air, air, air, B, air}, { air, air, air, air, B} },
            }
        ),
        new Roof(
            new Block(45,0), new Block(102,0), new Block(45,0),
            new Block[,,] {
                { { B, air, air, air, air, air, air }, { air, B, air, air, air, air, air }, { air, air, B, air, air, air, air }, { air, air, air, B, air, air, air}, { air, air, air, air, B, air, air}, {air,air,air,air,air,B,air }, {air,air,air,air,air,air,B } },
                { { B, air, air, air, air, air, air }, { air, B, air, air, air, air, air }, { air, air, B, air, air, air, air }, { air, air, air, B, air, air, air}, { air, air, air, air, B, air, air}, {air,air,air,air,air,B,air }, {air,air,air,air,air,air,B } },
                { { B, air, air, air, air, air, air }, { air, B, air, air, air, air, air }, { air, air, B, air, air, air, air }, { air, air, air, B, air, air, air}, { air, air, air, air, B, air, air}, {air,air,air,air,air,B,air }, {air,air,air,air,air,air,B } },
            }
        ),
        new Roof(
            new Block(24,2), new Block(160,4), null,
            new Block[,,] {
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,WF,WF,WF,air},   {air,air,air,WF,WF,WF,air},  {air,air,air,air,WF,WF,air}, {air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,W,W,WF,WF},      {air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,W,W,WF,WF},      {air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,WF,WF,WF,air},   {air,air,air,WF,WF,WF,air},  {air,air,air,air,WF,WF,air}, {air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
            }
        ),
        new Roof(
            new Block(24,2), new Block(102,0), null,
            new Block[,,] {
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,WF,WF,WF,air},   {air,air,air,WF,WF,WF,air},  {air,air,air,air,WF,WF,air}, {air,air,air,air,air,WF,air},{air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,W,W,WF,WF},      {air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,W,W,WF,WF},      {air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,WF,WF,WF,air},   {air,air,air,WF,WF,WF,air},  {air,air,air,air,WF,WF,air}, {air,air,air,air,air,WF,air},{air,air,air,air,air,air,B} },
            }
        ),
        new Roof(
            new Block(155,1), new Block(160,0), null,
            new Block[,,] {
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,WF,WF,WF,air},   {air,air,air,WF,WF,WF,air},  {air,air,air,air,WF,WF,air}, {air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,W,W,WF,WF},      {air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,W,W,WF,WF},      {air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,WF,WF,WF,WF,air},   {air,air,air,WF,WF,WF,air},  {air,air,air,air,WF,WF,air}, {air,air,air,air,air,WF,air},{air,air,air,air,air,air,WF} },
            }
        ),
        new Roof(
            new Block(45,0), new Block(102,0), null,
            new Block[,,] {
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,air,air,air,air}, {air,air,air,B,air,air,air}, {air,air,air,air,B,air,air}, {air,air,air,air,air,B,air}, {air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,B,B,B,air},   {air,air,air,B,B,B,air},  {air,air,air,air,B,B,air}, {air,air,air,air,air,B,air},{air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,W,W,B,B},      {air,air,air,air,air,B,air},{air,air,air,air,air,B,air},{air,air,air,air,air,B,air},{air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,W,W,B,B},      {air,air,air,air,air,B,air},{air,air,air,air,air,B,air},{air,air,air,air,air,B,air},{air,air,air,air,air,air,B} },
                { {B,air,air,air,air,air,air}, {air,B,air,air,air,air,air}, {air,air,B,B,B,B,air},   {air,air,air,B,B,B,air},  {air,air,air,air,B,B,air}, {air,air,air,air,air,B,air},{air,air,air,air,air,air,B} },
            }
        ),
        new Roof(
            new Block(45,0), new Block(102,0), new Block(45,0),
            new Block[,,] {
                { { new Block(44,4),air,air,air}, { new Block(44,12),air,air,air}, { air,new Block(44,4),air,air}, { air,new Block(44,12),air,air}, { air,air,new Block(44,4),air }, { air,air,new Block(44,12),air}, { air,air,air,new Block(44,4) }, { air,air,air,new Block(44,12) } },
                { { new Block(44,4),air,air,air}, { new Block(44,12),air,air,air}, { air,new Block(44,4),air,air}, { air,new Block(44,12),air,air}, { air,air,new Block(44,4),air }, { air,air,new Block(44,12),air}, { air,air,air,new Block(44,4) }, { air,air,air,new Block(44,12) } },
                { { new Block(44,4),air,air,air}, { new Block(44,12),air,air,air}, { air,new Block(44,4),air,air}, { air,new Block(44,12),air,air}, { air,air,new Block(44,4),air }, { air,air,new Block(44,12),air}, { air,air,air,new Block(44,4) }, { air,air,air,new Block(44,12) } },
            }
        ),
        new Roof(
            new Block(24,0), new Block(102,0), new Block(35,7),
            new Block[,,] {
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {B, B, B, air,air,air},                            {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {WF, WF, WF, WF, new Block(128,0), air},           {B, B, B, B, B, air } },
                { { WF,air,air,air,air,air }, {WF, W, W, W, WF, new Block(44,1)},                {air,air,air,air, B, air} },
                { { WF,air,air,air,air,air }, {WF, W, W, W, WF, new Block(44,1)},                {air,air,air,air, B, air} },
                { { WF,air,air,air,air,air }, {WF, WF,WF,WF, new Block(128,1), air},             {air,air,air,air, B, air} },
            }
        )
    };
        private readonly FirstFloor?[] FirstFloorConfig = {
        null,
        null,
        null,
        new FirstFloor(
            new Block(159,7), new Block(160,8), null, null,
            new Block[,,] {
                { {B,air,air}, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {air,air,air }, {air,air,air }, {B,air,air } },
                { {B,air,air}, {W,air,air }, {W,air,air }, {W,air,air }, {W,air,air }, {B,air,air }, {air,air,air }, {air,air,air }, {B,air,air } },
                { {B,air,air}, {W,air,air }, {W,air,air }, {W,air,air }, {W,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air } },
                { {B,air,air}, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air }, {B,air,air } },
                { {B,air,new Block(35,4)}, {B,air,new Block(35,0)}, {B,air,new Block(35,4)}, {B,air,new Block(35,0)}, {B,air,new Block(35,4)}, {B,air,new Block(35,0)}, {B,air,new Block(35,4)}, {B,air,new Block(35,0)}, {B,air,new Block(35,4)} },
                { {B,new Block(35,4),air}, {B,new Block(35,0),air}, {B,new Block(35,4),air}, {B,new Block(35,0),air}, {B,new Block(35,4),air}, {B,new Block(35,0),air}, {B,new Block(35,4),air}, {B,new Block(35,0),air}, {B,new Block(35,4),air} }
            }
        ),
        new FirstFloor(
            new Block(159,7), new Block(160,8), null, null,
            new Block[,,] {
                { {new Block(162,1),air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {new Block(162,1),air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {new Block(162,1),air,air}, {B,air,air}, {air,air,air}, {air,air,air}, {B,air,air}, {new Block(162,1),air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {B,air,air} },
                { {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {new Block(162,1),air,air}, {B,air,air}, {air,air,air },{air,air,air}, {B,air,air}, {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air} },
                { {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {new Block(162,1),air,air}, {B,air,air}, {air,air,air },{air,air,air}, {B,air,air}, {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air} },
                { {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {new Block(162,1),air,air}, {B,air,air}, {B,air,air },  {B,air,air},   {B,air,air}, {new Block(162,1),air,air}, {W,air,air}, {W,air,air}, {W,air,air}, {W,air,air} },
                { {new Block(162,1),air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {new Block(162,1),air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {new Block(162,1),air,air}, {B,air,air}, {B,air,air},   {B,air,air},   {B,air,air}, {new Block(162,1),air,air}, {B,air,air}, {B,air,air}, {B,air,air}, {B,air,air} },
                { {new Block(162,1),air,new Block(134,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {new Block(162,1),air,new Block(134,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {new Block(162,1),air,new Block(134,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {new Block(162,1),air,new Block(134,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)}, {B,air,new Block(53,3)} },
                { {new Block(162,1),new Block(134,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {new Block(162,1),new Block(134,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {new Block(162,1),new Block(134,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {new Block(162,1),new Block(134,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air}, {B,new Block(53,3),air} }
            }
        ),
        new FirstFloor(
            null, new Block(102,0), new Block(35,11), new Block(35,3),
            new Block[,,] {
                {
                    {B,air,air,air,new Block(114,4),new Block(114,4),new Block(114,4)}, {B,air,air,air,new Block(53,1),new Block(53,1),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,new Block(156,4),new Block(156,4),new Block(114,7)}, {B,air,air,air,new Block(156,5),new Block(156,5),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,new Block(53,0),new Block(53,0),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {air,air,air,air,air,new Block(58,0),new Block(114,7)}, {air,air,air,air,air,new Block(54,0),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)},
                                                                                        {B,air,air,air,new Block(53,1),new Block(53,1),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,new Block(156,4),new Block(156,4),new Block(114,7)}, {B,air,air,air,new Block(156,5),new Block(156,5),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,new Block(53,0),new Block(53,0),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,air,new Block(58,0),new Block(114,7)},   {B,air,air,air,air,new Block(54,0),new Block(114,7)},   {B,air,air,air,air,air,new Block(114,7)},
                                                                                        {B,air,air,air,new Block(53,1),new Block(53,1),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,new Block(156,4),new Block(156,4),new Block(114,7)}, {B,air,air,air,new Block(156,5),new Block(156,5),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,new Block(53,0),new Block(53,0),new Block(114,7)}, {B,air,air,air,air,air,new Block(114,7)}, {B,air,air,air,air,new Block(58,0),new Block(114,7)},   {B,air,air,air,air,new Block(54,0),new Block(114,7)},   {B,air,air,air,air,air,new Block(114,7)},
                    {B,air,air,air,new Block(114,5),new Block(114,5),new Block(114,5)}
                },
                {
                    {B,air,air,air,new Block(113,0),air,new Block(113,0)},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,new Block(463,0),air},                           {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {air,air,air,air,air,new Block(117,0),new Block(113,0)},{air,air,air,air,air,air,air},                          {B,air,air,air,air,air,air},
                                                                                        {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,new Block(171,0),new Block(171,12),air},             {W,air,air,air,new Block(171,12),new Block(171,0),new Block(113,0)},{B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,new Block(117,0),new Block(113,0)},  {W,air,air,air,air,air,air},                            {B,air,air,air,air,air,air},
                                                                                        {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,new Block(463,0),air},                           {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,new Block(117,0),new Block(113,0)},  {W,air,air,air,air,air,air},                            {B,air,air,air,air,air,air},
                    {B,air,air,air,new Block(113,0),air,new Block(113,0)}
                },
                {
                    {B,air,air,air,new Block(113,0),air,new Block(113,0)},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                        {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {air,air,air,air,air,air,new Block(113,0)},             {air,air,air,air,air,air,air},                          {B,air,air,air,air,air,air},
                                                                                        {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                        {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,new Block(113,0)},               {W,air,air,air,air,air,air},                            {B,air,air,air,air,air,air},
                                                                                        {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                        {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,new Block(113,0)},               {W,air,air,air,air,air,air},                            {B,air,air,air,air,air,air},
                    {B,air,air,air,new Block(113,0),air,new Block(113,0)}
                },
                {
                    {B,air,air,air,new Block(113,0),air,new Block(113,0)},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                        {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {air,air,air,air,air,air,new Block(113,0)},             {air,air,air,air,air,air,air},                          {B,air,air,air,air,air,air},
                                                                                        {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                        {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,new Block(113,0)},               {W,air,air,air,air,air,air},                            {B,air,air,air,air,air,air},
                                                                                        {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                        {W,air,air,air,air,air,new Block(113,0)},                           {B,air,air,air,air,air,air},              {W,air,air,air,air,air,air},                                      {W,air,air,air,air,air,air},              {W,air,air,air,air,air,new Block(113,0)},               {W,air,air,air,air,air,air},                            {B,air,air,air,air,air,air},
                    {B,air,air,air,new Block(113,0),air,new Block(113,0)}
                },
                {
                    {B,U1,U1,U1,U1,U1,U1},                                              {B,air,air,air,U2,U2,U2},                                         {B,air,air,air,U1,U1,U1},                 {B,air,air,air,U2,U2,U2},                                           {B,air,air,air,U1,U1,U1},                                           {B,air,air,air,U2,U2,U2},                 {B,air,air,air,U1,U1,U1},                                         {B,air,air,air,U2,U2,U2},                 {B,air,air,air,U1,U1,U1},                               {B,air,air,air,U2,U2,U2},                               {B,air,air,air,U1,U1,U1},
                                                                                        {B,air,air,air,U2,U2,U2},                                         {B,air,air,air,U1,U1,U1},                 {B,air,air,air,U2,U2,U2},                                           {B,air,air,air,U1,U1,U1},                                           {B,air,air,air,U2,U2,U2},                 {B,air,air,air,U1,U1,U1},                                         {B,air,air,air,U2,U2,U2},                 {B,air,air,air,U1,U1,U1},                               {B,air,air,air,U2,U2,U2},                               {B,air,air,air,U1,U1,U1},
                                                                                        {B,air,air,air,U2,U2,U2},                                         {B,air,air,air,U1,U1,U1},                 {B,air,air,air,U2,U2,U2},                                           {B,air,air,air,U1,U1,U1},                                           {B,air,air,air,U2,U2,U2},                 {B,air,air,air,U1,U1,U1},                                         {B,air,air,air,U2,U2,U2},                 {B,air,air,air,U1,U1,U1},                               {B,air,air,air,U2,U2,U2},                               {B,air,air,air,U1,U1,U1},
                    {B,U2,U2,U2,U2,U2,U2}
                },
                {
                    {B,U1,U1,U1,air,air,air},                                           {B,U2,U2,U2,air,air,air},                                         {B,U1,U1,U1,air,air,air},                 {B,U2,U2,U2,air,air,air},                                           {B,U1,U1,U1,air,air,air},                                           {B,U2,U2,U2,air,air,air},                 {B,U1,U1,U1,air,air,air},                                         {B,U2,U2,U2,air,air,air},                 {B,U1,U1,U1,air,air,air},                               {B,U2,U2,U2,air,air,air},                               {B,U1,U1,U1,air,air,air},
                                                                                        {B,U2,U2,U2,air,air,air},                                         {B,U1,U1,U1,air,air,air},                 {B,U2,U2,U2,air,air,air},                                           {B,U1,U1,U1,air,air,air},                                           {B,U2,U2,U2,air,air,air},                 {B,U1,U1,U1,air,air,air},                                         {B,U2,U2,U2,air,air,air},                 {B,U1,U1,U1,air,air,air},                               {B,U2,U2,U2,air,air,air},                               {B,U1,U1,U1,air,air,air},
                                                                                        {B,U2,U2,U2,air,air,air},                                         {B,U1,U1,U1,air,air,air},                 {B,U2,U2,U2,air,air,air},                                           {B,U1,U1,U1,air,air,air},                                           {B,U2,U2,U2,air,air,air},                 {B,U1,U1,U1,air,air,air},                                         {B,U2,U2,U2,air,air,air},                 {B,U1,U1,U1,air,air,air},                               {B,U2,U2,U2,air,air,air},                               {B,U1,U1,U1,air,air,air},
                    {B,U2,U2,U2,air,air,air}
                }
            }
        ),
        new FirstFloor(
            null, new Block(102,0), new Block(35,14), new Block(35,0),
            new Block[,,] {
                {
                    {B,air,air,air}, {B,air,new Block(164,1),air}, {B,new Block(164,3),new Block(85,5),new Block(164,2)}, {B,air,new Block(164,0),air}, {B,air,air,air}, {B,air,air,air}, {new Block(24,0),new Block(145,3),air,air}, {new Block(196,1),air,air,air}, {new Block(196,1),air,air,air}, {new Block(24,0),new Block(145,3),air,air},
                    {B,air,air,air}, {B,air,new Block(164,1),air}, {B,new Block(164,3),new Block(85,5),new Block(164,2)}, {B,air,new Block(164,0),air}, {B,air,air,air}, {B,air,new Block(164,1),air}, {B,new Block(164,3),new Block(85,5),new Block(164,2)}, {B,air,new Block(164,0),air}, {B,air,air,air}, {B,air,air,air}, {B,air,air,air}
                },
                {
                    {B,air,air,air}, {W,air,air,air},              {W,air,new Block(171,14),air},                         {W,air,air,air},              {W,air,air,air}, {B,air,air,air}, {new Block(24,0),new Block(18,0),air,air},  {new Block(196,9),air,air,air}, {new Block(196,8),air,air,air}, {new Block(24,0),new Block(18,0),air,air},
                    {B,air,air,air}, {W,air,air,air},              {W,air,new Block(171,14),air},                         {W,air,air,air},              {W,air,air,air}, {B,air,air,air}, {W,air,new Block(171,14),air},              {W,air,air,air},                {W,air,air,air},                {W,air,air,air},        {B,air,air,air}
                },
                {
                    {B,air,air,air}, {W,air,air,air},              {W,air,air,air},                                       {W,air,air,air},              {W,air,air,air}, {B,air,air,air}, {new Block(24,0),new Block(18,0),air,air},  {new Block(164,7),air,air,air}, {new Block(164,7),air,air,air}, {new Block(24,0),new Block(18,0),air,air},
                    {B,air,air,air}, {W,air,air,air},              {W,air,air,air},                                       {W,air,air,air},              {W,air,air,air}, {B,air,air,air}, {W,air,air,air},                            {W,air,air,air},                {W,air,air,air},                {W,air,air,air},        {B,air,air,air}
                },
                {
                    {B,air,air,air}, {W,air,air,air},              {W,air,air,air},                                       {W,air,air,air},              {W,air,air,air}, {B,air,air,air}, {new Block(24,0),new Block(128,7),air,air}, {new Block(24,0),new Block(128,7),air,air}, {new Block(24,0),new Block(128,7),air,air}, {new Block(24,0),new Block(128,7),air,air},
                    {B,air,air,air}, {W,air,air,air},              {W,air,air,air},                                       {W,air,air,air},              {W,air,air,air}, {B,air,air,air}, {W,air,air,air},                            {W,air,air,air},                {W,air,air,air},                {W,air,air,air},        {B,air,air,air}
                },
                {
                    {B,air,air,air}, {B,air,air,U1},               {B,air,air,U2},                                        {B,air,air,U1},               {B,air,air,U2},  {B,air,air,U1},  {new Block(24,0),new Block(24,0),air,U2},   {new Block(24,0),new Block(24,0),air,U1},   {new Block(24,0),new Block(24,0),air,U2},   {new Block(24,0),new Block(24,0),air,U1},
                    {B,air,air,U2},  {B,air,air,U1},               {B,air,air,U2},                                        {B,air,air,U1},               {B,air,air,U2},  {B,air,air,air}, {B,air,air,air},                            {B,air,air,air},                {B,air,air,air},                {B,air,air,air},        {B,air,air,air}
                },
                {
                    {B,air,air,air}, {B,U1,U1,air},                {B,U2,U2,air},                                         {B,U1,U1,air},                {B,U2,U2,air},   {B,U1,U1,air},   {B,U2,U2,air},                              {B,U1,U1,air},                              {B,U2,U2,air},                               {B,U1,U1,air},
                    {B,U2,U2,air},   {B,U1,U1,air},                {B,U2,U2,air},                                         {B,U1,U1,air},                {B,U2,U2,air},   {B,air,air,air}, {B,air,air,air},                            {B,air,air,air},                {B,air,air,air},                {B,air,air,air},        {B,air,air,air}
                }
            }
        ),
        new FirstFloor(
            null, new Block(160,0), new Block(35,14,1,1,15), new Block(35,0),
            new Block[,,] {
                { {B,air,air,air,air}, {B,air,air,air,air}, {new Block(64,3),air,air,air,air}, {new Block(64,3),air,air,air,air}, {B,air,air,air,air}, {B,new Block(162,1),air,air,air}, {B,new Block(158,13),air,air,air}, {B,new Block(158,13),air,air,air},{B,new Block(158,13),air,air,air},{B,new Block(158,13),air,air,air}, {B,new Block(162,1),air,air,air} },
                { {B,air,air,air,air}, {B,air,air,air,air}, {new Block(64,9),air,air,air,air}, {new Block(64,8),air,air,air,air}, {B,air,air,air,air}, {B,new Block(162,1),air,air,air}, {B,new Block(140,0),air,air,air},  {B,air,air,air,air},              {B,new Block(140,0),air,air,air}, {B,new Block(140,0),air,air,air},  {B,new Block(162,1),air,air,air} },
                { {B,air,air,air,air}, {B,air,air,air,air}, {new Block(128,7),air,air,air,air},{new Block(128,7),air,air,air,air},{B,air,air,air,air}, {B,new Block(162,1),air,air,air}, {B,new Block(158,13),air,air,air}, {B,new Block(158,13),air,air,air},{B,new Block(158,13),air,air,air},{B,new Block(158,13),air,air,air}, {B,new Block(162,1),air,air,air} },
                { {B,new Block(85,0),new Block(85,0),new Block(85,0),new Block(85,0)}, {B,air,air,air,air},{new Block(241,0),air,air,air,air}, {new Block(241,0),air,air,air,air}, {B,air,air,air,air}, {B,new Block(162,1),air,air,air}, {B,air,air,air,air},{B,new Block(140,0),air,air,air}, {B,air,air,air,air}, {B,air,air,air,air}, {B,new Block(162,1),air,air,air} },
                { {B,air,air,U1,U1},   {B,air,air,U2,U2},   {new Block(241,0),air,air,U1,U1},  {new Block(241,0),air,air,U2,U2},  {B,air,air,U1,U1},   {B,new Block(158,5),air,U2,U2},   {B,new Block(158,5),air,U1,U1},    {B,new Block(158,5),air,U2,U2},   {B,new Block(158,5),air,U1,U1},   {B,new Block(158,5),air,U2,U2},   {B,new Block(158,5),air,U1,U1} },
                { {B,U1,U1,air,air},   {B,U2,U2,air,air},   {B,U1,U1,air,air},   {B,U2,U2,air,air},  {B,U1,U1,air,air},   {B,U2,U2,air,air},   {B,U1,U1,air,air},   {B,U2,U2,air,air},   {B,U1,U1,air,air},   {B,U2,U2,air,air},   {B,U1,U1,air,air} }
            }
        )
    };
        private readonly Interior?[] InteriorConfig = {
        //null,
        new Interior(
            new Block(5,1), new Block(89,0),
            new Block[,,] {
                {
                    {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,B,B,air,air,B,B,B}, {B,new Block(164,3),new Block(164,1),new Block(164,1),new Block(164,2),air,air,air}, {B,air,air,air,air,air,air,air}, {B,new Block(158,2),new Block(158,2),new Block(158,2),air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,new Block(164,3),new Block(164,0),new Block(164,0),new Block(164,2),air,air,air},
                    {B,air,air,air,air,air,air,air}, {B,new Block(164,3),new Block(164,1),new Block(164,1),new Block(164,2),air,air,air}, {B,air,air,air,air,air,air,air}, {B,new Block(158,2),new Block(158,2),new Block(158,2),air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,new Block(164,3),new Block(164,0),new Block(164,0),new Block(164,2),air,air,air},
                    {B,air,air,air,air,air,air,air}, {B,new Block(164,3),new Block(164,1),new Block(164,1),new Block(164,2),air,air,air}, {B,air,air,air,air,air,air,air}, {B,new Block(158,2),new Block(158,2),new Block(158,2),air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,new Block(164,3),new Block(164,0),new Block(164,0),new Block(164,2),air,air,air},
                    {B,B,B,air,air,B,B,B}
                },
                {
                    {B,B,B,air,air,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,B,B,air,air,B,B,B}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,B,B,L,L,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,L,L,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,L,L,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,L,L,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,L,L,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B}
                }
            }
        ),
        new Interior(
            new Block(5,3), new Block(89,0),
            new Block[,,] {
                {
                    {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,new Block(145,3),air,air,air,air,air,new Block(145,3)}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,new Block(134,5)}, {B,new Block(114,3), new Block(85,0), new Block(114,2),air,air,air,new Block(134,4)}, {B,new Block(114,3),air,air,air,air,air,air},
                    {B,new Block(114,3),air,air,air,air,air,air}, {B,new Block(114,3), new Block(85,0), new Block(114,2),air,air,air,new Block(134,4)}, {B,air,air,air,air,air,air,air}
                },
                {
                    {B,new Block(18,0),air,air,air,air,air,new Block(18,0)},   {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,new Block(140,0)}, {B,air,              new Block(171,12),air,            air,air,air,new Block(140,0)}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air},              {B,air,new Block(171,12),air,air,air,air,air},                                        {B,air,air,air,air,air,air,air}
                },
                {
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air}
                },
                {
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air}
                },
                {
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air}
                },
                {
                    {B,B,B,L,L,L,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,L,L,L,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}
                }
            }
        ),
        new Interior(
            new Block(5,3,1,0,5), new Block(89,0),
            new Block[,,] {
                {
                    {B,B,B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B,B,B}
                },
                {
                    {B,new Block(145,3),air,air,air,air,air,air,air,new Block(145,3)}, {B,air,air,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air,air,new Block(134,5)}, {B,new Block(114,3), new Block(85,0), new Block(114,2),air,air,air,air,air,new Block(134,4)}, {B,new Block(114,3),air,air,air,air,air,air,air,air},
                    {B,new Block(114,3),air,air,air,air,air,air,air,air}, {B,new Block(114,3), new Block(85,0), new Block(114,2),air,air,air,air,air,new Block(134,4)}, {B,air,air,air,air,air,air,air,air,air}
                },
                {
                    {B,new Block(18,0),air,air,air,air,air,air,air,new Block(18,0)},   {B,air,air,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air,air,new Block(140,0)}, {B,air,              new Block(72,0),air,            air,air,air,air,air,new Block(140,0)}, {B,air,air,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air,air,air},              {B,air,new Block(72,0),air,air,air,air,air,air,air},                                        {B,air,air,air,air,air,air,air,air,air}
                },
                {
                    {B,air,air,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air}
                },
                {
                    {B,air,air,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air}
                },
                {
                    {B,air,air,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air},{B,air,air,air,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,L,L,L,B,B,B}, {B,B,B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B,B,B}, {B,B,B,B,L,L,L,B,B,B}, {B,B,B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B,B,B}
                }
            }
        ),
        new Interior(
            new Block(5, 0, 3, 0, 9), new Block(89, 0),
            new Block[,,] {
                {
                    {B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},
                    {B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B},{B,B,B,B,B,B,B}
                },
                {
                    {B,air,air,B,B,B,B}, {B,air,air,air,new Block(53,1,3,0,9),new Block(53,1,3,0,9),new Block(53,1,3,0,9)}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(85,0,3,0,9),new Block(85,0,3,0,9),new Block(85,0,3,0,9)}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(53,0,3,0,9),new Block(53,0,3,0,9),new Block(53,0,3,0,9)}, {B,air,air,air,air,air,air},
                                         {B,air,air,air,new Block(53,1,3,0,9),new Block(53,1,3,0,9),new Block(53,1,3,0,9)}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(85,0,3,0,9),new Block(85,0,3,0,9),new Block(85,0,3,0,9)}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(53,0,3,0,9),new Block(53,0,3,0,9),new Block(53,0,3,0,9)}, {B,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,new Block(53,3,3,0,9),air,new Block(53,6,3,0,9)}, {B,air,air,air,new Block(53,3,3,0,9),air,new Block(53,6,3,0,9)}, {B,air,air,air,air,air,air},
                                         {B,air,air,air,new Block(53,1,3,0,9),new Block(53,1,3,0,9),new Block(53,1,3,0,9)}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(85,0,3,0,9),new Block(85,0,3,0,9),new Block(85,0,3,0,9)}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(53,0,3,0,9),new Block(53,0,3,0,9),new Block(53,0,3,0,9)}
                },
                {
                    {B,air,air,B,B,B,B}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(171,12),new Block(171,12),new Block(171,12)},
                    {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,new Block(171,12),new Block(171,12),new Block(171,12)}, {B,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,new Block(140,0)}, {B,air,air,air,air,air,new Block(171,0)}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,new Block(171,12),new Block(171,12),new Block(171,12)}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air},
                    {B,air,air,air,air,air,air}, {B,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B}, {B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},{B,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air},{B,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B},
                    {B,B,B,L,L,B,B}, {B,B,B,L,L,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B},
                    {B,B,B,L,L,B,B}, {B,B,B,L,L,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B},
                    {B,B,B,L,L,B,B}, {B,B,B,L,L,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B},
                    {B,B,B,L,L,B,B}, {B,B,B,L,L,B,B}, {B,B,B,B,B,B,B}, {B,B,B,B,B,B,B}
                }
            }
        ),
        new Interior(
            new Block(5, 0, 3, 0, 9), new Block(169, 0),
            new Block[,,] {
                {
                    {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B},
                    {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},
                    {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},
                    {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B}
                },
                {
                    {B,air,air,B,B,B,B,B}, {B,air,air,air,air,B,B,new Block(47,0)}, {B,air,air,air,air,new Block(53,5,3,0,9),new Block(53,5,3,0,9),new Block(47,0)}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,new Block(53,0,3,0,9),new Block(53,0,3,0,9),air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,new Block(35,3),new Block(35,3),new Block(35,0)}, {B,air,air,air,air,new Block(35,3),new Block(35,3),new Block(35,0)}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,new Block(53,6,3,0,9),B}, {B,air,air,air,air,air,new Block(53,6,3,0,9),B}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,new Block(35,6),new Block(35,6),new Block(35,0)}, {B,air,air,air,air,new Block(35,6),new Block(35,6),new Block(35,0)}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,new Block(53,1,3,0,9),new Block(53,1,3,0,9),new Block(53,1,3,0,9)}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,new Block(85,0,3,0,9),new Block(85,0,3,0,9),new Block(85,0,3,0,9)}
                },
                {
                    {B,air,air,B,B,B,B,B}, {B,air,air,air,air,new Block(140,0),air,air}, {B,air,air,air,air,new Block(171,0),new Block(171,0),air}, {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,new Block(171,0)}, {B,air,air,air,air,air,air,new Block(171,0)}, {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,new Block(463,0)}, {B,air,air,air,air,air,air,new Block(140,0)}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,new Block(171,0)}, {B,air,air,air,air,air,air,new Block(171,0)}, {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,new Block(171,12),new Block(171,12),new Block(171,12)}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {new Block(20,0),air,air,air,air,air,air,air}, {new Block(20,0),air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air},
                    {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}, {B,air,air,air,air,air,air,air}
                },
                {
                    {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B}, {B,B,B,B,B,B,B,B},
                    {B,B,B,L,L,L,B,B}, {B,B,B,L,L,L,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},
                    {B,B,B,L,L,L,B,B}, {B,B,B,L,L,L,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},
                    {B,B,B,L,L,L,B,B}, {B,B,B,L,L,L,B,B}, {B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B},{B,B,B,B,B,B,B,B}
                }
            }
        )
    };
        private static Block B = new Block(-1);
        private static Block WF = new Block(-2);
        private static Block W = new Block(-3);
        private static Block U1 = new Block(-4);
        private static Block U2 = new Block(-5);
        private static Block L = new Block(-6);
        private static Block air = new Block(0);
        // 以下为全局变量
        private double v2_prob;
        private int c_rand_data = -1;
        private int cnt = 0;
        private int[] Cnt = new int[MAX_LEVEL];
        private List<Node> roof_node_list = new List<Node>();
        private Dictionary<Vector3, Block> block_list = new Dictionary<Vector3, Block>();
        // 以下为结构体声明
        struct Block {
            public int id { get; set; }
            public int data { get; set; }
            public bool bump { get; set; }
            public int random, rand_min, rand_max;
            public Block(int id, int data, bool bump) {
                this.id = id;
                this.data = data;
                this.bump = bump;
                this.random = 0;
                this.rand_min = data;
                this.rand_max = data;
            }
            public Block(int id, int data) {
                this.id = id;
                this.data = data;
                this.bump = false;
                this.random = 0;
                this.rand_min = data;
                this.rand_max = data;
            }
            public Block(int id) {
                this.id = id;
                this.data = 0;
                this.bump = false;
                this.random = 0;
                this.rand_min = data;
                this.rand_max = data;
            }
            public Block(int id, int data, int random, int rand_min, int rand_max) {
                this.id = id;
                this.data = data;
                this.random = random;
                this.rand_min = rand_min;
                this.rand_max = rand_max;
                this.bump = false;
            }
            public Block(int id, int data, bool bump, int random, int rand_min, int rand_max) {
                this.id = id;
                this.data = data;
                this.bump = bump;
                this.random = random;
                this.rand_min = rand_min;
                this.rand_max = rand_max;
            }
        }
        public struct Coordinate {
            public int x, z;
            public Coordinate(int x, int z) {
                this.x = x;
                this.z = z;
            }
        }
        struct Node {
            public int x, y;
            public Node(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }
        struct Roof {
            public Block WindowFrame;
            public Block Window;
            public Block? Base;
            public Block[,,] Data;
            public Roof(Block windowFrame, Block window, Block? Base, Block[,,] Data) {
                this.WindowFrame = windowFrame;
                this.Window = window;
                this.Base = Base;
                this.Data = Data;
            }
            public int GetReduceDelta() {
                //return Data.GetLength(1) - 1;
                return 2;
            }
            public int GetLength() {
                return Data.GetLength(0);
            }
            public int GetWidth() {
                return Data.GetLength(1);
            }
            public int GetHeight() {
                return Data.GetLength(2);
            }
        }
        struct FirstFloor {
            public Block? Base;
            public Block Window;
            public Block? U1;
            public Block? U2;
            public Block[,,] Data;
            public FirstFloor(Block? Base, Block window, Block? u1, Block? u2, Block[,,] Data) {
                this.Base = Base;
                this.Window = window;
                this.U1 = u1;
                this.U2 = u2;
                this.Data = Data;
            }
            public int GetReduceDelta() {
                //return GetWidth();
                return 2;
            }
            public int GetHeight() {
                return Data.GetLength(0);
            }
            public int GetLength() {
                return Data.GetLength(1);
            }
            public int GetWidth() {
                return Data.GetLength(2);
            }
        }
        struct Interior {
            public Block? Base;
            public Block? Light;
            public Block[,,] Data;
            public Interior(Block? Base, Block? Light, Block[,,] Data) {
                this.Base = Base;
                this.Light = Light;
                this.Data = Data;
            }
            public int GetReduceDelta() {
                //return this.GetWidth();
                return Math.Min(6, GetWidth() / 2);
            }
            public int GetHeight() {
                return Data.GetLength(0);
            }
            public int GetLength() {
                return Data.GetLength(1);
            }
            public int GetWidth() {
                return Data.GetLength(2);
            }
        }
        struct FirstFloorInfo {
            public Node start, end;
            public int firstfloor_kind;
            public Block? Base;
            public FirstFloorInfo(Node start, Node end, int firstfloor_kind, Block? Base) {
                this.start = start;
                this.end = end;
                this.firstfloor_kind = firstfloor_kind;
                this.Base = Base;
            }
        }
        struct InteriorInfo {
            public Node start, end;
            public int interior_kind;
            public int clevel;
            public int sh;
            public Block? Base;
            public InteriorInfo(Node start, Node end, int interior_kind, int clevel, int sh, Block? Base) {
                this.start = start;
                this.end = end;
                this.interior_kind = interior_kind;
                this.clevel = clevel;
                this.sh = sh;
                this.Base = Base;
            }
        }
        class Vector2 {
            public float x, y;
            public Vector2(float x, float y) {
                this.x = x;
                this.y = y;
            }
            public static float Dot(Vector2 a, Vector2 b) {
                return a.x * b.x + a.y * b.y;
            }
            public float Length() {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }
        struct Vector3 {
            public float x, y, z;
            public Vector3(float x, float y, float z) {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        //以下为函数
        /* void Generate(Coordinate[] coordinates)，主函数，函数入口点
         * 参数coordinates[]：顶点坐标数组，顶点的格式为(x,z)
         */
        public void Generate(Coordinate[] coordinates) {
            // 仅使用多边形坐标生成建筑物
            // 移植到ModPE或Addon大概会更方便
            // 当前版本日期为2021.3.2（可能是最终版）
            // Initialize
            v2_prob = (double)WallConfig_v2.GetLength(0) / (double)(WallConfig.GetLength(0) + WallConfig_v2.GetLength(0));
            int lastz = 0, lastx = 0;
            const bool building = true;
            const bool dynamic_add_nodes = true;
            List<Node> building_node_list = new List<Node>();
            List<Node> const_building_node_list = new List<Node>();
            int building_version;
            int height = -1, levels = -1;
            int wall_kind = -1;
            bool doNotChangeStyle = false;
            List<FirstFloorInfo> firstFloorInfos = new List<FirstFloorInfo>();
            List<InteriorInfo> interiorInfos = new List<InteriorInfo>();

            foreach (Coordinate coordinate in coordinates)
                const_building_node_list.Add(new Node(coordinate.x, coordinate.z));

            double prob = rd.NextDouble();
            if (prob >= (1 - v2_prob)) building_version = 2;
            else building_version = 1;

            if (building_version == 1)
                wall_kind = rd.Next(0, WallConfig.GetLength(0));
            else if (building_version == 2)
                wall_kind = rd.Next(0, WallConfig_v2.GetLength(0));

            //确定楼层数
            if (levels == -1 && height == -1)
                levels = rd.Next(min_level, max_level);
            else if (height != -1) {
                if (building_version == 1)
                    levels = (int)Math.Ceiling((double)height / (double)WallConfig[wall_kind].GetLength(0));
                else if (building_version == 2)
                    levels = (int)Math.Ceiling((double)height / (double)WallConfig_v2[wall_kind].GetLength(0));
            }
            if (IsSmallBuilding(const_building_node_list))
                levels = Math.Min(levels, max_small_level);
            if (IsMiniBuilding(const_building_node_list))
                levels = 1;
            levels = Math.Min(levels, max_legal_level);

            if (building_version == 1)
                height = levels * WallConfig[wall_kind].GetLength(0);
            else if (building_version == 2)
                height = levels * WallConfig_v2[wall_kind].GetLength(0);

            foreach (var coordinate in coordinates) {
                //新的
                int cx = coordinate.x;
                int cz = coordinate.z;
                if (IsUndefined(new Node(cx, cz))) {
                    lastx = undefined;
                    lastz = undefined;
                    building_node_list.Add(new Node(undefined, undefined));
                    ClearCnt();
                    continue;
                }

                int currentz = cz;
                int currentx = cx;

                if (building && dynamic_add_nodes) {
                    bool valid = true;
                    if (building_node_list.Count > 0 && building_node_list[building_node_list.Count - 1].Equals(new Node(currentx, currentz))) valid = false;
                    for (int i = building_node_list.Count - 1; i >= 0; i--) {
                        if (i == 0 || IsUndefined(building_node_list[i])) {
                            if (building_node_list[i].Equals(new Node(currentx, currentz)))
                                valid = false;
                            break;
                        }
                    }
                    if (valid) building_node_list.Add(new Node(currentx, currentz));
                }
                //连线
                if (!IsUndefined(new Node(lastx, lastz))) {
                    if (building) {
                        //一楼选定
                        int startheight = 0;
                        int firstfloor_kind = rd.Next(0, FirstFloorConfig.GetLength(0));
                        if (FirstFloorConfig[firstfloor_kind].HasValue) {
                            if (Math.Max(Math.Abs(currentx - lastx), Math.Abs(currentz - lastz)) < FirstFloorConfig[firstfloor_kind].Value.GetLength())
                                startheight = 0;
                            else {
                                int singleheight = 0;
                                Block? Base = null;
                                if (building_version == 1) {
                                    Base = BaseBlock[wall_kind];
                                    singleheight = WallConfig[wall_kind].GetLength(0);
                                } else if (building_version == 2) {
                                    Base = BaseBlock_v2[wall_kind];
                                    singleheight = WallConfig_v2[wall_kind].GetLength(0);
                                }
                                if (Base.HasValue && Base.Value.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(Base.Value.rand_min, Base.Value.rand_max);
                                    Block tBase = Base.Value;
                                    tBase.data = c_rand_data;
                                    Base = tBase;
                                }
                                startheight = FirstFloorConfig[firstfloor_kind].Value.GetHeight();
                                firstFloorInfos.Add(new FirstFloorInfo(new Node(lastx, lastz), new Node(currentx, currentz), firstfloor_kind, Base));
                            }
                        }
                        for (int level = 0; level < levels; level++) {
                            int interior_kind = rd.Next(0, InteriorConfig.GetLength(0));
                            if (building_version == 1) {
                                //内饰选定
                                int shI = level * WallConfig[wall_kind].GetLength(0);
                                Block Base = BaseBlock[wall_kind];
                                if (Base.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(Base.rand_min, Base.rand_max);
                                    Base.data = c_rand_data;
                                }
                                interiorInfos.Add(new InteriorInfo(new Node(lastx, lastz), new Node(currentx, currentz), interior_kind, level, shI, Base));
                                //墙面建造
                                int shW = Math.Max(startheight, level * WallConfig[wall_kind].GetLength(0));
                                DrawLine_Building_Advanced(lastx, lastz, currentx, currentz, level, shW, WallConfig[wall_kind]);
                            } else if (building_version == 2) {
                                int shI = level * WallConfig_v2[wall_kind].GetLength(0);
                                Block Base = BaseBlock_v2[wall_kind];
                                if (Base.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(Base.rand_min, Base.rand_max);
                                    Base.data = c_rand_data;
                                }
                                interiorInfos.Add(new InteriorInfo(new Node(lastx, lastz), new Node(currentx, currentz), interior_kind, level, shI, Base));
                                int shW = Math.Max(startheight, level * WallConfig_v2[wall_kind].GetLength(0));
                                DrawLine_Building_Advanced_v2(lastx, lastz, currentx, currentz, level, shW, WallConfig_v2[wall_kind]);
                            }
                        }
                    }
                }
                lastz = currentz;
                lastx = currentx;
            }

            /////////////////////////////////上一个建筑物/用地的多边形填充////////////////////////////
            try {
                if (building_node_list.Count >= 3) {
                    int id = 0, data = 0;
                    if (building_version == 1) {
                        id = BaseBlock[wall_kind].id;
                        data = BaseBlock[wall_kind].data;
                        if (c_rand_data != -1 && BaseBlock[wall_kind].random == 1) data = c_rand_data;
                    } else if (building_version == 2) {
                        id = BaseBlock_v2[wall_kind].id;
                        data = BaseBlock_v2[wall_kind].data;
                        if (c_rand_data != -1 && BaseBlock[wall_kind].random == 1) data = c_rand_data;
                    }
                    //填充建筑物最顶层的平面
                    FillPolygonScanline(building_node_list, height, id, data);
                    //建筑物屋顶与内饰放置，必须为非迷你建筑
                    if (!IsMiniBuilding(building_node_list)) {
                        //建筑物屋顶放置
                        cnt = 0;
                        int roof_kind = rd.Next(0, RoofConfig.GetLength(0));
                        Roof? roof = RoofConfig[roof_kind];
                        if (!(roof is null) && !IsSmallBuilding(building_node_list)) {
                            if (!(roof.Value.Base is null)) {
                                id = roof.Value.Base.Value.id;
                                data = roof.Value.Base.Value.data;
                            }
                            for (int i = 0; i < building_node_list.Count; i++) {
                                Node startnode = building_node_list[i];
                                Node endnode = startnode;
                                if (i != building_node_list.Count - 1) endnode = building_node_list[i + 1];
                                if (IsUndefined(startnode)) {
                                    roof_node_list.Add(startnode);
                                    continue;
                                }
                                Node? lastnode, nextnode;
                                if (i == 0 || building_node_list[i - 1].x == undefined || building_node_list[i - 1].y == undefined) {
                                    nextnode = building_node_list[i + 2];
                                    int j;
                                    for (j = i + 1; j < building_node_list.Count; j++) {
                                        if (building_node_list[j].x == undefined || building_node_list[j].y == undefined)
                                            break;
                                    }
                                    lastnode = building_node_list[j - 1];
                                } else if (i == building_node_list.Count - 1 || building_node_list[i + 1].x == undefined) {
                                    lastnode = building_node_list[i - 1];
                                    int j;
                                    for (j = i - 1; j >= 0; j--) {
                                        if (building_node_list[j].x == undefined || building_node_list[j].y == undefined)
                                            break;
                                    }
                                    endnode = building_node_list[j + 1];
                                    nextnode = building_node_list[j + 2];
                                } else if (i == building_node_list.Count - 2 || building_node_list[i + 2].x == undefined || building_node_list[i + 2].y == undefined) {
                                    lastnode = building_node_list[i - 1];
                                    int j;
                                    for (j = i - 1; j >= 0; j--) {
                                        if (building_node_list[j].x == undefined || building_node_list[j].y == undefined)
                                            break;
                                    }
                                    nextnode = building_node_list[j + 1];
                                } else {
                                    lastnode = building_node_list[i - 1];
                                    nextnode = building_node_list[i + 2];
                                }
                                if (lastnode.Value.x == undefined || lastnode.Value.y == undefined) lastnode = null;
                                if (nextnode.Value.x == undefined || nextnode.Value.y == undefined) nextnode = null;
                                Block Base = new Block(id, data);
                                if (building_node_list.Count < cmplx_building_nodenum)
                                    DrawLine_Roof_Improved_v2(startnode.x, startnode.y, endnode.x, endnode.y, height + 1, roof.Value, building_node_list, lastnode, nextnode, Base, skipKCheck);
                                else
                                    DrawLine_Roof_Improved_v2(startnode.x, startnode.y, endnode.x, endnode.y, height + 1, roof.Value, building_node_list, lastnode, nextnode, Base, false);
                            }
                            FillPolygonScanline(roof_node_list, height + roof.Value.Data.GetLength(2), id, data);
                        }
                        //建筑物内饰放置
                        ClearCnt();
                        int interior_kind = rd.Next(0, InteriorConfig.GetLength(0));
                        for (int i = 0; i < building_node_list.Count; i++) {
                            if (InteriorConfig[interior_kind] is null) continue;
                            Node startnode = building_node_list[i];
                            Node endnode = startnode;
                            if (i != building_node_list.Count - 1) endnode = building_node_list[i + 1];
                            if (IsUndefined(startnode)) continue;
                            Node? lastnode, nextnode;
                            if (i == 0 || building_node_list[i - 1].x == undefined || building_node_list[i - 1].y == undefined) {
                                nextnode = building_node_list[i + 2];
                                int j;
                                for (j = i + 1; j < building_node_list.Count; j++) {
                                    if (building_node_list[j].x == undefined || building_node_list[j].y == undefined)
                                        break;
                                }
                                lastnode = building_node_list[j - 1];
                            } else if (i == building_node_list.Count - 1 || building_node_list[i + 1].x == undefined) {
                                lastnode = building_node_list[i - 1];
                                int j;
                                for (j = i - 1; j >= 0; j--) {
                                    if (building_node_list[j].x == undefined || building_node_list[j].y == undefined)
                                        break;
                                }
                                endnode = building_node_list[j + 1];
                                nextnode = building_node_list[j + 2];
                            } else if (i == building_node_list.Count - 2 || building_node_list[i + 2].x == undefined || building_node_list[i + 2].y == undefined) {
                                lastnode = building_node_list[i - 1];
                                int j;
                                for (j = i - 1; j >= 0; j--) {
                                    if (building_node_list[j].x == undefined || building_node_list[j].y == undefined)
                                        break;
                                }
                                nextnode = building_node_list[j + 1];
                            } else {
                                lastnode = building_node_list[i - 1];
                                nextnode = building_node_list[i + 2];
                            }
                            if (IsUndefined(lastnode.Value)) lastnode = null;
                            if (IsUndefined(nextnode.Value)) nextnode = null;
                            int max_level = (int)Math.Floor((double)height / (double)InteriorConfig[interior_kind].Value.GetHeight());
                            Block Base = new Block(id, data);
                            DrawLine_Interior_v3(startnode.x, startnode.y, endnode.x, endnode.y, max_level, InteriorConfig[interior_kind].Value, building_node_list, lastnode, nextnode, Base);
                        }

                        foreach (var ffi in firstFloorInfos) {
                            if (FirstFloorConfig[ffi.firstfloor_kind].HasValue) {
                                cnt = 0;
                                DrawLine_FirstFloor(ffi.start.x, ffi.start.y, ffi.end.x, ffi.end.y, FirstFloorConfig[ffi.firstfloor_kind].Value, building_node_list, ffi.Base);
                            }
                        }
                    }
                }
            }
            catch {
                print("INFO: Skipping roof, interior or first floor for current building");
            }
            building_node_list.Clear();
            roof_node_list.Clear();
            firstFloorInfos.Clear();
            interiorInfos.Clear();
            ClearCnt();
            cnt = 0;
            if (!doNotChangeStyle) c_rand_data = -1;
            block_list.Clear();
            roof_node_list.Clear();
        }
        void DrawLine_Building_Advanced(int startx, int starty, int endx, int endy, int clevel, int sh, Block[,] WallConfig) {
            if (startx != endx) {
                double k = (double)(endy - starty) / (double)(endx - startx);
                double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
                if (-1 < k && k < 1) {
                    if (startx <= endx) {
                        for (float x = startx; x < endx; x++) {
                            int y = (int)Math.Round(k * x + b);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)];
                                int id = cblock.id, data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (cblock.bump) {
                                    setTile((int)x, h + 1, y - 1, id, data);
                                    setTile((int)x, h + 1, y + 1, id, data);
                                }
                                setTile((int)x, h + 1, y, id, data);
                            }
                            Cnt[clevel]++;
                        }
                    } else {
                        for (float x = startx; x > endx; x--) {
                            int y = (int)Math.Round(k * x + b);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)];
                                int id = cblock.id, data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (cblock.bump) {
                                    setTile((int)x, h + 1, y - 1, id, data);
                                    setTile((int)x, h + 1, y + 1, id, data);
                                }
                                setTile((int)x, h + 1, y, id, data);
                            }
                            Cnt[clevel]++;
                        }
                    }
                } else {
                    if (starty <= endy) {
                        for (float y = starty; y < endy; y++) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)];
                                int id = cblock.id, data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (cblock.bump) {
                                    setTile(x - 1, h + 1, (int)y, id, data);
                                    setTile(x + 1, h + 1, (int)y, id, data);
                                }
                                setTile(x, h + 1, (int)y, id, data);
                            }
                            Cnt[clevel]++;
                        }
                    } else {
                        for (float y = starty; y > endy; y--) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)];
                                int id = cblock.id, data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (cblock.bump) {
                                    setTile(x - 1, h + 1, (int)y, id, data);
                                    setTile(x + 1, h + 1, (int)y, id, data);
                                }
                                setTile(x, h + 1, (int)y, id, data);
                            }
                            Cnt[clevel]++;
                        }
                    }
                }
            } else {
                if (starty <= endy) {
                    for (float y = starty; y < endy; y++) {
                        for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                            Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)];
                            int id = cblock.id, data = cblock.data;
                            if (cblock.random == 1) {
                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                data = c_rand_data;
                            } else if (cblock.random == 2) {
                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                            }
                            if (cblock.bump) {
                                setTile(startx - 1, h + 1, (int)y, id, data);
                                setTile(startx + 1, h + 1, (int)y, id, data);
                            }
                            setTile(startx, h + 1, (int)y, id, data);
                        }
                        Cnt[clevel]++;
                    }
                } else {
                    for (float y = starty; y > endy; y--) {
                        for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                            Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)];
                            int id = cblock.id, data = cblock.data;
                            if (cblock.random == 1) {
                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                data = c_rand_data;
                            } else if (cblock.random == 2) {
                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                            }
                            if (cblock.bump) {
                                setTile(startx - 1, h + 1, (int)y, id, data);
                                setTile(startx + 1, h + 1, (int)y, id, data);
                            }
                            setTile(startx, h + 1, (int)y, id, data);
                        }
                        Cnt[clevel]++;
                    }
                }
            }
        }
        void DrawLine_Building_Advanced_v2(int startx, int starty, int endx, int endy, int clevel, int sh, Block[,][] WallConfig) {
            if (startx != endx) {
                double k = (double)(endy - starty) / (double)(endx - startx);
                double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
                if (-1 < k && k < 1) {
                    if (startx <= endx) {
                        for (float x = startx; x < endx; x++) {
                            int y = (int)Math.Round(k * x + b);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                int delta = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)].GetLength(0);
                                for (int dy = 0; dy < delta; dy++) {
                                    Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)][dy];
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    }
                                    if (id == 0) continue;
                                    setTile((int)x, h + 1, y - dy, id, data, 0, null);
                                    if (dy != 0) setTile((int)x, h + 1, y + dy, id, data, 0, true);
                                }
                            }
                            Cnt[clevel]++;
                        }
                    } else {
                        for (float x = startx; x > endx; x--) {
                            int y = (int)Math.Round(k * x + b);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                int delta = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)].GetLength(0);
                                for (int dy = 0; dy < delta; dy++) {
                                    Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)][dy];
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    }
                                    if (id == 0) continue;
                                    if (dy != 0) setTile((int)x, h + 1, y - dy, id, data, 180, true);
                                    setTile((int)x, h + 1, y + dy, id, data, 180, null);
                                }
                            }
                            Cnt[clevel]++;
                        }
                    }
                } else {
                    if (starty <= endy) {
                        for (float y = starty; y < endy; y++) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                int delta = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)].GetLength(0);
                                for (int dx = 0; dx < delta; dx++) {
                                    Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)][dx];
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    }
                                    if (id == 0) continue;
                                    if (dx != 0) setTile(x - dx, h + 1, (int)y, id, data, 90, false);
                                    setTile(x + dx, h + 1, (int)y, id, data, 90, null);
                                }
                            }
                            Cnt[clevel]++;
                        }
                    } else {
                        for (float y = starty; y > endy; y--) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                                int delta = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)].GetLength(0);
                                for (int dx = 0; dx < delta; dx++) {
                                    Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)][dx];
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    }
                                    setTile(x - dx, h + 1, (int)y, id, data, 270, null);
                                    if (dx != 0) setTile(x + dx, h + 1, (int)y, id, data, 270, false);
                                }
                            }
                            Cnt[clevel]++;
                        }
                    }
                }
            } else {
                if (starty <= endy) {
                    for (float y = starty; y < endy; y++) {
                        for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                            int delta = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)].GetLength(0);
                            for (int dx = 0; dx < delta; dx++) {
                                Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)][dx];
                                int id = cblock.id;
                                int data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (id == 0) continue;
                                if (dx != 0) setTile(startx - dx, h + 1, (int)y, id, data, 90, false);
                                setTile(startx + dx, h + 1, (int)y, id, data, 90, null);
                            }
                        }
                        Cnt[clevel]++;
                    }
                } else {
                    for (float y = starty; y > endy; y--) {
                        for (int h = sh; h < (clevel + 1) * WallConfig.GetLength(0); h++) {
                            int delta = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)].GetLength(0);
                            for (int dx = 0; dx < delta; dx++) {
                                Block cblock = WallConfig[h % WallConfig.GetLength(0), Cnt[clevel] % WallConfig.GetLength(1)][dx];
                                int id = cblock.id;
                                int data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (id == 0) continue;
                                setTile(startx - dx, h + 1, (int)y, id, data, 270, null);
                                if (dx != 0) setTile(startx + dx, h + 1, (int)y, id, data, 270, false);
                            }
                        }
                        Cnt[clevel]++;
                    }
                }
            }
        }
        void DrawLine_Roof_Improved_v2(int startx, int starty, int endx, int endy, int sh, Roof RoofConfig, List<Node> nodes, Node? last, Node? next, Block? Base = null, bool skipKCheck = false) {
            var roof = RoofConfig.Data;
            if (RoofConfig.Base != null) Base = RoofConfig.Base;
            if (last is null || next is null) return;
            if (startx != endx) {
                double k = (double)(endy - starty) / (double)(endx - startx);
                double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
                if (-1 <= k && k < 1) {
                    bool reflex_last = false;
                    Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                    Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                    Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                    float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                    float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                    if (!(last is null) && -cos_th < cos1 && cos1 < cos_th) {
                        List<Node> triangle1 = new List<Node>();
                        triangle1.Add(last.Value);
                        triangle1.Add(new Node(startx, starty));
                        triangle1.Add(new Node(endx, endy));
                        bool? PosiInPolygon = null, PosiInTriangle = null;
                        if (pnpoly4(nodes, new Node(startx, starty - RoofConfig.GetReduceDelta()))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(startx, starty + RoofConfig.GetReduceDelta()))) PosiInPolygon = true;
                        if (pnpoly3(triangle1, new Node(startx, starty - RoofConfig.GetReduceDelta()))) PosiInTriangle = false;
                        else if (pnpoly3(triangle1, new Node(startx, starty + RoofConfig.GetReduceDelta()))) PosiInTriangle = true;
                        if (PosiInPolygon is null || PosiInTriangle is null) {
                            if (pnpoly4(nodes, new Node(startx - RoofConfig.GetReduceDelta(), starty))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(startx + RoofConfig.GetReduceDelta(), starty))) PosiInPolygon = true;
                            if (pnpoly3(triangle1, new Node(startx - RoofConfig.GetReduceDelta(), starty))) PosiInTriangle = false;
                            else if (pnpoly3(triangle1, new Node(startx + RoofConfig.GetReduceDelta(), starty))) PosiInTriangle = true;
                            if (PosiInPolygon != null && PosiInTriangle != null)
                                reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            else
                                reflex_last = false;
                        } else {
                            reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        }
                        if (reflex_last && DEBUG) {
                            print("reflex_last is true");
                            setTile(startx, sh + roof.GetLength(2) + 2, starty, 133, 0);
                            print("last:(" + last.Value.x + "," + last.Value.y + ")");
                            print("start:(" + startx + "," + starty + ")");
                            print("end:(" + endx + "," + endy + ")");
                            print("next:(" + next.Value.x + "," + next.Value.y + ")");
                        }
                    }
                    bool reflex_next = false;
                    if (!(next is null) && -cos_th < cos2 && cos2 < cos_th) {
                        List<Node> triangle2 = new List<Node>();
                        triangle2.Add(new Node(startx, starty));
                        triangle2.Add(new Node(endx, endy));
                        triangle2.Add(next.Value);
                        bool? PosiInPolygon = null, PosiInTriangle = null;
                        if (pnpoly4(nodes, new Node(endx, endy - RoofConfig.GetReduceDelta()))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(endx, endy + RoofConfig.GetReduceDelta()))) PosiInPolygon = true;
                        if (pnpoly3(triangle2, new Node(endx, endy - RoofConfig.GetReduceDelta()))) PosiInTriangle = false;
                        else if (pnpoly3(triangle2, new Node(endx, endy + RoofConfig.GetReduceDelta()))) PosiInTriangle = true;
                        if (PosiInPolygon == null || PosiInTriangle == null) {
                            if (pnpoly4(nodes, new Node(endx - RoofConfig.GetReduceDelta(), endy))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(endx + RoofConfig.GetReduceDelta(), endy))) PosiInPolygon = true;
                            if (pnpoly3(triangle2, new Node(endx - RoofConfig.GetReduceDelta(), endy))) PosiInTriangle = false;
                            else if (pnpoly3(triangle2, new Node(endx + RoofConfig.GetReduceDelta(), endy))) PosiInTriangle = true;
                            if (PosiInPolygon != null && PosiInTriangle != null)
                                reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            else
                                reflex_next = false;
                        } else {
                            reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        }
                        if (reflex_next && DEBUG) {
                            print("reflex_next is true");
                            setTile(endx, sh + roof.GetLength(2) + 2, endy, 133, 0);
                            print("last:(" + last.Value.x + "," + last.Value.y + ")");
                            print("start:(" + startx + "," + starty + ")");
                            print("end:(" + endx + "," + endy + ")");
                            print("next:(" + next.Value.x + "," + next.Value.y + ")");
                        }
                    }

                    int d = 0;
                    int mx = (int)Math.Round((double)(startx + endx) / 2);
                    int my = (int)Math.Round(k * mx + b);
                    if (pnpoly2(nodes, new Node(mx, my - RoofConfig.GetReduceDelta()))) d = -1;
                    else if (pnpoly2(nodes, new Node(mx, my + RoofConfig.GetReduceDelta()))) d = 1;

                    if (startx <= endx) {
                        if (reflex_last) {
                            for (float x = startx - roof.GetLength(1); x < startx; x++) {
                                int y = (int)Math.Round(k * x + b);
                                int start_dy = startx - (int)x;
                                for (int _dy = start_dy; _dy < roof.GetLength(1); _dy++) {
                                    int dy = (_dy - 1) * d;
                                    if (_dy == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node((int)x, y + dy));
                                        if (DEBUG) setTile((int)x, sh + roof.GetLength(2), y + dy, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dy, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 0, null);
                                        else if (d == 1)
                                            setTile((int)x, h, y + dy, id, data, 0, true);
                                    }
                                }
                                cnt++;
                            }
                        }
                        for (float x = startx; x <= endx; x++) {
                            int y = (int)Math.Round(k * x + b);
                            if (DEBUG && reflex_last) print("d=" + d + " x=" + x);
                            int end_dy = roof.GetLength(1);
                            if (!(last is null) && !reflex_last) {
                                if (-cos_th < cos1 && cos1 < cos_th) {
                                    if (startx == last.Value.x) {
                                        end_dy = Math.Min(roof.GetLength(1), (int)x - startx + 1);
                                    } else {
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (last_k >= 1 || last_k < -1 || skipKCheck) end_dy = Math.Min(roof.GetLength(1), (int)x - startx + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (endx == next.Value.x) {
                                        end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                    } else {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (next_k >= 1 || next_k < -1 || skipKCheck) end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                    }
                                }
                            }
                            for (int _dy = 0; _dy < end_dy; _dy++) {
                                int dy = (_dy - 1) * d;
                                if (_dy == roof.GetLength(1) - 1) {
                                    roof_node_list.Add(new Node((int)x, y + dy));
                                    if (DEBUG) setTile((int)x, sh + roof.GetLength(2), y + dy, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dy, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile((int)x, h, y + dy, id, data, 0, null);
                                    else if (d == 1)
                                        setTile((int)x, h, y + dy, id, data, 0, true);
                                }
                            }
                            cnt++;
                        }
                        if (reflex_next) {
                            for (float x = endx; x < endx + roof.GetLength(1); x++) {
                                int y = (int)Math.Round(k * x + b);
                                int start_dy = (int)x - endx;
                                for (int _dy = start_dy; _dy < roof.GetLength(1); _dy++) {
                                    int dy = (_dy - 1) * d;
                                    if (_dy == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node((int)x, y + dy));
                                        if (DEBUG) setTile((int)x, sh + roof.GetLength(2), y + dy, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dy, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 0, null);
                                        else if (d == 1)
                                            setTile((int)x, h, y + dy, id, data, 0, true);
                                    }
                                }
                                cnt++;
                            }
                        }
                    } else {
                        if (reflex_last) {
                            for (float x = startx + roof.GetLength(1); x > startx; x--) {
                                int y = (int)Math.Round(k * x + b);
                                int start_dy = (int)x - startx;
                                for (int _dy = start_dy; _dy < roof.GetLength(1); _dy++) {
                                    int dy = (_dy - 1) * d;
                                    if (_dy == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node((int)x, y + dy));
                                        if (DEBUG) setTile((int)x, sh + roof.GetLength(2), y + dy, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dy, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 180, true);
                                        else if (d == 1)
                                            setTile((int)x, h, y + dy, id, data, 180, null);
                                    }
                                }
                                cnt++;
                            }
                        }
                        for (float x = startx; x >= endx; x--) {
                            int y = (int)Math.Round(k * x + b);
                            int end_dy = roof.GetLength(1);
                            if (!(last is null) && !reflex_last) {
                                if (-cos_th < cos1 && cos1 < cos_th) {
                                    if (startx == last.Value.x) {
                                        end_dy = Math.Min(roof.GetLength(1), startx - (int)x + 1);
                                    } else {
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (last_k >= 1 || last_k < -1 || skipKCheck) end_dy = Math.Min(roof.GetLength(1), startx - (int)x + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (next.Value.x == endx) {
                                        end_dy = Math.Min(end_dy, (int)x - endx + 1);
                                    } else {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (next_k >= 1 || next_k < -1 || skipKCheck) end_dy = Math.Min(end_dy, (int)x - endx + 1);
                                    }
                                }
                            }
                            for (int _dy = 0; _dy < end_dy; _dy++) {
                                int dy = (_dy - 1) * d;
                                if (_dy == roof.GetLength(1) - 1) {
                                    roof_node_list.Add(new Node((int)x, y + dy));
                                    if (DEBUG) setTile((int)x, sh + roof.GetLength(2), y + dy, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dy, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile((int)x, h, y + dy, id, data, 180, true);
                                    else if (d == 1)
                                        setTile((int)x, h, y + dy, id, data, 180, null);
                                }
                            }
                            cnt++;
                        }
                        if (reflex_next) {
                            for (float x = endx - 1; x >= endx - roof.GetLength(1); x--) {
                                int y = (int)Math.Round(k * x + b);
                                int start_dy = endx - (int)x;
                                for (int _dy = start_dy; _dy < roof.GetLength(1); _dy++) {
                                    int dy = (_dy - 1) * d;
                                    if (_dy == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node((int)x, y + dy));
                                        if (DEBUG) setTile((int)x, sh + roof.GetLength(2), y + dy, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dy, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 180, true);
                                        else if (d == 1)
                                            setTile((int)x, h, y + dy, id, data, 180, null);
                                    }
                                }
                                cnt++;
                            }
                        }
                    }
                } else {
                    bool reflex_last = false;
                    Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                    Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                    Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                    float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                    float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                    if (!(last is null) && -cos_th < cos1 && cos1 < cos_th) {
                        List<Node> triangle1 = new List<Node>();
                        triangle1.Add(last.Value);
                        triangle1.Add(new Node(startx, starty));
                        triangle1.Add(new Node(endx, endy));
                        bool? PosiInPolygon = null, PosiInTriangle = null;
                        if (pnpoly4(nodes, new Node(startx - RoofConfig.GetReduceDelta(), starty))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(startx + RoofConfig.GetReduceDelta(), starty))) PosiInPolygon = true;
                        if (pnpoly3(triangle1, new Node(startx - RoofConfig.GetReduceDelta(), starty))) PosiInTriangle = false;
                        else if (pnpoly3(triangle1, new Node(startx + RoofConfig.GetReduceDelta(), starty))) PosiInTriangle = true;
                        if (PosiInPolygon is null || PosiInTriangle is null) {
                            if (pnpoly4(nodes, new Node(startx, starty - RoofConfig.GetReduceDelta()))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(startx, starty + RoofConfig.GetReduceDelta()))) PosiInPolygon = true;
                            if (pnpoly3(triangle1, new Node(startx, starty - RoofConfig.GetReduceDelta()))) PosiInTriangle = false;
                            else if (pnpoly3(triangle1, new Node(startx, starty + RoofConfig.GetReduceDelta()))) PosiInTriangle = true;
                            if (PosiInPolygon != null && PosiInTriangle != null)
                                reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            else
                                reflex_last = false;
                        } else {
                            reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        }
                        if (reflex_last && DEBUG) {
                            print("reflex_last is true");
                            setTile(startx, sh + roof.GetLength(2) + 2, starty, 133, 0);
                            print("last:(" + last.Value.x + "," + last.Value.y + ")");
                            print("start:(" + startx + "," + starty + ")");
                            print("end:(" + endx + "," + endy + ")");
                            print("next:(" + next.Value.x + "," + next.Value.y + ")");
                        }
                    }
                    bool reflex_next = false;
                    if (!(next is null) && -cos_th < cos2 && cos2 < cos_th) {
                        List<Node> triangle2 = new List<Node>();
                        triangle2.Add(new Node(startx, starty));
                        triangle2.Add(new Node(endx, endy));
                        triangle2.Add(next.Value);
                        bool? PosiInPolygon = null, PosiInTriangle = null;
                        if (pnpoly4(nodes, new Node(endx - RoofConfig.GetReduceDelta(), endy))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(endx + RoofConfig.GetReduceDelta(), endy))) PosiInPolygon = true;
                        if (pnpoly3(triangle2, new Node(endx - RoofConfig.GetReduceDelta(), endy))) PosiInTriangle = false;
                        else if (pnpoly3(triangle2, new Node(endx + RoofConfig.GetReduceDelta(), endy))) PosiInTriangle = true;
                        if (PosiInPolygon == null || PosiInTriangle == null) {
                            if (pnpoly4(nodes, new Node(endx, endy - RoofConfig.GetReduceDelta()))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(endx, endy + RoofConfig.GetReduceDelta()))) PosiInPolygon = true;
                            if (pnpoly3(triangle2, new Node(endx, endy - RoofConfig.GetReduceDelta()))) PosiInTriangle = false;
                            else if (pnpoly3(triangle2, new Node(endx, endy + RoofConfig.GetReduceDelta()))) PosiInTriangle = true;
                            if (PosiInPolygon != null && PosiInTriangle != null)
                                reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            else
                                reflex_next = false;
                        } else {
                            reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        }
                        if (reflex_next && DEBUG) {
                            print("reflex_next is true");
                            setTile(endx, sh + roof.GetLength(2) + 2, endy, 133, 0);
                            print("last:(" + last.Value.x + "," + last.Value.y + ")");
                            print("start:(" + startx + "," + starty + ")");
                            print("end:(" + endx + "," + endy + ")");
                            print("next:(" + next.Value.x + "," + next.Value.y + ")");
                        }
                    }

                    int d = 0;
                    int my = (int)Math.Round((double)(starty + endy) / 2);
                    int mx = (int)Math.Floor((float)(my - b) / k);
                    if (pnpoly2(nodes, new Node(mx - RoofConfig.GetReduceDelta(), my))) d = -1;
                    else if (pnpoly2(nodes, new Node(mx + RoofConfig.GetReduceDelta(), my))) d = 1;

                    if (starty <= endy) {
                        if (reflex_last) {
                            for (float y = starty - roof.GetLength(1); y < starty; y++) {
                                int x = (int)Math.Floor((float)(y - b) / k);
                                int start_dx = starty - (int)y;
                                for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                    int dx = (_dx - 1) * d;
                                    if (_dx == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node(x + dx, (int)y));
                                        if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 90, null);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false);
                                    }
                                }
                                cnt++;
                            }
                        }
                        for (float y = starty; y <= endy; y++) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            int end_dx = roof.GetLength(1);
                            if (!(last is null) && !reflex_last) {
                                if (-cos_th < cos1 && cos1 < cos_th) {
                                    if (startx != last.Value.x) {
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (-1 <= last_k && last_k < 1 || skipKCheck) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (next.Value.x != endx) {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (-1 <= next_k && next_k < 1 || skipKCheck) end_dx = Math.Min(end_dx, endy - (int)y + 1);
                                    }
                                }
                            }
                            for (int _dx = 0; _dx < end_dx; _dx++) {
                                int dx = (_dx - 1) * d;
                                if (_dx == roof.GetLength(1) - 1) {
                                    roof_node_list.Add(new Node(x + dx, (int)y));
                                    if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false);
                                    else if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 90, null);
                                }
                            }
                            cnt++;
                        }
                        if (reflex_next) {
                            for (float y = endy; y < endy + roof.GetLength(1); y++) {
                                int x = (int)Math.Floor((float)(y - b) / k);
                                int start_dx = (int)y - endy;
                                for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                    int dx = (_dx - 1) * d;
                                    if (_dx == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node(x + dx, (int)y));
                                        if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 90, null);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false);
                                    }
                                }
                                cnt++;
                            }
                        }
                    } else {
                        if (reflex_last) {
                            for (float y = starty + roof.GetLength(1); y > starty; y--) {
                                int x = (int)Math.Floor((float)(y - b) / k);
                                int start_dx = (int)y - starty;
                                for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                    int dx = (_dx - 1) * d;
                                    if (_dx == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node(x + dx, (int)y));
                                        if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, false);
                                    }
                                }
                                cnt++;
                            }
                        }
                        for (float y = starty; y >= endy; y--) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            int end_dx = roof.GetLength(1);
                            if (!(last is null) && !reflex_last) {
                                if (-cos_th < cos1 && cos1 < cos_th) {
                                    if (startx != last.Value.x) {
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (-1 <= last_k && last_k < 1 || skipKCheck) end_dx = Math.Min(roof.GetLength(1), starty - (int)y + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (next.Value.x != endx) {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (-1 <= next_k && next_k < 1 || skipKCheck) end_dx = Math.Min(end_dx, (int)y - endy + 1);
                                    }
                                }
                            }
                            for (int _dx = 0; _dx < end_dx; _dx++) {
                                int dx = (_dx - 1) * d;
                                if (_dx == roof.GetLength(1) - 1) {
                                    roof_node_list.Add(new Node(x + dx, (int)y));
                                    if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    }
                                    if (id == 0) continue;
                                    if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, false);
                                    else if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null);
                                }
                            }
                            cnt++;
                        }
                        if (reflex_next) {
                            for (float y = endy - 1; y > endy - roof.GetLength(1); y--) {
                                int x = (int)Math.Floor((float)(y - b) / k);
                                int start_dx = endy - (int)y;
                                for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                    int dx = (_dx - 1) * d;
                                    if (_dx == roof.GetLength(1) - 1 && refalist) {
                                        roof_node_list.Add(new Node(x + dx, (int)y));
                                        if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                    }
                                    for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                        int h = _h + sh;
                                        Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                        if (cblock.Equals(B))
                                            cblock = Base.Value;
                                        else if (cblock.Equals(WF))
                                            cblock = RoofConfig.WindowFrame;
                                        else if (cblock.Equals(W))
                                            cblock = RoofConfig.Window;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, false);
                                    }
                                }
                                cnt++;
                            }
                        }
                    }
                }
            } else {
                bool reflex_last = false;
                Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                if (!(last is null) && -cos_th < cos1 && cos1 < cos_th) {
                    List<Node> triangle1 = new List<Node>();
                    triangle1.Add(last.Value);
                    triangle1.Add(new Node(startx, starty));
                    triangle1.Add(new Node(endx, endy));
                    bool? PosiInPolygon = null, PosiInTriangle = null;
                    if (pnpoly4(nodes, new Node(startx - RoofConfig.GetReduceDelta(), starty))) PosiInPolygon = false;
                    else if (pnpoly4(nodes, new Node(startx + RoofConfig.GetReduceDelta(), starty))) PosiInPolygon = true;
                    if (pnpoly3(triangle1, new Node(startx - RoofConfig.GetReduceDelta(), starty))) PosiInTriangle = false;
                    else if (pnpoly3(triangle1, new Node(startx + RoofConfig.GetReduceDelta(), starty))) PosiInTriangle = true;
                    if (PosiInPolygon is null || PosiInTriangle is null) {
                        if (pnpoly4(nodes, new Node(startx, starty - RoofConfig.GetReduceDelta()))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(startx, starty + RoofConfig.GetReduceDelta()))) PosiInPolygon = true;
                        if (pnpoly3(triangle1, new Node(startx, starty - RoofConfig.GetReduceDelta()))) PosiInTriangle = false;
                        else if (pnpoly3(triangle1, new Node(startx, starty + RoofConfig.GetReduceDelta()))) PosiInTriangle = true;
                        if (PosiInPolygon != null && PosiInTriangle != null)
                            reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        else
                            reflex_last = false;
                    } else {
                        reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                    }
                    if (reflex_last && DEBUG) {
                        print("reflex_last is true");
                        setTile(startx, sh + roof.GetLength(2) + 2, starty, 133, 0);
                        print("last:(" + last.Value.x + "," + last.Value.y + ")");
                        print("start:(" + startx + "," + starty + ")");
                        print("end:(" + endx + "," + endy + ")");
                        print("next:(" + next.Value.x + "," + next.Value.y + ")");
                    }
                }
                bool reflex_next = false;
                if (!(next is null) && -cos_th < cos2 && cos2 < cos_th) {
                    List<Node> triangle2 = new List<Node>();
                    triangle2.Add(new Node(startx, starty));
                    triangle2.Add(new Node(endx, endy));
                    triangle2.Add(next.Value);
                    bool? PosiInPolygon = null, PosiInTriangle = null;
                    if (pnpoly4(nodes, new Node(endx - RoofConfig.GetReduceDelta(), endy))) PosiInPolygon = false;
                    else if (pnpoly4(nodes, new Node(endx + RoofConfig.GetReduceDelta(), endy))) PosiInPolygon = true;
                    if (pnpoly3(triangle2, new Node(endx - RoofConfig.GetReduceDelta(), endy))) PosiInTriangle = false;
                    else if (pnpoly3(triangle2, new Node(endx + RoofConfig.GetReduceDelta(), endy))) PosiInTriangle = true;
                    if (PosiInPolygon == null || PosiInTriangle == null) {
                        if (pnpoly4(nodes, new Node(endx, endy - RoofConfig.GetReduceDelta()))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(endx, endy + RoofConfig.GetReduceDelta()))) PosiInPolygon = true;
                        if (pnpoly3(triangle2, new Node(endx, endy - RoofConfig.GetReduceDelta()))) PosiInTriangle = false;
                        else if (pnpoly3(triangle2, new Node(endx, endy + RoofConfig.GetReduceDelta()))) PosiInTriangle = true;
                        if (PosiInPolygon != null && PosiInTriangle != null)
                            reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        else
                            reflex_next = false;
                    } else {
                        reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                    }
                    if (reflex_next && DEBUG) {
                        print("reflex_next is true");
                        setTile(endx, sh + roof.GetLength(2) + 2, endy, 133, 0);
                        print("last:(" + last.Value.x + "," + last.Value.y + ")");
                        print("start:(" + startx + "," + starty + ")");
                        print("end:(" + endx + "," + endy + ")");
                        print("next:(" + next.Value.x + "," + next.Value.y + ")");
                    }
                }

                int d = 0;
                int my = (int)Math.Round((double)(starty + endy) / 2);
                int mx = startx;
                if (pnpoly2(nodes, new Node(mx - RoofConfig.GetReduceDelta(), my))) d = -1;
                else if (pnpoly2(nodes, new Node(mx + RoofConfig.GetReduceDelta(), my))) d = 1;

                if (starty <= endy) {
                    if (reflex_last) {
                        for (float y = starty - roof.GetLength(1); y < starty; y++) {
                            int x = startx;
                            int start_dx = starty - (int)y;
                            for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                int dx = (_dx - 1) * d;
                                if (_dx == roof.GetLength(1) - 1 && refalist) {
                                    roof_node_list.Add(new Node(x + dx, (int)y));
                                    if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, null);
                                }
                            }
                            cnt++;
                        }
                    }
                    for (float y = starty; y <= endy; y++) {
                        int x = startx;
                        int end_dx = roof.GetLength(1);
                        if (!(last is null) && !reflex_last) {
                            if (-cos_th < cos1 && cos1 < cos_th) {
                                if (startx != last.Value.x) {
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (-1 <= last_k && last_k < 1 || skipKCheck) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                                }
                            }
                        }
                        if (!(next is null) && !reflex_next) {
                            if (-cos_th < cos2 && cos2 < cos_th) {
                                if (next.Value.x != endx) {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (-1 <= next_k && next_k < 1 || skipKCheck) end_dx = Math.Min(end_dx, endy - (int)y + 1);
                                }
                            }
                        }
                        for (int _dx = 0; _dx < end_dx; _dx++) {
                            int dx = (_dx - 1) * d;
                            if (_dx == roof.GetLength(1) - 1) {
                                roof_node_list.Add(new Node(x + dx, (int)y));
                                if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                            }
                            for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                int h = _h + sh;
                                Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                if (cblock.Equals(B))
                                    cblock = Base.Value;
                                else if (cblock.Equals(WF))
                                    cblock = RoofConfig.WindowFrame;
                                else if (cblock.Equals(W))
                                    cblock = RoofConfig.Window;
                                int id = cblock.id;
                                int data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (id == 0) continue;
                                if (d == 1)
                                    setTile(x + dx, h, (int)y, id, data, 90, null);
                                else if (d == -1)
                                    setTile(x + dx, h, (int)y, id, data, 90, false);
                            }
                        }
                        cnt++;
                    }
                    if (reflex_next) {
                        for (float y = endy; y < endy + roof.GetLength(1); y++) {
                            int x = startx;
                            int start_dx = (int)y - endy;
                            for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                int dx = (_dx - 1) * d;
                                if (_dx == roof.GetLength(1) - 1 && refalist) {
                                    roof_node_list.Add(new Node(x + dx, (int)y));
                                    if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, null);
                                }
                            }
                            cnt++;
                        }
                    }
                } else {
                    if (reflex_last) {
                        for (float y = starty + roof.GetLength(1); y > starty; y--) {
                            int x = startx;
                            int start_dx = (int)y - starty;
                            for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                int dx = (_dx - 1) * d;
                                if (_dx == roof.GetLength(1) - 1 && refalist) {
                                    roof_node_list.Add(new Node(x + dx, (int)y));
                                    if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, false);
                                }
                            }
                            cnt++;
                        }
                    }
                    for (float y = starty; y >= endy; y--) {
                        int x = startx;
                        int end_dx = roof.GetLength(1);
                        if (!(last is null) && !reflex_last) {
                            if (-cos_th < cos1 && cos1 < cos_th) {
                                if (startx != last.Value.x) {
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (-1 <= last_k && last_k < 1 || skipKCheck) end_dx = Math.Min(roof.GetLength(1), starty - (int)y + 1);
                                }
                            }
                        }
                        if (!(next is null) && !reflex_next) {
                            if (-cos_th < cos2 && cos2 < cos_th) {
                                if (next.Value.x != endx) {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (-1 <= next_k && next_k < 1 || skipKCheck) end_dx = Math.Min(end_dx, (int)y - endy + 1);
                                }
                            }
                        }
                        for (int _dx = 0; _dx < end_dx; _dx++) {
                            int dx = (_dx - 1) * d;
                            if (_dx == roof.GetLength(1) - 1) {
                                roof_node_list.Add(new Node(x + dx, (int)y));
                                if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                            }
                            for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                int h = _h + sh;
                                Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                if (cblock.Equals(B))
                                    cblock = Base.Value;
                                else if (cblock.Equals(WF))
                                    cblock = RoofConfig.WindowFrame;
                                else if (cblock.Equals(W))
                                    cblock = RoofConfig.Window;
                                int id = cblock.id;
                                int data = cblock.data;
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (id == 0) continue;
                                if (d == 1)
                                    setTile(x + dx, h, (int)y, id, data, 270, false);
                                else if (d == -1)
                                    setTile(x + dx, h, (int)y, id, data, 270, null);
                            }
                        }
                        cnt++;
                    }
                    if (reflex_next) {
                        for (float y = endy - 1; y > endy - roof.GetLength(1); y--) {
                            int x = startx;
                            int start_dx = endy - (int)y;
                            for (int _dx = start_dx; _dx < roof.GetLength(1); _dx++) {
                                int dx = (_dx - 1) * d;
                                if (_dx == roof.GetLength(1) - 1 && refalist) {
                                    roof_node_list.Add(new Node(x + dx, (int)y));
                                    if (DEBUG) setTile(x + dx, sh + roof.GetLength(2), (int)y, 133, 0);
                                }
                                for (int _h = 0; _h < roof.GetLength(2); _h++) {
                                    int h = _h + sh;
                                    Block cblock = roof[cnt % roof.GetLength(0), _dx, _h];
                                    if (cblock.Equals(B))
                                        cblock = Base.Value;
                                    else if (cblock.Equals(WF))
                                        cblock = RoofConfig.WindowFrame;
                                    else if (cblock.Equals(W))
                                        cblock = RoofConfig.Window;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, false);
                                }
                            }
                            cnt++;
                        }
                    }
                }
            }
        }
        void DrawLine_Interior_v3(int startx, int starty, int endx, int endy, int max_level, Interior InteriorConfig, List<Node> nodes, Node? last, Node? next, Block? Base = null, bool skipKCheck = false) {
            // v3的内饰放置算法：和v2相比，对于扩展判定如果cos范围大于cos_th直接认定为需要扩展
            Block[,,] interior = InteriorConfig.Data;
            if (InteriorConfig.Base == null) InteriorConfig.Base = Base;
            if (last is null || next is null) return;
            int c_rand_type = -1;
            if (startx != endx) {
                double k = (double)(endy - starty) / (double)(endx - startx);
                double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
                if (Math.Abs(k) <= 1) {
                    bool reflex_last = false, reflex_next = false;
                    Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                    Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                    Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                    float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                    float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                    if (startx == last.Value.x) last_k = inf;
                    if (next.Value.x == endx) next_k = inf;
                    if (Math.Abs(last_k) > 1) {
                        if (Math.Abs(cos1) > cos_th)
                            reflex_last = true;
                        else {
                            List<Node> triangle1 = new List<Node>();
                            triangle1.Add(last.Value);
                            triangle1.Add(new Node(startx, starty));
                            triangle1.Add(new Node(endx, endy));
                            bool? PosiInPolygon = null, PosiInTriangle = null;
                            if (pnpoly4(nodes, new Node(startx, starty - InteriorConfig.GetReduceDelta()))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(startx, starty + InteriorConfig.GetReduceDelta()))) PosiInPolygon = true;
                            if (pnpoly3(triangle1, new Node(startx, starty - InteriorConfig.GetReduceDelta()))) PosiInTriangle = false;
                            else if (pnpoly3(triangle1, new Node(startx, starty + InteriorConfig.GetReduceDelta()))) PosiInTriangle = true;
                            if (PosiInPolygon is null || PosiInTriangle is null) {
                                if (pnpoly4(nodes, new Node(startx - InteriorConfig.GetReduceDelta(), starty))) PosiInPolygon = false;
                                else if (pnpoly4(nodes, new Node(startx + InteriorConfig.GetReduceDelta(), starty))) PosiInPolygon = true;
                                if (pnpoly3(triangle1, new Node(startx - InteriorConfig.GetReduceDelta(), starty))) PosiInTriangle = false;
                                else if (pnpoly3(triangle1, new Node(startx + InteriorConfig.GetReduceDelta(), starty))) PosiInTriangle = true;
                                if (PosiInPolygon != null && PosiInTriangle != null)
                                    reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                                else
                                    reflex_last = false;
                            } else {
                                reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            }
                        }
                    }
                    if (Math.Abs(next_k) > 1) {
                        if (Math.Abs(cos2) > cos_th)
                            reflex_next = true;
                        else {
                            List<Node> triangle2 = new List<Node>();
                            triangle2.Add(new Node(startx, starty));
                            triangle2.Add(new Node(endx, endy));
                            triangle2.Add(next.Value);
                            bool? PosiInPolygon = null, PosiInTriangle = null;
                            if (pnpoly4(nodes, new Node(endx, endy - InteriorConfig.GetReduceDelta()))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(endx, endy + InteriorConfig.GetReduceDelta()))) PosiInPolygon = true;
                            if (pnpoly3(triangle2, new Node(endx, endy - InteriorConfig.GetReduceDelta()))) PosiInTriangle = false;
                            else if (pnpoly3(triangle2, new Node(endx, endy + InteriorConfig.GetReduceDelta()))) PosiInTriangle = true;
                            if (PosiInPolygon == null || PosiInTriangle == null) {
                                if (pnpoly4(nodes, new Node(endx - InteriorConfig.GetReduceDelta(), endy))) PosiInPolygon = false;
                                else if (pnpoly4(nodes, new Node(endx + InteriorConfig.GetReduceDelta(), endy))) PosiInPolygon = true;
                                if (pnpoly3(triangle2, new Node(endx - InteriorConfig.GetReduceDelta(), endy))) PosiInTriangle = false;
                                else if (pnpoly3(triangle2, new Node(endx + InteriorConfig.GetReduceDelta(), endy))) PosiInTriangle = true;
                                if (PosiInPolygon != null && PosiInTriangle != null)
                                    reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                                else
                                    reflex_next = false;
                            } else {
                                reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            }
                        }
                    }

                    int d = 0;
                    int mx = (int)Math.Round((double)(startx + endx) / 2);
                    int my = (int)Math.Round(k * mx + b);
                    if (pnpoly2(nodes, new Node(mx, my - InteriorConfig.GetReduceDelta()))) d = -1;
                    else if (pnpoly2(nodes, new Node(mx, my + InteriorConfig.GetReduceDelta()))) d = 1;
                    else return;

                    int GetValidWidth(int x, int y) {
                        int ret = 1;
                        for (; ret <= InteriorConfig.GetWidth(); ret++) {
                            if (!pnpoly4(nodes, new Node(x, y + ret * d))) break;
                        }
                        return ret - 1;
                    }

                    for (int clevel = 0; clevel < max_level; clevel++) {
                        int sh = clevel * InteriorConfig.GetHeight();
                        if (startx <= endx) {
                            if (reflex_last) {
                                for (float x = startx - InteriorConfig.GetWidth() - 1; x < startx; x++) {
                                    int y = (int)Math.Round(k * x + b);
                                    int start_dy = startx - (int)x;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dy = GetValidWidth((int)x, y); _dy > start_dy; _dy--) {
                                            int dy = _dy * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dy];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == 1)
                                                setTile((int)x, h, y + dy, id, data, 0, null, true);
                                            else if (d == -1)
                                                setTile((int)x, h, y + dy, id, data, 0, true, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                            for (float x = startx; x < endx; x++) {
                                int y = (int)Math.Round(k * x + b);
                                int end_dy = GetValidWidth((int)x, y);
                                if (!(last is null) && !reflex_last) {
                                    if (-cos_th < cos1 && cos1 < cos_th) {
                                        if (startx == last.Value.x) {
                                            end_dy = Math.Min(end_dy, (int)x - startx + 1);
                                        } else {
                                            if (Math.Abs(last_k) > 1 || skipKCheck)
                                                end_dy = Math.Min(end_dy, (int)x - startx + 1);
                                        }
                                    }
                                }
                                if (!(next is null) && !reflex_next) {
                                    if (-cos_th < cos2 && cos2 < cos_th) {
                                        if (endx == next.Value.x) {
                                            end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                        } else {
                                            if (Math.Abs(next_k) > 1 || skipKCheck)
                                                end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                        }
                                    }
                                }
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dy = end_dy; _dy > 0; _dy--) {
                                        int dy = _dy * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dy];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == 1)
                                            setTile((int)x, h, y + dy, id, data, 0, null, true);
                                        else if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 0, true, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                            if (reflex_next) {
                                for (float x = endx; x <= endx + InteriorConfig.GetWidth(); x++) {
                                    int y = (int)Math.Round(k * x + b);
                                    int start_dy = (int)x - endx;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dy = GetValidWidth((int)x, y); _dy > start_dy; _dy--) {
                                            int dy = _dy * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dy];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == 1)
                                                setTile((int)x, h, y + dy, id, data, 0, null, true);
                                            else if (d == -1)
                                                setTile((int)x, h, y + dy, id, data, 0, true, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                        } else {
                            if (reflex_last) {
                                for (float x = startx + InteriorConfig.GetWidth() + 1; x > startx; x--) {
                                    int y = (int)Math.Round(k * x + b);
                                    int start_dy = (int)x - startx;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dy = GetValidWidth((int)x, y); _dy > start_dy; _dy--) {
                                            int dy = _dy * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dy];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == 1)
                                                setTile((int)x, h, y + dy, id, data, 180, true, true);
                                            else if (d == -1)
                                                setTile((int)x, h, y + dy, id, data, 180, null, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                            for (float x = startx; x > endx; x--) {
                                int y = (int)Math.Round(k * x + b);
                                int end_dy = GetValidWidth((int)x, y);
                                if (!(last is null) && !reflex_last) {
                                    if (-cos_th < cos1 && cos1 < cos_th) {
                                        if (startx == last.Value.x) {
                                            end_dy = Math.Min(end_dy, startx - (int)x + 1);
                                        } else {
                                            if (Math.Abs(last_k) > 1 || skipKCheck)
                                                end_dy = Math.Min(end_dy, startx - (int)x + 1);
                                        }
                                    }
                                }
                                if (!(next is null) && !reflex_next) {
                                    if (-cos_th < cos2 && cos2 < cos_th) {
                                        if (next.Value.x == endx) {
                                            end_dy = Math.Min(end_dy, (int)x - endx + 1);
                                        } else {
                                            if (Math.Abs(next_k) > 1 || skipKCheck)
                                                end_dy = Math.Min(end_dy, (int)x - endx + 1);
                                        }
                                    }
                                }
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dy = end_dy; _dy > 0; _dy--) {
                                        int dy = _dy * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dy];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == 1)
                                            setTile((int)x, h, y + dy, id, data, 180, true, true);
                                        else if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 180, null, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                            if (reflex_next) {
                                for (float x = endx; x >= endx - InteriorConfig.GetWidth(); x--) {
                                    int y = (int)Math.Round(k * x + b);
                                    int start_dy = endx - (int)x;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dy = GetValidWidth((int)x, y); _dy > start_dy; _dy--) {
                                            int dy = _dy * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dy];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == 1)
                                                setTile((int)x, h, y + dy, id, data, 180, true, true);
                                            else if (d == -1)
                                                setTile((int)x, h, y + dy, id, data, 180, null, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                        }
                    }
                } else {
                    bool reflex_last = false, reflex_next = false;
                    Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                    Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                    Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                    float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                    float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                    if (startx == last.Value.x) last_k = inf;
                    if (next.Value.x == endx) next_k = inf;
                    if (Math.Abs(last_k) <= 1) {
                        if (Math.Abs(cos1) > cos_th)
                            reflex_last = true;
                        else {
                            List<Node> triangle1 = new List<Node>();
                            triangle1.Add(last.Value);
                            triangle1.Add(new Node(startx, starty));
                            triangle1.Add(new Node(endx, endy));
                            bool? PosiInPolygon = null, PosiInTriangle = null;
                            if (pnpoly4(nodes, new Node(startx - InteriorConfig.GetReduceDelta(), starty))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(startx + InteriorConfig.GetReduceDelta(), starty))) PosiInPolygon = true;
                            if (pnpoly3(triangle1, new Node(startx - InteriorConfig.GetReduceDelta(), starty))) PosiInTriangle = false;
                            else if (pnpoly3(triangle1, new Node(startx + InteriorConfig.GetReduceDelta(), starty))) PosiInTriangle = true;
                            if (PosiInPolygon is null || PosiInTriangle is null) {
                                if (pnpoly4(nodes, new Node(startx, starty - InteriorConfig.GetReduceDelta()))) PosiInPolygon = false;
                                else if (pnpoly4(nodes, new Node(startx, starty + InteriorConfig.GetReduceDelta()))) PosiInPolygon = true;
                                if (pnpoly3(triangle1, new Node(startx, starty - InteriorConfig.GetReduceDelta()))) PosiInTriangle = false;
                                else if (pnpoly3(triangle1, new Node(startx, starty + InteriorConfig.GetReduceDelta()))) PosiInTriangle = true;
                                if (PosiInPolygon != null && PosiInTriangle != null)
                                    reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                                else
                                    reflex_last = false;
                            } else {
                                reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            }
                        }
                    }
                    if (Math.Abs(next_k) <= 1) {
                        if (Math.Abs(cos2) > cos_th)
                            reflex_next = true;
                        else {
                            List<Node> triangle2 = new List<Node>();
                            triangle2.Add(new Node(startx, starty));
                            triangle2.Add(new Node(endx, endy));
                            triangle2.Add(next.Value);
                            bool? PosiInPolygon = null, PosiInTriangle = null;
                            if (pnpoly4(nodes, new Node(endx - InteriorConfig.GetReduceDelta(), endy))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(endx + InteriorConfig.GetReduceDelta(), endy))) PosiInPolygon = true;
                            if (pnpoly3(triangle2, new Node(endx - InteriorConfig.GetReduceDelta(), endy))) PosiInTriangle = false;
                            else if (pnpoly3(triangle2, new Node(endx + InteriorConfig.GetReduceDelta(), endy))) PosiInTriangle = true;
                            if (PosiInPolygon == null || PosiInTriangle == null) {
                                if (pnpoly4(nodes, new Node(endx, endy - InteriorConfig.GetReduceDelta()))) PosiInPolygon = false;
                                else if (pnpoly4(nodes, new Node(endx, endy + InteriorConfig.GetReduceDelta()))) PosiInPolygon = true;
                                if (pnpoly3(triangle2, new Node(endx, endy - InteriorConfig.GetReduceDelta()))) PosiInTriangle = false;
                                else if (pnpoly3(triangle2, new Node(endx, endy + InteriorConfig.GetReduceDelta()))) PosiInTriangle = true;
                                if (PosiInPolygon != null && PosiInTriangle != null)
                                    reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                                else
                                    reflex_next = false;
                            } else {
                                reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            }
                        }
                    }

                    int d = 0;
                    int my = (int)Math.Round((double)(starty + endy) / 2);
                    int mx = (int)Math.Floor((float)(my - b) / k);
                    if (pnpoly2(nodes, new Node(mx - InteriorConfig.GetReduceDelta(), my))) d = -1;
                    else if (pnpoly2(nodes, new Node(mx + InteriorConfig.GetReduceDelta(), my))) d = 1;
                    else return;

                    int GetValidWidth(int x, int y) {
                        int ret = 1;
                        for (; ret <= InteriorConfig.GetWidth(); ret++) {
                            if (!pnpoly4(nodes, new Node(x + ret * d, y))) break;
                        }
                        return ret - 1;
                    }

                    for (int clevel = 0; clevel < max_level; clevel++) {
                        int sh = clevel * InteriorConfig.GetHeight();
                        if (starty <= endy) {
                            if (reflex_last) {
                                for (float y = starty - InteriorConfig.GetWidth() - 1; y < starty; y++) {
                                    int x = (int)Math.Floor((float)(y - b) / k);
                                    int start_dx = starty - (int)y;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                            int dx = _dx * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == -1)
                                                setTile(x + dx, h, (int)y, id, data, 90, null, true);
                                            else if (d == 1)
                                                setTile(x + dx, h, (int)y, id, data, 90, false, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                            for (float y = starty; y < endy; y++) {
                                int x = (int)Math.Floor((float)(y - b) / k);
                                int end_dx = GetValidWidth(x, (int)y);
                                if (!(last is null) && !reflex_last) {
                                    if (-cos_th < cos1 && cos1 < cos_th) {
                                        if (startx != last.Value.x) {
                                            if (Math.Abs(last_k) <= 1 || skipKCheck)
                                                end_dx = Math.Min(end_dx, (int)y - starty + 1);
                                        }
                                    }
                                }
                                if (!(next is null) && !reflex_next) {
                                    if (-cos_th < cos2 && cos2 < cos_th) {
                                        if (endx != next.Value.x) {
                                            if (Math.Abs(next_k) <= 1 || skipKCheck)
                                                end_dx = Math.Min(end_dx, endy - (int)y + 1);
                                        }
                                    }
                                }
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dx = end_dx; _dx > 0; _dx--) {
                                        int dx = _dx * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 90, null, true);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                            if (reflex_next) {
                                for (float y = endy; y <= endy + InteriorConfig.GetWidth(); y++) {
                                    int x = (int)Math.Floor((float)(y - b) / k);
                                    int start_dx = (int)y - endy;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                            int dx = _dx * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == -1)
                                                setTile(x + dx, h, (int)y, id, data, 90, null, true);
                                            else if (d == 1)
                                                setTile(x + dx, h, (int)y, id, data, 90, false, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                        } else {
                            if (reflex_last) {
                                for (float y = starty + InteriorConfig.GetWidth() + 1; y > starty; y--) {
                                    int x = (int)Math.Floor((float)(y - b) / k);
                                    int start_dx = (int)y - starty;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                            int dx = _dx * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == -1)
                                                setTile(x + dx, h, (int)y, id, data, 270, false, true);
                                            else if (d == 1)
                                                setTile(x + dx, h, (int)y, id, data, 270, null, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                            for (float y = starty; y > endy; y--) {
                                int x = (int)Math.Floor((float)(y - b) / k);
                                int end_dx = GetValidWidth(x, (int)y);
                                if (!(last is null) && !reflex_last) {
                                    if (-cos_th < cos1 && cos1 < cos_th) {
                                        if (startx != last.Value.x) {
                                            if (Math.Abs(last_k) <= 1 || skipKCheck)
                                                end_dx = Math.Min(end_dx, starty - (int)y + 1);
                                        }
                                    }
                                }
                                if (!(next is null) && !reflex_next) {
                                    if (-cos_th < cos2 && cos2 < cos_th) {
                                        if (endx != next.Value.x) {
                                            if (Math.Abs(next_k) <= 1 || skipKCheck)
                                                end_dx = Math.Min(end_dx, (int)y - endy + 1);
                                        }
                                    }
                                }
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dx = end_dx; _dx > 0; _dx--) {
                                        int dx = _dx * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 270, false, true);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                            if (reflex_next) {
                                for (float y = endy; y >= endy + InteriorConfig.GetWidth(); y--) {
                                    int x = (int)Math.Floor((float)(y - b) / k);
                                    int start_dx = endy - (int)y;
                                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                        int h = _h + sh;
                                        for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                            int dx = _dx * d;
                                            Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                            if (cblock.Equals(B))
                                                cblock = InteriorConfig.Base.Value;
                                            else if (cblock.Equals(L))
                                                cblock = InteriorConfig.Light.Value;
                                            int id = cblock.id;
                                            int data = cblock.data;
                                            if (id == 0) continue;
                                            if (cblock.random == 1) {
                                                if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                                data = c_rand_data;
                                            } else if (cblock.random == 2) {
                                                data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            } else if (cblock.random == 3) {
                                                if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                                Block customed = setCustomStyle(cblock, c_rand_type);
                                                id = customed.id;
                                                data = customed.data;
                                            }
                                            if (d == -1)
                                                setTile(x + dx, h, (int)y, id, data, 270, false, true);
                                            else if (d == 1)
                                                setTile(x + dx, h, (int)y, id, data, 270, null, true);
                                        }
                                    }
                                    Cnt[clevel]++;
                                }
                            }
                        }
                    }
                }
            } else {
                bool reflex_last = false, reflex_next = false;
                Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                if (startx == last.Value.x) last_k = inf;
                if (next.Value.x == endx) next_k = inf;
                if (Math.Abs(last_k) <= 1) {
                    if (Math.Abs(cos1) > cos_th)
                        reflex_last = true;
                    else {
                        List<Node> triangle1 = new List<Node>();
                        triangle1.Add(last.Value);
                        triangle1.Add(new Node(startx, starty));
                        triangle1.Add(new Node(endx, endy));
                        bool? PosiInPolygon = null, PosiInTriangle = null;
                        if (pnpoly4(nodes, new Node(startx - InteriorConfig.GetReduceDelta(), starty))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(startx + InteriorConfig.GetReduceDelta(), starty))) PosiInPolygon = true;
                        if (pnpoly3(triangle1, new Node(startx - InteriorConfig.GetReduceDelta(), starty))) PosiInTriangle = false;
                        else if (pnpoly3(triangle1, new Node(startx + InteriorConfig.GetReduceDelta(), starty))) PosiInTriangle = true;
                        if (PosiInPolygon is null || PosiInTriangle is null) {
                            if (pnpoly4(nodes, new Node(startx, starty - InteriorConfig.GetReduceDelta()))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(startx, starty + InteriorConfig.GetReduceDelta()))) PosiInPolygon = true;
                            if (pnpoly3(triangle1, new Node(startx, starty - InteriorConfig.GetReduceDelta()))) PosiInTriangle = false;
                            else if (pnpoly3(triangle1, new Node(startx, starty + InteriorConfig.GetReduceDelta()))) PosiInTriangle = true;
                            if (PosiInPolygon != null && PosiInTriangle != null)
                                reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            else
                                reflex_last = false;
                        } else {
                            reflex_last = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        }
                    }
                }
                if (Math.Abs(next_k) <= 1) {
                    if (Math.Abs(cos2) > cos_th)
                        reflex_next = true;
                    else {
                        List<Node> triangle2 = new List<Node>();
                        triangle2.Add(new Node(startx, starty));
                        triangle2.Add(new Node(endx, endy));
                        triangle2.Add(next.Value);
                        bool? PosiInPolygon = null, PosiInTriangle = null;
                        if (pnpoly4(nodes, new Node(endx - InteriorConfig.GetReduceDelta(), endy))) PosiInPolygon = false;
                        else if (pnpoly4(nodes, new Node(endx + InteriorConfig.GetReduceDelta(), endy))) PosiInPolygon = true;
                        if (pnpoly3(triangle2, new Node(endx - InteriorConfig.GetReduceDelta(), endy))) PosiInTriangle = false;
                        else if (pnpoly3(triangle2, new Node(endx + InteriorConfig.GetReduceDelta(), endy))) PosiInTriangle = true;
                        if (PosiInPolygon == null || PosiInTriangle == null) {
                            if (pnpoly4(nodes, new Node(endx, endy - InteriorConfig.GetReduceDelta()))) PosiInPolygon = false;
                            else if (pnpoly4(nodes, new Node(endx, endy + InteriorConfig.GetReduceDelta()))) PosiInPolygon = true;
                            if (pnpoly3(triangle2, new Node(endx, endy - InteriorConfig.GetReduceDelta()))) PosiInTriangle = false;
                            else if (pnpoly3(triangle2, new Node(endx, endy + InteriorConfig.GetReduceDelta()))) PosiInTriangle = true;
                            if (PosiInPolygon != null && PosiInTriangle != null)
                                reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                            else
                                reflex_next = false;
                        } else {
                            reflex_next = PosiInPolygon.Value ^ PosiInTriangle.Value;
                        }
                    }
                }

                int d = 0;
                int my = (int)Math.Round((double)(starty + endy) / 2);
                int mx = startx;
                if (pnpoly2(nodes, new Node(mx - InteriorConfig.GetReduceDelta(), my))) d = -1;
                else if (pnpoly2(nodes, new Node(mx + InteriorConfig.GetReduceDelta(), my))) d = 1;
                else return;

                int GetValidWidth(int x, int y) {
                    int ret = 1;
                    for (; ret <= InteriorConfig.GetWidth(); ret++) {
                        if (!pnpoly4(nodes, new Node(x + ret * d, y))) break;
                    }
                    return ret - 1;
                }

                for (int clevel = 0; clevel < max_level; clevel++) {
                    int sh = clevel * InteriorConfig.GetHeight();
                    if (starty <= endy) {
                        if (reflex_last) {
                            for (float y = starty - InteriorConfig.GetWidth() - 1; y < starty; y++) {
                                int x = startx;
                                int start_dx = starty - (int)y;
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                        int dx = _dx * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 90, null, true);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                        }
                        for (float y = starty; y < endy; y++) {
                            int x = startx;
                            int end_dx = GetValidWidth(x, (int)y);
                            if (!(last is null) && !reflex_last) {
                                if (-cos_th < cos1 && cos1 < cos_th) {
                                    if (startx != last.Value.x) {
                                        if (Math.Abs(last_k) <= 1 || skipKCheck)
                                            end_dx = Math.Min(end_dx, (int)y - starty + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (endx != next.Value.x) {
                                        if (Math.Abs(next_k) <= 1 || skipKCheck)
                                            end_dx = Math.Min(end_dx, endy - (int)y + 1);
                                    }
                                }
                            }
                            for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                int h = _h + sh;
                                for (int _dx = end_dx; _dx > 0; _dx--) {
                                    int dx = _dx * d;
                                    Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                    if (cblock.Equals(B))
                                        cblock = InteriorConfig.Base.Value;
                                    else if (cblock.Equals(L))
                                        cblock = InteriorConfig.Light.Value;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    } else if (cblock.random == 3) {
                                        if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                        Block customed = setCustomStyle(cblock, c_rand_type);
                                        id = customed.id;
                                        data = customed.data;
                                    }
                                    if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 90, null, true);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false, true);
                                }
                            }
                            Cnt[clevel]++;
                        }
                        if (reflex_next) {
                            for (float y = endy; y <= endy + InteriorConfig.GetWidth(); y++) {
                                int x = startx;
                                int start_dx = (int)y - endy;
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                        int dx = _dx * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 90, null, true);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                        }
                    } else {
                        if (reflex_last) {
                            for (float y = starty + InteriorConfig.GetWidth() + 1; y > starty; y--) {
                                int x = startx;
                                int start_dx = (int)y - starty;
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                        int dx = _dx * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 270, false, true);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                        }
                        for (float y = starty; y > endy; y--) {
                            int x = startx;
                            int end_dx = GetValidWidth(x, (int)y);
                            if (!(last is null) && !reflex_last) {
                                if (-cos_th < cos1 && cos1 < cos_th) {
                                    if (startx != last.Value.x) {
                                        if (Math.Abs(last_k) <= 1 || skipKCheck)
                                            end_dx = Math.Min(end_dx, starty - (int)y + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (endx != next.Value.x) {
                                        if (Math.Abs(next_k) <= 1 || skipKCheck)
                                            end_dx = Math.Min(end_dx, (int)y - endy + 1);
                                    }
                                }
                            }
                            for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                int h = _h + sh;
                                for (int _dx = end_dx; _dx > 0; _dx--) {
                                    int dx = _dx * d;
                                    Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                    if (cblock.Equals(B))
                                        cblock = InteriorConfig.Base.Value;
                                    else if (cblock.Equals(L))
                                        cblock = InteriorConfig.Light.Value;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (cblock.random == 1) {
                                        if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        data = c_rand_data;
                                    } else if (cblock.random == 2) {
                                        data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    } else if (cblock.random == 3) {
                                        if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                        Block customed = setCustomStyle(cblock, c_rand_type);
                                        id = customed.id;
                                        data = customed.data;
                                    }
                                    if (d == -1)
                                        setTile(x + dx, h, (int)y, id, data, 270, false, true);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null, true);
                                }
                            }
                            Cnt[clevel]++;
                        }
                        if (reflex_next) {
                            for (float y = endy; y >= endy + InteriorConfig.GetWidth(); y--) {
                                int x = startx;
                                int start_dx = endy - (int)y;
                                for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                                    int h = _h + sh;
                                    for (int _dx = GetValidWidth(x, (int)y); _dx > start_dx; _dx--) {
                                        int dx = _dx * d;
                                        Block cblock = interior[_h, Cnt[clevel] % InteriorConfig.GetLength(), InteriorConfig.GetWidth() - _dx];
                                        if (cblock.Equals(B))
                                            cblock = InteriorConfig.Base.Value;
                                        else if (cblock.Equals(L))
                                            cblock = InteriorConfig.Light.Value;
                                        int id = cblock.id;
                                        int data = cblock.data;
                                        if (id == 0) continue;
                                        if (cblock.random == 1) {
                                            if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                            data = c_rand_data;
                                        } else if (cblock.random == 2) {
                                            data = rd.Next(cblock.rand_min, cblock.rand_max);
                                        } else if (cblock.random == 3) {
                                            if (c_rand_type == -1) c_rand_type = rd.Next(cblock.rand_min, cblock.rand_max);
                                            Block customed = setCustomStyle(cblock, c_rand_type);
                                            id = customed.id;
                                            data = customed.data;
                                        }
                                        if (d == -1)
                                            setTile(x + dx, h, (int)y, id, data, 270, false, true);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null, true);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                        }
                    }
                }
            }
        }
        void DrawLine_FirstFloor(int startx, int starty, int endx, int endy, FirstFloor FirstFloorConfig, List<Node> nodes, Block? Base) {
            var firstfloor = FirstFloorConfig.Data;
            if (FirstFloorConfig.Base == null) FirstFloorConfig.Base = Base;
            if (startx != endx) {
                double k = (double)(endy - starty) / (double)(endx - startx);
                double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
                if (-1 <= k && k < 1) {
                    int d = 0;
                    int mx = (int)Math.Round((double)(startx + endx) / 2);
                    int my = (int)Math.Round(k * mx + b);
                    if (pnpoly2(nodes, new Node(mx, my - FirstFloorConfig.GetReduceDelta()))) d = 1;
                    else if (pnpoly2(nodes, new Node(mx, my + FirstFloorConfig.GetReduceDelta()))) d = -1;
                    if (startx <= endx) {
                        for (float x = startx; x <= endx; x++) {
                            int y = (int)Math.Round(k * x + b);
                            for (int h = 0; h < firstfloor.GetLength(0); h++) {
                                for (int _dy = 0; _dy < firstfloor.GetLength(2); _dy++) {
                                    int dy = _dy * d;
                                    Block cblock = firstfloor[h, cnt % FirstFloorConfig.GetLength(), _dy];
                                    if (cblock.Equals(B))
                                        cblock = FirstFloorConfig.Base.Value;
                                    else if (cblock.Equals(W))
                                        cblock = FirstFloorConfig.Window;
                                    else if (cblock.Equals(U1))
                                        cblock = FirstFloorConfig.U1.Value;
                                    else if (cblock.Equals(U2))
                                        cblock = FirstFloorConfig.U2.Value;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile((int)x, h + 1, y + dy, id, data, 0, null);
                                    else if (d == 1)
                                        setTile((int)x, h + 1, y + dy, id, data, 0, true);
                                }
                            }
                            cnt++;
                        }
                    } else {
                        for (float x = startx; x >= endx; x--) {
                            int y = (int)Math.Round(k * x + b);
                            for (int h = 0; h < FirstFloorConfig.GetHeight(); h++) {
                                for (int _dy = 0; _dy < FirstFloorConfig.GetWidth(); _dy++) {
                                    int dy = _dy * d;
                                    Block cblock = firstfloor[h, cnt % FirstFloorConfig.GetLength(), _dy];
                                    if (cblock.Equals(B))
                                        cblock = FirstFloorConfig.Base.Value;
                                    else if (cblock.Equals(W))
                                        cblock = FirstFloorConfig.Window;
                                    else if (cblock.Equals(U1))
                                        cblock = FirstFloorConfig.U1.Value;
                                    else if (cblock.Equals(U2))
                                        cblock = FirstFloorConfig.U2.Value;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == -1)
                                        setTile((int)x, h + 1, y + dy, id, data, 180, true);
                                    else if (d == 1)
                                        setTile((int)x, h + 1, y + dy, id, data, 180, null);
                                }
                            }
                            cnt++;
                        }
                    }
                } else {
                    int d = 0;
                    int my = (int)Math.Round((double)(starty + endy) / 2);
                    int mx = (int)Math.Floor((float)(my - b) / k);
                    if (pnpoly2(nodes, new Node(mx - FirstFloorConfig.GetReduceDelta(), my))) d = 1;
                    else if (pnpoly2(nodes, new Node(mx + FirstFloorConfig.GetReduceDelta(), my))) d = -1;
                    if (starty <= endy) {
                        for (float y = starty; y <= endy; y++) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            for (int h = 0; h < FirstFloorConfig.GetHeight(); h++) {
                                for (int _dx = 0; _dx < FirstFloorConfig.GetWidth(); _dx++) {
                                    int dx = _dx * d;
                                    Block cblock = firstfloor[h, cnt % FirstFloorConfig.GetLength(), _dx];
                                    if (cblock.Equals(B))
                                        cblock = FirstFloorConfig.Base.Value;
                                    else if (cblock.Equals(W))
                                        cblock = FirstFloorConfig.Window;
                                    else if (cblock.Equals(U1))
                                        cblock = FirstFloorConfig.U1.Value;
                                    else if (cblock.Equals(U2))
                                        cblock = FirstFloorConfig.U2.Value;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == 1)
                                        setTile(x + dx, h + 1, (int)y, id, data, 90, null);
                                    else if (d == -1)
                                        setTile(x + dx, h + 1, (int)y, id, data, 90, false);
                                }
                            }
                            cnt++;
                        }
                    } else {
                        for (float y = starty; y >= endy; y--) {
                            int x = (int)Math.Floor((float)(y - b) / k);
                            for (int h = 0; h < FirstFloorConfig.GetHeight(); h++) {
                                for (int _dx = 0; _dx < FirstFloorConfig.GetWidth(); _dx++) {
                                    int dx = _dx * d;
                                    Block cblock = firstfloor[h, cnt % FirstFloorConfig.GetLength(), _dx];
                                    if (cblock.Equals(B))
                                        cblock = FirstFloorConfig.Base.Value;
                                    else if (cblock.Equals(W))
                                        cblock = FirstFloorConfig.Window;
                                    else if (cblock.Equals(U1))
                                        cblock = FirstFloorConfig.U1.Value;
                                    else if (cblock.Equals(U2))
                                        cblock = FirstFloorConfig.U2.Value;
                                    int id = cblock.id;
                                    int data = cblock.data;
                                    if (id == 0) continue;
                                    if (d == 1)
                                        setTile(x + dx, h + 1, (int)y, id, data, 270, false);
                                    else if (d == -1)
                                        setTile(x + dx, h + 1, (int)y, id, data, 270, null);
                                }
                            }
                            cnt++;
                        }
                    }
                }
            } else {
                int d = 0;
                int my = (int)Math.Round((double)(starty + endy) / 2);
                int mx = startx;
                if (pnpoly2(nodes, new Node(mx - FirstFloorConfig.GetReduceDelta(), my))) d = 1;
                else if (pnpoly2(nodes, new Node(mx + FirstFloorConfig.GetReduceDelta(), my))) d = -1;
                if (starty <= endy) {
                    for (float y = starty; y <= endy; y++) {
                        int x = startx;
                        for (int h = 0; h < FirstFloorConfig.GetHeight(); h++) {
                            for (int _dx = 0; _dx < FirstFloorConfig.GetWidth(); _dx++) {
                                int dx = _dx * d;
                                Block cblock = firstfloor[h, cnt % FirstFloorConfig.GetLength(), _dx];
                                if (cblock.Equals(B))
                                    cblock = FirstFloorConfig.Base.Value;
                                else if (cblock.Equals(W))
                                    cblock = FirstFloorConfig.Window;
                                else if (cblock.Equals(U1))
                                    cblock = FirstFloorConfig.U1.Value;
                                else if (cblock.Equals(U2))
                                    cblock = FirstFloorConfig.U2.Value;
                                int id = cblock.id;
                                int data = cblock.data;
                                if (id == 0) continue;
                                if (d == 1)
                                    setTile(x + dx, h + 1, (int)y, id, data, 90, null);
                                else if (d == -1)
                                    setTile(x + dx, h + 1, (int)y, id, data, 90, false);
                            }
                        }
                        cnt++;
                    }
                } else {
                    for (float y = starty; y >= endy; y--) {
                        int x = startx;
                        for (int h = 0; h < FirstFloorConfig.GetHeight(); h++) {
                            for (int _dx = 0; _dx < FirstFloorConfig.GetWidth(); _dx++) {
                                int dx = _dx * d;
                                Block cblock = firstfloor[h, cnt % FirstFloorConfig.GetLength(), _dx];
                                if (cblock.Equals(B))
                                    cblock = FirstFloorConfig.Base.Value;
                                else if (cblock.Equals(W))
                                    cblock = FirstFloorConfig.Window;
                                else if (cblock.Equals(U1))
                                    cblock = FirstFloorConfig.U1.Value;
                                else if (cblock.Equals(U2))
                                    cblock = FirstFloorConfig.U2.Value;
                                int id = cblock.id;
                                int data = cblock.data;
                                if (id == 0) continue;
                                if (d == 1)
                                    setTile(x + dx, h + 1, (int)y, id, data, 270, false);
                                else if (d == -1)
                                    setTile(x + dx, h + 1, (int)y, id, data, 270, null);
                            }
                        }
                        cnt++;
                    }
                }
            }
        }

        void FillPolygonScanline(List<Node> nodes, int h, int id, int data) {
            int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
            foreach (var node in nodes) {
                if (node.x == undefined || node.y == undefined) continue;
                minx = Math.Min(node.x, minx);
                miny = Math.Min(node.y, miny);
                maxx = Math.Max(node.x, maxx);
                maxy = Math.Max(node.y, maxy);
            }
            for (int x = minx; x <= maxx; x++) {
                for (int y = miny; y <= maxy; y++) {
                    if (pnpoly2(nodes, new Node(x, y)))
                        setTile(x, h, y, id, data);
                }
            }
        }
        // Pnpoly2: 增加了对套娃曲线判定的支持
        bool pnpoly2(List<Node> nodes, Node test) {
            bool c = false;
            int n = nodes.Count;
            for (int i = 0; i < n; i++) {
                //int j = (i == 0) ? (n - 1) : (i - 1);
                int j;
                if (i == 0 || nodes[i - 1].x == undefined || nodes[i - 1].y == undefined) {
                    for (j = i + 1; j < nodes.Count; j++) {
                        if (nodes[j].x == undefined || nodes[j].y == undefined) {
                            break;
                        }
                    }
                    j--;
                } else {
                    j = i - 1;
                }
                if (nodes[i].x == undefined || nodes[j].x == undefined) continue;
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
        // Pnpoly3: 专为Roof建造过程中的判定三角形准备，应用于普通多边形（非套娃曲线），并且边缘部分将会被判定为true
        bool pnpoly3(List<Node> nodes, Node test) {
            bool c = false;
            int n = nodes.Count;
            for (int i = 0; i < n; i++) {
                int j = (i == 0) ? (n - 1) : (i - 1);
                if (nodes[i].x == test.x && test.x == nodes[j].x && Math.Min(nodes[i].y, nodes[j].y) <= test.y && test.y <= Math.Max(nodes[i].y, nodes[j].y)
                    || nodes[i].y == test.y && test.y == nodes[j].y && Math.Min(nodes[i].x, nodes[j].x) <= test.x && test.x <= Math.Max(nodes[i].x, nodes[j].x))
                    return true;
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
        // Pnpoly4: 在Pnpoly2的基础上，边缘部分将会被判定为false
        bool pnpoly4(List<Node> nodes, Node test) {
            bool c = false;
            int n = nodes.Count;
            for (int i = 0; i < n; i++) {
                int j;
                if (i == 0 || nodes[i - 1].x == undefined || nodes[i - 1].y == undefined) {
                    for (j = i + 1; j < nodes.Count; j++) {
                        if (nodes[j].x == undefined || nodes[j].y == undefined) {
                            break;
                        }
                    }
                    j--;
                } else {
                    j = i - 1;
                }
                if (nodes[i].x == undefined || nodes[j].x == undefined) continue;
                if (nodes[i].x == test.x && test.x == nodes[j].x && Math.Min(nodes[i].y, nodes[j].y) <= test.y && test.y <= Math.Max(nodes[i].y, nodes[j].y)
                    || nodes[i].y == test.y && test.y == nodes[j].y && Math.Min(nodes[i].x, nodes[j].x) <= test.x && test.x <= Math.Max(nodes[i].x, nodes[j].x))
                    return false;
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
        bool IsSmallBuilding(List<Node> nodes) {
            const int small_threshold = 50;
            int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
            foreach (var node in nodes) {
                if (IsUndefined(node)) continue;
                minx = Math.Min(node.x, minx);
                miny = Math.Min(node.y, miny);
                maxx = Math.Max(node.x, maxx);
                maxy = Math.Max(node.y, maxy);
            }
            if (maxx - minx < small_threshold && maxy - miny < small_threshold)
                return true;
            else
                return false;
        }
        bool IsMiniBuilding(List<Node> nodes) {
            const int mini_threshold = 15;
            int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
            foreach (var node in nodes) {
                if (IsUndefined(node)) continue;
                minx = Math.Min(node.x, minx);
                miny = Math.Min(node.y, miny);
                maxx = Math.Max(node.x, maxx);
                maxy = Math.Max(node.y, maxy);
            }
            if (maxx - minx < mini_threshold && maxy - miny < mini_threshold)
                return true;
            else
                return false;
        }
        bool IsUndefined(Node node) {
            if (node.x == undefined && node.y == undefined) return true;
            else return false;
        }
        bool IsStair(int id) {
            int[] stairs = { 53, 134, 135, 136, 163, 164, 108, 109, 114, 128, 180, 156, 203 };
            foreach (var v in stairs) {
                if (id == v) return true;
            }
            return false;
        }
        void ClearCnt() {
            for (int i = 0; i < MAX_LEVEL; i++)
                Cnt[i] = 0;
        }
        Block setCustomStyle(Block origin, int type) {
            int id = origin.id, idata = origin.data;
            //Style
            if (id == 53 || id == 136) {    //Stair
                switch (type) {
                    case 0: //Oak
                        id = 53;
                        break;
                    case 1: //Spruce
                        id = 134;
                        break;
                    case 2: //Birch
                        id = 135;
                        break;
                    case 3: //Jungle
                        id = 136;
                        break;
                    case 4: //Acacia
                        id = 163;
                        break;
                    case 5: //Dark oak
                        id = 164;
                        break;
                    case 6: //Sand
                        id = 128;
                        break;
                    case 7: //Red sand
                        id = 180;
                        break;
                    case 8: //Purpur
                        id = 203;
                        break;
                    case 9: //Quartz
                        id = 156;
                        break;
                    default:
                        print("Unsupported format");
                        break;
                }
            } else if (id == 67) {  //Cobble stair
                switch (type) {
                    case 6: //Sand
                        id = 128;
                        break;
                    case 7: //Red sand
                        id = 180;
                        break;
                    case 8: //Purpur
                        id = 203;
                        break;
                    case 9: //Quartz
                        id = 156;
                        break;
                }
            } else if (id == 5) {   //Plank
                switch (type) {
                    case 0: //Oak
                        idata = 0;
                        break;
                    case 1: //Spruce
                        idata = 1;
                        break;
                    case 2: //Birch
                        idata = 2;
                        break;
                    case 3: //Jungle
                        idata = 3;
                        break;
                    case 4: //Acacia
                        idata = 4;
                        break;
                    case 5: //Dark oak
                        idata = 5;
                        break;
                    case 6: //Sand
                        id = 24;
                        idata = 2;
                        break;
                    case 7: //Red sand
                        id = 179;
                        idata = 2;
                        break;
                    case 8: //Purpur
                        id = 201;
                        idata = 0;
                        break;
                    case 9: //Quartz
                        id = 155;
                        idata = 0;
                        break;
                    default:
                        print("Unsupported format");
                        break;
                }
            } else if (id == 17) {  //Log
                switch (type) {
                    case 0: //Oak
                        idata = 0;
                        break;
                    case 1: //Spruce
                        idata = 1;
                        break;
                    case 2: //Birch
                        idata = 2;
                        break;
                    case 3: //Jungle
                        idata = 3;
                        break;
                    case 4: //Acacia
                        id = 162;
                        idata = 0;
                        break;
                    case 5: //Dark oak
                        id = 162;
                        idata = 1;
                        break;
                    case 6: //Sand
                        id = 24;
                        idata = 0;
                        break;
                    case 7: //Red sand
                        id = 179;
                        idata = 0;
                        break;
                    case 8: //Purpur
                        id = 201;
                        idata = 2;
                        break;
                    case 9: //Quartz
                        id = 155;
                        idata = 2;
                        break;
                    default:
                        print("Unsupported format");
                        break;
                }
            } else if (id == 4) {   //Cobblestone
                switch (type) {
                    case 6: //Sand
                        id = 24;
                        break;
                    case 7: //Red sand
                        id = 179;
                        break;
                    case 8: //Purpur
                        id = 201;
                        break;
                    case 9: //Quartz
                        id = 155;
                        break;
                }
            } else if (id == 64 || id == 195) { //Door
                switch (type) {
                    case 0: //Oak
                        id = 64;
                        break;
                    case 1: //Spruce
                        id = 193;
                        break;
                    case 2: //Birch
                        id = 194;
                        break;
                    case 3: //Jungle
                        id = 195;
                        break;
                    case 4: //Acacia
                        id = 196;
                        break;
                    case 5: //Dark oak
                        id = 197;
                        break;
                    case 9: //Quartz
                        id = 194;
                        break;
                    default:
                        id = 64;
                        break;
                }
            } else if (id == 85) {  //Fence
                switch (type) {
                    case 0: //Oak
                        idata = 0;
                        break;
                    case 1: //Spruce
                        idata = 1;
                        break;
                    case 2: //Birch
                        idata = 2;
                        break;
                    case 3: //Jungle
                        idata = 3;
                        break;
                    case 4: //Acacia
                        idata = 4;
                        break;
                    case 5: //Dark oak
                        idata = 5;
                        break;
                    case 9: //Quartz
                        idata = 2;
                        break;
                    default:
                        idata = 0;
                        break;
                }
            } else if (id == 198) { //Grass walk
                switch (type) {
                    case 6: //Sand
                        id = 24; idata = 0;
                        break;
                    case 7: //Red sand
                        id = 179; idata = 0;
                        break;
                    case 8: //Purpur
                        id = 121; idata = 0;
                        break;
                    case 9: //Quartz
                        id = 155; idata = 0;
                        break;
                }
            } else if (id == 2) {   //Grass block
                switch (type) {
                    case 6: //Sand
                        id = 3;
                        break;
                    case 7: //Red sand
                        id = 3;
                        break;
                    case 8: //Purpur
                        id = 206;
                        break;
                    case 9: //Quartz
                        id = 155;
                        break;
                }
            }
            Block ret = new Block(id, idata);
            return ret;
        }
        int RotateData(int id, int data, int rot) {
            //Rotate, 旋转, 方向逆时针
            int idata = data;
            if (IsStair(id)) {  //Stair
                switch (rot) {
                    case 0:
                        break;
                    case 90:
                        switch (idata) {
                            case 1: idata = 2; break;
                            case 2: idata = 0; break;
                            case 0: idata = 3; break;
                            case 3: idata = 1; break;
                            case 5: idata = 6; break;
                            case 6: idata = 4; break;
                            case 4: idata = 7; break;
                            case 7: idata = 5; break;
                        }
                        break;
                    case 180:
                        switch (idata) {
                            case 1: idata = 0; break;
                            case 2: idata = 3; break;
                            case 0: idata = 1; break;
                            case 3: idata = 2; break;
                            case 5: idata = 4; break;
                            case 4: idata = 5; break;
                            case 6: idata = 7; break;
                            case 7: idata = 6; break;
                        }
                        break;
                    case 270:
                        switch (idata) {
                            case 1: idata = 3; break;
                            case 2: idata = 1; break;
                            case 0: idata = 2; break;
                            case 3: idata = 0; break;
                            case 5: idata = 7; break;
                            case 6: idata = 5; break;
                            case 4: idata = 6; break;
                            case 7: idata = 4; break;
                        }
                        break;
                    default:
                        print("Unsupported rotate angle");
                        break;
                }
            } else if (id == 68 || id == 61 || id == 54 || id == 65 || id == 77 || id == 143) { //Wallsign, Furnace, Chest, Ladder, Stone Button, Wood Button
                switch (rot) {
                    case 0:
                        break;
                    case 90:
                        switch (idata) {
                            case 5: idata = 2; break;
                            case 2: idata = 4; break;
                            case 4: idata = 3; break;
                            case 3: idata = 5; break;
                        }
                        break;
                    case 180:
                        switch (idata) {
                            case 5: idata = 4; break;
                            case 2: idata = 3; break;
                            case 4: idata = 5; break;
                            case 3: idata = 2; break;
                        }
                        break;
                    case 270:
                        switch (idata) {
                            case 5: idata = 3; break;
                            case 2: idata = 5; break;
                            case 4: idata = 2; break;
                            case 3: idata = 4; break;
                        }
                        break;
                    default:
                        print("Unsupported rotate angle");
                        break;
                }
            } else if (id == 50) {  //Torch
                switch (rot) {
                    case 0:
                        break;
                    case 90:
                        switch (idata) {
                            case 1: idata = 4; break;
                            case 4: idata = 2; break;
                            case 2: idata = 3; break;
                            case 3: idata = 1; break;
                        }
                        break;
                    case 180:
                        switch (idata) {
                            case 1: idata = 2; break;
                            case 4: idata = 3; break;
                            case 2: idata = 1; break;
                            case 3: idata = 4; break;
                        }
                        break;
                    case 270:
                        switch (idata) {
                            case 1: idata = 3; break;
                            case 4: idata = 1; break;
                            case 2: idata = 4; break;
                            case 3: idata = 2; break;
                        }
                        break;
                }
            } else if (id == 64 || id == 71 || id == 193 || id == 194 || id == 195 || id == 196 || id == 197 || id == 199) {    //Door, Item Frame
                switch (rot) {
                    case 0:
                        break;
                    case 90:
                        switch (idata) {
                            case 2: idata = 1; break;
                            case 1: idata = 0; break;
                            case 0: idata = 3; break;
                            case 3: idata = 2; break;
                            case 7: idata = 6; break;
                            case 6: idata = 5; break;
                            case 5: idata = 4; break;
                            case 4: idata = 7; break;
                        }
                        break;
                    case 180:
                        switch (idata) {
                            case 2: idata = 0; break;
                            case 1: idata = 3; break;
                            case 0: idata = 2; break;
                            case 3: idata = 1; break;
                            case 7: idata = 5; break;
                            case 5: idata = 7; break;
                            case 6: idata = 4; break;
                            case 4: idata = 6; break;
                        }
                        break;
                    case 270:
                        switch (idata) {
                            case 2: idata = 3; break;
                            case 1: idata = 2; break;
                            case 0: idata = 1; break;
                            case 3: idata = 0; break;
                            case 4: idata = 5; break;
                            case 5: idata = 6; break;
                            case 6: idata = 7; break;
                            case 7: idata = 4; break;
                        }
                        break;
                }
            }
            return idata;
        }

        int FlipData(int id, int data, bool Xaxis) {
            //Flip, 翻转, Xasix为true则以X轴为轴翻转，false则以Y轴为轴翻转
            int idata = data;
            if (IsStair(id)) {  //Stair
                switch (Xaxis) {
                    case true:
                        switch (idata) {
                            case 0:
                            case 1:
                            case 4:
                            case 5:
                                break;  //不变
                            case 2:
                                idata = 3;
                                break;
                            case 3:
                                idata = 2;
                                break;
                            case 6:
                                idata = 7;
                                break;
                            case 7:
                                idata = 6;
                                break;
                        }
                        break;
                    case false:
                        switch (idata) {
                            case 2: case 3: case 6: case 7: break;
                            case 0:
                                idata = 1;
                                break;
                            case 1:
                                idata = 0;
                                break;
                            case 4:
                                idata = 5;
                                break;
                            case 5:
                                idata = 4;
                                break;
                        }
                        break;
                }
            } else if (id == 68 || id == 61 || id == 54 || id == 65 || id == 77 || id == 143) { //Wallsign, Furnace, Chest, Ladder, Stone Button, Wood Button
                switch (Xaxis) {
                    case true:
                        switch (idata) {
                            case 4:
                            case 5:
                                break;
                            case 2:
                                idata = 3;
                                break;
                            case 3:
                                idata = 2;
                                break;
                        }
                        break;
                    case false:
                        switch (idata) {
                            case 2:
                            case 3:
                                break;
                            case 4:
                                idata = 5;
                                break;
                            case 5:
                                idata = 4;
                                break;
                        }
                        break;
                }
            } else if (id == 50) {  //Torch
                print("Not implement warning: Torch flip");
            } else if (id == 64 || id == 71 || id == 193 || id == 194 || id == 195 || id == 196 || id == 197 || id == 199) {    //Door, Item Frame
                switch (Xaxis) {
                    case true:
                        switch (data) {
                            case 0:
                            case 2:
                            case 5:
                            case 7:
                                break;
                            case 1:
                                idata = 3;
                                break;
                            case 3:
                                idata = 1;
                                break;
                            case 4: idata = 6; break;
                            case 6: idata = 4; break;
                        }
                        break;
                    case false:
                        switch (data) {
                            case 1:
                            case 3:
                            case 4:
                            case 6:
                                break;
                            case 0:
                                idata = 2;
                                break;
                            case 2:
                                idata = 0;
                                break;
                            case 5:
                                idata = 7;
                                break;
                            case 7:
                                idata = 5;
                                break;
                        }
                        break;
                }
            }
            return idata;
        }
        void setTile(float x, float y, float z, int id, int data, int rot = 0, bool? Flip_Xaxis = null, bool doNotReplace = false) {
            if (doNotReplace) {
                Block _block;
                bool exist = block_list.TryGetValue(new Vector3(x, y, z), out _block);
                if (exist) return;
            }
            if (rot != 0) data = RotateData(id, data, rot);
            if (Flip_Xaxis != null) data = FlipData(id, data, Flip_Xaxis.Value);
            data = FlipData(id, data, true);

            if (id == 0) try {
                    block_list.Remove(new Vector3(x, y, z));
                }
                catch { }
            string blockStr = IDtoString(id);

            NCAPI.setBlock((int)x, (int)y, (int)z, blockStr, data);     //调用NC的setBlock

            try {
                block_list.Add(new Vector3(x, y, z), new Block(id, data));
            }
            catch (ArgumentException) {
                block_list.Remove(new Vector3(x, y, z));
                block_list.Add(new Vector3(x, y, z), new Block(id, data));
            }
        }
        string IDtoString(int id) {
            string tn;
            switch (id) {
                case 0:
                    tn = "air";
                    break;
                case 1:
                    tn = "stone";
                    break;
                case 2:
                    tn = "grass"; break;
                case 3:
                    tn = "dirt"; break;
                case 4:
                    tn = "cobblestone"; break;
                case 5:
                    tn = "planks";
                    break;
                case 6:
                    tn = "sapling";
                    break;
                case 7:
                    tn = "bedrock";
                    break;
                case 8:
                    tn = "flowing_water";
                    break;
                case 9:
                    tn = "water";
                    break;
                case 10:
                    tn = "flowing_lava"; break;
                case 11:
                    tn = "lava"; break;
                case 12:
                    tn = "sand";
                    break;
                case 13:
                    tn = "gravel"; break;
                case 14:
                    tn = "gold_ore"; break;
                case 15:
                    tn = "iron_ore"; break;
                case 16:
                    tn = "coal_ore"; break;
                case 17:
                    tn = "log";
                    break;
                case 18:
                    tn = "leaves";
                    break;
                case 19:
                    tn = "sponge"; break;
                case 20:
                    tn = "glass";
                    break;
                case 21:
                    tn = "lapis_ore";
                    break;
                case 22:
                    tn = "lapis_block";
                    break;
                case 23:
                    tn = "dispenser";
                    break;
                case 24:
                    tn = "sandstone";
                    break;
                case 25:
                    tn = "noteblock"; break;
                case 26:
                    tn = "bed";
                    break;
                case 27:
                    tn = "golden_rail";
                    break;
                case 28:
                    tn = "detector_rail";
                    break;
                case 29:
                    tn = "sticky_piston";
                    break;
                case 30:
                    tn = "web";
                    break;
                case 31:
                    tn = "tallgrass";
                    break;
                case 32:
                    tn = "deadbush";
                    break;
                case 33:
                    tn = "piston";
                    break;
                case 34:
                    tn = "piston";
                    break;
                case 35:
                    tn = "wool";
                    break;
                case 37:
                    tn = "yellow_flower";
                    break;
                case 38:
                    tn = "red_flower";
                    break;
                case 39:
                    tn = "brown_mushroom";
                    break;
                case 40:
                    tn = "red_mushroom";
                    break;
                case 41:
                    tn = "gold_block";
                    break;
                case 42:
                    tn = "iron_block";
                    break;
                case 43:
                    tn = "double_stone_slab";
                    break;
                case 44:
                    tn = "stone_slab";
                    break;
                case 45:
                    tn = "brick_block";
                    break;
                case 46:
                    tn = "tnt";
                    break;
                case 47:
                    tn = "bookshelf";
                    break;
                case 48:
                    tn = "mossy_cobblestone";
                    break;
                case 49:
                    tn = "obsidian";
                    break;
                case 50:
                    tn = "torch";
                    break;
                case 51:
                    tn = "fire";
                    break;
                case 52:
                    tn = "mob_spawner";
                    break;
                case 53:
                    tn = "oak_stairs";
                    break;
                case 54:
                    tn = "chest";
                    break;
                case 55:
                    tn = "redstone_wire";
                    break;
                case 56:
                    tn = "diamond_ore";
                    break;
                case 57:
                    tn = "diamond_block";
                    break;
                case 58:
                    tn = "crafting_table";
                    break;
                case 59:
                    tn = "wheat";
                    break;
                case 60:
                    tn = "farmland";
                    break;
                case 61:
                    tn = "furnace";
                    break;
                case 62:
                    tn = "lit_furnace";
                    break;
                case 63:
                    tn = "standing_sign";
                    break;
                case 64:
                    tn = "wooden_door";
                    break;
                case 65:
                    tn = "ladder";
                    break;
                case 66:
                    tn = "rail";
                    break;
                case 67:
                    tn = "stone_stairs";
                    break;
                case 68:
                    tn = "wall_sign";
                    break;
                case 69:
                    tn = "lever";
                    break;
                case 70:
                    tn = "stone_pressure_plate";
                    break;
                case 71:
                    tn = "iron_door";
                    break;
                case 72:
                    tn = "wooden_pressure_plate";
                    break;
                case 73:
                    tn = "redstone_ore";
                    break;
                case 74:
                    tn = "lit_redstone_ore";
                    break;
                case 75:
                    tn = "unlit_redstone_torch";
                    break;
                case 76:
                    tn = "redstone_torch";
                    break;
                case 77:
                    tn = "stone_button";
                    break;
                case 78:
                    tn = "snow_layer";
                    break;
                case 79:
                    tn = "ice";
                    break;
                case 80:
                    tn = "snow";
                    break;
                case 81:
                    tn = "cactus";
                    break;
                case 82:
                    tn = "clay";
                    break;
                case 83:
                    tn = "reeds";
                    break;
                case 84:
                    tn = "jukebox";
                    break;
                case 85:
                    tn = "fence";
                    break;
                case 86:
                    tn = "pumpkin";
                    break;
                case 87:
                    tn = "netherrack";
                    break;
                case 88:
                    tn = "soul_sand";
                    break;
                case 89:
                    tn = "glowstone";
                    break;
                case 90:
                    tn = "portal";
                    break;
                case 91:
                    tn = "lit_pumpkin";
                    break;
                case 92:
                    tn = "cake";
                    break;
                case 93:
                    tn = "unpowered_repeater";
                    break;
                case 94:
                    tn = "powered_repeater";
                    break;
                case 96:
                    tn = "trapdoor";
                    break;
                case 97:
                    tn = "monster_egg";
                    break;
                case 98:
                    tn = "stonebrick";
                    break;
                case 99:
                    tn = "brown_mushroom_block";
                    break;
                case 100:
                    tn = "red_mushroom_block";
                    break;
                case 101:
                    tn = "iron_bars";
                    break;
                case 102:
                    //tn = "glass_pane";
                    tn = "glass";
                    break;
                case 103:
                    tn = "melon_block";
                    break;
                case 104:
                    tn = "pumpkin_stem";
                    break;
                case 105:
                    tn = "melon_stem";
                    break;
                case 106:
                    tn = "vine";
                    break;
                case 107:
                    tn = "fence_gate";
                    break;
                case 108:
                    tn = "brick_stairs";
                    break;
                case 109:
                    tn = "stone_brick_stairs";
                    break;
                case 110:
                    tn = "mycelium";
                    break;
                case 111:
                    tn = "waterlily";
                    break;
                case 112:
                    tn = "nether_brick";
                    break;
                case 113:
                    tn = "nether_brick_fence";
                    break;
                case 114:
                    tn = "nether_brick_stairs";
                    break;
                case 115:
                    tn = "nether_wart";
                    break;
                case 116:
                    tn = "enchanting_table";
                    break;
                case 117:
                    tn = "brewing_stand";
                    break;
                case 118:
                    tn = "cauldron";
                    break;
                case 119:
                    tn = "end_portal";
                    break;
                case 120:
                    tn = "end_portal_frame";
                    break;
                case 121:
                    tn = "end_stone";
                    break;
                case 122:
                    tn = "dragon_egg";
                    break;
                case 123:
                    tn = "redstone_lamp";
                    break;
                case 124:
                    tn = "lit_redstone_lamp";
                    break;
                case 125:
                    tn = "dropper";
                    break;
                case 126:
                    tn = "activator_rail";
                    break;
                case 127:
                    tn = "cocoa";
                    break;
                case 128:
                    tn = "sandstone_stairs";
                    break;
                case 129:
                    tn = "emerald_ore";
                    break;
                case 130:
                    tn = "ender_chest";
                    break;
                case 131:
                    tn = "tripwire_hook";
                    break;
                case 132:
                    tn = "trip_wire";
                    break;
                case 133:
                    tn = "emerald_block";
                    break;
                case 134:
                    tn = "spruce_stairs";
                    break;
                case 135:
                    tn = "birch_stairs";
                    break;
                case 136:
                    tn = "jungle_stairs";
                    break;
                case 137:
                    tn = "command_block";
                    break;
                case 138:
                    tn = "beacon";
                    break;
                case 139:
                    tn = "cobblestone_wall";
                    break;
                case 140:
                    tn = "flower_pot";
                    break;
                case 141:
                    tn = "carrots";
                    break;
                case 142:
                    tn = "potatoes";
                    break;
                case 143:
                    tn = "wooden_button";
                    break;
                case 144:
                    tn = "skull";
                    break;
                case 145:
                    tn = "anvil";
                    break;
                case 146:
                    tn = "trapped_chest";
                    break;
                case 147:
                    tn = "light_weighted_pressure_plate";
                    break;
                case 148:
                    tn = "heavy_weighted_pressure_plate";
                    break;
                case 149:
                    tn = "unpowered_comparator";
                    break;
                case 150:
                    tn = "powered_comparator";
                    break;
                case 151:
                    tn = "daylight_detector";
                    break;
                case 152:
                    tn = "redstone_block";
                    break;
                case 153:
                    tn = "quartz_ore";
                    break;
                case 154:
                    tn = "hopper";
                    break;
                case 155:
                    tn = "quartz_block";
                    break;
                case 156:
                    tn = "quartz_stairs";
                    break;
                case 157:
                    tn = "double_wooden_slab";
                    break;
                case 158:
                    tn = "wooden_slab";
                    break;
                case 159:
                    tn = "stained_hardened_clay";
                    break;
                case 160:
                    //tn = "stained_glass_pane";
                    tn = "stained_glass";
                    break;
                case 161:
                    tn = "leaves2";
                    break;
                case 162:
                    tn = "log2";
                    break;
                case 163:
                    tn = "acacia_stairs";
                    break;
                case 164:
                    tn = "dark_oak_stairs";
                    break;
                case 165:
                    tn = "slime";
                    break;
                case 167:
                    tn = "iron_trapdoor";
                    break;
                case 168:
                    tn = "prismarine";
                    break;
                case 169:
                    tn = "sealantern";
                    break;
                case 170:
                    tn = "hay_block";
                    break;
                case 171:
                    tn = "carpet";
                    break;
                case 172:
                    tn = "hardened_clay";
                    break;
                case 173:
                    tn = "coal_block";
                    break;
                case 174:
                    tn = "packed_ice";
                    break;
                case 175:
                    tn = "double_plant";
                    break;
                case 176:
                    tn = "standing_banner";
                    break;
                case 177:
                    tn = "wall_banner";
                    break;
                case 178:
                    tn = "daylight_detector_inverted";
                    break;
                case 179:
                    tn = "red_sandstone";
                    break;
                case 180:
                    tn = "red_sandstone_stairs";
                    break;
                case 181:
                    tn = "double_stone_slab2";
                    break;
                case 182:
                    tn = "stone_slab2";
                    break;
                case 183:
                    tn = "spruce_fence_gate";
                    break;
                case 184:
                    tn = "birch_fence_gate";
                    break;
                case 185:
                    tn = "jungle_fence_gate";
                    break;
                case 186:
                    tn = "dark_oak_fence_gate";
                    break;
                case 187:
                    tn = "acacia_fence_gate";
                    break;
                case 188:
                    tn = "repeating_command_block";
                    break;
                case 189:
                    tn = "chain_command_block";
                    break;
                case 190:
                    //tn = "hard_glass_pane";
                    tn = "hard_glass";
                    break;
                case 191:
                    //tn = "hard_stained_glass_pane";
                    tn = "hard_stained_glass";
                    break;
                case 192:
                    tn = "chemical_heat";
                    break;
                case 193:
                    tn = "spruce_door";
                    break;
                case 194:
                    tn = "birch_door";
                    break;
                case 195:
                    tn = "jungle_door";
                    break;
                case 196:
                    tn = "acacia_door";
                    break;
                case 197:
                    tn = "dark_oak_door";
                    break;
                case 198:
                    tn = "grass_path";
                    break;
                case 199:
                    tn = "frame";
                    break;
                case 200:
                    tn = "chorus_flower";
                    break;
                case 201:
                    tn = "purpur_block";
                    break;
                case 202:
                    tn = "colored_torch_rg";
                    break;
                case 203:
                    tn = "purpur_stairs";
                    break;
                case 204:
                    tn = "colored_torch_bp";
                    break;
                case 205:
                    tn = "undyed_shulker_box";
                    break;
                case 206:
                    tn = "end_bricks";
                    break;
                case 207:
                    tn = "frosted_ice";
                    break;
                case 208:
                    tn = "end_rod";
                    break;
                case 209:
                    tn = "end_gateway";
                    break;
                case 213:
                    tn = "magma";
                    break;
                case 214:
                    tn = "nether_wart_block";
                    break;
                case 215:
                    tn = "red_nether_brick";
                    break;
                case 216:
                    tn = "bone_block";
                    break;
                case 218:
                    tn = "shulker_box";
                    break;
                case 220:
                    tn = "white_glazed_terracotta";
                    break;
                case 221:
                    tn = "orange_glazed_terracotta";
                    break;
                case 222:
                    tn = "magenta_glazed_terracotta";
                    break;
                case 223:
                    tn = "light_blue_glazed_terracotta";
                    break;
                case 224:
                    tn = "yellow_glazed_terracotta";
                    break;
                case 225:
                    tn = "lime_glazed_terracotta";
                    break;
                case 226:
                    tn = "pink_glazed_terracotta";
                    break;
                case 227:
                    tn = "gray_glazed_terracotta";
                    break;
                case 228:
                    tn = "silver_glazed_terracotta";
                    break;
                case 229:
                    tn = "cyan_glazed_terracotta";
                    break;
                case 219:
                    tn = "purple_glazed_terracotta";
                    break;
                case 231:
                    tn = "blue_glazed_terracotta";
                    break;
                case 232:
                    tn = "brown_glazed_terracotta";
                    break;
                case 233:
                    tn = "green_glazed_terracotta";
                    break;
                case 234:
                    tn = "red_glazed_terracotta";
                    break;
                case 235:
                    tn = "black_glazed_terracotta";
                    break;
                case 236:
                    tn = "concrete";
                    break;
                case 237:
                    tn = "concretepowder";
                    break;
                case 238:
                    tn = "chemistry_table";
                    break;
                case 239:
                    tn = "underwater_torch";
                    break;
                case 240:
                    tn = "chorus_plant";
                    break;
                case 241:
                    tn = "stained_glass";
                    break;
                case 243:
                    tn = "podzol";
                    break;
                case 244:
                    tn = "beetroot";
                    break;
                case 245:
                    tn = "stonecutter";
                    break;
                case 246:
                    tn = "glowingobsidian";
                    break;
                case 247:
                    tn = "netherreactor";
                    break;
                case 251:
                    tn = "observer";
                    break;
                case 252:
                    tn = "structure_block";
                    break;
                case 253:
                    tn = "hard_glass";
                    break;
                case 254:
                    tn = "hard_stained_glass";
                    break;
                case 256:
                    return null;
                case 257:
                    tn = "prismarine_stairs";
                    break;
                case 258:
                    tn = "dark_prismarine_stairs";
                    break;
                case 259:
                    tn = "prismarine_bricks_stairs";
                    break;
                case 260:
                    tn = "stripped_spruce_log";
                    break;
                case 261:
                    tn = "stripped_birch_log";
                    break;
                case 262:
                    tn = "stripped_jungle_log";
                    break;
                case 263:
                    tn = "stripped_acacia_log";
                    break;
                case 264:
                    tn = "stripped_dark_oak_log";
                    break;
                case 265:
                    tn = "stripped_oak_log";
                    break;
                case 266:
                    tn = "blue_ice";
                    break;
                case 417:
                    tn = "stone_slab3";
                    break;
                case 421:
                    tn = "stone_slab4";
                    break;
                case 463:
                    tn = "lantern";
                    break;
                default:
                    Console.WriteLine("Unknown ID! ID=" + id);
                    return null;
            }
            tn = "minecraft:" + tn;
            return tn;
        }
        void print(string str) {
            Console.WriteLine(str);
        }
    }

    static class NCAPI {
        public static extern void setBlock(int x, int y, int z, string blockName, int data);
    }

    class Program {
        static void Main(string[] args) {

        }
    }
}