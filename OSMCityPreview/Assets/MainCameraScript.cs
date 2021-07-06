#define UNITY
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Schema;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using System.ComponentModel;
using UnityEngine.VR;
using UnityEngine.XR;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

public class MainCameraScript : MonoBehaviour {
    // Unity related variables
    private bool[] move_array;
    public GameObject player;
    private bool mainCameraRotation = true;
    private List<GameObject> gameObjects = new List<GameObject>();
    private Dictionary<Vector3, GameObject> gameObject_list = new Dictionary<Vector3, GameObject>();
    private int movespeed = 32;
    private readonly float sensitivityX = 4F;
    private readonly float sensitivityY = 4F;
    private readonly float minimumY = -90;
    private readonly float maximumY = 90;
    private readonly int max_legal_level_editor = 5;        // 在Unity编辑器内最高允许楼层（为节省内存）
    private float rotationY = 0F;
    private int block_cnt = 0;
    private readonly bool use_texture = true;
    private readonly bool use_high_ppi = false;             // 启用高PPI的生成
    private int MAX_Z = 3000, MAX_X = 3000, MIN_Z = -1500, MIN_X = -1500;
    private bool reach_maxblocknum = false;
    private int distanceThreshold = 720;
    // Const values
    private const int undefined = -1073741824;
    private const int inf = 1073741824;
    private const int hidden = 256;
    private const int MAX_LEVEL = 256;
    private const int MAX_NODE = 4096;
    private const int MAX_DISTANCE = 3000;
    private const int MAX_COMPONENT_DIS = 1500;
    // Building & Road Parameters
    private readonly float ratio = 0.75f;                   // 一格多少米
    private readonly int per_width = 5;                     // 马路每车道宽
    private readonly int min_level = 3;                     // 最矮楼层数
    private readonly int max_level = 7;                     // 最高楼层数
    private readonly int max_legal_level = 36;              // 最高允许楼层
    private readonly int max_small_level = 3;               // 小建筑最高楼层
    private readonly float cos_th = 0.5f;                   // 使用在屋顶和内饰生成内的，两条边夹角的余弦值需要位于(-cos_th,cos_th)区间内才会有扩展或收缩的资格（60°~120°）
    private readonly int cmplx_building_nodenum = 30;       // 复杂建筑物顶点数
    private readonly bool DEBUG = false;                    // DEBUG模式，将显示部分debug info
    private readonly bool refalist = false;                 // Reflex add to nodelist
    private readonly bool skipKCheck = true;                // （对于简单建筑物）跳过检查斜率判断是否缩短，关闭增加闭合概率和交叉概率，开启反之亦然
    private readonly int road_flat_height = 7;              // 道路夷平高度
    private readonly int small_threshold = 50;              // 小建筑长宽判定（用于判断是否生成屋顶）
    private readonly double change_wallkind_prob = 0.3;     // 更改建筑风格的概率阈值
    /* 已弃用变量：旧版生成building的参数
    private readonly int wall_pernum = 5;
    private readonly int window_pernum = 2;
    private readonly int min_height = 7;
    private readonly int max_height = 32;
    private readonly int per_height = 6;
    */
    private double v2_prob;
    private static System.Random rd = new System.Random();

    // 墙面配置文件v1
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
        /*new [,] {
            {new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false) },
            {new Block(159, 3, false), new Block(159, 3, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159, 3, false) },
            {new Block(159, 3, false), new Block(159, 3, false), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159, 3, false) },
            {new Block(159, 3, false), new Block(159, 3, false), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159, 3, false) },
            {new Block(159, 3, false), new Block(159, 3, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159, 3, false) },
            {new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false), new Block(159, 3, false) },
            {new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true) }
        },
        new [,] {
            {new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false) },
            {new Block(159, 4, false), new Block(159, 4, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159, 4, false) },
            {new Block(159, 4, false), new Block(159, 4, false), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159, 4, false) },
            {new Block(159, 4, false), new Block(159, 4, false), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159, 4, false) },
            {new Block(159, 4, false), new Block(159, 4, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159, 4, false) },
            {new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false), new Block(159, 4, false) },
            {new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true) }
        },
        new [,] {
            {new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false) },
            {new Block(159, 5, false), new Block(159, 5, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159, 5, false) },
            {new Block(159, 5, false), new Block(159, 5, false), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159, 5, false) },
            {new Block(159, 5, false), new Block(159, 5, false), new Block(155, 1, false), new Block(20, 0, false), new Block(20, 0, false), new Block(155, 1, false), new Block(159, 5, false) },
            {new Block(159, 5, false), new Block(159, 5, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(155, 1, false), new Block(159, 5, false) },
            {new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false), new Block(159, 5, false) },
            {new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true), new Block(155, 0, true) }
        },*/
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
        /*new [,] {
            {new Block(159,0,false), new Block(155,1,false), new Block(155,1,false), new Block(159,0,false), new Block(155,1,false), new Block(155,1,false), new Block(159,0,false), new Block(155,2,false)},
            {new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(155,2,false) },
            {new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(155,2,false) },
            {new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(20,0,false), new Block(20,0,false), new Block(159,0,false), new Block(155,2,false) },
            {new Block(159,0,false), new Block(155,1,false), new Block(155,1,false), new Block(159,0,false), new Block(155,1,false), new Block(155,1,false), new Block(159,0,false), new Block(155,2,false)},
            {new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(159,0,false), new Block(155,2,false) }
        },*/
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
        /*,new[,] {
            {new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,1,false), new Block(20,0,false), new Block(20,0,false), new Block(35,1,false), new Block(35,1,false), new Block(20,0,false), new Block(20,0,false), new Block(35,1,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) },
            {new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,1,false), new Block(20,0,false), new Block(20,0,false), new Block(35,1,false), new Block(35,1,false), new Block(20,0,false), new Block(20,0,false), new Block(35,1,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,1,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) }
        },
        new[,] {
            {new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,3,false), new Block(20,0,false), new Block(20,0,false), new Block(35,3,false), new Block(35,3,false), new Block(20,0,false), new Block(20,0,false), new Block(35,3,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) },
            {new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,3,false), new Block(20,0,false), new Block(20,0,false), new Block(35,3,false), new Block(35,3,false), new Block(20,0,false), new Block(20,0,false), new Block(35,3,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,3,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) }
        },
        new[,] {
            {new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,5,false), new Block(20,0,false), new Block(20,0,false), new Block(35,5,false), new Block(35,5,false), new Block(20,0,false), new Block(20,0,false), new Block(35,5,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) },
            {new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,5,false), new Block(20,0,false), new Block(20,0,false), new Block(35,5,false), new Block(35,5,false), new Block(20,0,false), new Block(20,0,false), new Block(35,5,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,5,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) }
        },
        new[,] {
            {new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,6,false), new Block(20,0,false), new Block(20,0,false), new Block(35,6,false), new Block(35,6,false), new Block(20,0,false), new Block(20,0,false), new Block(35,6,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) },
            {new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,6,false), new Block(20,0,false), new Block(20,0,false), new Block(35,6,false), new Block(35,6,false), new Block(20,0,false), new Block(20,0,false), new Block(35,6,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,6,false), new Block(35,0,true), new Block(20,0,true), new Block(20,0,true), new Block(20,0,true), new Block(35,0,true) },
            {new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,false), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true), new Block(35,0,true) }
        }*/
    };
    // 墙面配置文件v2：请注意，阶梯默认按照X轴正方向为默认方向
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

    private static Block B = new Block(-1);
    private static Block WF = new Block(-2);
    private static Block W = new Block(-3);
    private static Block U1 = new Block(-4);
    private static Block U2 = new Block(-5);
    private static Block L = new Block(-6);
    private static Block air = new Block(0);

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

    // Variables that used in generator
    private int cnt = 0;
    private int[] Cnt = new int[MAX_LEVEL];
    private int c_rand_data = -1;
    private bool[,] cross = new bool[2, 2];
    private bool[,] cross2 = new bool[2, 2];
    private bool[,] cross3 = new bool[2, 2];
    private double lastk = 0;
    private int lastd = 0;
    private List<Node> setted = new List<Node>();
    private List<Node> roof_node_list = new List<Node>();
    private Dictionary<Vector3, Block> block_list = new Dictionary<Vector3, Block>();

    struct Node {
        public int x, y;
        public Node(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
    struct Coordinate {
        public int x, z;
        public Coordinate(int x, int z) {
            this.x = x;
            this.z = z;
        }
    }
    struct Line {
        public Node p1, p2;
        public Line(Node p1, Node p2) {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
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
    struct Quintuple {
        public int x, y, z, id, data;
        public Quintuple(int x, int y, int z, int id, int data) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.id = id;
            this.data = data;
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

    struct LineInfo {
        public bool inc;
        public double k;
        public LineInfo(bool inc, double k) {
            this.inc = inc;
            this.k = k;
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

    // Start is called before the first frame update
    void Start() {
        move_array = new bool[6];
        player = GameObject.Find("Main Camera");

        //Test();
        Test_PolygonBuilding();
        //Generate();
    }

    void Test() {
        for (int x = 0; x <= 266; x++) {
            for (int d = 0; d <= 2; d++) {
                setTile(x, 10, d, x, d);
            }
        }
        for (int z = 0; z <= 15; z++) {
            for (int d = 0; d <= 2; d++) {
                setTile(d, 10, z, z, d);
            }
        }
        for (int d = 0; d < 8; d++) {
            setTile(2 * d, 10, -10, 53, d);
        }
        setTile(20, 10, -10, 53, 0, 0);
        setTile(22, 10, -10, 53, 0, 90);
        setTile(24, 10, -10, 53, 0, 180);
        setTile(26, 10, -10, 53, 0, 180, false);
        setTile(28, 10, -10, 53, 1, 270, true);

        setTile(0, 4, -1, 44, 6);
        setTile(0, 4, -2, 44, 14);
    }

    void Test_PolygonBuilding() {
        const int ratio = 3;
        Coordinate[] coordinates = {
            new Coordinate(0,0),
            new Coordinate(10,0),
            new Coordinate(16,2),
            new Coordinate(18,6),
            new Coordinate(16,10),
            new Coordinate(10,12),
            new Coordinate(6,8),
            new Coordinate(-4,10),
            new Coordinate(0,6)
        };

        Coordinate[] parameter = new Coordinate[coordinates.Count() + 1];
        for (int i = 0; i < coordinates.Count(); i++) {
            parameter[i].x = coordinates[i].x * ratio;
            parameter[i].z = coordinates[i].z * ratio;
        }
        parameter[parameter.Count() - 1] = parameter[0];
        //Generate_OnlyPolygon(parameter);
        Generate_OnlyPolygon_v2(parameter);
    }

    void Init() {
        v2_prob = (double)WallConfig_v2.GetLength(0) / (double)(WallConfig.GetLength(0) + WallConfig_v2.GetLength(0));
    }

    // Write your generate function here!
    public void Generate() {
        // Initialize
        Init();
        // Read OSM txt file
        string[] lines = System.IO.File.ReadAllLines(getFileName());
        bool first = true;
        int lastz = 0, lastx = 0;
        double minlat = undefined, minlon = undefined;
        bool way = false, building = false, landuse = false, tree = false;
        string maintype = "";
        string subtype = "";
        List<Node> building_node_list = new List<Node>();
        int building_version = 1;
        int lanes = -1, height = -1, levels = -1, pedestrian_kind = 2;
        bool bridge = false, oneway = false, ground_bridge = false;
        int wall_kind = -1;
        bool doNotChangeStyle = false;
        List<FirstFloorInfo> firstFloorInfos = new List<FirstFloorInfo>();
        List<InteriorInfo> interiorInfos = new List<InteriorInfo>();

        foreach (string line in lines) {
            string[] cline = line.Split(' ');
            bool newkind = !IsNumberic(cline[0]);
            if (newkind) {      //新的建筑物/道路了
                /////////////////////////////////上一个建筑物/用地的多边形填充////////////////////////////
                if (building) {
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
                                        //if (IsUndefined(startnode) || IsUndefined(endnode)) continue;
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
                                        /* 旧版屋顶生成
                                         * DrawLine_Roof_Improved(startnode.x, startnode.y, endnode.x, endnode.y, height, roof.Value, building_node_list, lastnode, nextnode, Base);
                                         * DrawLine_Roof_Improved_v2(startnode.x, startnode.y, endnode.x, endnode.y, height, roof.Value, building_node_list, lastnode, nextnode, Base);
                                         * DrawLine_Roof_Lite(startnode.x, startnode.y, endnode.x, endnode.y, height, roof.Value, building_node_list, Base);
                                        */
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
                                    //DrawLine_Interior_v2(startnode.x, startnode.y, endnode.x, endnode.y, max_level, InteriorConfig[interior_kind].Value, building_node_list, lastnode, nextnode, Base);
                                    DrawLine_Interior_v3(startnode.x, startnode.y, endnode.x, endnode.y, max_level, InteriorConfig[interior_kind].Value, building_node_list, lastnode, nextnode, Base);
                                }

                                foreach (var ffi in firstFloorInfos) {
                                    if (FirstFloorConfig[ffi.firstfloor_kind].HasValue) {
                                        cnt = 0;
                                        DrawLine_FirstFloor(ffi.start.x, ffi.start.y, ffi.end.x, ffi.end.y, FirstFloorConfig[ffi.firstfloor_kind].Value, building_node_list, ffi.Base);
                                    }
                                }
                                /* v1旧版内饰生成
                                 ClearCnt();
                                 foreach(var ini in interiorInfos) {
                                    if (InteriorConfig[ini.interior_kind].HasValue) {
                                        DrawLine_Interior(ini.start.x, ini.start.y, ini.end.x, ini.end.y, ini.clevel, ini.sh, InteriorConfig[ini.interior_kind].Value, building_node_list, ini.Base);
                                    }
                                 }
                                */
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
                    doNotChangeStyle = false;
                } else if (landuse) {
                    int id = 2, data = 0;
                    switch (subtype) {
                        case "grass":
                            id = 2; data = 0;
                            break;
                        case "garden":
                        case "park":
                            //id = hidden;
                            id = 2; data = 0;
                            break;
                        case "commercial":
                        case "retail":
                            id = 1; data = 2;
                            break;
                        case "pedestrian":
                            if (pedestrian_kind == 1) {
                                id = 1; data = 2;
                                //id = 417; data = 7;
                            } else if (pedestrian_kind == 2) {
                                id = 98; data = 0;
                            }
                            break;
                        case "water":
                        case "reservoir":
                        case "fountain":
                        case "basin":
                        case "pond":
                        case "lake":
                        case "river":
                            id = 9; data = 0;
                            break;
                        case "construction":
                            id = 1; data = 0;
                            break;
                        case "recreation_ground":
                        case "playground":
                            id = 45; data = 0;
                            break;
                        case "residential":
                            id = 43; data = 0;
                            break;
                        case "wood":
                        case "forest":
                        case "tree_row":
                            break;
                        default:
                            print("WARNING: Unsupported landuse: " + subtype);
                            id = hidden;
                            break;
                    }
                    if (subtype == "wood" || subtype == "forest" || subtype == "tree_row")
                        FillPolygonPlantTree(building_node_list, 0);
                    else if (subtype == "park" || subtype == "garden")
                        FillPolygonPark(building_node_list, 0);
                    else if (subtype == "grass")
                        FillPolygonGrass(building_node_list, 0);
                    else if (subtype == "water" || subtype == "reservoir" || subtype == "fountain")
                        FillPolygonWater(building_node_list, 0);
                    else if (subtype == "river")
                        FillPolygonWater(building_node_list, -1);
                    else
                        FillPolygonLanduse(building_node_list, 0, id, data);

                    building_node_list.Clear();
                } else if (way) {
                    cross[0, 0] = false;
                    cross[0, 1] = false;
                    cross[1, 0] = false;
                    cross[1, 1] = false;
                    cross2[0, 0] = false;
                    cross2[0, 1] = false;
                    cross2[1, 0] = false;
                    cross2[1, 1] = false;
                    cross3[0, 0] = false;
                    cross3[0, 1] = false;
                    cross3[1, 0] = false;
                    cross3[1, 1] = false;
                }

                ///////////////////////////////////下一个建筑/道路/////////////////////////////////////

                lastz = undefined;
                lastx = undefined;
                way = false;
                bridge = false;
                ground_bridge = false;
                oneway = false;
                building = false;
                landuse = false;
                tree = false;
                pedestrian_kind = 2;

                maintype = cline[0];
                switch (maintype) {
                    case "way":
                        way = true;
                        subtype = cline[1];
                        lanes = Convert.ToInt32(cline[2]);
                        if (cline[3] == "true") bridge = true; 
                        else if (cline[3] == "ground") bridge = ground_bridge = true;
                        if (cline[4] == "true") oneway = true;
                        break;
                    case "building":
                        building = true;
                        subtype = cline[1];
                        height = Convert.ToInt32(cline[2]);
                        levels = (int)Convert.ToDouble(cline[3]);
                        double c_wallkind_prob = rd.NextDouble();
                        if (c_wallkind_prob < change_wallkind_prob || wall_kind == -1) {
                            double building_version_prob = rd.NextDouble();
                            if (building_version_prob >= (1 - v2_prob)) building_version = 2;
                            else building_version = 1;
                            if (building_version == 1)
                                wall_kind = rd.Next(0, WallConfig.GetLength(0));
                            else if (building_version == 2)
                                wall_kind = rd.Next(0, WallConfig_v2.GetLength(0));
                        } else {
                            doNotChangeStyle = true;
                        }
                        break;
                    case "landuse":
                        landuse = true;
                        subtype = cline[1];
                        if (subtype == "pedestrian")
                            pedestrian_kind = Convert.ToInt32(cline[2]);
                        break;
                    case "tree":
                        tree = true;
                        break;
                }
            } else {        //仍旧是上一个建筑物/道路
                double clat = Convert.ToDouble(cline[0]);
                double clon = Convert.ToDouble(cline[1]);
                if (IsUndefined(new Node((int)clat, (int)clon))) {
                    lastx = undefined;
                    lastz = undefined;
                    building_node_list.Add(new Node(undefined, undefined));
                    ClearCnt();
                    continue;
                }

                if (first) {
                    minlat = clat;
                    minlon = clon;
                    first = false;
                } else {
                    //转换为相对经纬度
                    double dlat = clat - minlat;
                    double dlon = clon - minlon;
                    //转换为相对坐标
                    int currentz = (int)(Math.Floor(dlat * 111195) / ratio);
                    int currentx = (int)(Math.Floor(dlon * Math.Cos(minlat * Math.PI / 180) * 111195) / ratio);
                    if (currentz > MAX_Z || currentz < MIN_Z || currentx > MAX_X || currentx < MIN_X) continue;

                    if (building || landuse) {
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
                    //种树
                    if (tree) {
                        setTile(currentx, 0, currentz, 2, 0);
                        setTile(currentx, 1, currentz, 6, 0);
                        continue;
                    }
                    //连线
                    if (!IsUndefined(new Node(lastx, lastz))) {
                        if (way) {
                            int h = 0, road_half_width = per_width, lamp_step = 33, pedestrian_width = 0;
                            Block black = new Block(159, 9, false);
                            Block white = new Block(236, 0, false);
                            Block yellow = new Block(159, 4, false);
                            if (bridge && !ground_bridge) h = 20;
                            //路宽
                            if (lanes == -1) {
                                switch (subtype) {
                                    case "motorway":
                                        road_half_width = 4 * per_width;
                                        pedestrian_width = 12;
                                        break;
                                    case "trunk":
                                        road_half_width = 3 * per_width;
                                        pedestrian_width = 12;
                                        break;
                                    case "primary":
                                        road_half_width = 3 * per_width;
                                        pedestrian_width = 10;
                                        break;
                                    case "secondary":
                                        road_half_width = 2 * per_width;
                                        pedestrian_width = 8;
                                        break;
                                    case "tertiary":
                                        road_half_width = 2 * per_width;
                                        pedestrian_width = 6;
                                        break;
                                    case "unclassified":
                                        road_half_width = 1 * per_width;
                                        pedestrian_width = 4;
                                        lamp_step = 40;
                                        break;
                                    case "residential":
                                        road_half_width = 1 * per_width;
                                        pedestrian_width = 3;
                                        lamp_step = 40;
                                        break;
                                    case "pedestrian":
                                        road_half_width = 7;
                                        lamp_step = 50;
                                        break;
                                    case "living_street":
                                        road_half_width = 5;
                                        lamp_step = 45;
                                        break;
                                    case "footway":
                                        road_half_width = 3;
                                        lamp_step = 0;
                                        break;
                                    case "river":
                                        road_half_width = 70;
                                        lamp_step = 0;
                                        break;
                                    case "riverbank":
                                        road_half_width = 15;
                                        lamp_step = 0;
                                        break;
                                    case "motorway_link":
                                    case "trunk_link":
                                    case "primary_link":
                                    case "secondary_link":
                                    case "tertiary_link":
                                        road_half_width = 1 * per_width;
                                        pedestrian_width = 0;
                                        break;
                                    default:
                                        print("WARNING: Unsupported way kind: " + subtype);
                                        continue;
                                }
                                if (oneway && road_half_width >= 2 * per_width) road_half_width = (int)Math.Ceiling((double)road_half_width / 2);
                            } else {
                                if (lanes == 1)
                                    road_half_width = (int)Math.Ceiling(1.5 * per_width);
                                else
                                    road_half_width = lanes * per_width;
                                road_half_width = (int)Math.Round((double)road_half_width / 2);
                                pedestrian_width = lanes * 3;
                                if (lanes < 2) lamp_step = 45;
                            }
                            //道路材质并定制化
                            switch (subtype) {
                                case "footway":
                                case "path":
                                    black = new Block(hidden, 0, false);
                                    white = new Block(hidden, 0, false);
                                    yellow = new Block(hidden, 0, false);
                                    continue;
                                case "pedestrian":
                                    black = new Block(98, 0, false);
                                    DrawLine_Road_Pedestrian(lastx, lastz, currentx, currentz, h, black, road_half_width, bridge, lamp_step);
                                    break;
                                case "river":
                                    black = new Block(9, 0);
                                    DrawLine_Road_Waterway(lastx, lastz, currentx, currentz, h - 1, black, road_half_width, 1);
                                    break;
                                case "riverbank":
                                    black = new Block(12, 0);
                                    DrawLine_Road_Waterway(lastx, lastz, currentx, currentz, h, black, road_half_width);
                                    break;
                                default:
                                    //DrawLine_Road_Lite(currentx, currentz, lastx, lastz, h, black, white, yellow, road_half_width, oneway);
                                    //DrawLine_Road_Lite_v2(lastx, lastz, currentx, currentz, h, black, white, yellow, road_half_width, oneway);
                                    DrawLine_Road_Lite_v3(lastx, lastz, currentx, currentz, h, black, white, yellow, road_half_width, oneway, bridge, lamp_step, pedestrian_width);
                                    break;
                            }
                        } else if (building) {
                            /* 旧版building生成代码：
                            if (height == -1) {
                                if (levels == -1) height = rd.Next(min_height, max_height);
                                else height = levels * per_height;
                            }
                            for (int h = 0; h < height; h++) {
                                if (h % (wall_pernum + window_pernum) >= wall_pernum)
                                    DrawLine_Building(currentx, currentz, lastx, lastz, h, 155, 0, 4, 2);
                                else
                                    DrawLine_Building(currentx, currentz, lastx, lastz, h, 155, 0, 4, 0);
                            }
                            */
                            //确定楼层数
                            if (levels == -1 && height == -1)
                                levels = rd.Next(min_level, max_level);
                            else if (height != -1) {
                                if (building_version == 1)
                                    levels = (int)Math.Ceiling((double)height / (double)WallConfig[wall_kind].GetLength(0));
                                else if (building_version == 2)
                                    levels = (int)Math.Ceiling((double)height / (double)WallConfig_v2[wall_kind].GetLength(0));
                            }
                            if (IsSmallBuilding(building_node_list))
                                levels = Math.Min(levels, max_small_level);
                            if (IsMiniBuilding(building_node_list))
                                levels = 1;
#if UNITY_EDITOR
                            levels = Math.Min(levels, max_legal_level_editor);
#else
                            levels = Math.Min(levels, max_legal_level);
#endif
                            if (building_version == 1)
                                height = levels * WallConfig[wall_kind].GetLength(0);
                            else if (building_version == 2)
                                height = levels * WallConfig_v2[wall_kind].GetLength(0);
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
                                    //startlevel = (int)Math.Ceiling((double)FirstFloorConfig[firstfloor_kind].Value.GetHeight() / (double)singleheight);
                                    //int target_height = startlevel * singleheight;
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
                                    //DrawLine_Building_Advanced(currentx, currentz, lastx, lastz, level, WallConfig[wall_kind]);
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
                        } else if (landuse) {
                            switch (subtype) {
                                case "tree_row":
                                    DrawLine(currentx, currentz, lastx, lastz, 0, 2, 0);
                                    DrawLine(currentx, currentz, lastx, lastz, 1, 6, 0);
                                    break;
                            }
                            if (DEBUG) {
                                switch (subtype) {
                                    case "grass":
                                    case "park":
                                    case "garden":
                                        DrawLine(currentx, currentz, lastx, lastz, 0, 2, 0);
                                        break;
                                    case "commercial":
                                    case "retail":
                                        DrawLine(currentx, currentz, lastx, lastz, 0, 1, 2);
                                        break;
                                    case "pedestrian":
                                        if (pedestrian_kind == 1)
                                            DrawLine(currentx, currentz, lastx, lastz, 0, 1, 2);
                                        else if (pedestrian_kind == 2)
                                            DrawLine(currentx, currentz, lastx, lastz, 0, 98, 0);
                                        break;
                                    case "water":
                                    case "reservoir":
                                        DrawLine(currentx, currentz, lastx, lastz, 0, 9, 0);
                                        break;
                                    case "construction":
                                        DrawLine(currentx, currentz, lastx, lastz, 0, 1, 0);
                                        break;
                                    case "recreation_ground":
                                    case "playground":
                                        DrawLine(currentx, currentz, lastx, lastz, 0, 45, 0);
                                        break;
                                    case "residential":
                                        DrawLine(currentx, currentz, lastx, lastz, 0, 43, 0);
                                        break;
                                }
                            }
                        }
                    }
                    lastz = currentz;
                    lastx = currentx;
                }
            }
        }

        // 在末尾的孤儿（非实时更新）
        if (building) {
            int id = 0, data = 0;
            if (building_version == 1) {
                id = BaseBlock[wall_kind].id;
                data = BaseBlock[wall_kind].data;
                if (c_rand_data != -1) data = c_rand_data;
            } else if (building_version == 2) {
                id = BaseBlock_v2[wall_kind].id;
                data = BaseBlock_v2[wall_kind].data;
                if (c_rand_data != -1) data = c_rand_data;
            }
            FillPolygonScanline(building_node_list, height - 1, id, data);
            building_node_list.Clear();
            cnt = 0;
            c_rand_data = -1;
            print("INFO: 在末尾的孤儿是一栋建筑物");
        } else if (landuse) {
            int id = 0, data = 0;
            switch (subtype) {
                case "grass":
                    id = 2; data = 0;
                    break;
                case "garden":
                case "park":
                    id = hidden;
                    break;
                case "commercial":
                    id = 1; data = 2;
                    break;
                case "pedestrian":
                    if (pedestrian_kind == 1) {
                        id = 1; data = 2;
                    } else if (pedestrian_kind == 2) {
                        id = 98; data = 0;
                    }
                    break;
                case "water":
                case "reservoir":
                case "fountain":
                    id = 9; data = 0;
                    break;
                case "construction":
                    id = 1; data = 0;
                    break;
                case "recreation_ground":
                case "playground":
                    id = 45; data = 0;
                    break;
                case "residential":
                    id = 43; data = 0;
                    break;
            }
            if (subtype == "wood" || subtype == "forest" || subtype == "tree_row")
                FillPolygonPlantTree(building_node_list, 0);
            else
                FillPolygonScanline(building_node_list, 0, id, data);

            building_node_list.Clear();
            print("INFO: 在末尾的孤儿是特定用途用地");
        }
#if UNITY
        StaticBatchingUtility.Combine(gameObjects.ToArray(), GameObject.Find("Cube"));
#endif
    }
    
    void Generate_OnlyPolygon(Coordinate[] coordinates) {
        // 仅使用多边形坐标生成建筑物
        // 移植到ModPE或Addon大概会更方便；同时作为debug用建筑物算法的测试
        // 非实时更新，当前版本日期为2020.8.22
        Init();
        int lastz = 0, lastx = 0;
        const bool building = true;
        List<Node> building_node_list = new List<Node>();
        int building_version = 1;
        int height = -1, levels = -1;
        int wall_kind = -1;
        List<FirstFloorInfo> firstFloorInfos = new List<FirstFloorInfo>();
        List<InteriorInfo> interiorInfos = new List<InteriorInfo>();

        double prob = rd.NextDouble();
        if (prob >= (1 - v2_prob)) building_version = 2;
        else building_version = 1;
        if (building_version == 1)
            wall_kind = rd.Next(0, WallConfig.GetLength(0));
        else if (building_version == 2)
            wall_kind = rd.Next(0, WallConfig_v2.GetLength(0));

        foreach (Coordinate coordinate in coordinates) {
            int cx = coordinate.x;
            int cz = coordinate.z;
            if (IsUndefined(new Node(cx, cz))) {
                lastx = undefined;
                lastz = undefined;
                building_node_list.Add(new Node(undefined, undefined));
                ClearCnt();
                continue;
            }

            if (building) {
                bool valid = true;
                if (building_node_list.Count > 0 && building_node_list[building_node_list.Count - 1].Equals(new Node(cx, cz))) valid = false;
                for (int i = building_node_list.Count - 1; i >= 0; i--) {
                    if (i == 0 || IsUndefined(building_node_list[i])) {
                        if (building_node_list[i].Equals(new Node(cx, cz)))
                            valid = false;
                        break;
                    }
                }
                if (valid) building_node_list.Add(new Node(cx, cz));
            }

            //连线
            if (lastx != undefined) {
                if (building) {
                    //确定楼层数
                    if (levels == -1 && height == -1)
                        levels = rd.Next(min_level, max_level);
                    else if (height != -1) {
                        if (building_version == 1)
                            levels = (int)Math.Ceiling((double)height / (double)WallConfig[wall_kind].GetLength(0));
                        else if (building_version == 2)
                            levels = (int)Math.Ceiling((double)height / (double)WallConfig_v2[wall_kind].GetLength(0));
                    }
                    levels = Math.Min(levels, max_legal_level);
                    if (building_version == 1)
                        height = levels * WallConfig[wall_kind].GetLength(0);
                    else if (building_version == 2)
                        height = levels * WallConfig_v2[wall_kind].GetLength(0);
                    //一楼选定
                    int startheight = 0;
                    int firstfloor_kind = rd.Next(0, FirstFloorConfig.GetLength(0));
                    if (FirstFloorConfig[firstfloor_kind].HasValue) {
                        if (Math.Max(Math.Abs(cx - lastx), Math.Abs(cz - lastz)) < FirstFloorConfig[firstfloor_kind].Value.GetLength())
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
                            firstFloorInfos.Add(new FirstFloorInfo(new Node(lastx, lastz), new Node(cx, cz), firstfloor_kind, Base));
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
                            interiorInfos.Add(new InteriorInfo(new Node(lastx, lastz), new Node(cx, cz), interior_kind, level, shI, Base));
                            //墙面建造
                            int shW = Math.Max(startheight, level * WallConfig[wall_kind].GetLength(0));
                            DrawLine_Building_Advanced(lastx, lastz, cx, cz, level, shW, WallConfig[wall_kind]);
                        } else if (building_version == 2) {
                            int shI = level * WallConfig_v2[wall_kind].GetLength(0);
                            Block Base = BaseBlock_v2[wall_kind];
                            if (Base.random == 1) {
                                if (c_rand_data == -1) c_rand_data = rd.Next(Base.rand_min, Base.rand_max);
                                Base.data = c_rand_data;
                            }
                            interiorInfos.Add(new InteriorInfo(new Node(lastx, lastz), new Node(cx, cz), interior_kind, level, shI, Base));
                            int shW = Math.Max(startheight, level * WallConfig_v2[wall_kind].GetLength(0));
                            DrawLine_Building_Advanced_v2(lastx, lastz, cx, cz, level, shW, WallConfig_v2[wall_kind]);
                        }
                    }
                }
            }
            lastz = cz;
            lastx = cx;
        }

        /////////////////////////////////上一个建筑物/用地的多边形填充////////////////////////////
        if (building) {
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
                //建筑物屋顶放置
                cnt = 0;
                int roof_kind = rd.Next(0, RoofConfig.GetLength(0));
                Roof? roof = RoofConfig[roof_kind];
                if (!(roof is null)) {
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
                        //if (IsUndefined(startnode) || IsUndefined(endnode)) continue;
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
            building_node_list.Clear();
            roof_node_list.Clear();
            firstFloorInfos.Clear();
            interiorInfos.Clear();
            ClearCnt();
            cnt = 0;
            c_rand_data = -1;
        }
    }

    void Generate_OnlyPolygon_v2(Coordinate[] coordinates) {
        // 仅使用多边形坐标生成建筑物
        // 移植到ModPE或Addon大概会更方便
        // 当前版本日期为2021.3.2（可能是最终版）
        // Initialize
        Init();
        int lastz = 0, lastx = 0;
        const bool building = true;
        const bool dynamic_add_nodes = true;
        List<Node> building_node_list = new List<Node>();
        List<Node> const_building_node_list = new List<Node>();
        int building_version = 1;
        int height = -1, levels = -1;
        int wall_kind = -1;
        bool doNotChangeStyle = false;
        List<FirstFloorInfo> firstFloorInfos = new List<FirstFloorInfo>();
        List<InteriorInfo> interiorInfos = new List<InteriorInfo>();

        if (!dynamic_add_nodes)
			foreach (Coordinate coordinate in coordinates)
				building_node_list.Add(new Node(coordinate.x, coordinate.z));
        else
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
#if UNITY_EDITOR
        levels = Math.Min(levels, max_legal_level_editor);
#else
        levels = Math.Min(levels, max_legal_level);
#endif
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
        if (building) {
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
                                //if (IsUndefined(startnode) || IsUndefined(endnode)) continue;
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
                                /* 旧版屋顶生成
                                 * DrawLine_Roof_Improved(startnode.x, startnode.y, endnode.x, endnode.y, height, roof.Value, building_node_list, lastnode, nextnode, Base);
                                 * DrawLine_Roof_Improved_v2(startnode.x, startnode.y, endnode.x, endnode.y, height, roof.Value, building_node_list, lastnode, nextnode, Base);
                                 * DrawLine_Roof_Lite(startnode.x, startnode.y, endnode.x, endnode.y, height, roof.Value, building_node_list, Base);
                                */
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
                            //DrawLine_Interior_v2(startnode.x, startnode.y, endnode.x, endnode.y, max_level, InteriorConfig[interior_kind].Value, building_node_list, lastnode, nextnode, Base);
                            DrawLine_Interior_v3(startnode.x, startnode.y, endnode.x, endnode.y, max_level, InteriorConfig[interior_kind].Value, building_node_list, lastnode, nextnode, Base);
                        }

                        foreach (var ffi in firstFloorInfos) {
                            if (FirstFloorConfig[ffi.firstfloor_kind].HasValue) {
                                cnt = 0;
                                DrawLine_FirstFloor(ffi.start.x, ffi.start.y, ffi.end.x, ffi.end.y, FirstFloorConfig[ffi.firstfloor_kind].Value, building_node_list, ffi.Base);
                            }
                        }
                        /* v1旧版内饰生成
                         ClearCnt();
                         foreach(var ini in interiorInfos) {
                            if (InteriorConfig[ini.interior_kind].HasValue) {
                                DrawLine_Interior(ini.start.x, ini.start.y, ini.end.x, ini.end.y, ini.clevel, ini.sh, InteriorConfig[ini.interior_kind].Value, building_node_list, ini.Base);
                            }
                         }
                        */
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
            doNotChangeStyle = false;
        }
    }

    void FillPolygon(List<Node> nodes, int h, int id, int data) {
        int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = 0;
        foreach (var node in nodes) {
            minx = Math.Min(node.x, minx);
            miny = Math.Min(node.y, miny);
            maxx = Math.Max(node.x, maxx);
            maxy = Math.Max(node.y, maxy);
            n++;
        }
        Node lastnode = nodes[n - 1];
        List<Line> lines = new List<Line>();
        foreach (var node in nodes) {
            lines.Add(new Line(lastnode, node));
            lastnode = node;
        }
        for (int x = minx; x <= maxx; x++) {
            List<int> ylist = new List<int>();
            List<KeyValuePair<int, Line>> DuanDian = new List<KeyValuePair<int, Line>>();
            foreach (var line in lines) {
                if (line.p1.x == line.p2.x) continue;
                double k = (double)(line.p1.y - line.p2.y) / (double)(line.p1.x - line.p2.x);
                double b = (double)(line.p2.y * line.p1.x - line.p1.y * line.p2.x) / (double)(line.p1.x - line.p2.x);
                int y = (int)(k * x + b);
                if (Math.Min(line.p1.y, line.p2.y) < y && y < Math.Max(line.p1.y, line.p2.y)) {
                    ylist.Add(y);
                } else if (y == Math.Min(line.p1.y, line.p2.y) || y == Math.Max(line.p1.y, line.p2.y)) {
                    DuanDian.Add(new KeyValuePair<int, Line>(y, line));
                }
            }
            for (int i = 0; i < DuanDian.ToArray().Length; i++) {
                int line_minx = Math.Min(DuanDian[i].Value.p1.x, DuanDian[i].Value.p2.x);
                int line_maxx = Math.Max(DuanDian[i].Value.p1.x, DuanDian[i].Value.p2.x);
                int cy = DuanDian[i].Key;
                int debugvar = 0;
                for (int j = i + 1; j < DuanDian.ToArray().Length; j++) {
                    if (DuanDian[j].Key == cy) {
                        line_minx = Math.Min(line_minx, Math.Min(DuanDian[j].Value.p1.x, DuanDian[j].Value.p2.x));
                        line_maxx = Math.Max(line_maxx, Math.Max(DuanDian[j].Value.p1.x, DuanDian[j].Value.p2.x));
                        debugvar++;
                    }
                }
                if (debugvar > 1) {
                    setTile(0, -1, -1, 1, 0);
                }
                if (line_minx < x && x < line_maxx) {    //符合条件需要变号
                    ylist.Add(cy);
                } else if (line_minx == x) {
                    ylist.Add(cy);
                    ylist.Add(cy);
                }
            }
            /*
            bool NeedSet = false;
            for(int y = miny; y <= maxy; y++) {
                if (NeedSet) setTile(x, h, y, id, data);
                int cnt = 0;
                foreach(var py in ylist)
                    if (py == y) cnt++;
                if (cnt % 2 == 1) NeedSet = !NeedSet;
            }*/

            ylist.Sort();
            for (int i = 0; i < ylist.Count - 1; i += 2) {
                int y1 = ylist[i];
                int y2 = ylist[i + 1];
                DrawLine(x, y1, x, y2, h, id, data);
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

    void FillPolygonLanduse(List<Node> nodes, int h, int id, int data) {
        int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
        foreach (var node in nodes) {
            if (IsUndefined(node)) continue;
            minx = Math.Min(node.x, minx);
            miny = Math.Min(node.y, miny);
            maxx = Math.Max(node.x, maxx);
            maxy = Math.Max(node.y, maxy);
        }
        if (maxx - minx >= MAX_COMPONENT_DIS && maxy - miny >= MAX_COMPONENT_DIS) {
            print("WARNING: To large landuse, skipping");
            return;
        }
        for (int x = minx; x <= maxx; x++) {
            for (int y = miny; y <= maxy; y++) {
                if (pnpoly2(nodes, new Node(x, y))) {
                    Block bottom = getTile(x, h, y);
                    if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                        Block up = getTile(x, h + 1, y);
                        if (bottom.Equals(new Block(2, 0)) && up.Equals(new Block(6, 0))) continue;
                        setTile(x, h, y, id, data);
                        if (up.id == 6 || up.id == 38)
                            setTile(x, h + 1, y, 0, 0);
                    }
                }
            }
        }
    }

    void FillPolygonPlantTree(List<Node> nodes, int h) {
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
                if (pnpoly2(nodes, new Node(x, y))) {
                    Block cbase = getTile(x, h, y);
                    if (cbase.Equals(air) || cbase.Equals(new Block(2, 0))) {
                        Block cup = getTile(x, h + 1, y);
                        if (cup.Equals(air)) {
                            if (rd.NextDouble() >= 0.991) {
                                setTile(x, h + 1, y, 6, 0);
                            }
                            if (rd.NextDouble() >= 0.991) {
                                int data = rd.Next(0, 10);
                                setTile(x, h + 1, y, 38, data);
                            }
                        }
                    }
                }
            }
        }
    }

    void FillPolygonPark(List<Node> nodes, int h) {
        int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
        foreach (var node in nodes) {
            if (IsUndefined(node)) continue;
            minx = Math.Min(node.x, minx);
            miny = Math.Min(node.y, miny);
            maxx = Math.Max(node.x, maxx);
            maxy = Math.Max(node.y, maxy);
        }
        for (int x = minx; x <= maxx; x++) {
            for (int y = miny; y <= maxy; y++) {
                if (pnpoly2(nodes, new Node(x, y))) {
                    Block cbase = getTile(x, h, y);
                    Block cbottom = getTile(x, h - 1, y);
                    if (cbase.Equals(air) && cbottom.id != 8 && cbottom.id != 9) {
                        setTile(x, h, y, 2, 0);
                        Block cup = getTile(x, h + 1, y);
                        if (cup.Equals(air)) {
                            if (rd.NextDouble() >= 0.992) {
                                setTile(x, h + 1, y, 6, 0);
                            }
                            if (rd.NextDouble() >= 0.99) {
                                int data = rd.Next(0, 10);
                                setTile(x, h + 1, y, 38, data);
                            }
                        }
                    }
                }
            }
        }
    }

    void FillPolygonGrass(List<Node> nodes, int h) {
        int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
        foreach (var node in nodes) {
            if (IsUndefined(node)) continue;
            minx = Math.Min(node.x, minx);
            miny = Math.Min(node.y, miny);
            maxx = Math.Max(node.x, maxx);
            maxy = Math.Max(node.y, maxy);
        }
        for (int x = minx; x <= maxx; x++) {
            for (int y = miny; y <= maxy; y++) {
                if (pnpoly2(nodes, new Node(x, y))) {
                    Block cbase = getTile(x, h, y);
                    if (cbase.Equals(air)) {
                        setTile(x, h, y, 2, 0);
                        Block cup = getTile(x, h + 1, y);
                        if (cup.Equals(air)) {
                            if (rd.NextDouble() >= 0.991) {
                                int data = rd.Next(0, 4);
                                setTile(x, h + 1, y, 31, data);
                            }
                            if (rd.NextDouble() >= 0.991) {
                                int data = rd.Next(0, 5);
                                setTile(x, h + 1, y, 175, data);
                                setTile(x, h + 1, y, 175, data + 8);
                            }
                        }
                    }
                }
            }
        }
    }

    void FillPolygonWater(List<Node> nodes, int h) {
        int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = nodes.Count;
        foreach (var node in nodes) {
            if (IsUndefined(node)) continue;
            minx = Math.Min(node.x, minx);
            miny = Math.Min(node.y, miny);
            maxx = Math.Max(node.x, maxx);
            maxy = Math.Max(node.y, maxy);
        }
        for (int x = minx; x <= maxx; x++) {
            for (int y = miny; y <= maxy; y++) {
                if (pnpoly2(nodes, new Node(x, y))) {
                    Block cbase = getTile(x, h, y);
                    if (!IsRoadBlock(cbase)) {
                        Block cup = getTile(x, h + 1, y);
                        if (!IsRoadBlock(cup))
                            setTile(x, h + 1, y, 0, 0);
                        setTile(x, h, y, 9, 0);
                    }
                }
            }
        }
    }

    void FillPolygonDfs(List<Node> nodes, int h, int id, int data) {
        setted.Clear();
        int minx = inf, miny = inf, maxx = -inf, maxy = -inf, n = 0;
        foreach (var node in nodes) {
            minx = Math.Min(node.x, minx);
            miny = Math.Min(node.y, miny);
            maxx = Math.Max(node.x, maxx);
            maxy = Math.Max(node.y, maxy);
            n++;
        }
        Node lastnode = nodes[n - 1];
        List<Line> lines = new List<Line>();
        foreach (var node in nodes) {
            lines.Add(new Line(lastnode, node));
            lastnode = node;
        }
        int sx = undefined, sy = undefined;
        for (int x = minx; x <= maxx; x += 3) {
            for (int y = miny; y <= maxy; y += 3) {
                if (pnpoly(nodes, new Node(x, y))) {
                    double p = rd.NextDouble();
                    sx = x;
                    sy = y;
                    break;
                }
            }
            if (sx != undefined) break;
        }
        if (sx != undefined) {
            //fill_dfs(sx, h, sy, id, data, lines);
            fill_dfs2(sx, h, sy, id, data, nodes);
        }
    }

    void fill_dfs(int x, int h, int y, int id, int data, List<Line> lines) {
        if (setted.Contains(new Node(x, y))) return;
        foreach (var line in lines) {
            if (line.p1.x == line.p2.x) {
                if (x == line.p1.x) return;
            } else {
                double k = (double)(line.p1.y - line.p2.y) / (double)(line.p1.x - line.p2.x);
                double b = (double)(line.p2.y * line.p1.x - line.p1.y * line.p2.x) / (double)(line.p1.x - line.p2.x);
                if (-1 <= k && k <= 1) {
                    int ly = (int)(k * (double)x + b);
                    if (ly == y && Math.Min(line.p1.y, line.p2.y) <= y && y <= Math.Max(line.p1.y, line.p2.y))
                        return;
                } else {
                    int lx = (int)(((double)y - b) / k);
                    if (lx == x && Math.Min(line.p1.x, line.p2.x) <= x && x <= Math.Max(line.p1.x, line.p2.x))
                        return;
                }
            }
        }
        setTile(x, h, y, id, data);
        setted.Add(new Node(x, y));
        fill_dfs(x - 1, h, y, id, data, lines);
        fill_dfs(x + 1, h, y, id, data, lines);
        fill_dfs(x, h, y - 1, id, data, lines);
        fill_dfs(x, h, y + 1, id, data, lines);
    }

    void fill_dfs2(int x, int h, int y, int id, int data, List<Node> nodes) {
        if (setted.Contains(new Node(x, y))) return;
        if (!pnpoly(nodes, new Node(x, y))) return;
        setTile(x, h, y, id, data);
        setted.Add(new Node(x, y));
        fill_dfs2(x - 1, h, y, id, data, nodes);
        fill_dfs2(x + 1, h, y, id, data, nodes);
        fill_dfs2(x, h, y - 1, id, data, nodes);
        fill_dfs2(x, h, y + 1, id, data, nodes);
    }

    // Pnpoly: 基础版的pnpoly算法，应用于普通多边形，不支持套娃曲线
    bool pnpoly(List<Node> nodes, Node test) {
        bool c = false;
        int n = 0;
        foreach (var node in nodes) {
            n++;
        }
        for (int i = 0; i < n; i++) {
            int j = (i == 0) ? (n - 1) : (i - 1);
            if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                c = !c;
            }
        }
        return c;
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

    struct AET {
        public double x;
        public double dx, ymax;
        public unsafe AET* next;
    }
    struct NET {
        public double x;
        public double dx, ymax;
        public unsafe NET* next;
    }
    /* 活性边表法填充多边形：该方法与函数已弃用 */
    unsafe void FillPolygonAET(List<Node> nodes, int h, int id, int data) {
        int MinX = inf, MinY = inf, MaxX = -inf, MaxY = -inf, vertNum = 0, i;
        foreach (var node in nodes) {
            MinX = Math.Min(node.x, MinX);
            MinY = Math.Min(node.y, MinY);
            MaxX = Math.Max(node.x, MaxX);
            MaxY = Math.Max(node.y, MaxY);
            vertNum++;
        }
        /*初始化AET表，这是一个有头结点的链表*/
        unsafe {
            /*定义结构体用于活性边表AET和新边表NET*/
            AET pAET = new AET {
                next = null
            };
            /*初始化NET表，这也是一个有头结点的链表，头结点的dx，x，ymax都初始化为0*/
            NET[] pNET = new NET[1024];
            for (i = 0; i <= MaxY; i++) {
                pNET[i] = new NET();
                pNET[i].dx = 0;
                pNET[i].x = 0;
                pNET[i].ymax = 0;
                pNET[i].next = null;
            }

            /*扫描并建立NET表*/
            for (i = MinY; i <= MaxY; i++) {
                /*i表示扫描线，扫描线从多边形的最底端开始，向上扫描*/
                for (int j = 0; j < vertNum; j++) {
                    /*如果多边形的该顶点与扫描线相交，判断该点为顶点的两条直线是否在扫描线上方
                     *如果在上方，就记录在边表中，并且是头插法记录，结点并没有按照x值进行排序，毕竟在更新AET的时候还要重新排一次
                     *所以NET表可以暂时不排序
                    */
                    if (nodes[j].y == i) {
                        /*笔画前面的那个点*/
                        if (nodes[(j - 1 + vertNum) % vertNum].y > nodes[j].y) {
                            NET p = new NET();
                            p.x = nodes[j].x;
                            p.ymax = nodes[(j - 1 + vertNum) % vertNum].y;
                            p.dx = (double)(nodes[(j - 1 + vertNum) % vertNum].x - nodes[j].x) / (double)(nodes[(j - 1 + vertNum) % vertNum].y - nodes[j].y);
                            p.next = pNET[i].next;
                            pNET[i].next = &p;
                        }
                        /*笔画后面的那个点*/
                        if (nodes[(j + 1 + vertNum) % vertNum].y > nodes[j].y) {
                            NET p = new NET();
                            p.x = nodes[j].x;
                            p.ymax = nodes[(j + 1 + vertNum) % vertNum].y;
                            p.dx = (double)(nodes[(j + 1 + vertNum) % vertNum].x - nodes[j].x) / (double)(nodes[(j + 1 + vertNum) % vertNum].y - nodes[j].y);
                            p.next = pNET[i].next;
                            pNET[i].next = &p;
                        }
                    }
                }
            }


            /*建立并更新活性边表AET*/
            for (i = MinY; i <= MaxY; i++) {
                /*更新活性边表AET，计算扫描线与边的新的交点x，此时y值没有达到临界值的话*/
                NET* p = (NET*)pAET.next;
                while (p != null) {
                    p->x = p->x + p->dx;
                    p = p->next;
                }

                /*更新完以后，对活性边表AET按照x值从小到大排序*/
                AET* tq = &pAET;
                p = (NET*)pAET.next;
                tq->next = null;
                while (p != null) {
                    while (tq->next != null && p->x >= tq->next->x)
                        tq = tq->next;
                    NET* s = p->next;
                    p->next = (NET*)tq->next;
                    tq->next = (AET*)p;
                    p = s;
                    tq = &pAET;
                }

                /*从AET表中删除ymax==i的结点*/
                AET* q = &pAET;
                p = (NET*)q->next;
                while (p != null) {
                    if (p->ymax == i) {
                        q->next = (AET*)p->next;
                        //delete p;
                        p = (NET*)q->next;
                    } else {
                        q = q->next;
                        p = (NET*)q->next;
                    }
                }
                /*将NET中的新点加入AET，并用插入法按X值递增排序*/
                p = pNET[i].next;
                q = &pAET;
                while (p != null) {
                    while (q->next != null && p->x >= q->next->x)
                        q = q->next;
                    NET* s = p->next;
                    p->next = (NET*)q->next;
                    q->next = (AET*)p;
                    p = s;
                    q = &pAET;
                }

                p = (NET*)pAET.next;
                while (p != null && p->next != null) {
                    for (double j = p->x; j <= p->next->x; j++) {
                        //pDC.SetPixel(static_cast<int>(j), i,fillCol);
                        setTile((int)j, h, i, id, data);
                    }
                    p = p->next->next;
                }
            }
        }
    }

    void DrawLine(int x0, int y0, int x1, int y1, int h, int id, int data, int brush_radius = 0) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)((double)brush_radius / Math.Sqrt(1 + k * k));
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    if (brush_radius == 0) {
                        setTile((int)x, h, y, id, data);
                    } else {
                        if (x == Math.Min(x0, x1) || x == Math.Max(x0, x1)) {
                            DrawCircle((int)x, h, y, brush_radius, id, data);
                        }
                        for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                            setTile((int)x, h, y + dy, id, data);
                        }
                    }
                }
            } else {
                brush_radius = (int)((double)brush_radius * k / Math.Sqrt(1 + k * k));
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    if (brush_radius == 0) {
                        setTile(x, h, (int)y, id, data);
                    } else {
                        if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) {
                            DrawCircle(x, h, (int)y, brush_radius, id, data);
                        }
                        for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                            setTile(x + dx, h, (int)y, id, data);
                        }
                    }
                }
            }
        } else {
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                if (brush_radius == 0)
                    setTile(x0, h, (int)y, id, data);
                else {
                    if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) {
                        DrawCircle(x0, h, (int)y, brush_radius, id, data);
                    }
                    for (int dx = -brush_radius; dx <= brush_radius; dx++)
                        setTile(x0 + dx, h, (int)y, id, data);
                }
            }
        }
    }

    void DrawLine_Road(int x0, int y0, int x1, int y1, int h, Block black, Block white, Block yellow, int brush_radius, bool oneway) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)Math.Round((double)brush_radius / Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width / Math.Sqrt(1 + k * k));
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    //if (x == Math.Min(x0, x1) || x == Math.Max(x0, x1)) DrawCircle((int)x, h, y, brush_radius, id, data);
                    for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                        if (dy == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile((int)x, h, y + dy, yellow.id, yellow.data);
                        else if (dy % actual_per_width == 0 && (x - Math.Min(x0, x1)) % 7 >= 3 && brush_radius > actual_per_width)
                            setTile((int)x, h, y + dy, white.id, white.data);
                        else
                            setTile((int)x, h, y + dy, black.id, black.data);
                    }
                }
            } else {
                brush_radius = (int)Math.Round((double)brush_radius * Math.Abs(k) / Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width * Math.Abs(k) / Math.Sqrt(1 + k * k));
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    //if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) DrawCircle(x, h, (int)y, brush_radius, id, data);
                    for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                        if (dx == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile(x + dx, h, (int)y, yellow.id, yellow.data);
                        else if (dx % actual_per_width == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius > actual_per_width)
                            setTile(x + dx, h, (int)y, white.id, white.data);
                        else
                            setTile(x + dx, h, (int)y, black.id, black.data);
                    }
                }
            }
        } else {
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                //if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) DrawCircle(x0, h, (int)y, brush_radius, id, data);
                for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                    if (dx == 0 && brush_radius >= per_width && !oneway)
                        setTile(x0 + dx, h, (int)y, yellow.id, yellow.data);
                    else if (dx % per_width == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius > per_width)
                        setTile(x0 + dx, h, (int)y, white.id, white.data);
                    else
                        setTile(x0 + dx, h, (int)y, black.id, black.data);
                }
            }
        }
    }

    void DrawLine_Road_Lite(int x0, int y0, int x1, int y1, int h, Block black, Block white, Block yellow, int brush_radius, bool oneway) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)Math.Round((double)brush_radius / Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width / Math.Sqrt(1 + k * k));
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                        if (dy == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile((int)x, h, y + dy, yellow.id, yellow.data);
                        else if (dy == 0 && brush_radius >= actual_per_width && (x - Math.Min(x0, x1)) % 7 >= 3 && oneway)
                            setTile((int)x, h, y + dy, white.id, white.data);
                        else
                            setTile((int)x, h, y + dy, black.id, black.data);
                    }
                }
            } else {
                brush_radius = (int)Math.Round((double)brush_radius * Math.Abs(k) / Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width * Math.Abs(k) / Math.Sqrt(1 + k * k));
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                        if (dx == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile(x + dx, h, (int)y, yellow.id, yellow.data);
                        else if (dx == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius >= actual_per_width && oneway)
                            setTile(x + dx, h, (int)y, white.id, white.data);
                        else
                            setTile(x + dx, h, (int)y, black.id, black.data);
                    }
                }
            }
        } else {
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                    if (dx == 0 && brush_radius >= per_width && !oneway)
                        setTile(x0 + dx, h, (int)y, yellow.id, yellow.data);
                    else if (dx == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius >= per_width && oneway)
                        setTile(x0 + dx, h, (int)y, white.id, white.data);
                    else
                        setTile(x0 + dx, h, (int)y, black.id, black.data);
                }
            }
        }
    }

    void DrawLine_Road_Lite_v2(int x0, int y0, int x1, int y1, int h, Block black, Block white, Block yellow, int brush_radius, bool oneway) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)Math.Round((double)brush_radius * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width * Math.Sqrt(1 + k * k));
                //填充间隙
                if (x0 < x1) {      //右
                    cross[0, 1] = true;
                    cross[1, 1] = true;
                } else {            //左
                    cross[0, 0] = true;
                    cross[1, 0] = true;
                }
                if (lastk > 1 || lastk < -1) {
                    if (!cross[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                    if (!cross[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                    if (!cross[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                    if (!cross[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
                }
                //线刷子
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                        if (dy == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile((int)x, h, y + dy, yellow.id, yellow.data);
                        else if (dy == 0 && brush_radius >= actual_per_width && (x - Math.Min(x0, x1)) % 7 >= 3 && oneway)
                            setTile((int)x, h, y + dy, white.id, white.data);
                        else
                            setTile((int)x, h, y + dy, black.id, black.data);
                    }
                }
                //擦除并写入此次岔口数据
                cross[0, 0] = cross[0, 1] = cross[1, 0] = cross[1, 1] = false;
                if (x0 < x1) {      //左
                    cross[0, 0] = true;
                    cross[1, 0] = true;
                } else {            //右
                    cross[0, 1] = true;
                    cross[1, 1] = true;
                }
                lastk = k;
            } else {
                brush_radius = (int)Math.Round((double)brush_radius / Math.Abs(k) * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width / Math.Abs(k) * Math.Sqrt(1 + k * k));
                //填充间隙
                if (y0 < y1) {      //上
                    cross[0, 0] = true;
                    cross[0, 1] = true;
                } else {            //下
                    cross[1, 0] = true;
                    cross[1, 1] = true;
                }
                if (-1 <= lastk && lastk <= 1) {
                    if (!cross[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                    if (!cross[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                    if (!cross[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                    if (!cross[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
                }
                //线刷子
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                        if (dx == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile(x + dx, h, (int)y, yellow.id, yellow.data);
                        else if (dx == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius >= actual_per_width && oneway)
                            setTile(x + dx, h, (int)y, white.id, white.data);
                        else
                            setTile(x + dx, h, (int)y, black.id, black.data);
                    }
                }
                //擦除并写入此次岔口数据
                cross[0, 0] = cross[0, 1] = cross[1, 0] = cross[1, 1] = false;
                if (y0 < y1) {      //下
                    cross[1, 0] = true;
                    cross[1, 1] = true;
                } else {            //上
                    cross[0, 0] = true;
                    cross[0, 1] = true;
                }
                lastk = k;
            }
        } else {
            //填充间隙
            if (y0 < y1) {      //上
                cross[0, 0] = true;
                cross[0, 1] = true;
            } else {            //下
                cross[1, 0] = true;
                cross[1, 1] = true;
            }
            if (-1 <= lastk && lastk <= 1) {
                if (!cross[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                if (!cross[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                if (!cross[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                if (!cross[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
            }
            //线刷子
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                    if (dx == 0 && brush_radius >= per_width && !oneway)
                        setTile(x0 + dx, h, (int)y, yellow.id, yellow.data);
                    else if (dx == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius >= per_width && oneway)
                        setTile(x0 + dx, h, (int)y, white.id, white.data);
                    else
                        setTile(x0 + dx, h, (int)y, black.id, black.data);
                }
            }
            //擦除并写入此次岔口数据
            cross[0, 0] = cross[0, 1] = cross[1, 0] = cross[1, 1] = false;
            if (y0 < y1) {      //下
                cross[1, 0] = true;
                cross[1, 1] = true;
            } else {            //上
                cross[0, 0] = true;
                cross[0, 1] = true;
            }
            lastk = inf;
        }
    }

    void DrawLine_Road_Lite_v3(int x0, int y0, int x1, int y1, int h, Block black, Block white, Block yellow, int brush_radius, bool oneway, bool bridge = false, int lamp_step = 0, int pedestrian_width = 0) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)Math.Round((double)brush_radius * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width * Math.Sqrt(1 + k * k));
                //填充间隙
                if (x0 < x1) {      //右
                    cross[0, 1] = true;
                    cross[1, 1] = true;
                } else {            //左
                    cross[0, 0] = true;
                    cross[1, 0] = true;
                }
                if (lastk > 1 || lastk < -1) {
                    if (!cross[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                    if (!cross[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                    if (!cross[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                    if (!cross[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
                }
                //线刷子
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                        if (dy == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile((int)x, h, y + dy, yellow.id, yellow.data);
                        else if (dy == 0 && brush_radius >= actual_per_width && (x - Math.Min(x0, x1)) % 7 >= 3 && oneway)
                            setTile((int)x, h, y + dy, white.id, white.data);
                        else
                            setTile((int)x, h, y + dy, black.id, black.data);
                        flat_height((int)x, y + dy, h, road_flat_height);
                    }
                    if (bridge) {
                        int dy = brush_radius + 1;
                        setTile((int)x, h + 1, y - dy, 98, 0);
                        setTile((int)x, h + 1, y + dy, 98, 0);
                    } else {
                        if (lamp_step > 0 && cnt % lamp_step == 0) {
                            int dy = brush_radius + 2;
                            Block bottom = getTile((int)x, h, y + dy);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile((int)x, h + dh, y + dy, 85, 2);
                                setTile((int)x, h + 6, y + dy, 124, 0);     //Redstone lamp
                                setTile((int)x, h + 7, y + dy, 178, 0);     //Inverted daylight detector
                            }
                            bottom = getTile((int)x, h, y - dy);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile((int)x, h + dh, y - dy, 85, 2);
                                setTile((int)x, h + 6, y - dy, 124, 0);
                                setTile((int)x, h + 7, y - dy, 178, 0);
                            }
                        }
                        for (int dy = brush_radius + 1; dy <= brush_radius + pedestrian_width; dy++) {
                            Block bottom = getTile((int)x, h, y + dy);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                                setTile((int)x, h + 1, y + dy, 44, 0, true);
                            bottom = getTile((int)x, h, y - dy);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                                setTile((int)x, h + 1, y - dy, 44, 0, true);
                        }
                    }
                    cnt++;
                }
                //擦除并写入此次岔口数据
                cross[0, 0] = cross[0, 1] = cross[1, 0] = cross[1, 1] = false;
                if (x0 < x1) {      //左
                    cross[0, 0] = true;
                    cross[1, 0] = true;
                } else {            //右
                    cross[0, 1] = true;
                    cross[1, 1] = true;
                }
                lastk = k;
            } else {
                brush_radius = (int)Math.Round((double)brush_radius / Math.Abs(k) * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width / Math.Abs(k) * Math.Sqrt(1 + k * k));
                //填充间隙
                if (y0 < y1) {      //上
                    cross[0, 0] = true;
                    cross[0, 1] = true;
                } else {            //下
                    cross[1, 0] = true;
                    cross[1, 1] = true;
                }
                if (-1 <= lastk && lastk <= 1) {
                    if (!cross[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                    if (!cross[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                    if (!cross[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                    if (!cross[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
                }
                //线刷子
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                        if (dx == 0 && brush_radius >= actual_per_width && !oneway)
                            setTile(x + dx, h, (int)y, yellow.id, yellow.data);
                        else if (dx == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius >= actual_per_width && oneway)
                            setTile(x + dx, h, (int)y, white.id, white.data);
                        else
                            setTile(x + dx, h, (int)y, black.id, black.data);
                        flat_height(x + dx, (int)y, h, road_flat_height);
                    }
                    if (bridge) {
                        int dx = brush_radius + 1;
                        setTile(x + dx, h + 1, (int)y, 98, 0);
                        setTile(x - dx, h + 1, (int)y, 98, 0);
                    } else {
                        if (lamp_step > 0 && cnt % lamp_step == 0) {
                            int dx = brush_radius + 2;
                            Block bottom = getTile(x + dx, h, (int)y);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile(x + dx, h + dh, (int)y, 85, 2);
                                setTile(x + dx, h + 6, (int)y, 124, 0);     //Redstone lamp
                                setTile(x + dx, h + 7, (int)y, 178, 0);     //Inverted daylight detector
                            }
                            bottom = getTile(x - dx, h, (int)y);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile(x - dx, h + dh, (int)y, 85, 2);
                                setTile(x - dx, h + 6, (int)y, 124, 0);
                                setTile(x - dx, h + 7, (int)y, 178, 0);
                            }
                        }
                        for (int dx = brush_radius + 1; dx <= brush_radius + pedestrian_width; dx++) {
                            Block bottom = getTile(x + dx, h, (int)y);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                                setTile(x + dx, h + 1, (int)y, 44, 0, true);
                            bottom = getTile(x - dx, h, (int)y);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                                setTile(x - dx, h + 1, (int)y, 44, 0, true);
                        }
                    }
                    cnt++;
                }
                //擦除并写入此次岔口数据
                cross[0, 0] = cross[0, 1] = cross[1, 0] = cross[1, 1] = false;
                if (y0 < y1) {      //下
                    cross[1, 0] = true;
                    cross[1, 1] = true;
                } else {            //上
                    cross[0, 0] = true;
                    cross[0, 1] = true;
                }
                lastk = k;
            }
        } else {
            //填充间隙
            if (y0 < y1) {      //上
                cross[0, 0] = true;
                cross[0, 1] = true;
            } else {            //下
                cross[1, 0] = true;
                cross[1, 1] = true;
            }
            if (-1 <= lastk && lastk <= 1) {
                if (!cross[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                if (!cross[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                if (!cross[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                if (!cross[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
            }
            //线刷子
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                    if (dx == 0 && brush_radius >= per_width && !oneway)
                        setTile(x0 + dx, h, (int)y, yellow.id, yellow.data);
                    else if (dx == 0 && (y - Math.Min(y0, y1)) % 7 >= 3 && brush_radius >= per_width && oneway)
                        setTile(x0 + dx, h, (int)y, white.id, white.data);
                    else
                        setTile(x0 + dx, h, (int)y, black.id, black.data);
                    flat_height(x0 + dx, (int)y, h, road_flat_height);
                }
                int x = x0;
                if (bridge) {
                    int dx = brush_radius + 1;
                    setTile(x0 + dx, h + 1, (int)y, 98, 0);
                    setTile(x0 - dx, h + 1, (int)y, 98, 0);
                } else {
                    if (lamp_step > 0 && cnt % lamp_step == 0) {
                        int dx = brush_radius + 2;
                        Block bottom = getTile(x + dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                            for (int dh = 2; dh <= 5; dh++)
                                setTile(x + dx, h + dh, (int)y, 85, 2);
                            setTile(x + dx, h + 6, (int)y, 124, 0);     //Redstone lamp
                            setTile(x + dx, h + 7, (int)y, 178, 0);     //Inverted daylight detector
                        }
                        bottom = getTile(x - dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                            for (int dh = 2; dh <= 5; dh++)
                                setTile(x - dx, h + dh, (int)y, 85, 2);
                            setTile(x - dx, h + 6, (int)y, 124, 0);
                            setTile(x - dx, h + 7, (int)y, 178, 0);
                        }
                    }
                    for (int dx = brush_radius + 1; dx <= brush_radius + pedestrian_width; dx++) {
                        Block bottom = getTile(x + dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                            setTile(x + dx, h + 1, (int)y, 44, 0, true);
                        bottom = getTile(x - dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                            setTile(x - dx, h + 1, (int)y, 44, 0, true);
                    }
                }
                cnt++;
            }
            //擦除并写入此次岔口数据
            cross[0, 0] = cross[0, 1] = cross[1, 0] = cross[1, 1] = false;
            if (y0 < y1) {      //下
                cross[1, 0] = true;
                cross[1, 1] = true;
            } else {            //上
                cross[0, 0] = true;
                cross[0, 1] = true;
            }
            lastk = inf;
        }
    }

    void DrawLine_Road_Pedestrian(int x0, int y0, int x1, int y1, int h, Block black, int brush_radius, bool bridge = false, int lamp_step = 0) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)Math.Round((double)brush_radius * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width * Math.Sqrt(1 + k * k));
                //填充间隙
                if (x0 < x1) {      //右
                    cross2[0, 1] = true;
                    cross2[1, 1] = true;
                } else {            //左
                    cross2[0, 0] = true;
                    cross2[1, 0] = true;
                }
                if (lastk > 1 || lastk < -1) {
                    if (!cross2[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                    if (!cross2[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                    if (!cross2[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                    if (!cross2[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
                }
                //线刷子
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                        Block bottom = getTile((int)x, h, y + dy);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                        {
                            setTile((int)x, h, y + dy, black.id, black.data);
                            flat_height((int)x, y + dy, h, road_flat_height);
                        }
                    }
                    if (bridge) {
                        int dy = brush_radius + 1;
                        setTile((int)x, h + 1, y - dy, 98, 0);
                        setTile((int)x, h + 1, y + dy, 98, 0);
                    } else {
                        if (lamp_step > 0 && cnt % lamp_step == 0) {
                            int dy = brush_radius + 2;
                            Block bottom = getTile((int)x, h, y + dy);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile((int)x, h + dh, y + dy, 85, 2);
                                setTile((int)x, h + 6, y + dy, 124, 0);     //Redstone lamp
                                setTile((int)x, h + 7, y + dy, 178, 0);     //Inverted daylight detector
                            }
                            bottom = getTile((int)x, h, y - dy);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile((int)x, h + dh, y - dy, 85, 2);
                                setTile((int)x, h + 6, y - dy, 124, 0);
                                setTile((int)x, h + 7, y - dy, 178, 0);
                            }
                        }
                    }
                    cnt++;
                }
                //擦除并写入此次岔口数据
                cross2[0, 0] = cross2[0, 1] = cross2[1, 0] = cross2[1, 1] = false;
                if (x0 < x1) {      //左
                    cross2[0, 0] = true;
                    cross2[1, 0] = true;
                } else {            //右
                    cross2[0, 1] = true;
                    cross2[1, 1] = true;
                }
                lastk = k;
            } else {
                brush_radius = (int)Math.Round((double)brush_radius / Math.Abs(k) * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width / Math.Abs(k) * Math.Sqrt(1 + k * k));
                //填充间隙
                if (y0 < y1) {      //上
                    cross2[0, 0] = true;
                    cross2[0, 1] = true;
                } else {            //下
                    cross2[1, 0] = true;
                    cross2[1, 1] = true;
                }
                if (-1 <= lastk && lastk <= 1) {
                    if (!cross2[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                    if (!cross2[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                    if (!cross2[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                    if (!cross2[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
                }
                //线刷子
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                        Block bottom = getTile(x + dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                        {
                            setTile(x + dx, h, (int)y, black.id, black.data);
                            flat_height(x + dx, (int)y, h, road_flat_height);
                        }
                    }
                    if (bridge) {
                        int dx = brush_radius + 1;
                        setTile(x + dx, h + 1, (int)y, 98, 0);
                        setTile(x - dx, h + 1, (int)y, 98, 0);
                    } else {
                        if (lamp_step > 0 && cnt % lamp_step == 0) {
                            int dx = brush_radius + 2;
                            Block bottom = getTile(x + dx, h, (int)y);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile(x + dx, h + dh, (int)y, 85, 2);
                                setTile(x + dx, h + 6, (int)y, 124, 0);     //Redstone lamp
                                setTile(x + dx, h + 7, (int)y, 178, 0);     //Inverted daylight detector
                            }
                            bottom = getTile(x - dx, h, (int)y);
                            if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                                for (int dh = 2; dh <= 5; dh++)
                                    setTile(x - dx, h + dh, (int)y, 85, 2);
                                setTile(x - dx, h + 6, (int)y, 124, 0);
                                setTile(x - dx, h + 7, (int)y, 178, 0);
                            }
                        }
                    }
                    cnt++;
                }
                //擦除并写入此次岔口数据
                cross2[0, 0] = cross2[0, 1] = cross2[1, 0] = cross2[1, 1] = false;
                if (y0 < y1) {      //下
                    cross2[1, 0] = true;
                    cross2[1, 1] = true;
                } else {            //上
                    cross2[0, 0] = true;
                    cross2[0, 1] = true;
                }
                lastk = k;
            }
        } else {
            //填充间隙
            if (y0 < y1) {      //上
                cross2[0, 0] = true;
                cross2[0, 1] = true;
            } else {            //下
                cross2[1, 0] = true;
                cross2[1, 1] = true;
            }
            if (-1 <= lastk && lastk <= 1) {
                if (!cross2[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black);
                if (!cross2[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black);
                if (!cross2[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black);
                if (!cross2[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black);
            }
            //线刷子
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                    Block bottom = getTile(x0 + dx, h, (int)y);
                    if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
                    {
                        setTile(x0 + dx, h, (int)y, black.id, black.data);
                        flat_height(x0 + dx, (int)y, h, road_flat_height);
                    }
                }
                int x = x0;
                if (bridge) {
                    int dx = brush_radius + 1;
                    setTile(x0 + dx, h + 1, (int)y, 98, 0);
                    setTile(x0 - dx, h + 1, (int)y, 98, 0);
                } else {
                    if (lamp_step > 0 && cnt % lamp_step == 0) {
                        int dx = brush_radius + 2;
                        Block bottom = getTile(x + dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                            for (int dh = 2; dh <= 5; dh++)
                                setTile(x + dx, h + dh, (int)y, 85, 2);
                            setTile(x + dx, h + 6, (int)y, 124, 0);     //Redstone lamp
                            setTile(x + dx, h + 7, (int)y, 178, 0);     //Inverted daylight detector
                        }
                        bottom = getTile(x - dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                            for (int dh = 2; dh <= 5; dh++)
                                setTile(x - dx, h + dh, (int)y, 85, 2);
                            setTile(x - dx, h + 6, (int)y, 124, 0);
                            setTile(x - dx, h + 7, (int)y, 178, 0);
                        }
                    }
                }
                cnt++;
            }
            //擦除并写入此次岔口数据
            cross2[0, 0] = cross2[0, 1] = cross2[1, 0] = cross2[1, 1] = false;
            if (y0 < y1) {      //下
                cross2[1, 0] = true;
                cross2[1, 1] = true;
            } else {            //上
                cross2[0, 0] = true;
                cross2[0, 1] = true;
            }
            lastk = inf;
        }
    }

    void DrawLine_Road_Waterway(int x0, int y0, int x1, int y1, int h, Block black, int brush_radius, int flat_height = 0) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)Math.Round((double)brush_radius * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width * Math.Sqrt(1 + k * k));
                //填充间隙
                if (x0 < x1) {      //右
                    cross3[0, 1] = true;
                    cross3[1, 1] = true;
                } else {            //左
                    cross3[0, 0] = true;
                    cross3[1, 0] = true;
                }
                if (lastk > 1 || lastk < -1) {
                    if (!cross3[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black, flat_height);
                    if (!cross3[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black, flat_height);
                    if (!cross3[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black, flat_height);
                    if (!cross3[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black, flat_height);
                }
                //线刷子
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                        Block bottom = getTile((int)x, h, y + dy);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                            setTile((int)x, h, y + dy, black.id, black.data);
                            Block up = getTile((int)x, h + 1, y + dy);
                            if (!IsRoadBlock(up)) setTile((int)x, h + 1, y + dy, 0, 0);
                        }
                    }
                    cnt++;
                }
                //擦除并写入此次岔口数据
                cross3[0, 0] = cross3[0, 1] = cross3[1, 0] = cross3[1, 1] = false;
                if (x0 < x1) {      //左
                    cross3[0, 0] = true;
                    cross3[1, 0] = true;
                } else {            //右
                    cross3[0, 1] = true;
                    cross3[1, 1] = true;
                }
                lastk = k;
            } else {
                brush_radius = (int)Math.Round((double)brush_radius / Math.Abs(k) * Math.Sqrt(1 + k * k));
                int actual_per_width = (int)Math.Floor((double)per_width / Math.Abs(k) * Math.Sqrt(1 + k * k));
                //填充间隙
                if (y0 < y1) {      //上
                    cross3[0, 0] = true;
                    cross3[0, 1] = true;
                } else {            //下
                    cross3[1, 0] = true;
                    cross3[1, 1] = true;
                }
                if (-1 <= lastk && lastk <= 1) {
                    if (!cross3[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black, flat_height);
                    if (!cross3[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black, flat_height);
                    if (!cross3[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black, flat_height);
                    if (!cross3[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black, flat_height);
                }
                //线刷子
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                        Block bottom = getTile(x + dx, h, (int)y);
                        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                            setTile(x + dx, h, (int)y, black.id, black.data);
                            Block up = getTile(x + dx, h + 1, (int)y);
                            if (!IsRoadBlock(up)) setTile(x + dx, h + 1, (int)y, 0, 0);
                        }
                    }
                    cnt++;
                }
                //擦除并写入此次岔口数据
                cross3[0, 0] = cross3[0, 1] = cross3[1, 0] = cross3[1, 1] = false;
                if (y0 < y1) {      //下
                    cross3[1, 0] = true;
                    cross3[1, 1] = true;
                } else {            //上
                    cross3[0, 0] = true;
                    cross3[0, 1] = true;
                }
                lastk = k;
            }
        } else {
            //填充间隙
            if (y0 < y1) {      //上
                cross3[0, 0] = true;
                cross3[0, 1] = true;
            } else {            //下
                cross3[1, 0] = true;
                cross3[1, 1] = true;
            }
            if (-1 <= lastk && lastk <= 1) {
                if (!cross3[0, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, -1, black, flat_height);
                if (!cross3[0, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, -1, black, flat_height);
                if (!cross3[1, 0]) DrawCircle_Quarter(x0, h, y0, brush_radius, -1, 1, black, flat_height);
                if (!cross3[1, 1]) DrawCircle_Quarter(x0, h, y0, brush_radius, 1, 1, black, flat_height);
            }
            //线刷子
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                    Block bottom = getTile(x0 + dx, h, (int)y);
                    if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4))) {
                        setTile(x0 + dx, h, (int)y, black.id, black.data);
                        Block up = getTile(x0 + dx, h + 1, (int)y);
                        if (!IsRoadBlock(up)) setTile(x0 + dx, h + 1, (int)y, 0, 0);
                    }
                }
                cnt++;
            }
            //擦除并写入此次岔口数据
            cross3[0, 0] = cross3[0, 1] = cross3[1, 0] = cross3[1, 1] = false;
            if (y0 < y1) {      //下
                cross3[1, 0] = true;
                cross3[1, 1] = true;
            } else {            //上
                cross3[0, 0] = true;
                cross3[0, 1] = true;
            }
            lastk = inf;
        }
    }

    void DrawLine_Building(int x0, int y0, int x1, int y1, int h, int id, int data, int wall_pernum, int window_pernum) {
        int cnt = 0;
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)Math.Round(k * x + b);
                    int rid = id, rdata = data;
                    if (cnt % (wall_pernum + window_pernum) >= wall_pernum) {
                        rid = 20;
                        rdata = 0;
                    }
                    setTile((int)x, h, y, rid, rdata);
                    cnt++;
                }
            } else {
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)Math.Floor((float)(y - b) / k);
                    int rid = id, rdata = data;
                    if (cnt % (wall_pernum + window_pernum) >= wall_pernum) {
                        rid = 20;
                        rdata = 0;
                    }
                    setTile(x, h, (int)y, rid, rdata);
                    cnt++;
                }
            }
        } else {
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                int rid = id, rdata = data;
                if (cnt % (wall_pernum + window_pernum) >= wall_pernum) {
                    rid = 20;
                    rdata = 0;
                }
                setTile(x0, h, (int)y, rid, rdata);
                cnt++;
            }
        }
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
                                //CAUTION: 旋转角Minecraft和Unity相反，这里按照Unity规则写
                                //原先旋转角：0
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
                                //原先旋转角：180
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
                                //原先旋转角：90
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
                                //原先旋转角：270
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

    void DrawLine_Roof(int startx, int starty, int endx, int endy, int sh, Roof RoofConfig, List<Node> nodes, Node? last, Node? next, Block? Base = null) {
        var roof = RoofConfig.Data;
        if (startx != endx) {
            double k = (double)(endy - starty) / (double)(endx - startx);
            double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
            if (-1 < k && k < 1) {
                if (startx <= endx) {
                    for (float x = startx; x <= endx; x++) {
                        int y = (int)Math.Round(k * x + b);
                        int d = 0;
                        if (pnpoly2(nodes, new Node((int)x, y - roof.GetLength(1) + 1))) d = -1;
                        else if (pnpoly2(nodes, new Node((int)x, y + roof.GetLength(1) - 1))) d = 1;
                        if (x == startx) {
                            if (d == 0) {
                                roof_node_list.Add(new Node((int)x, y + (roof.GetLength(1) - 1) * lastd));
                                continue;
                            } else {
                                roof_node_list.Add(new Node((int)x, y + (roof.GetLength(1) - 1) * d));
                            }
                        }
                        if (d == 0) continue;
                        int end_dy = roof.GetLength(1);
                        if (!(last is null)) {
                            if (startx == last.Value.x) {
                                end_dy = Math.Min(roof.GetLength(1), (int)x - startx + 1);
                            } else {
                                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                if (last_k > 1 || last_k < -1) end_dy = Math.Min(roof.GetLength(1), (int)x - startx + 1);
                            }
                        }
                        if (!(next is null)) {
                            if (endx == next.Value.x) {
                                end_dy = Math.Min(end_dy, endx - (int)x + 1);
                            } else {
                                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                if (next_k > 1 || next_k < -1) end_dy = Math.Min(end_dy, endx - (int)x + 1);
                            }
                        }
                        //for (int _dy = 0; _dy < Math.Min(roof.GetLength(1), Math.Min(x - startx, endx - x)); _dy++) {
                        for (int _dy = 0; _dy < end_dy; _dy++) {
                            int dy = (_dy - 1) * d;
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
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (id == 0) continue;
                                setTile((int)x, h, y + dy, id, data, 0);
                            }
                        }
                        cnt++;
                        lastd = d;
                    }
                } else {
                    for (float x = startx; x >= endx; x--) {
                        int y = (int)Math.Round(k * x + b);
                        int d = 0;
                        if (pnpoly2(nodes, new Node((int)x, y - roof.GetLength(1) + 1))) d = -1;
                        else if (pnpoly2(nodes, new Node((int)x, y + roof.GetLength(1) - 1))) d = 1;
                        if (x == startx) {
                            if (d == 0) {
                                roof_node_list.Add(new Node((int)x, y + (roof.GetLength(1) - 1) * lastd));
                                continue;
                            } else {
                                roof_node_list.Add(new Node((int)x, y + (roof.GetLength(1) - 1) * d));
                            }
                        }
                        if (d == 0) continue;
                        int end_dy = roof.GetLength(1);
                        if (!(last is null)) {
                            if (startx == last.Value.x) {
                                end_dy = Math.Min(roof.GetLength(1), startx - (int)x + 1);
                            } else {
                                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                if (last_k > 1 || last_k < -1) end_dy = Math.Min(roof.GetLength(1), startx - (int)x + 1);
                            }
                        }
                        if (!(next is null)) {
                            if (next.Value.x == endx) {
                                end_dy = Math.Min(end_dy, (int)x - endx + 1);
                            } else {
                                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                if (next_k > 1 || next_k < -1) end_dy = Math.Min(end_dy, (int)x - endx + 1);
                            }
                        }
                        for (int _dy = 0; _dy < end_dy; _dy++) {
                            int dy = (_dy - 1) * d;
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
                                if (cblock.random == 1) {
                                    if (c_rand_data == -1) c_rand_data = rd.Next(cblock.rand_min, cblock.rand_max);
                                    data = c_rand_data;
                                } else if (cblock.random == 2) {
                                    data = rd.Next(cblock.rand_min, cblock.rand_max);
                                }
                                if (id == 0) continue;
                                setTile((int)x, h, y + dy, id, data, 0);
                            }
                        }
                        cnt++;
                        lastd = d;
                    }
                }
            } else {
                if (starty <= endy) {
                    for (float y = starty; y <= endy; y++) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        int d = 0;
                        if (pnpoly2(nodes, new Node(x - roof.GetLength(1) + 1, (int)y))) d = -1;
                        else if (pnpoly2(nodes, new Node(x + roof.GetLength(1) - 1, (int)y))) d = 1;
                        if (y == starty) {
                            if (d == 0) {
                                roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * lastd, (int)y));
                                continue;
                            } else {
                                roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * d, (int)y));
                            }
                        }
                        if (d == 0) continue;
                        int end_dx = roof.GetLength(1);
                        if (!(last is null)) {
                            if (startx == last.Value.x) {
                                end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                            } else {
                                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                if (-1 < last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                            }
                        }
                        if (!(next is null)) {
                            if (next.Value.x == endx) {
                                end_dx = Math.Min(end_dx, endy - (int)y + 1);
                            } else {
                                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                if (-1 < next_k && next_k < 1) end_dx = Math.Min(end_dx, endy - (int)y + 1);
                            }
                        }
                        for (int _dx = 0; _dx < end_dx; _dx++) {
                            int dx = (_dx - 1) * d;
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
                                setTile(x + dx, h, (int)y, id, data, 0);
                            }
                        }
                        cnt++;
                        lastd = d;
                    }
                } else {
                    for (float y = starty; y >= endy; y--) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        int d = 0;
                        if (pnpoly2(nodes, new Node(x - roof.GetLength(1) + 1, (int)y))) d = -1;
                        else if (pnpoly2(nodes, new Node(x + roof.GetLength(1) - 1, (int)y))) d = 1;
                        if (y == starty) {
                            if (d == 0) {
                                roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * lastd, (int)y));
                                continue;
                            } else {
                                roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * d, (int)y));
                            }
                        }
                        if (d == 0) continue;
                        int end_dx = roof.GetLength(1);
                        if (!(last is null)) {
                            if (startx == last.Value.x) {
                                end_dx = Math.Min(roof.GetLength(1), starty - (int)y + 1);
                            } else {
                                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                if (-1 < last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), starty - (int)y + 1);
                            }
                        }
                        if (!(next is null)) {
                            if (next.Value.x == endx) {
                                end_dx = Math.Min(end_dx, (int)y - endy + 1);
                            } else {
                                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                if (-1 < next_k && next_k < 1) end_dx = Math.Min(end_dx, (int)y - endy + 1);
                            }
                        }
                        for (int _dx = 0; _dx < end_dx; _dx++) {
                            int dx = (_dx - 1) * d;
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
                                setTile(x + dx, h, (int)y, id, data, 0);
                            }
                        }
                        cnt++;
                        lastd = d;
                    }
                }
            }
        } else {
            if (starty <= endy) {
                for (float y = starty; y <= endy; y++) {
                    int x = startx;
                    int d = 0;
                    if (pnpoly2(nodes, new Node(x - roof.GetLength(1) + 1, (int)y))) d = -1;
                    else if (pnpoly2(nodes, new Node(x + roof.GetLength(1) - 1, (int)y))) d = 1;
                    if (y == starty) {
                        if (d == 0) {
                            roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * lastd, (int)y));
                            continue;
                        } else {
                            roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * d, (int)y));
                        }
                    }
                    if (d == 0) continue;
                    int end_dx = roof.GetLength(1);
                    if (!(last is null)) {
                        if (startx == last.Value.x) {
                            end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                        } else {
                            double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                            if (-1 < last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                        }
                    }
                    if (!(next is null)) {
                        if (next.Value.x == endx) {
                            end_dx = Math.Min(end_dx, endy - (int)y + 1);
                        } else {
                            double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                            if (-1 < next_k && next_k < 1) end_dx = Math.Min(end_dx, endy - (int)y + 1);
                        }
                    }
                    for (int _dx = 0; _dx < end_dx; _dx++) {
                        int dx = (_dx - 1) * d;
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
                            setTile(x + dx, h, (int)y, id, data, 0);
                        }
                    }
                    cnt++;
                    lastd = d;
                }
            } else {
                for (float y = starty; y >= endy; y--) {
                    int x = startx;
                    int d = 0;
                    if (pnpoly2(nodes, new Node(x - roof.GetLength(1) + 1, (int)y))) d = -1;
                    else if (pnpoly2(nodes, new Node(x + roof.GetLength(1) - 1, (int)y))) d = 1;
                    if (y == starty) {
                        if (d == 0) {
                            roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * lastd, (int)y));
                            continue;
                        } else {
                            roof_node_list.Add(new Node(x + (roof.GetLength(1) - 1) * d, (int)y));
                        }
                    }
                    if (d == 0) continue;
                    int end_dx = roof.GetLength(1);
                    if (!(last is null)) {
                        if (startx == last.Value.x) {
                            end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                        } else {
                            double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                            if (-1 < last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                        }
                    }
                    if (!(next is null)) {
                        if (next.Value.x == endx) {
                            end_dx = Math.Min(end_dx, endy - (int)y + 1);
                        } else {
                            double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                            if (-1 < next_k && next_k < 1) end_dx = Math.Min(end_dx, endy - (int)y + 1);
                        }
                    }
                    for (int _dx = 0; _dx < end_dx; _dx++) {
                        int dx = (_dx - 1) * d;
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
                            setTile(x + dx, h, (int)y, id, data, 0);
                        }
                    }
                    cnt++;
                    lastd = d;
                }
            }
        }
    }

    void DrawLine_Roof_Improved(int startx, int starty, int endx, int endy, int sh, Roof RoofConfig, List<Node> nodes, Node? last, Node? next, Block? Base = null) {
        var roof = RoofConfig.Data;
        if (RoofConfig.Base != null) Base = RoofConfig.Base;
        if (startx != endx) {
            double k = (double)(endy - starty) / (double)(endx - startx);
            double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
            if (-1 <= k && k < 1) {
                if (startx <= endx) {
                    for (float x = startx; x <= endx; x++) {
                        int y = (int)Math.Round(k * x + b);
                        int d = 0;
                        if (pnpoly2(nodes, new Node((int)x, y - RoofConfig.GetReduceDelta()))) d = -1;
                        else if (pnpoly2(nodes, new Node((int)x, y + RoofConfig.GetReduceDelta()))) d = 1;
                        else continue;
                        int end_dy = roof.GetLength(1);
                        if (!(last is null)) {
                            Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            float cos = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (startx == last.Value.x) {
                                    end_dy = Math.Min(roof.GetLength(1), (int)x - startx + 1);
                                } else {
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (last_k >= 1 || last_k < -1) end_dy = Math.Min(roof.GetLength(1), (int)x - startx + 1);
                                }
                            }
                        }
                        if (!(next is null)) {
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                            float cos = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (endx == next.Value.x) {
                                    end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                } else {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (next_k >= 1 || next_k < -1) end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                }
                            }
                        }
                        //for (int _dy = 0; _dy < Math.Min(roof.GetLength(1), Math.Min(x - startx, endx - x)); _dy++) {
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
                                //CAUTION: 旋转角Minecraft和Unity相反，这里按照Minecraft规则写
                                if (d == -1)
                                    setTile((int)x, h, y + dy, id, data, 0);
                                else if (d == 1)
                                    setTile((int)x, h, y + dy, id, data, 180);
                            }
                        }
                        cnt++;
                    }
                } else {
                    for (float x = startx; x >= endx; x--) {
                        int y = (int)Math.Round(k * x + b);
                        int d = 0;
                        if (pnpoly2(nodes, new Node((int)x, y - RoofConfig.GetReduceDelta()))) d = -1;
                        else if (pnpoly2(nodes, new Node((int)x, y + RoofConfig.GetReduceDelta()))) d = 1;
                        else continue;
                        int end_dy = roof.GetLength(1);
                        if (!(last is null)) {
                            Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            float cos = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (startx == last.Value.x) {
                                    end_dy = Math.Min(roof.GetLength(1), startx - (int)x + 1);
                                } else {
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (last_k >= 1 || last_k < -1) end_dy = Math.Min(roof.GetLength(1), startx - (int)x + 1);
                                }
                            }
                        }
                        if (!(next is null)) {
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                            float cos = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (next.Value.x == endx) {
                                    end_dy = Math.Min(end_dy, (int)x - endx + 1);
                                } else {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (next_k >= 1 || next_k < -1) end_dy = Math.Min(end_dy, (int)x - endx + 1);
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
                                    setTile((int)x, h, y + dy, id, data, 180);
                                else if (d == 1)
                                    setTile((int)x, h, y + dy, id, data, 0);
                            }
                        }
                        cnt++;
                    }
                }
            } else {
                if (starty <= endy) {
                    for (float y = starty; y <= endy; y++) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        int d = 0;
                        if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                        else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                        else continue;
                        int end_dx = roof.GetLength(1);
                        if (!(last is null)) {
                            Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            float cos = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (startx != last.Value.x) {
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (-1 <= last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                                }
                            }
                        }
                        if (!(next is null)) {
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                            float cos = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (next.Value.x != endx) {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (-1 <= next_k && next_k < 1) end_dx = Math.Min(end_dx, endy - (int)y + 1);
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
                                    setTile(x + dx, h, (int)y, id, data, 90);
                                else if (d == -1)
                                    setTile(x + dx, h, (int)y, id, data, 270);
                            }
                        }
                        cnt++;
                    }
                } else {
                    for (float y = starty; y >= endy; y--) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        int d = 0;
                        if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                        else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                        else continue;
                        int end_dx = roof.GetLength(1);
                        if (!(last is null)) {
                            Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            float cos = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (startx != last.Value.x) {
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (-1 <= last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), starty - (int)y + 1);
                                }
                            }
                        }
                        if (!(next is null)) {
                            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                            Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                            float cos = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                            if (-cos_th < cos && cos < cos_th) {
                                if (next.Value.x != endx) {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (-1 <= next_k && next_k < 1) end_dx = Math.Min(end_dx, (int)y - endy + 1);
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
                                    setTile(x + dx, h, (int)y, id, data, 90);
                                else if (d == -1)
                                    setTile(x + dx, h, (int)y, id, data, 270);
                            }
                        }
                        cnt++;
                    }
                }
            }
        } else {
            if (starty <= endy) {
                for (float y = starty; y <= endy; y++) {
                    int x = startx;
                    int d = 0;
                    if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                    else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                    else continue;
                    int end_dx = roof.GetLength(1);
                    if (!(last is null)) {
                        Vector2 vec1 = new Vector2(startx - last.Value.x, endx - last.Value.x);
                        Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                        float cos = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                        if (-cos_th < cos && cos < cos_th) {
                            if (startx != last.Value.x) {
                                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                if (-1 <= last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                            }
                        }
                    }
                    if (!(next is null)) {
                        Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                        Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                        float cos = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                        if (-cos_th < cos && cos < cos_th) {
                            if (next.Value.x != endx) {
                                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                if (-1 <= next_k && next_k < 1) end_dx = Math.Min(end_dx, endy - (int)y + 1);
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
                                setTile(x + dx, h, (int)y, id, data, 90);
                            else if (d == -1)
                                setTile(x + dx, h, (int)y, id, data, 270);
                        }
                    }
                    cnt++;
                }
            } else {
                for (float y = starty; y >= endy; y--) {
                    int x = startx;
                    int d = 0;
                    if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                    else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                    else continue;
                    int end_dx = roof.GetLength(1);
                    if (!(last is null)) {
                        Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
                        Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                        float cos = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
                        if (-cos_th < cos && cos < cos_th) {
                            if (startx != last.Value.x) {
                                double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                if (-1 <= last_k && last_k < 1) end_dx = Math.Min(roof.GetLength(1), (int)y - starty + 1);
                            }
                        }
                    }
                    if (!(next is null)) {
                        Vector2 vec2 = new Vector2(endx - startx, endy - starty);
                        Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
                        float cos = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
                        if (-cos_th < cos && cos < cos_th) {
                            if (next.Value.x != endx) {
                                double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                if (-1 <= next_k && next_k < 1) end_dx = Math.Min(end_dx, endy - (int)y + 1);
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
                                setTile(x + dx, h, (int)y, id, data, 90);
                            else if (d == -1)
                                setTile(x + dx, h, (int)y, id, data, 270);
                        }
                    }
                    cnt++;
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
                        //for (int _dy = 0; _dy < Math.Min(roof.GetLength(1), Math.Min(x - startx, endx - x)); _dy++) {
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
                        //int d = 0;
                        //if (pnpoly2(nodes, new Node((int)x, y - RoofConfig.GetReduceDelta()))) d = -1;
                        //else if (pnpoly2(nodes, new Node((int)x, y + RoofConfig.GetReduceDelta()))) d = 1;
                        //else continue;
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

    void DrawLine_Roof_Lite(int startx, int starty, int endx, int endy, int sh, Roof RoofConfig, List<Node> nodes, Block? Base = null) {
        var roof = RoofConfig.Data;
        if (RoofConfig.Base != null) Base = RoofConfig.Base;
        if (startx != endx) {
            double k = (double)(endy - starty) / (double)(endx - startx);
            double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
            if (-1 <= k && k <= 1) {
                if (startx <= endx) {
                    for (float x = startx; x <= endx; x++) {
                        int y = (int)Math.Round(k * x + b);
                        int d = 0;
                        if (pnpoly2(nodes, new Node((int)x, y - RoofConfig.GetReduceDelta()))) d = -1;
                        else if (pnpoly2(nodes, new Node((int)x, y + RoofConfig.GetReduceDelta()))) d = 1;
                        else continue;
                        int end_dy = roof.GetLength(1);
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
                                //CAUTION: 旋转角Minecraft和Unity相反，这里按照Minecraft规则写
                                if (d == -1)
                                    setTile((int)x, h, y + dy, id, data, 0);
                                else if (d == 1)
                                    setTile((int)x, h, y + dy, id, data, 180);
                            }
                        }
                        cnt++;
                    }
                } else {
                    for (float x = startx; x >= endx; x--) {
                        int y = (int)Math.Round(k * x + b);
                        int d = 0;
                        if (pnpoly2(nodes, new Node((int)x, y - RoofConfig.GetReduceDelta()))) d = -1;
                        else if (pnpoly2(nodes, new Node((int)x, y + RoofConfig.GetReduceDelta()))) d = 1;
                        else continue;
                        int end_dy = roof.GetLength(1);
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
                                    setTile((int)x, h, y + dy, id, data, 180);
                                else if (d == 1)
                                    setTile((int)x, h, y + dy, id, data, 0);
                            }
                        }
                        cnt++;
                    }
                }
            } else {
                if (starty <= endy) {
                    for (float y = starty; y <= endy; y++) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        int d = 0;
                        if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                        else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                        else continue;
                        int end_dx = roof.GetLength(1);
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
                                    setTile(x + dx, h, (int)y, id, data, 90);
                                else if (d == -1)
                                    setTile(x + dx, h, (int)y, id, data, 270);
                            }
                        }
                        cnt++;
                    }
                } else {
                    for (float y = starty; y >= endy; y--) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        int d = 0;
                        if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                        else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                        else continue;
                        int end_dx = roof.GetLength(1);
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
                                    setTile(x + dx, h, (int)y, id, data, 90);
                                else if (d == -1)
                                    setTile(x + dx, h, (int)y, id, data, 270);
                            }
                        }
                        cnt++;
                    }
                }
            }
        } else {
            if (starty <= endy) {
                for (float y = starty; y <= endy; y++) {
                    int x = startx;
                    int d = 0;
                    if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                    else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                    else continue;
                    int end_dx = roof.GetLength(1);
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
                                setTile(x + dx, h, (int)y, id, data, 90);
                            else if (d == -1)
                                setTile(x + dx, h, (int)y, id, data, 270);
                        }
                    }
                    cnt++;
                }
            } else {
                for (float y = starty; y >= endy; y--) {
                    int x = startx;
                    int d = 0;
                    if (pnpoly2(nodes, new Node(x - RoofConfig.GetReduceDelta(), (int)y))) d = -1;
                    else if (pnpoly2(nodes, new Node(x + RoofConfig.GetReduceDelta(), (int)y))) d = 1;
                    else continue;
                    int end_dx = roof.GetLength(1);
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
                                setTile(x + dx, h, (int)y, id, data, 90);
                            else if (d == -1)
                                setTile(x + dx, h, (int)y, id, data, 270);
                        }
                    }
                    cnt++;
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

    void DrawLine_Interior(int startx, int starty, int endx, int endy, int clevel, int sh, Interior InteriorConfig, List<Node> nodes, Block? Base) {
        var interior = InteriorConfig.Data;
        if (InteriorConfig.Base == null) InteriorConfig.Base = Base;
        int c_rand_type = -1;
        if (startx != endx) {
            double k = (double)(endy - starty) / (double)(endx - startx);
            double b = (double)(endy * startx - starty * endx) / (double)(startx - endx);
            if (-1 <= k && k < 1) {
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

                if (startx <= endx) {
                    for (float x = startx; x < endx; x++) {
                        int y = (int)Math.Round(k * x + b);
                        for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                            int h = _h + sh;
                            for (int _dy = GetValidWidth((int)x, y); _dy > 0; _dy--) {
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
                                    setTile((int)x, h, y + dy, id, data, 0, null);
                                else if (d == -1)
                                    setTile((int)x, h, y + dy, id, data, 0, true);
                            }
                        }
                        Cnt[clevel]++;
                    }
                } else {
                    for (float x = startx; x > endx; x--) {
                        int y = (int)Math.Round(k * x + b);
                        for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                            int h = _h + sh;
                            for (int _dy = GetValidWidth((int)x, y); _dy > 0; _dy--) {
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
                                    setTile((int)x, h, y + dy, id, data, 180, true);
                                else if (d == -1)
                                    setTile((int)x, h, y + dy, id, data, 180, null);
                            }
                        }
                        Cnt[clevel]++;
                    }
                }
            } else {
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

                if (starty <= endy) {
                    for (float y = starty; y < endy; y++) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                            int h = _h + sh;
                            for (int _dx = GetValidWidth(x, (int)y); _dx > 0; _dx--) {
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
                                    setTile(x + dx, h, (int)y, id, data, 90, null);
                                else if (d == 1)
                                    setTile(x + dx, h, (int)y, id, data, 90, false);
                            }
                        }
                        Cnt[clevel]++;
                    }
                } else {
                    for (float y = starty; y > endy; y--) {
                        int x = (int)Math.Floor((float)(y - b) / k);
                        for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                            int h = _h + sh;
                            for (int _dx = GetValidWidth(x, (int)y); _dx > 0; _dx--) {
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
                                    setTile(x + dx, h, (int)y, id, data, 270, false);
                                else if (d == 1)
                                    setTile(x + dx, h, (int)y, id, data, 270, null);
                            }
                        }
                        Cnt[clevel]++;
                    }
                }
            }
        } else {
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

            if (starty <= endy) {
                for (float y = starty; y < endy; y++) {
                    int x = startx;
                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                        int h = _h + sh;
                        for (int _dx = GetValidWidth(x, (int)y); _dx > 0; _dx--) {
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
                                setTile(x + dx, h, (int)y, id, data, 90, null);
                            else if (d == 1)
                                setTile(x + dx, h, (int)y, id, data, 90, false);
                        }
                    }
                    Cnt[clevel]++;
                }
            } else {
                for (float y = starty; y > endy; y--) {
                    int x = startx;
                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                        int h = _h + sh;
                        for (int _dx = GetValidWidth(x, (int)y); _dx > 0; _dx--) {
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
                                setTile(x + dx, h, (int)y, id, data, 270, false);
                            else if (d == 1)
                                setTile(x + dx, h, (int)y, id, data, 270, null);
                        }
                    }
                    Cnt[clevel]++;
                }
            }
        }
    }

    void DrawLine_Interior_v2(int startx, int starty, int endx, int endy, int max_level, Interior InteriorConfig, List<Node> nodes, Node? last, Node? next, Block? Base = null, bool skipKCheck = false) {
        // v2的内饰放置算法：采用了和屋顶一样的交叉检测
        const float cos_th2 = 1.0f;
        Block[,,] interior = InteriorConfig.Data;
        if (InteriorConfig.Base == null) InteriorConfig.Base = Base;
        int c_rand_type = -1;
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
                if (!(last is null) && -cos_th2 < cos1 && cos1 < cos_th2) {
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
                bool reflex_next = false;
                if (!(next is null) && -cos_th2 < cos2 && cos2 < cos_th2) {
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
                                            setTile((int)x, h, y + dy, id, data, 0, null);
                                        else if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 0, true);
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
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (last_k >= 1 || last_k < -1 || skipKCheck)
                                            end_dy = Math.Min(end_dy, (int)x - startx + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (endx == next.Value.x) {
                                        end_dy = Math.Min(end_dy, endx - (int)x + 1);
                                    } else {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (next_k >= 1 || next_k < -1 || skipKCheck)
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
                                        setTile((int)x, h, y + dy, id, data, 0, null);
                                    else if (d == -1)
                                        setTile((int)x, h, y + dy, id, data, 0, true);
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
                                            setTile((int)x, h, y + dy, id, data, 0, null);
                                        else if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 0, true);
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
                                            setTile((int)x, h, y + dy, id, data, 180, true);
                                        else if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 180, null);
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
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (last_k >= 1 || last_k < -1 || skipKCheck)
                                            end_dy = Math.Min(end_dy, startx - (int)x + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (next.Value.x == endx) {
                                        end_dy = Math.Min(end_dy, (int)x - endx + 1);
                                    } else {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (next_k >= 1 || next_k < -1 || skipKCheck)
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
                                        setTile((int)x, h, y + dy, id, data, 180, true);
                                    else if (d == -1)
                                        setTile((int)x, h, y + dy, id, data, 180, null);
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
                                            setTile((int)x, h, y + dy, id, data, 180, true);
                                        else if (d == -1)
                                            setTile((int)x, h, y + dy, id, data, 180, null);
                                    }
                                }
                                Cnt[clevel]++;
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
                if (!(last is null) && -cos_th2 < cos1 && cos1 < cos_th2) {
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
                bool reflex_next = false;
                if (!(next is null) && -cos_th2 < cos2 && cos2 < cos_th2) {
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
                                            setTile(x + dx, h, (int)y, id, data, 90, null);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false);
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
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (-1 < last_k && last_k <= 1 || skipKCheck)
                                            end_dx = Math.Min(end_dx, (int)y - starty + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (endx != next.Value.x) {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (-1 < next_k && next_k <= 1 || skipKCheck)
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
                                        setTile(x + dx, h, (int)y, id, data, 90, null);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false);
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
                                            setTile(x + dx, h, (int)y, id, data, 90, null);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 90, false);
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
                                            setTile(x + dx, h, (int)y, id, data, 270, false);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null);
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
                                        double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                        if (-1 < last_k && last_k <= 1 || skipKCheck)
                                            end_dx = Math.Min(end_dx, starty - (int)y + 1);
                                    }
                                }
                            }
                            if (!(next is null) && !reflex_next) {
                                if (-cos_th < cos2 && cos2 < cos_th) {
                                    if (endx != next.Value.x) {
                                        double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                        if (-1 < next_k && next_k <= 1 || skipKCheck)
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
                                        setTile(x + dx, h, (int)y, id, data, 270, false);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null);
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
                                            setTile(x + dx, h, (int)y, id, data, 270, false);
                                        else if (d == 1)
                                            setTile(x + dx, h, (int)y, id, data, 270, null);
                                    }
                                }
                                Cnt[clevel]++;
                            }
                        }
                    }
                }
            }
        } else {
            //print("Not implemented yet.");
            //setTile(startx, 40, starty, 133, 0);
            //setTile(endx, 40, endy, 133, 0);
            bool reflex_last = false;
            Vector2 vec1 = new Vector2(startx - last.Value.x, starty - last.Value.y);
            Vector2 vec2 = new Vector2(endx - startx, endy - starty);
            Vector2 vec3 = new Vector2(next.Value.x - endx, next.Value.y - endy);
            float cos1 = Vector2.Dot(vec1, vec2) / (vec1.Length() * vec2.Length());
            float cos2 = Vector2.Dot(vec2, vec3) / (vec2.Length() * vec3.Length());
            if (!(last is null) && -cos_th2 < cos1 && cos1 < cos_th2) {
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
            bool reflex_next = false;
            if (!(next is null) && -cos_th2 < cos2 && cos2 < cos_th2) {
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
                                        setTile(x + dx, h, (int)y, id, data, 90, null);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false);
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
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (-1 < last_k && last_k <= 1 || skipKCheck)
                                        end_dx = Math.Min(end_dx, (int)y - starty + 1);
                                }
                            }
                        }
                        if (!(next is null) && !reflex_next) {
                            if (-cos_th < cos2 && cos2 < cos_th) {
                                if (endx != next.Value.x) {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (-1 < next_k && next_k <= 1 || skipKCheck)
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
                                    setTile(x + dx, h, (int)y, id, data, 90, null);
                                else if (d == 1)
                                    setTile(x + dx, h, (int)y, id, data, 90, false);
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
                                        setTile(x + dx, h, (int)y, id, data, 90, null);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 90, false);
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
                                        setTile(x + dx, h, (int)y, id, data, 270, false);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null);
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
                                    double last_k = (double)(starty - last.Value.y) / (double)(startx - last.Value.x);
                                    if (-1 < last_k && last_k <= 1 || skipKCheck)
                                        end_dx = Math.Min(end_dx, starty - (int)y + 1);
                                }
                            }
                        }
                        if (!(next is null) && !reflex_next) {
                            if (-cos_th < cos2 && cos2 < cos_th) {
                                if (endx != next.Value.x) {
                                    double next_k = (double)(next.Value.y - endy) / (double)(next.Value.x - endx);
                                    if (-1 < next_k && next_k <= 1 || skipKCheck)
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
                                    setTile(x + dx, h, (int)y, id, data, 270, false);
                                else if (d == 1)
                                    setTile(x + dx, h, (int)y, id, data, 270, null);
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
                                        setTile(x + dx, h, (int)y, id, data, 270, false);
                                    else if (d == 1)
                                        setTile(x + dx, h, (int)y, id, data, 270, null);
                                }
                            }
                            Cnt[clevel]++;
                        }
                    }
                }
            }
            /* 旧版DrawLine_Interior相关代码
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

            if (starty <= endy) {
                for (float y = starty; y < endy; y++) {
                    int x = startx;
                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                        int h = _h + sh;
                        for (int _dx = GetValidWidth(x, (int)y); _dx > 0; _dx--) {
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
                                setTile(x + dx, h, (int)y, id, data, 90, null);
                            else if (d == 1)
                                setTile(x + dx, h, (int)y, id, data, 90, false);
                        }
                    }
                    Cnt[clevel]++;
                }
            } else {
                for (float y = starty; y > endy; y--) {
                    int x = startx;
                    for (int _h = 0; _h < InteriorConfig.GetHeight(); _h++) {
                        int h = _h + sh;
                        for (int _dx = GetValidWidth(x, (int)y); _dx > 0; _dx--) {
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
                                setTile(x + dx, h, (int)y, id, data, 270, false);
                            else if (d == 1)
                                setTile(x + dx, h, (int)y, id, data, 270, null);
                        }
                    }
                    Cnt[clevel]++;
                }
            }
            */
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

    void DrawLine_Debug(int x0, int y0, int x1, int y1, int h, int id, int data, int brush_radius = 0) {
        if (x0 != x1) {
            double k = (double)(y1 - y0) / (double)(x1 - x0);
            double b = (double)(y1 * x0 - y0 * x1) / (double)(x0 - x1);
            if (-1 <= k && k <= 1) {
                brush_radius = (int)((double)brush_radius / Math.Sqrt(1 + k * k));
                for (float x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++) {
                    int y = (int)(k * x + b);
                    if (x == Math.Min(x0, x1) || x == Math.Max(x0, x1)) {
                        setTile((int)x, h + 1, y, 133, 0);
                    } else {
                        if (brush_radius == 0) {
                            setTile((int)x, h, y, id, data);
                        } else {
                            if (x == Math.Min(x0, x1) || x == Math.Max(x0, x1)) {
                                DrawCircle((int)x, h, y, brush_radius, id, data);
                            }
                            for (int dy = -brush_radius; dy <= brush_radius; dy++) {
                                setTile((int)x, h, y + dy, id, data);
                            }
                        }
                    }
                }
            } else {
                brush_radius = (int)((double)brush_radius * k / Math.Sqrt(1 + k * k));
                for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                    int x = (int)((float)(y - b) / k);
                    if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) {
                        setTile(x, h + 1, (int)y, 133, 0);
                    } else {
                        if (brush_radius == 0) {
                            setTile(x, h, (int)y, id, data);
                        } else {
                            if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) {
                                DrawCircle(x, h, (int)y, brush_radius, id, data);
                            }
                            for (int dx = -brush_radius; dx <= brush_radius; dx++) {
                                setTile(x + dx, h, (int)y, id, data);
                            }
                        }
                    }
                }
            }
        } else {
            for (float y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++) {
                if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) {
                    setTile(x0, h + 1, (int)y, 133, 0);
                } else {
                    if (brush_radius == 0)
                        setTile(x0, h, (int)y, id, data);
                    else {
                        if (y == Math.Min(y0, y1) || y == Math.Max(y0, y1)) {
                            DrawCircle(x0, h, (int)y, brush_radius, id, data);
                        }
                        for (int dx = -brush_radius; dx <= brush_radius; dx++)
                            setTile(x0 + dx, h, (int)y, id, data);
                    }
                }
            }
        }
    }

    void DrawCircle(int cx, int cy, int cz, int r, int id, int data) {
        for (int x = -r + 1; x <= r - 1; x++) {
            for (int z = -r + 1; z <= r - 1; z++) {
                if (x * x + z * z < (r - 1) * (r - 1)) {
                    setTile(cx + x, cy, cz + z, id, data);
                }
            }
        }
    }

    void DrawCircle_Quarter(int cx, int cy, int cz, int r, int dx, int dz, Block block) {
        int rx = r * dx;
        int rz = r * dz;
        rz = -rz;
        for (int x = Math.Min(0, rx); x <= Math.Max(0, rx); x++) {
            for (int z = Math.Min(0, rz); z <= Math.Max(0, rz); z++) {
                if (x * x + z * z <= r * r && !(x == 0 && z == 0)) {
                    setTile(cx + x, cy, cz + z, block.id, block.data);
                }
            }
        }
    }

    void DrawCircle_Quarter(int cx, int cy, int cz, int r, int dx, int dz, Block block, int fh) {
        int rx = r * dx;
        int rz = r * dz;
        rz = -rz;
        for (int x = Math.Min(0, rx); x <= Math.Max(0, rx); x++) {
            for (int z = Math.Min(0, rz); z <= Math.Max(0, rz); z++) {
                if (x * x + z * z <= r * r && !(x == 0 && z == 0)) {
                    setTile(cx + x, cy, cz + z, block.id, block.data);
                    for (int h = cy + 1; h <= cy + fh; h++) {
                        Block cup = getTile(cx + x, h, cz + z);
                        if (!IsRoadBlock(cup))
                            setTile(cx + x, h, cz + z, 0, 0);
                    }
                }
            }
        }
    }

    void flat_height(int x, int y, int sh, int dh) {
        for (int h = sh + 1; h <= sh + dh; h++)
            setTile(x, h, y, 0, 0);
    }

    bool IsSmallBuilding(List<Node> nodes) {
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

    bool IsNumberic(string s) {
        try {
            double result = Convert.ToDouble(s);
            return true;
        }
        catch {
            return false;
        }
    }

    bool IsStair(int id) {
        int[] stairs = { 53, 134, 135, 136, 163, 164, 108, 109, 114, 128, 180, 156, 203 };
        foreach (var v in stairs) {
            if (id == v) return true;
        }
        return false;
    }

    bool IsFence(int id) {
        int[] fences = { 85, 113 };
        foreach (var v in fences) {
            if (id == v) return true;
        }
        return false;
    }

    bool IsRoadBlock(Block bottom) {
        if (!bottom.Equals(new Block(159, 9)) && !bottom.Equals(new Block(236, 0)) && !bottom.Equals(new Block(159, 4)))
            return false;
        else
            return true;
	}

    bool IsUndefined(Node node) {
        if (node.x == undefined && node.y == undefined) return true;
        else return false;
    }

    void ClearCnt() {
        for (int i = 0; i < MAX_LEVEL; i++)
            Cnt[i] = 0;
    }

    string getFileName() {
#if UNITY_EDITOR
        //return "samples/osm_simp.txt";
        //return "samples/osm_cmplx.txt";
        //return "samples/osm_cmplx2.txt";
        //return "samples/osm_cmplx3.txt";
        //return "samples/osm_norm.txt";
        //return "samples/osm_norm2.txt";
        //return "samples/osm_simp2.txt";
        //return "samples/osm_way.txt";
        //return "samples/osm_cmplx4.txt";
        //return "samples/osm_park.txt";
        //return "samples/kiosque.txt";
        //return "samples/osm_river.txt";
        //return "samples/osm_river2.txt";
        //return "samples/osm_stadium.txt";
        return "samples/osm.txt";
#else
        return "osm.txt";
#endif
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

    void setTile(float x, float y, float z, int id, int data, int rot = 0, bool? Flip_Xaxis = null, bool doNotReplace = false) {
#if UNITY_EDITOR
        const int max_blocknum = 700000;
        if (block_cnt > max_blocknum) {
            if (!reach_maxblocknum) {
                print("INFO: Maximum block num (" + max_blocknum + ") reached. Ignoring subsequent blocks.");
                reach_maxblocknum = true;
            }
            return;
        }
#else
        const int max_blocknum2 = 8000000;
        if (block_cnt > max_blocknum2) {
            if (!reach_maxblocknum) {
                print("INFO: Maximum block num (" + max_blocknum2 + ") reached. Ignoring subsequent blocks.");
                reach_maxblocknum = true;
            }
            return;
        }
#endif
        if (doNotReplace) {
            Block _block;
            bool exist = block_list.TryGetValue(new Vector3(x, y, z), out _block);
            if (exist) return;
        }
        GameObject gameObject;
        if (use_high_ppi) {
            x *= ratio;
            y *= ratio;
            z *= ratio;
        }
        if (IsStair(id)) {
            gameObject = Instantiate(GameObject.Find("stair"));
            gameObject.transform.localScale = new Vector3(40, 40, 40);
            gameObject.AddComponent<MeshRenderer>();
            gameObject.transform.position = new Vector3(x, y, z);
        } else if (id == 145) {
            gameObject = Instantiate(GameObject.Find("anvil"));
            gameObject.AddComponent<MeshRenderer>();
            gameObject.transform.position = new Vector3(x, y - 0.5f, z);
        } else if (IsFence(id)) {
            gameObject = Instantiate(GameObject.Find("fence"));
            gameObject.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            gameObject.AddComponent<MeshRenderer>();
            gameObject.transform.position = new Vector3(x, y - 0.5f, z);
        } else if (id == 117) {
            gameObject = Instantiate(GameObject.Find("juicer"));
            gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            gameObject.AddComponent<MeshRenderer>();
            gameObject.transform.position = new Vector3(x, y, z);
        } else if (id == 463) {
            gameObject = Instantiate(GameObject.Find("lantern"));
            gameObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            gameObject.AddComponent<MeshRenderer>();
            gameObject.transform.position = new Vector3(x, y - .5f, z);
        } else if (id == 140) {
            gameObject = Instantiate(GameObject.Find("flower_pot"));
            gameObject.transform.localScale = new Vector3(.5f, .5f, .5f);
            gameObject.AddComponent<MeshRenderer>();
            gameObject.transform.position = new Vector3(x, y - .5f, z);
        } else {
            //gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //gameObject.transform.position = new Vector3(x, y, z);
            gameObject = Instantiate(GameObject.Find("Cube"), new Vector3(x, y, z), new Quaternion(0, 0, 0, 0));
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        if (rot != 0) data = RotateData(id, data, rot);
        if (Flip_Xaxis != null) data = FlipData(id, data, Flip_Xaxis.Value);

        if (IsStair(id)) {
            //应用旋转：Minecraft逆时针为正，unity顺时针为正
            switch (data) {
                case 5:
                    gameObject.transform.Rotate(Vector3.up * 180);
                    break;
                case 1:
                    gameObject.transform.Rotate(Vector3.up * 90);
                    break;
                case 4:
                    gameObject.transform.Rotate(Vector3.up * 270);
                    break;
                case 3:
                    gameObject.transform.Rotate(0, 0, 270);
                    break;
                case 6:
                    gameObject.transform.Rotate(0, 0, 270);
                    gameObject.transform.Rotate(Vector3.up * 180);
                    break;
                case 2:
                    gameObject.transform.Rotate(0, 0, 270);
                    gameObject.transform.Rotate(Vector3.up * 90);
                    break;
                case 7:
                    gameObject.transform.Rotate(0, 0, 270);
                    gameObject.transform.Rotate(Vector3.up * 270);
                    break;
            }
        }

        if (!use_texture) return;
        string tn = "";
        bool transparent = false, emission_y = false, emission_b = false;
        switch (id) {
            case 0:
                GameObject deGameObject;
                bool succeed = gameObject_list.TryGetValue(new Vector3(x, y, z), out deGameObject);
                if (succeed) {
                    Destroy(deGameObject);
                    gameObject_list.Remove(new Vector3(x, y, z));
                    block_list.Remove(new Vector3(x, y, z));
                }
                Destroy(gameObject);
                return;
            case 1:
                switch (data) {
                    case 0:
                        tn = "stone"; break;
                    case 1:
                        tn = "stone_granite"; break;
                    case 2:
                        tn = "stone_granite_smooth"; break;
                    case 3:
                        tn = "stone_diorite"; break;
                    case 4:
                        tn = "stone_diorite_smooth"; break;
                    case 5:
                        tn = "stone_andesite"; break;
                    case 6:
                        tn = "stone_andesite_smooth"; break;
                }
                break;
            case 2:
                tn = "grass_carried";
                break;
            case 3:
                tn = "dirt"; break;
            case 4:
                tn = "cobblestone"; break;
            case 5:
            case 85:
            case 157:
                switch (data) {
                    case 0:
                        tn = "planks_oak"; break;
                    case 1:
                        tn = "planks_spruce"; break;
                    case 2:
                        tn = "planks_birch"; break;
                    case 3:
                        tn = "planks_jungle"; break;
                    case 4:
                        tn = "planks_acacia"; break;
                    case 5:
                        tn = "planks_big_oak"; break;
                }
                break;
            case 6:
                tn = "sapling_oak";
                transparent = true;
                break;
            case 7:
                tn = "bedrock"; break;
            case 8:
                tn = "water_placeholder";
                break;
            case 9:
                tn = "water_placeholder";
                break;
            case 10:
                tn = "lava_placeholder"; break;
            case 11:
                tn = "lava_placeholder"; break;
            case 12:
                switch (data) {
                    case 0:
                        tn = "sand"; break;
                    case 1:
                        tn = "red_sand"; break;
                }
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
                switch (data) {
                    case 0:
                        tn = "log_oak"; break;
                    case 1:
                        tn = "log_spruce"; break;
                    case 2:
                        tn = "log_birch"; break;
                }
                break;
            case 18:
                tn = "leaves_oak_opaque";
                transparent = true;
                break;
            case 19:
                tn = "sponge"; break;
            case 20:
                tn = "glass";
                transparent = true;
                break;
            case 21:
                tn = "lapis_ore";
                break;
            case 22:
                tn = "lapis_block";
                break;
            case 23:
                tn = "dispenser_front_horizontal";
                break;
            case 24:
                switch (data) {
                    case 0:
                        tn = "sandstone_normal"; break;
                    case 1:
                        tn = "sandstone_carved"; break;
                    case 2:
                        tn = "sandstone_smooth"; break;
                    case 3:
                        tn = "sandstone_top"; break;
                }
                break;
            case 25:
                tn = "noteblock"; break;
            case 26:
                tn = "bed_head_top";
                transparent = true;
                break;
            case 27:
                tn = "rail_golden_powered";
                transparent = true;
                break;
            case 28:
                tn = "rail_detector";
                transparent = true;
                break;
            case 29:
                tn = "piston_top_sticky"; break;
            case 30:
                tn = "web";
                transparent = true;
                break;
            case 31:
                tn = "double_plant_grass_carried";
                transparent = true;
                break;
            case 32:
                tn = "deadbush";
                transparent = true;
                break;
            case 33:
                tn = "piston_top_normal";
                break;
            case 34:
                tn = "piston_top_normal";
                break;
            case 35:
                switch (data) {
                    case 0:
                        tn = "wool_colored_white"; break;
                    case 1:
                        tn = "wool_colored_orange"; break;
                    case 2:
                        tn = "wool_colored_magenta"; break;
                    case 3:
                        tn = "wool_colored_light_blue"; break;
                    case 4:
                        tn = "wool_colored_yellow"; break;
                    case 5:
                        tn = "wool_colored_lime"; break;
                    case 6:
                        tn = "wool_colored_pink"; break;
                    case 7:
                        tn = "wool_colored_gray"; break;
                    case 8:
                        tn = "wool_colored_silver";
                        break;
                    case 9:
                        tn = "wool_colored_cyan";
                        break;
                    case 10:
                        tn = "wool_colored_purple";
                        break;
                    case 11:
                        tn = "wool_colored_blue";
                        break;
                    case 12:
                        tn = "wool_colored_brown";
                        break;
                    case 13:
                        tn = "wool_colored_green";
                        break;
                    case 14:
                        tn = "wool_colored_red";
                        break;
                    case 15:
                        tn = "wool_colored_black";
                        break;
                }
                break;
            case 37:
                tn = "flower_dandelion";
                transparent = true;
                break;
            case 38:
                tn = "flower_rose";
                transparent = true;
                break;
            case 39:
                tn = "mushroom_brown";
                transparent = true;
                break;
            case 40:
                tn = "mushroom_red";
                transparent = true;
                break;
            case 41:
                tn = "gold_block";
                break;
            case 42:
                tn = "iron_block";
                break;
            case 43:
                tn = "stone_slab_top";
                break;
            case 44:
                gameObject.transform.localScale = new Vector3(1f, 0.5f, 1f);
                switch (data) {
                    case 0:
                    case 8:
                        tn = "stone_slab_top";
                        break;
                    case 1:
                    case 9:
                        tn = "sandstone_normal";
                        break;
                    case 2:
                    case 10:
                        tn = "planks_oak";
                        break;
                    case 3:
                    case 11:
                        tn = "cobblestone";
                        break;
                    case 4:
                    case 12:
                        tn = "brick";
                        break;
                    case 5:
                    case 13:
                        tn = "stonebrick";
                        break;
                    case 6:
                    case 14:
                        tn = "quartz_block_side";
                        break;
                    case 7:
                    case 15:
                        tn = "nether_brick";
                        break;
                }
                if (data >= 8) gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
                gameObject.transform.position -= new Vector3(0f, 0.25f, 0f);
                break;
            case 45:
                tn = "brick";
                break;
            case 46:
                tn = "tnt_side";
                break;
            case 47:
                tn = "bookshelf";
                break;
            case 48:
                tn = "cobblestone_mossy";
                break;
            case 49:
                tn = "obsidian";
                break;
            case 50:
                tn = "torch_on";
                transparent = true;
                emission_y = true;
                break;
            case 51:
                tn = "fire_0_placeholder";
                transparent = true;
                break;
            case 52:
                tn = "mob_spawner";
                transparent = true;
                break;
            case 53:
                tn = "planks_oak";
                break;
            case 54:
                tn = "chest_front";
                break;
            case 55:
                tn = "redstone_dust_cross";
                transparent = true;
                break;
            case 56:
                tn = "diamond_ore";
                break;
            case 57:
                tn = "diamond_block";
                break;
            case 58:
                tn = "crafting_table_side";
                break;
            case 59:
                tn = "wheat_stage_7";
                break;
            case 60:
                tn = "farmland_wet";
                break;
            case 61:
                tn = "furnace_front_off";
                break;
            case 62:
                tn = "furnace_front_on";
                break;
            case 63:
                tn = "sign";
                transparent = true;
                break;
            case 64:
                tn = "door_wood_upper";
                transparent = true;
                break;
            case 65:
                tn = "ladder";
                transparent = true;
                break;
            case 66:
                tn = "rail_normal";
                transparent = true;
                break;
            case 67:
                tn = "stair";
                transparent = true;
                break;
            case 68:
                tn = "sign";
                transparent = true;
                break;
            case 69:
                tn = "lever";
                transparent = true;
                break;
            case 70:
                tn = "stone";
                break;
            case 71:
                tn = "door_iron_upper";
                transparent = true;
                break;
            case 72:
                tn = "planks_oak";
                gameObject.transform.localScale = new Vector3(1, 0.05f, 1);
                gameObject.transform.position -= new Vector3(0, 0.45f, 0);
                break;
            case 73:
                tn = "redstone_ore";
                break;
            case 74:
                tn = "redstone_ore";
                break;
            case 75:
                tn = "redstone_torch_off";
                transparent = true;
                break;
            case 76:
                tn = "redstone_torch_on";
                transparent = true;
                break;
            case 77:
                tn = "button";
                transparent = true;
                break;
            case 78:
                tn = "snow";
                break;
            case 79:
                tn = "ice";
                break;
            case 80:
                tn = "snow";
                break;
            case 81:
                tn = "cactus_side";
                break;
            case 82:
                tn = "clay";
                break;
            case 83:
                tn = "reeds";
                transparent = true;
                break;
            case 84:
                tn = "jukebox_side";
                break;
            case 86:
                tn = "pumpkin_face_off";
                break;
            case 87:
                tn = "netherrack";
                break;
            case 88:
                tn = "soul_sand";
                break;
            case 89:
                tn = "glowstone";
                emission_y = true;
                break;
            case 90:
                tn = "portal_placeholder";
                break;
            case 91:
                tn = "pumpkin_face_on";
                break;
            case 92:
                tn = "cake_top";
                break;
            case 93:
                tn = "repeater_off";
                break;
            case 94:
                tn = "repeater_on";
                break;
            case 96:
                tn = "trapdoor";
                transparent = true;
                break;
            case 97:
                tn = "stone";
                break;
            case 98:
                tn = "stonebrick";
                break;
            case 99:
                tn = "mushroom_block_skin_brown";
                break;
            case 100:
                tn = "mushroom_block_skin_red";
                break;
            case 101:
                tn = "iron_bars";
                transparent = true;
                break;
            case 102:
                tn = "glass";
                transparent = true;
                break;
            case 103:
                tn = "melon_top";
                break;
            case 104:
                tn = "pumpkin_stem_disconnected";
                transparent = true;
                break;
            case 105:
                tn = "melon_stem_disconnected";
                transparent = true;
                break;
            case 106:
                tn = "vine";
                transparent = true;
                break;
            case 107:
                tn = "fencegate";
                transparent = true;
                break;
            case 108:
                tn = "brick";
                break;
            case 109:
                tn = "stonebrick";
                break;
            case 110:
                tn = "mycelium_top";
                break;
            case 111:
                tn = "carried_waterlily";
                transparent = true;
                break;
            case 112:
            case 113:
                tn = "nether_brick";
                break;
            case 114:
                tn = "nether_brick";
                break;
            case 115:
                tn = "nether_wart_stage_2";
                transparent = true;
                break;
            case 116:
                tn = "enchanting_table_top";
                break;
            case 117:
                tn = "brewing_stand_base";
                transparent = true;
                break;
            case 118:
                tn = "cauldron_side";
                break;
            case 119:
                tn = "endframe_eye";
                break;
            case 120:
                tn = "endframe_top";
                break;
            case 121:
                tn = "end_stone";
                break;
            case 122:
                tn = "dragon_egg";
                transparent = true;
                break;
            case 123:
                tn = "redstone_lamp_off";
                break;
            case 124:
                tn = "redstone_lamp_on";
                emission_y = true;
                break;
            case 125:
                tn = "dropper_front_horizontal";
                break;
            case 126:
                tn = "rail_activator_powered";
                break;
            case 127:
                tn = "cocoa_stage_2";
                transparent = true;
                break;
            case 128:
                tn = "sandstone_normal";
                break;
            case 129:
                tn = "emerald_ore";
                break;
            case 130:
                tn = "ender_chest_front";
                break;
            case 131:
                tn = "trip_wire_source";
                transparent = true;
                break;
            case 132:
                tn = "trip_wire";
                transparent = true;
                break;
            case 133:
                tn = "emerald_block";
                break;
            case 134:
                tn = "planks_spruce";
                break;
            case 135:
                tn = "planks_birch";
                break;
            case 136:
                tn = "planks_jungle";
                break;
            case 137:
                tn = "command_block";
                break;
            case 138:
                tn = "beacon";
                break;
            case 139:
                tn = "cobblestone";
                break;
            case 140:
                tn = "flower_pot";
                break;
            case 141:
                tn = "carrots_stage_3";
                transparent = true;
                break;
            case 142:
                tn = "potatoes_stage_3";
                transparent = true;
                break;
            case 145:
                tn = "anvil_base";
                break;
            case 146:
                tn = "chest_front";
                break;
            case 147:
                tn = "gold_block";
                break;
            case 148:
                tn = "iron_block";
                break;
            case 149:
                tn = "comparator_off";
                break;
            case 150:
                tn = "comparator_on";
                break;
            case 151:
                tn = "daylight_detector_top";
                break;
            case 152:
                tn = "redstone_block";
                break;
            case 153:
                tn = "quartz_ore";
                break;
            case 154:
                tn = "hopper_outside";
                break;
            case 155:
                switch (data) {
                    case 0:
                        tn = "quartz_block_side";
                        break;
                    case 1:
                        tn = "quartz_block_chiseled";
                        break;
                    case 2:
                        tn = "quartz_block_lines";
                        break;
                    case 6:
                        tn = "quartz_block_lines";
                        gameObject.transform.rotation = UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(5, 0, 0));
                        break;
                    case 10:
                        tn = "quartz_block_lines_top";
                        break;
                }
                break;
            case 156:
                tn = "quartz_block_side";
                break;
            case 158:
                gameObject.transform.localScale = new Vector3(1f, 0.5f, 1f);
                switch (data) {
                    case 0:
                    case 8:
                        tn = "planks_oak"; break;
                    case 1:
                    case 9:
                        tn = "planks_spruce"; break;
                    case 2:
                    case 10:
                        tn = "planks_birch"; break;
                    case 3:
                    case 11:
                        tn = "planks_jungle"; break;
                    case 4:
                    case 12:
                        tn = "planks_acacia"; break;
                    case 5:
                    case 13:
                        tn = "planks_big_oak"; break;
                }
                if (data >= 8) gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
                gameObject.transform.position -= new Vector3(0f, 0.25f, 0f);
                break;
            case 159:
                switch (data) {
                    case 0:
                        tn = "hardened_clay_stained_white"; break;
                    case 1:
                        tn = "hardened_clay_stained_orange"; break;
                    case 2:
                        tn = "hardened_clay_stained_magenta"; break;
                    case 3:
                        tn = "hardened_clay_stained_light_blue"; break;
                    case 4:
                        tn = "hardened_clay_stained_yellow"; break;
                    case 5:
                        tn = "hardened_clay_stained_lime"; break;
                    case 6:
                        tn = "hardened_clay_stained_pink"; break;
                    case 7:
                        tn = "hardened_clay_stained_gray"; break;
                    case 8:
                        tn = "hardened_clay_stained_silver";
                        break;
                    case 9:
                        tn = "hardened_clay_stained_cyan";
                        break;
                    case 10:
                        tn = "hardened_clay_stained_purple";
                        break;
                    case 11:
                        tn = "hardened_clay_stained_blue";
                        break;
                    case 12:
                        tn = "hardened_clay_stained_brown";
                        break;
                    case 13:
                        tn = "hardened_clay_stained_green";
                        break;
                    case 14:
                        tn = "hardened_clay_stained_red";
                        break;
                    case 15:
                        tn = "hardened_clay_stained_black";
                        break;
                }
                break;
            case 160:
                switch (data) {
                    case 0:
                        tn = "glass_white"; break;
                    case 1:
                        tn = "glass_orange"; break;
                    case 2:
                        tn = "glass_magenta"; break;
                    case 3:
                        tn = "glass_light_blue"; break;
                    case 4:
                        tn = "glass_yellow"; break;
                    case 5:
                        tn = "glass_lime"; break;
                    case 6:
                        tn = "glass_pink"; break;
                    case 7:
                        tn = "glass_gray"; break;
                    case 8:
                        tn = "glass_silver";
                        break;
                    case 9:
                        tn = "glass_cyan";
                        break;
                    case 10:
                        tn = "glass_purple";
                        break;
                    case 11:
                        tn = "glass_blue";
                        break;
                    case 12:
                        tn = "glass_brown";
                        break;
                    case 13:
                        tn = "glass_green";
                        break;
                    case 14:
                        tn = "glass_red";
                        break;
                    case 15:
                        tn = "glass_black";
                        break;
                }
                transparent = true;
                break;
            case 161:
                tn = "leaves_oak_opaque";
                transparent = true;
                break;
            case 162:
                switch (data) {
                    case 0:
                        tn = "log_acacia"; break;
                    case 1:
                        tn = "log_big_oak"; break;
                }
                break;
            case 163:
                tn = "planks_acacia";
                transparent = true;
                break;
            case 164:
                tn = "planks_big_oak";
                transparent = true;
                break;
            case 165:
                tn = "slime";
                break;
            case 167:
                tn = "iron_trapdoor";
                transparent = true;
                break;
            case 168:
                tn = "prismarine_rough";
                break;
            case 169:
                tn = "sea_lantern";
                emission_b = true;
                break;
            case 170:
                tn = "hay_block_side";
                break;
            case 171:
                gameObject.transform.localScale = new Vector3(1, 0.05f, 1);
                gameObject.transform.position -= new Vector3(0, 0.45f, 0);
                switch (data) {
                    case 0:
                        tn = "wool_colored_white"; break;
                    case 1:
                        tn = "wool_colored_orange"; break;
                    case 2:
                        tn = "wool_colored_magenta"; break;
                    case 3:
                        tn = "wool_colored_light_blue"; break;
                    case 4:
                        tn = "wool_colored_yellow"; break;
                    case 5:
                        tn = "wool_colored_lime"; break;
                    case 6:
                        tn = "wool_colored_pink"; break;
                    case 7:
                        tn = "wool_colored_gray"; break;
                    case 8:
                        tn = "wool_colored_silver";
                        break;
                    case 9:
                        tn = "wool_colored_cyan";
                        break;
                    case 10:
                        tn = "wool_colored_purple";
                        break;
                    case 11:
                        tn = "wool_colored_blue";
                        break;
                    case 12:
                        tn = "wool_colored_brown";
                        break;
                    case 13:
                        tn = "wool_colored_green";
                        break;
                    case 14:
                        tn = "wool_colored_red";
                        break;
                    case 15:
                        tn = "wool_colored_black";
                        break;
                }
                break;
            case 172:
                tn = "hardened_clay";
                break;
            case 173:
                tn = "coal_block";
                break;
            case 174:
                tn = "ice_packed";
                break;
            case 175:
                tn = "double_plant_paeonia_top";
                transparent = true;
                break;
            case 176:
                tn = "banner";
                transparent = true;
                break;
            case 177:
                tn = "banner";
                transparent = true;
                break;
            case 178:
                tn = "daylight_detector_inverted_top";
                break;
            case 179:
                switch (data) {
                    case 0:
                        tn = "red_sandstone_normal"; break;
                    case 1:
                        tn = "red_sandstone_carved"; break;
                    case 2:
                        tn = "red_sandstone_smooth"; break;
                    case 3:
                        tn = "red_sandstone_top"; break;
                }
                break;
            case 180:
                tn = "red_sandstone_normal";
                break;
            case 181:
                tn = "red_sandstone_normal";
                break;
            case 182:
                tn = "red_sandstone_normal";
                break;
            case 183:
                tn = "fencegate";
                transparent = true;
                break;
            case 184:
                tn = "fencegate";
                transparent = true;
                break;
            case 185:
                tn = "fencegate";
                transparent = true;
                break;
            case 186:
                tn = "fencegate";
                transparent = true;
                break;
            case 187:
                tn = "fencegate";
                transparent = true;
                break;
            case 188:
                tn = "command_block";
                break;
            case 189:
                tn = "command_block";
                break;
            case 190:
                tn = "hard_glass";
                transparent = true;
                break;
            case 191:
                switch (data) {
                    case 0:
                        tn = "hard_glass_white"; break;
                    case 1:
                        tn = "hard_glass_orange"; break;
                    case 2:
                        tn = "hard_glass_magenta"; break;
                    case 3:
                        tn = "hard_glass_light_blue"; break;
                    case 4:
                        tn = "hard_glass_yellow"; break;
                    case 5:
                        tn = "hard_glass_lime"; break;
                    case 6:
                        tn = "hard_glass_pink"; break;
                    case 7:
                        tn = "hard_glass_gray"; break;
                    case 8:
                        tn = "hard_glass_silver";
                        break;
                    case 9:
                        tn = "hard_glass_cyan";
                        break;
                    case 10:
                        tn = "hard_glass_purple";
                        break;
                    case 11:
                        tn = "hard_glass_blue";
                        break;
                    case 12:
                        tn = "hard_glass_brown";
                        break;
                    case 13:
                        tn = "hard_glass_green";
                        break;
                    case 14:
                        tn = "hard_glass_red";
                        break;
                    case 15:
                        tn = "hard_glass_black";
                        break;
                }
                transparent = true;
                break;
            case 193:
                tn = "door_spruce_lower";
                transparent = true;
                break;
            case 194:
                tn = "door_birch_upper";
                transparent = true;
                break;
            case 195:
                tn = "door_jungle_lower";
                transparent = true;
                break;
            case 196:
                tn = "door_acacia_upper";
                transparent = true;
                break;
            case 197:
                tn = "door_dark_oak_upper";
                transparent = true;
                break;
            case 198:
                tn = "grass_path_top";
                break;
            case 199:
                tn = "itemframe_background";
                break;
            case 200:
                tn = "chorus_flower";
                break;
            case 201:
                switch (data) {
                    case 0:
                        tn = "purpur_block";
                        break;
                    case 1:
                        tn = "purpur_pillar";
                        break;
                }
                break;
            case 203:
                tn = "purpur_block";
                break;
            case 205:
                tn = "shulker_top_undyed";
                break;
            case 206:
                tn = "end_bricks";
                break;
            case 207:
                tn = "frosted_ice_0";
                transparent = true;
                break;
            case 208:
                tn = "end_rod";
                break;
            case 209:
                tn = "end_portal";
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
                tn = "bone_block_top";
                break;
            case 218:
                switch (data) {
                    case 0:
                        tn = "shulker_top_white"; break;
                    case 1:
                        tn = "shulker_top_orange"; break;
                    case 2:
                        tn = "shulker_top_magenta"; break;
                    case 3:
                        tn = "shulker_top_light_blue"; break;
                    case 4:
                        tn = "shulker_top_yellow"; break;
                    case 5:
                        tn = "shulker_top_lime"; break;
                    case 6:
                        tn = "shulker_top_pink"; break;
                    case 7:
                        tn = "shulker_top_gray"; break;
                    case 8:
                        tn = "shulker_top_silver";
                        break;
                    case 9:
                        tn = "shulker_top_cyan";
                        break;
                    case 10:
                        tn = "shulker_top_purple";
                        break;
                    case 11:
                        tn = "shulker_top_blue";
                        break;
                    case 12:
                        tn = "shulker_top_brown";
                        break;
                    case 13:
                        tn = "shulker_top_green";
                        break;
                    case 14:
                        tn = "shulker_top_red";
                        break;
                    case 15:
                        tn = "shulker_top_black";
                        break;
                }
                break;
            case 220:
                tn = "glazed_terracotta_white";
                break;
            case 221:
                tn = "glazed_terracotta_orange";
                break;
            case 222:
                tn = "glazed_terracotta_magenta";
                break;
            case 223:
                tn = "glazed_terracotta_light_blue";
                break;
            case 224:
                tn = "glazed_terracotta_yellow";
                break;
            case 225:
                tn = "glazed_terracotta_lime";
                break;
            case 226:
                tn = "glazed_terracotta_pink";
                break;
            case 227:
                tn = "glazed_terracotta_gray";
                break;
            case 228:
                tn = "glazed_terracotta_silver";
                break;
            case 229:
                tn = "glazed_terracotta_cyan";
                break;
            case 219:
                tn = "glazed_terracotta_purple";
                break;
            case 231:
                tn = "glazed_terracotta_blue";
                break;
            case 232:
                tn = "glazed_terracotta_brown";
                break;
            case 233:
                tn = "glazed_terracotta_green";
                break;
            case 234:
                tn = "glazed_terracotta_red";
                break;
            case 235:
                tn = "glazed_terracotta_black";
                break;
            case 236:
                switch (data) {
                    case 0:
                        tn = "concrete_white"; break;
                    case 1:
                        tn = "concrete_orange"; break;
                    case 2:
                        tn = "concrete_magenta"; break;
                    case 3:
                        tn = "concrete_light_blue"; break;
                    case 4:
                        tn = "concrete_yellow"; break;
                    case 5:
                        tn = "concrete_lime"; break;
                    case 6:
                        tn = "concrete_pink"; break;
                    case 7:
                        tn = "concrete_gray"; break;
                    case 8:
                        tn = "concrete_silver";
                        break;
                    case 9:
                        tn = "concrete_cyan";
                        break;
                    case 10:
                        tn = "concrete_purple";
                        break;
                    case 11:
                        tn = "concrete_blue";
                        break;
                    case 12:
                        tn = "concrete_brown";
                        break;
                    case 13:
                        tn = "concrete_green";
                        break;
                    case 14:
                        tn = "concrete_red";
                        break;
                    case 15:
                        tn = "concrete_black";
                        break;
                }
                break;
            case 237:
                switch (data) {
                    case 0:
                        tn = "concrete_powder_white"; break;
                    case 1:
                        tn = "concrete_powder_orange"; break;
                    case 2:
                        tn = "concrete_powder_magenta"; break;
                    case 3:
                        tn = "concrete_powder_light_blue"; break;
                    case 4:
                        tn = "concrete_powder_yellow"; break;
                    case 5:
                        tn = "concrete_powder_lime"; break;
                    case 6:
                        tn = "concrete_powder_pink"; break;
                    case 7:
                        tn = "concrete_powder_gray"; break;
                    case 8:
                        tn = "concrete_powder_silver";
                        break;
                    case 9:
                        tn = "concrete_powder_cyan";
                        break;
                    case 10:
                        tn = "concrete_powder_purple";
                        break;
                    case 11:
                        tn = "concrete_powder_blue";
                        break;
                    case 12:
                        tn = "concrete_powder_brown";
                        break;
                    case 13:
                        tn = "concrete_powder_green";
                        break;
                    case 14:
                        tn = "concrete_powder_red";
                        break;
                    case 15:
                        tn = "concrete_powder_black";
                        break;
                }
                break;
            case 240:
                tn = "chorus_flower";
                break;
            case 241:
                switch (data) {
                    case 0:
                        tn = "glass_white"; break;
                    case 1:
                        tn = "glass_orange"; break;
                    case 2:
                        tn = "glass_magenta"; break;
                    case 3:
                        tn = "glass_light_blue"; break;
                    case 4:
                        tn = "glass_yellow"; break;
                    case 5:
                        tn = "glass_lime"; break;
                    case 6:
                        tn = "glass_pink"; break;
                    case 7:
                        tn = "glass_gray"; break;
                    case 8:
                        tn = "glass_silver";
                        break;
                    case 9:
                        tn = "glass_cyan";
                        break;
                    case 10:
                        tn = "glass_purple";
                        break;
                    case 11:
                        tn = "glass_blue";
                        break;
                    case 12:
                        tn = "glass_brown";
                        break;
                    case 13:
                        tn = "glass_green";
                        break;
                    case 14:
                        tn = "glass_red";
                        break;
                    case 15:
                        tn = "glass_black";
                        break;
                }
                transparent = true;
                break;
            case 245:
                tn = "stonecutter_side";
                break;
            case 246:
                tn = "glowing_obsidian";
                break;
            case 247:
                switch (data) {
                    case 0:
                        tn = "reactor_core_stage_0";
                        break;
                    case 1:
                        tn = "reactor_core_stage_1";
                        break;
                    case 2:
                        tn = "reactor_core_stage_2";
                        break;
                }
                break;
            case 251:
                tn = "observer_front";
                break;
            case 253:
                tn = "hard_glass";
                transparent = true;
                break;
            case 254:
                switch (data) {
                    case 0:
                        tn = "hard_glass_white"; break;
                    case 1:
                        tn = "hard_glass_orange"; break;
                    case 2:
                        tn = "hard_glass_magenta"; break;
                    case 3:
                        tn = "hard_glass_light_blue"; break;
                    case 4:
                        tn = "hard_glass_yellow"; break;
                    case 5:
                        tn = "hard_glass_lime"; break;
                    case 6:
                        tn = "hard_glass_pink"; break;
                    case 7:
                        tn = "hard_glass_gray"; break;
                    case 8:
                        tn = "hard_glass_silver";
                        break;
                    case 9:
                        tn = "hard_glass_cyan";
                        break;
                    case 10:
                        tn = "hard_glass_purple";
                        break;
                    case 11:
                        tn = "hard_glass_blue";
                        break;
                    case 12:
                        tn = "hard_glass_brown";
                        break;
                    case 13:
                        tn = "hard_glass_green";
                        break;
                    case 14:
                        tn = "hard_glass_red";
                        break;
                    case 15:
                        tn = "hard_glass_black";
                        break;
                }
                transparent = true;
                break;
            case hidden:
                gameObject.SetActive(false);
                return;
            case 257:
                tn = "stair";
                transparent = true;
                break;
            case 258:
                tn = "stair";
                transparent = true;
                break;
            case 259:
                tn = "stair";
                transparent = true;
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
                gameObject.transform.localScale = new Vector3(1f, 0.5f, 1f);
                switch (data) {
                    case 6:
                        tn = "stone_granite"; break;
                    case 7:
                        tn = "stone_granite_smooth"; break;
                    case 4:
                        tn = "stone_diorite"; break;
                    case 5:
                        tn = "stone_diorite_smooth"; break;
                    case 3:
                        tn = "stone_andesite"; break;
                    case 2:
                        tn = "stone_andesite_smooth"; break;
                }
                if (data >= 8) gameObject.transform.position += new Vector3(0f, 0.5f, 0f);
                gameObject.transform.position -= new Vector3(0f, 0.25f, 0f);
                break;
            default:
                tn = "unknown";
                transparent = true;
                break;
        }

        if (use_high_ppi)
            gameObject.transform.localScale *= ratio;

        if (transparent) {
            Material mat = Resources.Load<Material>("transparent");
            gameObject.GetComponent<Renderer>().material = mat;
        }
        if (emission_y) {
            Material mat = Resources.Load<Material>("emission_y");
            gameObject.GetComponent<Renderer>().material = mat;
            gameObject.isStatic = true;
        } else if (emission_b) {
            Material mat = Resources.Load<Material>("emission_b");
            gameObject.GetComponent<Renderer>().material = mat;
            gameObject.isStatic = true;
        }
        if (IsStair(id) || IsFence(id) || id == 117 || id == 145 || id == 140) {
            MeshRenderer[] childMeshs = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var childMesh in childMeshs) {
                childMesh.material.mainTexture = (Texture)Resources.Load(tn);
            }
        } else if (id == 463) {
            MeshRenderer[] childMeshs = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var childMesh in childMeshs) {
                switch (childMesh.gameObject.name) {
                    case "ChamferBox001":
                        Material mat = Resources.Load<Material>("emission_lantern");
                        childMesh.gameObject.GetComponent<Renderer>().material = mat;
                        childMesh.material.mainTexture = (Texture)Resources.Load("lantern_side");
                        break;
                    case "Box001":
                        childMesh.material.mainTexture = (Texture)Resources.Load("lantern_top");
                        break;
                    case "Torus002":
                    case "Torus003":
                        childMesh.material.mainTexture = (Texture)Resources.Load("lantern_handle");
                        break;
                }
            }
        } else {
            gameObject.GetComponent<MeshRenderer>().material.mainTexture = (Texture)Resources.Load(tn);
            gameObject.isStatic = true;
        }

        gameObjects.Add(gameObject);

        try {
            block_list.Add(new Vector3(x, y, z), new Block(id, data));
        }
        catch (ArgumentException) {
            block_list.Remove(new Vector3(x, y, z));
            block_list.Add(new Vector3(x, y, z), new Block(id, data));
        }
        try {
            gameObject_list.Add(new Vector3(x, y, z), gameObject);
        }
        catch (ArgumentException) {
            GameObject deGameObject;
            bool existed = gameObject_list.TryGetValue(new Vector3(x, y, z), out deGameObject);
            if (existed) Destroy(deGameObject);
            gameObject_list.Remove(new Vector3(x, y, z));
            gameObject_list.Add(new Vector3(x, y, z), gameObject);
        }

#if !UNITY_EDITOR
        const int max_blocknum1 = 4000000;
        if (block_cnt > max_blocknum1) {
            gameObject.SetActive(false);
        }
#endif

        block_cnt++;
    }

    void setTile(int x, int y, int z, int id, int data, bool doNotReplace) {
        setTile(x, y, z, id, data, 0, null, doNotReplace);
	}

    Block getTile(int x, int y, int z) {
        Block ret;
        bool succeed = block_list.TryGetValue(new Vector3(x, y, z), out ret);
        if (succeed) return ret;
        else return new Block(0, 0);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        /* 设置可视度相关代码
        foreach(var gameObject in gameObjects) {
            float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
            if (distance > distanceThreshold) {
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            } else {
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                transform.LookAt(player.transform.position);
                transform.Rotate(rotateAxis);
            }
        }
        */
        if (Input.GetKeyDown(KeyCode.W)) {
            //move_status = "foreward";
            move_array[0] = true;
        }
        if (Input.GetKeyUp(KeyCode.W)) {
            //move_status = "stop";
            move_array[0] = false;
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            movespeed = 16;
        }
        if (Input.GetKeyUp(KeyCode.E)) {
            movespeed = 32;
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            movespeed = 96;
        }
        if (Input.GetKeyUp(KeyCode.F)) {
            movespeed = 32;
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            foreach (var gameObject in gameObjects) {
                try {
                    float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);
                    if (distance > distanceThreshold) {
                        //gameObject.GetComponent<MeshRenderer>().enabled = false;
                        gameObject.SetActive(false);
                    } else {
                        //gameObject.GetComponent<MeshRenderer>().enabled = true;
                        gameObject.SetActive(true);
                        //transform.LookAt(player.transform.position);
                        //transform.Rotate(rotateAxis);
                    }
                }
                catch { }
            }
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            //move_status = "backward";
            move_array[1] = true;
        }
        if (Input.GetKeyUp(KeyCode.S)) {
            //move_status = "stop";
            move_array[1] = false;
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            //move_status = "leftward";
            move_array[2] = true;
        }
        if (Input.GetKeyUp(KeyCode.A)) {
            //move_status = "stop";
            move_array[2] = false;
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            //move_status = "rightward";
            move_array[3] = true;
        }
        if (Input.GetKeyUp(KeyCode.D)) {
            //move_status = "stop";
            move_array[3] = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            mainCameraRotation = !mainCameraRotation;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            //move_status = "upward";
            move_array[4] = true;
        } else if (Input.GetKeyUp(KeyCode.Space)) {
            //move_status = "stop";
            move_array[4] = false;
        } else if (Input.GetKeyDown(KeyCode.LeftShift)) {
            //move_status = "downward";
            move_array[5] = true;
        } else if (Input.GetKeyUp(KeyCode.LeftShift)) {
            //move_status = "stop";
            move_array[5] = false;
        }
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift)) {
            move_array[0] = move_array[1] = move_array[2] = move_array[3] = move_array[4] = move_array[5] = false;
        }
        if (mainCameraRotation) {
            //旋转相机
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        Vector3 move_velocity = new Vector3(0, 0, 0);
        if (move_array[0])      //foreward
            move_velocity += new Vector3(movespeed * Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.y), 0, movespeed * Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.y));
        if (move_array[1])      //backward
            move_velocity += new Vector3(-movespeed * Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.y), 0, -movespeed * Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.y));
        if (move_array[2])      //leftward
            move_velocity += new Vector3(-movespeed * Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.y), 0, movespeed * Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.y));
        if (move_array[3])      //rightward
            move_velocity += new Vector3(movespeed * Mathf.Cos(Mathf.Deg2Rad * transform.localEulerAngles.y), 0, -movespeed * Mathf.Sin(Mathf.Deg2Rad * transform.localEulerAngles.y));
        if (move_array[4])      //upward
            move_velocity += new Vector3(0, movespeed, 0);
        if (move_array[5])      //downward
            move_velocity += new Vector3(0, -movespeed, 0);
        player.GetComponent<Rigidbody>().velocity = move_velocity;
    }

    void OnGUI() {
        string text;
        GUIStyle style = new GUIStyle();
        if (mainCameraRotation) {
            text = "视角旋转：开";
            style.normal.textColor = new Color(0, 1, 0);
        } else {
            text = "视角旋转：关";
            style.normal.textColor = new Color(1, 0, 0);
        }
        GUILayout.TextArea("当前方块数：" + block_cnt);
        GUILayout.TextArea(text, style);
    }

}
