using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Shed4Sprite))]
public class Shed4SpriteEditor : Editor
{
    Shed4Sprite shed = null;
    SerializedProperty sprite4Day;
    SerializedProperty sprite4Night;
    SerializedProperty material;
    SerializedProperty pivot;
    SerializedProperty size;
    SerializedProperty autoAdjust;
    SerializedProperty idColor;

    void OnEnable()
    {
        shed = target as Shed4Sprite;
        sprite4Day = serializedObject.FindProperty("sprite4Day");
        sprite4Night = serializedObject.FindProperty("sprite4Night");
        material = serializedObject.FindProperty("material");
        pivot = serializedObject.FindProperty("pivot");
        size = serializedObject.FindProperty("size");
        autoAdjust = serializedObject.FindProperty("autoSizeAdjust");
        idColor = serializedObject.FindProperty("IDColor");
    }

    public override void OnInspectorGUI()
    {
        //　シリアライズオブジェクトの更新
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(pivot);
        EditorGUILayout.PropertyField(sprite4Day);
        EditorGUILayout.PropertyField(sprite4Night);
        EditorGUILayout.PropertyField(material);
        EditorGUILayout.PropertyField(idColor);

        EditorGUILayout.PropertyField(autoAdjust);
        if (autoAdjust.boolValue)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(size);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.PropertyField(size);
        }
        //　シリアライズオブジェクトのプロパティの変更を更新
        serializedObject.ApplyModifiedProperties();


        if (EditorGUI.EndChangeCheck())
        {
            shed.UpdateMesh();
        }
    }

    // プレビューウィンドウを表示するかどうか
    public override bool HasPreviewGUI()
    {
        return true;
    }

    private bool ZoomAroundPivot
    {
        get { return shed.zoomAroundPivot; }
        set { shed.zoomAroundPivot = value; }
    }

    // プレビューウィンドウのヘッダーバーをカスタムする関数
    public override void OnPreviewSettings()
    {
        ZoomAroundPivot = GUILayout.Toggle(ZoomAroundPivot, "zoom pivot");
    }

    // プレビューウィンドウで描画させたいものはここで書く
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (ZoomAroundPivot) _drawAroundPivot(r);
        else _drawEntire(r);
    }

    private void _drawEntire(Rect r)
    {
        Vector2 size2D = shed.Size2D;

        const int border = 2;

        var scale = Mathf.Min((r.width - border * 2)/ size2D.x , (r.height - border * 2)/ size2D.y);
        var centerPos = new Vector2(r.x + r.width / 2, r.y + r.height / 2);
        var drawSize = size2D * scale;
        var size3D = ((Vector3)shed.size) * scale;

        var offsetPos = new Vector2(centerPos.x - border - drawSize.x / 2, centerPos.y -border - drawSize.y / 2);

        // 3D投影サイズ境界のx,zを赤と緑の外殻線で描く
        var maxPos = new Vector2(offsetPos.x + drawSize.x + border * 2, offsetPos.y + drawSize.y + border * 2);
        Handles.DrawSolidRectangleWithOutline(new Rect(offsetPos.x, offsetPos.y, size3D.x + border, border - 1), Color.red, Color.red);
        Handles.DrawSolidRectangleWithOutline(new Rect(offsetPos.x, offsetPos.y, border - 1, size3D.x / 2 + border), Color.red, Color.red);
        Handles.DrawSolidRectangleWithOutline(new Rect(offsetPos.x, maxPos.y, size3D.z + border, border - 1), Color.green, Color.green);
        Handles.DrawSolidRectangleWithOutline(new Rect(offsetPos.x, maxPos.y, border - 1, -size3D.z / 2), Color.green, Color.green);

        Handles.DrawSolidRectangleWithOutline(new Rect(maxPos.x, maxPos.y, -size3D.x - border, border - 1), Color.red, Color.red);
        Handles.DrawSolidRectangleWithOutline(new Rect(maxPos.x, maxPos.y, border - 1, -size3D.x / 2 - border), Color.red, Color.red);
        Handles.DrawSolidRectangleWithOutline(new Rect(maxPos.x, offsetPos.y, -size3D.z - border, border - 1), Color.green, Color.green);
        Handles.DrawSolidRectangleWithOutline(new Rect(maxPos.x, offsetPos.y, border - 1, size3D.z / 2 + border), Color.green, Color.green);

        var texture = shed.sprite4Day.texture;
        var texRect = shed.SpriteRect;
        var texSize = new Vector2(texture.width, texture.height);
        //Debug.LogFormat($"({texSize.x},{texSize.y}) , ({texRect.xMin},{texRect.yMin}) -({texRect.xMax}, {texRect.yMax})");

        // 中央にスプライト領域を描画
        var drawRect = new Rect(offsetPos.x + border, offsetPos.y + border, drawSize.x, drawSize.y);
        Rect texCoord = new Rect(texRect.x / texSize.x, texRect.y / texSize.y, texRect.width / texSize.x, texRect.height / texSize.y);
        GUI.DrawTextureWithTexCoords(drawRect, texture, texCoord);
    }

    private void _drawAroundPivot(Rect r)
    {
        Vector2 size2D = shed.Size2D;

        const int border = 2;
        const int zoom = 3;

        var scale = Mathf.Min(r.width / size2D.x, (r.height - border) / size2D.y);
        scale = Mathf.Max(3, scale);
        var hfWidth = r.width / 2;
        var hfHeight = r.height / 2;

        // 3D投影サイズ境界のx,zを赤と緑の外殻線で描く
        Handles.DrawSolidRectangleWithOutline(new Rect(r.x, r.yMax, hfWidth, border - 1), Color.red, Color.red);
        Handles.DrawSolidRectangleWithOutline(new Rect(r.x + hfWidth, r.yMax, hfWidth, border - 1), Color.green, Color.green);

        var texture = shed.sprite4Day.texture;
        var texRect = shed.SpriteRect;
        var texSize = new Vector2(texture.width, texture.height);
        //Debug.LogFormat($"({texSize.x},{texSize.y}) , ({texRect.xMin},{texRect.yMin}) -({texRect.xMax}, {texRect.yMax})");
        Vector2 pivot = shed.pivot;
        var clipSize = new Vector2(hfWidth / zoom, (r.height - border) / zoom);
        Rect texCoord = new Rect(
            (texRect.x + pivot.x - clipSize.x) / texSize.x, texRect.y / texSize.y,
            clipSize.x * 2 / texSize.x, clipSize.y / texSize.y);


        // 中央にスプライト領域を描画
        var drawRect = new Rect(r.x, r.y, r.width, r.height - border);
        GUI.DrawTextureWithTexCoords(drawRect, texture, texCoord);
    }
}