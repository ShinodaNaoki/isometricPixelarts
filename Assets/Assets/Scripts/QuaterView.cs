using UnityEngine;
using UnityEngine.Assertions;

public class QuaterView : MonoBehaviour
{
    private new Camera camera;

    [SerializeField, Header("カメラが移す中心地")]
    private Vector3Int lookAt;

    [SerializeField, Header("中心地の画面内上下位置(0～1)")]
    private float vertFocusPos;

    [SerializeField, Header("カメラを離す距離")]
    private int distance = 256;

    [SerializeField, Header("ピクセル倍率"), Min(1)]
    private int zoom = 1;


    void Awake()
    {
        camera = gameObject.GetComponent<Camera>();
        Assert.IsNotNull(camera);
        AdjustCamera();
    }

    void OnValidate()
    {
        Awake();
    }

    private Vector2 CalcOrthoSize()
    {
        // 画面の高さの半分＝等倍サイズ
        var hsize = camera.pixelHeight / (zoom * 2f);
        return new Vector2((float)camera.pixelWidth / camera.pixelHeight * hsize, hsize);
    }

    private void AdjustCamera()
    {
        camera.transform.rotation = Quaternion.identity;
        camera.transform.position = Vector3.zero;
        camera.ResetProjectionMatrix();

        int depth = (lookAt.x + lookAt.z) / 2;
        Vector3Int pos = new Vector3Int(lookAt.x - lookAt.z, lookAt.y + depth, lookAt.y - depth);

        var matrix = new Matrix4x4()
        {
            m00 = 1.0f, m01 = 0.0f, m02 =-1.0f, m03 = 0.5f - pos.x,
            m10 = 0.5f, m11 = 1.0f, m12 = 0.5f, m13 = 0.0f - pos.y,
            m20 =-0.5f, m21 = 1.0f, m22 =-0.5f, m23 = 0.0f - pos.z - distance,
            m30 = 0.0f, m31 = 0.0f, m32 = 0.0f, m33 = 1.0f            
        };
        
        var orthoSize = CalcOrthoSize();
        var projMatrix = Matrix4x4.Ortho(orthoSize.x * -1, orthoSize.x, orthoSize.y * -1, orthoSize.y, 0, 1000);
        camera.projectionMatrix = projMatrix;
        camera.worldToCameraMatrix = matrix;

    }
}
