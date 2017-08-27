using System.Collections;
using UnityEngine;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Battleground : MonoBehaviour
{
    //
    // 'Battle_field_hits' отрисовывает только попадания/промахи
    // корабли же находятся над 'default_field' и под вышеуказанным слоем
    //
    public int size_X = 10;
    public int size_Y = 10;

    public float tileSize = 1f;

    public Texture2D tileTexture;
    public Texture2D hitTexture;
    public Texture2D missTexture;
    public Texture2D noneTexture;

    public int tileResolution;

    private int[,] _battleFieldArray;        // -1 пустая ячейка, 1 - попадание, 0 - промах 
    public int[,] BattleFieldArray
    {
        get
        {
            return _battleFieldArray;
        }
        private set
        {
            _battleFieldArray = value;
        }
    }

    private void Awake()
    {
        BattleFieldArray = new int[size_X, size_Y];

        // заполняем карту
        for (int j = 0; j < size_Y; j++)
            for (int i = 0; i < size_X; i++)
                BattleFieldArray[i, j] = -1;
    }

    private void Start()
    {
        BuildMesh();
    }

    public void BuildMesh()
    {
        int numTiles = size_X * size_Y;
        int numTris  = numTiles * 2;        // кол-во треугольников

        int vsize_X  = size_X + 1;          // кол-во точек по Х и Y
        int vsize_Y  = size_Y + 1;
        int numVerts = vsize_X * vsize_Y;

        // Generate mesh data
        Vector3[] vertices  = new Vector3[numVerts];
        Vector3[] normals   = new Vector3[numVerts];
        Vector2[] uv        = new Vector2[numVerts];


        int[] triangles = new int[numTris * 3];

        int x, y;
        for (y = 0; y < vsize_Y; y++)
            for (x = 0; x < vsize_X; x++)
            {
                vertices[y * vsize_X + x] = new Vector3(x * tileSize, y * tileSize, 0);
                normals[y * vsize_X + x]  = Vector3.up;
                uv[y * vsize_X + x]       = new Vector2((float)x / size_X, (float)y / size_Y);
            }

        for (y = 0; y < size_Y; y++)
            for (x = 0; x < size_X; x++)
            {
                int squareIndex = y * size_X + x;
                int triOffset = squareIndex * 6;

                triangles[triOffset + 0] = y * vsize_X + x + 0;
                triangles[triOffset + 1] = y * vsize_X + x + vsize_X + 0;
                triangles[triOffset + 2] = y * vsize_X + x + vsize_X + 1;

                triangles[triOffset + 3] = y * vsize_X + x + 0;
                triangles[triOffset + 4] = y * vsize_X + x + vsize_X + 1;
                triangles[triOffset + 5] = y * vsize_X + x + 1;
            }

        // цепляем нашу сетку на меш
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;


       BuildTexture();

    }

    void BuildTexture()
    {
        if (tileTexture.height > 256)
        {
            Debug.Log("Too high texture resolution!!!");
            return;
        }

        int w = size_X * tileResolution;
        int h = size_Y * tileResolution;

        Texture2D texture = new Texture2D(w, h);
        for (int y = 0; y < size_Y; y++)
            for (int x = 0; x < size_X; x++)
            {
                Color[] col;
                if (this.gameObject.name != "Battle_field_hits" && this.gameObject.name != "Enemy_field_hits")
                {
                    col = tileTexture.GetPixels(0, 0, tileResolution, tileResolution);
                }
                else
                    col = noneTexture.GetPixels(0, 0, tileResolution, tileResolution);

                texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, col);
            }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();        // apply all previus changes

        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.sharedMaterials[0].mainTexture = texture;

        /*
        - У кораблей order in layer     = 1
        - У дефолтного поля             = 0
        - у поля с выстрелами/промахами = 2
        */
        if (gameObject.name == "Battle_field_hits" || gameObject.name == "Enemy_field_hits")
            mesh_renderer.sortingOrder = 2;
        else
            mesh_renderer.sortingOrder = 0;
    }

    public void BattleFieldUpdater(int x, int y, bool hit)
    {
        int w = size_X * tileResolution;
        int h = size_Y * tileResolution;

        Texture2D texture = new Texture2D(w, h);

        for (int j = 0; j < size_Y; j++)
            for (int i = 0; i < size_X; i++)
            {
                if (i == x && j == y)
                    BattleFieldArray[i, j] = hit ? 1 : 0;   // отмечаем попадание/промах

                Color[] col;

                if (gameObject.name == "Battle_field_hits" || gameObject.name == "Enemy_field_hits")
                {
                    col = BattleFieldArray[i, j] == -1 ? noneTexture.GetPixels(0, 0, tileResolution, tileResolution) :
                          BattleFieldArray[i, j] == 1  ? hitTexture.GetPixels (0, 0, tileResolution, tileResolution) :
                                                         missTexture.GetPixels(0, 0, tileResolution, tileResolution);
                }
                else
                    col = BattleFieldArray[i, j] == -1 ? tileTexture.GetPixels(0, 0, tileResolution, tileResolution) :
                          BattleFieldArray[i, j] == 1  ? hitTexture.GetPixels (0, 0, tileResolution, tileResolution) :
                                                         missTexture.GetPixels(0, 0, tileResolution, tileResolution);

                texture.SetPixels(i * tileResolution, j * tileResolution, tileResolution, tileResolution, col);
            }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();       

        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.sharedMaterials[0].mainTexture = texture;

        //print("Battle coord " + x + ","+ y + " updated." + "Enemy hit is " + hit + " " + BitConverter.GetBytes(x).Length);
    }
}
