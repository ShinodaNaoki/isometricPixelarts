#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class Shed : MonoBehaviour //opposite -> courtyard
{
    private bool needRestruct = true;

#if UNITY_EDITOR
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += this.OnEditorUpdate;
        needRestruct = true;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= this.OnEditorUpdate;
    }

    void OnEditorUpdate(SceneView vw)
    {
        Start();
    }
#endif

    [SerializeField, Header("ドット絵の3Dサイズ")]
    private Vector3Int size;

    [SerializeField, Header("テクスチャの最手前座標")]
    private Vector2Int pivot;

    // ドット絵を保持するマテリアル
    public Material material;

    // マテリアルのメインテクスチャサイズ
    private Vector2Int texSize;

    private void Awake()
    {
        // なぜか Start/Awake の両方から Mesh初期化しないと、シーン再生時に表示されない
        Start();
    }

    void Start()
    {
        if (!needRestruct) return;
        needRestruct = true;

        Mesh mesh = InitializeCube();
        texSize = GetTextureSize();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    /// <summary>
    /// テクスチャサイズを取得
    /// </summary>
    /// <returns></returns>
    private Vector2Int GetTextureSize()
    {
        Debug.Assert(material.mainTexture != null, "Main texture is NULL!");

        var tex = material.mainTexture;

        return new Vector2Int(tex.width, tex.height);
    }

    /// <summary>
    /// pivotからの相対位置をUV座標に変換する
    /// </summary>
    /// <param name="offestX"></param>
    /// <param name="offsetY"></param>
    /// <returns></returns>    
    private Vector2 ToUV(float offestX, float offsetY)
    {
        return new Vector2((pivot.x + offestX) / texSize.x, (texSize.y - pivot.y + offsetY) / texSize.y);
    }

    /// <summary>
    /// メッシュを生成する
    /// </summary>
    /// <returns></returns>
    private Mesh InitializeCube()
    {
       
        Mesh mesh = new Mesh();
        /*
         *  　6
         * 5＜ ＞3
         * |　1　|
         * 4＜|＞2
         *  　0
         */
        var vertices = new Vector3[] {            
            new Vector3(0, 0, 0), // 0:pivot
            new Vector3(0, size.y+1, 0), // 1:nearest top
            new Vector3(0, 0, size.z), // 2:bottom right
            new Vector3(0, size.y+1, size.z), // 3:top right
            new Vector3(size.x, 0, 0), // 4:bottom left
            new Vector3(size.x, size.y+1, 0), // 5:top left
            new Vector3(size.x, size.y+1, size.z), // 6:most far top
        };

        var harfX = size.x * 0.5f;
        var harfZ = size.z * 0.5f;
        var uv = new Vector2[] {
            ToUV(0, -0.5f), // 0:pivot
            ToUV(0, size.y+0.5f), // 1:nearest top
            ToUV(-size.z, harfZ-0.5f), // 4:bottom left
            ToUV(-size.z, harfZ + size.y+0.5f), // 5:top left
            ToUV(size.x, harfX-0.5f), // 2:bottom right
            ToUV(size.x, harfX + size.y+0.5f),  // 3:top right
            ToUV(size.x - size.z, harfX + harfZ + size.y+0.5f), // 6:most far top
        };

        var triangles = new int[] {
            0,3,1, 0,2,3, // Right Surface 
            5,4,0, 0,1,5, // Left Surface 
            1,3,5, 3,6,5, // Upper Surface
        };

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }
}
