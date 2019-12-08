#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class Shed4Sprite : MonoBehaviour //opposite -> courtyard
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

    public bool zoomAroundPivot;
#endif

    [SerializeField, Header("ドット絵の3Dサイズ")]
    public Vector3Int size;

    [SerializeField, Header("ドット絵の最手前座標")]
    public Vector2Int pivot;

    [SerializeField, Header("スプライトとpivot.xからsize自動計算")]
    public bool autoSizeAdjust = true;

    [SerializeField, Header("昼用スプライト")]
    public Sprite sprite4Day;

    [SerializeField, Header("夜用スプライト")]
    public Sprite sprite4Night;

    // ドット絵を保持するマテリアル
    public Material material;

    public Color32 IDColor;

    // マテリアルのメインテクスチャサイズ
    private Vector2Int texSize;

    // Spriteのテクスチャ領域
    private RectInt spriteRect;

    public RectInt SpriteRect
    {
        get { return spriteRect; }
    }

    public Vector2Int Size2D
    {
        get {
            if (autoSizeAdjust) return spriteRect.size;
            var w = size.x + size.z;
            return new Vector2Int(w, size.y + w / 2);
        }
    }

    private void Awake()
    {
        // なぜか Start/Awake の両方から Mesh初期化しないと、シーン再生時に表示されない
        Start();
    }

    public static Texture2D textureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }

    private void doAutoSizeAdjust()
    {
        spriteRect = GetSpriteRect();
        texSize = GetTextureSize();
        //Debug.LogFormat("Texture size:({0},{1})", texSize.x, texSize.y);
        //Debug.LogFormat("Sprite rect:({0},{1})-({2},{3})", spriteRect.x, spriteRect.y, spriteRect.width, spriteRect.height);
        pivot.y = 0;
        pivot.x = Mathf.Clamp(pivot.x, 0, spriteRect.width);

        size.z = pivot.x;
        size.x = spriteRect.width - pivot.x;
        size.y = spriteRect.height - spriteRect.width / 2;
    }

    void Start()
    {
        if (!needRestruct) return;
        needRestruct = true;

        if (autoSizeAdjust)
        {
            doAutoSizeAdjust();
        }

        Mesh mesh = InitializeCube();
        var newMaterial = Instantiate(material);
        newMaterial.SetTexture("_MainTex", sprite4Day.texture);
        newMaterial.SetTexture("_DayTex", sprite4Day.texture);
        newMaterial.SetTexture("_NightTex", sprite4Night.texture);
        Vector4 vect = (Color)IDColor;
        newMaterial.SetVector("_IDColor", vect);
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().material = newMaterial;
    }

#if UNITY_EDITOR
    /// <summary>
    ///  インスペクターからメッシュを再設定する
    /// </summary>
    public void UpdateMesh() {
        this.needRestruct = true;
        this.Start();
    }

    /// <summary>
    /// 設定されたテクスチャが3Dサイズで要求されるサイズに足りない
    /// </summary>
    public bool IsTextureHasNotEnoughSize
    {
        get
        {
            if (sprite4Day == null) return true;
            return texSize.y < size.y + (size.x + size.z) / 2;
        }
    }

#endif

    /// <summary>
    /// テクスチャサイズを取得
    /// </summary>
    /// <returns></returns>
    private Vector2Int GetTextureSize()
    {
        var tex = sprite4Day.texture;

        return new Vector2Int(tex.width, tex.height);
    }

    /// <summary>
    /// Spriteのテクスチャ範囲を取得
    /// </summary>
    /// <returns></returns>
    private RectInt GetSpriteRect()
    {
        var rect = sprite4Day.textureRect;

        return new RectInt((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    }

    /// <summary>
    /// pivotからの相対位置をUV座標に変換する
    /// </summary>
    /// <param name="offestX"></param>
    /// <param name="offsetY"></param>
    /// <returns></returns>
    private Vector2 ToUV(float offestX, float offsetY)
    {
        var x = pivot.x + offestX + spriteRect.x;
        var y = pivot.y + offsetY + spriteRect.y;
        var pos = new Vector2( x / texSize.x, y / texSize.y);
        //Debug.LogFormat("ToUV:({0},{1})",pos.x, pos.y);
        return pos;
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
