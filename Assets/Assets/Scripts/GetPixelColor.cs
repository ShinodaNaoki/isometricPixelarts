using UnityEngine;
using UnityEngine.Rendering;

namespace GetPixelColor
{
    public class GetPixelColor : MonoBehaviour
    {
        public Color32 color;
        public Texture2D Tex { get; private set; }

        void Start()
        {
            Tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                color = Tex.GetPixel(0, 0);
                Debug.Log(color.ToString());
            }
        }

        void OnPostRender()
        {
            Vector2 pos = Input.mousePosition;

            // 座標系がOpenGL方式かDirectX方式かによって上下が変わるのを考慮する
            // Metalの場合、例外的に反転は行わないようにした
            if (SystemInfo.graphicsUVStartsAtTop && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal)
            {
                pos.y = Screen.height - 1 - pos.y;
            }

            float x = Mathf.Clamp(pos.x, 0.0f, Screen.width - 1);
            float y = Mathf.Clamp(pos.y, 0.0f, Screen.height - 1);

            Tex.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
        }
    }
}
