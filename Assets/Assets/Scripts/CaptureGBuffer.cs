using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CaptureGBuffer : MonoBehaviour
{
    private CommandBuffer buf;

    public Color32 color;
    private Texture2D texture;
    private RenderTexture renderTexture;

    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
        renderTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
        renderTexture.filterMode = FilterMode.Point;
        Shader.SetGlobalVector("HighlightID", Color.black);

        buf = new CommandBuffer();
        buf.name = "GBuffer Test";
        foreach (var cam in Camera.allCameras)
        {
            if (!cam)
            {
                break;
            }
            cam.AddCommandBuffer(CameraEvent.AfterGBuffer, buf);
        }

#if UNITY_EDITOR
        var sceneViewCameras = SceneView.GetAllSceneCameras();
        foreach (var cam in sceneViewCameras)
        {
            if (!cam)
            {
                break;
            }
            cam.AddCommandBuffer(CameraEvent.AfterGBuffer, buf);
        }
#endif
    }

    Vector2Int getNormalizedMousePos()
    {
        Vector2 pos = Input.mousePosition;        

        /*
        // 座標系がOpenGL方式かDirectX方式かによって上下が変わるのを考慮する
        // Metalの場合、例外的に反転は行わないようにした
        if (SystemInfo.graphicsUVStartsAtTop && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal)
        {
            pos.y = Screen.height - 1 - pos.y;
        }
        */
        float x = Mathf.Clamp(pos.x, 0.0f, Screen.width - 1);
        float y = Mathf.Clamp(pos.y, 0.0f, Screen.height - 1);
        return new Vector2Int((int)x, (int)y);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        RenderTexture.active = renderTexture;        
        texture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        color = texture.GetPixel(0, 0);
        var seleted = FindObjectByIDColor(color);
        Debug.Log($"Color:{color}, Selection:{(seleted != null ? seleted.name:"N/A")}");
        if (seleted == null) return;
        Shader.SetGlobalVector("HighlightID", (Color)color);
    }
    

    void OnPostRender()
    {
        buf.Clear();

        RenderTargetIdentifier src = new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer1);
        RenderTargetIdentifier dst = renderTexture;
        Vector2Int pos = getNormalizedMousePos();
        buf.CopyTexture(src, 0, 0, pos.x, pos.y, 1, 1, dst, 0, 0, 0, 0);
    }

    private HasIDColor FindObjectByIDColor(Color32 idCol)
    {
        var structures = GameObject.FindObjectsOfType<HasIDColor>();

        var found = Array.Find(structures, shed => idCol.Equals(shed.IDColor));
        return found;
    }

    private void OnDestroy()
    {
        renderTexture.Release();
        buf.Release();
    }
}
