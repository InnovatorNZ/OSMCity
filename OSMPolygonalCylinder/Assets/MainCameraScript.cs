using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraScript : MonoBehaviour {
    // Unity related variables
    private bool[] move_array;
    public GameObject player;
    private bool mainCameraRotation = true;
    private List<GameObject> gameObjects = new List<GameObject>();
    private int movespeed = 32;
    private readonly float sensitivityX = 4F;
    private readonly float sensitivityY = 4F;
    private readonly float minimumY = -90;
    private readonly float maximumY = 90;
    private float rotationY = 0F;
    private int distanceThreshold = 720;
    struct Node {
        int x, y;
	}

    // Start is called before the first frame update
    void Start() {
        move_array = new bool[6];
        player = GameObject.Find("Main Camera");
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gameObject.transform.position = new Vector3(5, 1, -1);
        CreateHollowClinder(new Vector3(4, 0, 3), new Vector3(4, 3, 3), 0.5f, 1f);
        CreatePolygon();
        CreateCubeMesh();
    }

    void CreatePolygon() {
        GameObject go = new GameObject();
        go.AddComponent<MeshFilter>();
        Vector3[] Vertexes = new Vector3[] { Vector3.zero, new Vector3(-1, 1, 0), new Vector3(1, 2, 0), new Vector3(2, -1, 0) };
        Mesh _mesh = new Mesh();
        //得到三角形的数量
        int trianglesCount = Vertexes.Length - 2;
        //三角形顶点ID数组
        int[] triangles = new int[trianglesCount * 3];
        //绘制三角形
        _mesh.vertices = Vertexes;
        //三角形顶点索引,确保按照顺时针方向设置三角形顶点
        for (int i = 0; i < trianglesCount; i++) {
            for (int j = 0; j < 3; ++j) {
                triangles[i * 3 + j] = j == 0 ? 0 : i + j;
            }
        }
        _mesh.triangles = triangles;
        go.GetComponent<MeshFilter>().mesh = _mesh;
        go.transform.localScale = new Vector3(2, 2, 2);
        go.transform.position = new Vector3(0, 3, 0);
    }

    void CreatePolygonalCylinder(List<Node> node_list, int h) {
        List<Vector3> triangle = new List<Vector3>();
        
	}

    //圆柱体是由两个圆和一个长方形组成的  先输入长方形的顶点 然后在输入圆顶点
    /*private void UpdateMesh(Mesh mesh, int edg_x, int edg_y, float rad, float len) {
        edg_x = Mathf.Max(2, edg_x);//保证最低2个边
        edg_y = Mathf.Max(2, edg_y);
        int _deglen = (edg_x + 1) * edg_y;//长方体
        int totalcount = _deglen + (1 + edg_x + 1) * 2; //加两个圆


        Vector3[] normals = new Vector3[totalcount];
        Vector3[] verts = new Vector3[totalcount];
        Vector2[] uvs = new Vector2[totalcount];
        int[] trians = new int[edg_x * edg_y * 6];
        float reg = 6.28318f / edg_x;
        float _len = len / (edg_y - 1);



        for (int y = 0; y < edg_y; y++)
            for (int x = 0; x < edg_x + 1; x++)//多一个边来保存UV值
            {
                int i = x + y * (edg_x + 1);
                verts[i] = new Vector3(Mathf.Sin((reg * (x % edg_x)) % 6.28318f) * rad, Mathf.Cos((reg * (x % edg_x)) % 6.28318f) * rad, rightPos + y * _len);//计算顶点坐标
                normals[i] = new Vector3(verts[i].x, verts[i].y, 0);//计算法线方向
                int id = x % (edg_x + 1) * 6 + y * edg_x * 6;
                if (x < edg_x + 1 && y < edg_y - 1 && (id + 5) < trians.Length)//计算顶点数组
                {
                    if (length > 0) {
                        trians[id] = i;
                        trians[id + 1] = trians[id + 4] = i + edg_x + 1;
                        trians[id + 2] = trians[id + 3] = i + 1;
                        trians[id + 5] = i + edg_x + 2;
                    } else {
                        trians[id] = i;
                        trians[id + 1] = trians[id + 3] = i + 1;
                        trians[id + 2] = trians[id + 5] = i + edg_x + 1;
                        trians[id + 4] = i + edg_x + 2;
                    }

                }
                //if (edg_x != 2)//计算UV，考虑到2个边的情况
                //    uvs[i] = new Vector2(x == edg_x ? 1f : quaduvStep.x * x, y == edg_y - 1 ? (2*rad+len)/totalLen : quaduvStep.y * y);
                //else
                //    uvs[i] = new Vector2(x % edg_x, y == edg_y - 1 ? (2 * rad + len) / totalLen : quaduvStep.y * y);
            }

        int maxId = edg_x * (edg_y - 1) * 6;
        verts[_deglen] = new Vector3(0, 0, rightPos);

        normals[_deglen] = -Vector3.forward;

        //uvs[_deglen] = new Vector2(0.5f, (rad) / totalLen);
        //原点一面
        for (int x = 0; x < edg_x + 1; x++) {
            verts[_deglen + 1 + x] = new Vector3(Mathf.Sin((reg * (x % edg_x)) % 6.28318f) * rad, Mathf.Cos((reg * (x % edg_x)) % 6.28318f) * rad, rightPos);
            normals[_deglen + 1 + x] = -Vector3.forward;
            if (x == edg_x) continue;

            if (length > 0) {
                trians[3 * x + maxId] = _deglen;
                trians[3 * x + 1 + maxId] = _deglen + 1 + x;
                trians[3 * x + 2 + maxId] = _deglen + 2 + x;
            } else {
                trians[3 * x + maxId] = _deglen;
                trians[3 * x + 1 + maxId] = _deglen + 2 + x;
                trians[3 * x + 2 + maxId] = _deglen + 1 + x;
            }
        }


        //远点一面
        maxId += 3 * edg_x;
        verts[_deglen + 2 + edg_x] = new Vector3(0, 0, leftPos);
        normals[_deglen + 2 + edg_x] = Vector3.forward;
        //uvs[_deglen + 1] = new Vector2(0.5f, (3 * rad + len) / totalLen);

        for (int x = 0; x < edg_x + 1; x++) {
            verts[1 + x + edg_x + 2 + _deglen] = new Vector3(Mathf.Sin((reg * (x % edg_x)) % 6.28318f) * rad, Mathf.Cos((reg * (x % edg_x)) % 6.28318f) * rad, leftPos);
            normals[1 + x + edg_x + 2 + _deglen] = Vector3.forward;
            if (x == edg_x) continue;
            if (length > 0) {
                trians[3 * x + maxId] = _deglen + 2 + edg_x;
                trians[3 * x + 1 + maxId] = _deglen + 2 + edg_x + x + 2;
                trians[3 * x + 2 + maxId] = _deglen + 2 + edg_x + x + 1;
            } else {
                trians[3 * x + maxId] = _deglen + 2 + edg_x;
                trians[3 * x + 1 + maxId] = _deglen + 2 + edg_x + x + 1;
                trians[3 * x + 2 + maxId] = _deglen + 2 + edg_x + x + 2;
            }
        }
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = trians;
        //mesh.uv = uvs;
        mesh.normals = normals;
        mesh.RecalculateBounds();
    }
    */
    void CreateCubeMesh() {
        GameObject gameObject = new GameObject("Cube");
        gameObject.transform.position = Vector3.zero;

        //顶点数组
        Vector3[] _vertices =
        {
            // front
            new Vector3(-5.0f, 10.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, -5.0f),
            new Vector3(5.0f, 0.0f, -5.0f),
            new Vector3(5.0f, 10.0f, -5.0f),


            // left
            new Vector3(-5.0f, 10.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, 5.0f),//
            new Vector3(-5.0f, 10.0f, 5.0f),

            // back
            new Vector3(-5.0f, 10.0f, 5.0f),
            new Vector3(-5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 10.0f, 5.0f),


            // right
            new Vector3(5.0f, 10.0f, 5.0f),
            new Vector3(5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, -5.0f),
            new Vector3(5.0f, 10.0f, -5.0f),


            // Top
            new Vector3(-5.0f, 10.0f, 5.0f),
            new Vector3(5.0f, 10.0f, 5.0f),
            new Vector3(5.0f, 10.0f, -5.0f),
            new Vector3(-5.0f, 10.0f, -5.0f),

           // Bottom
            new Vector3(-5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, -5.0f),

        };
        //索引数组
        int[] _triangles =
        {
          //front
          2,1,0,
          0,3,2,
          //left
          4,5,6,
          4,6,7,
          //back
          9,11,8,
          9,10,11,
          //right
          12,13,14,
		  12,14,15,
          //up
          16,17,18,
		  16,18,19,
          //buttom
          21,23,22,
		  21,20,23,

          //不可跳跃设置索引值（否则会提示一些索引超出边界顶点   15直接20不可，要连续15-16）
          //17,19,18,
          //17,16,19,
        };

        //UV数组
        Vector2[] uvs =
        {
            // Front
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),


            // Left
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),


            // Back
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),


            // Right
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),

            // Top
            new Vector2(0.0f, 0.0f),
			new Vector2(1.0f, 0.0f),
			new Vector2(1.0f, 1.0f),
			new Vector2(0.0f, 1.0f),

            // Bottom
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

        };

        Mesh mesh = new Mesh() {
            vertices = _vertices,
            uv = uvs,
            triangles = _triangles,
        };

        //重新计算网格的法线
        //在修改完顶点后，通常会更新法线来反映新的变化。法线是根据共享的顶点计算出来的。
        //导入到网格有时不共享所有的顶点。例如：一个顶点在一个纹理坐标的接缝处将会被分成两个顶点。
        //因此这个RecalculateNormals函数将会在纹理坐标接缝处创建一个不光滑的法线。
        //RecalculateNormals不会自动产生切线，因此bumpmap着色器在调用RecalculateNormals之后不会工作。然而你可以提取你自己的切线。
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        //gameObject.AddComponent<MeshRenderer>().material.mainTexture = (Texture)Resources.Load("地砖1");
        //gameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("unity_builtin_extra/Default-Material");
        gameObject.AddComponent<MeshRenderer>().material.color = Color.green;
        //gameObject.GetComponent<MeshRenderer>().material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void experiment() {
        List<int> tlist = new List<int>();
        tlist.Add(-1);
        tlist.Add(-1);
        tlist.Add(-1);
        tlist.Add(-2);
        tlist.Add(-2);
        tlist.Add(-2);
        tlist.Add(-3);
        tlist.Add(-3);
        tlist.Add(-3);
        GameObject cg = new GameObject();
        Mesh mesh = new Mesh();
        mesh.triangles = tlist.ToArray();
        mesh.RecalculateNormals();
        cg.AddComponent<MeshFilter>();
        cg.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    GameObject CreateHollowClinder(Vector3 ptStart, Vector3 ptEnd, float innerRadius, float outterRadius) {
        //计算垂直于轴的起始向量
        Vector3 vec1 = ptEnd - ptStart;
        Vector3 vec2 = Vector3.up;
        float a = Vector3.Angle(vec1, vec2);
        if (Mathf.Approximately(a, 0.0f)) {
            vec2 = Vector3.right;
        }
        Vector3 vecStart = Vector3.Cross(vec1, vec2);

        //计算开始面内圆点、外圆点，结束面内圆点、外圆点
        List<Vector3> pointsStartInner = new List<Vector3>();
        List<Vector3> pointsStartOutter = new List<Vector3>();
        List<Vector3> pointsEndtInner = new List<Vector3>();
        List<Vector3> pointsEndOutter = new List<Vector3>();

        GameObject objStartInner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject objStartOutter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject objEndInner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject objEndOutter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        objStartInner.transform.position = ptStart + innerRadius * vecStart.normalized;
        objStartOutter.transform.position = ptStart + outterRadius * vecStart.normalized;
        objEndInner.transform.position = ptEnd + innerRadius * vecStart.normalized;
        objEndOutter.transform.position = ptEnd + outterRadius * vecStart.normalized;

        int devide = 30;//圆划分为多少等分
        float angleStep = 360.0f / (float)devide;

        float ang = 0.0f;
        for (ang = 0.0f; ang < 360.0f; ang += angleStep) {
            objStartInner.transform.RotateAround(ptStart, vec1, angleStep);
            objStartOutter.transform.RotateAround(ptStart, vec1, angleStep);
            objEndInner.transform.RotateAround(ptEnd, vec1, angleStep);
            objEndOutter.transform.RotateAround(ptEnd, vec1, angleStep);

            pointsStartInner.Add(objStartInner.transform.position);
            pointsStartOutter.Add(objStartOutter.transform.position);
            pointsEndtInner.Add(objEndInner.transform.position);
            pointsEndOutter.Add(objEndOutter.transform.position);
        }
        Destroy(objStartInner);
        Destroy(objStartOutter);
        Destroy(objEndInner);
        Destroy(objEndOutter);

        //构建曲面
        List<Vector3> vertexs = new List<Vector3>();
        vertexs.AddRange(pointsStartInner);//开始面内圆点
        vertexs.AddRange(pointsEndtInner); //结束面内圆点
        vertexs.AddRange(pointsStartOutter);//开始面外圆点
        vertexs.AddRange(pointsEndOutter); //结束面外圆点


        List<int> triangles = new List<int>();
        //构建内表面
        int startIndex = 0 * devide;
        int EndIndex = 0 * devide + devide;
        for (int i = startIndex; i < EndIndex; i++) {
            //边界面处
            int iNext = i + 1;
            int dNext = i + devide + 1;
            if (iNext >= startIndex + devide)
                iNext = startIndex;

            if (dNext >= startIndex + 2 * devide)
                dNext = startIndex + devide;

            triangles.Add(i);
            triangles.Add(i + devide);
            triangles.Add(iNext);

            triangles.Add(iNext);
            triangles.Add(i + devide);
            triangles.Add(dNext);
        }

        //构建外表面
        startIndex = 2 * devide;
        EndIndex = 2 * devide + devide;
        for (int i = startIndex; i < EndIndex; i++) {
            //边界面处
            int iNext = i + 1;
            int dNext = i + devide + 1;
            if (iNext >= startIndex + devide)
                iNext = startIndex;

            if (dNext >= startIndex + 2 * devide)
                dNext = startIndex + devide;

            triangles.Add(i);
            triangles.Add(iNext);
            triangles.Add(i + devide);

            triangles.Add(iNext);
            triangles.Add(dNext);
            triangles.Add(i + devide);
        }

        //构建上表面
        startIndex = 0 * devide;
        EndIndex = 0 * devide + devide;
        for (int i = startIndex; i < EndIndex; i++) {
            //边界面处
            int iNext = i + 1;
            int dNext = i + 2 * devide + 1;
            if (iNext >= startIndex + devide)
                iNext = startIndex;

            if (dNext >= startIndex + 3 * devide)
                dNext = startIndex + 2 * devide;

            triangles.Add(i);
            triangles.Add(iNext);
            triangles.Add(i + 2 * devide);

            triangles.Add(iNext);
            triangles.Add(dNext);
            triangles.Add(i + 2 * devide);
        }

        //构建下表面
        startIndex = 1 * devide;
        EndIndex = 1 * devide + devide;
        for (int i = startIndex; i < EndIndex; i++) {
            //边界面处
            int iNext = i + 1;
            int dNext = i + 2 * devide + 1;
            if (iNext >= startIndex + devide)
                iNext = startIndex;

            if (dNext >= startIndex + 3 * devide)
                dNext = startIndex + 2 * devide;

            triangles.Add(i);
            triangles.Add(i + 2 * devide);
            triangles.Add(iNext);

            triangles.Add(iNext);
            triangles.Add(i + 2 * devide);
            triangles.Add(dNext);
        }

        Mesh chunkMesh = new Mesh();
        chunkMesh.vertices = vertexs.ToArray();
        chunkMesh.triangles = triangles.ToArray();

        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();

        GameObject obj = new GameObject("MyClinder");
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.material.mainTexture = (Texture)Resources.Load("地砖1");
        mf.sharedMesh = chunkMesh;

        return obj;
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
}
