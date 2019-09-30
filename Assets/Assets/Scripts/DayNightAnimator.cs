#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class DayNightAnimator : MonoBehaviour
{
    const float SUNSET_BEGIN = 0.1f;
    const float SUNSET_END = 0.4f;
    const float SUNRISE_BEGIN = 0.6f;
    const float SUNRISE_END = 0.9f;
    const float LIGHT_BEGIN = 0.2f;
    const float LIGHT_END = 0.5f;

    private void Update()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        Material material  = renderer.sharedMaterial;
        float time = Mathf.Repeat(Time.fixedTime / 5f, 1f);

        if (SUNSET_BEGIN < time && time < SUNSET_END) // 夕焼けタイム
        {
            float rate = (time - SUNSET_BEGIN) / (SUNSET_END - SUNSET_BEGIN);
            material.SetFloat("_DayRatio", 1f - rate);
            material.SetFloat("_BurnRatio", rate);
            material.SetColor("_BurnColor", new Color(1f, 0.3f, 0f));
        }

        bool isLight = LIGHT_BEGIN < time && time < LIGHT_END;
        material.SetFloat("_NightTexEnabled", isLight ? 1f : 0f);

        if (SUNRISE_BEGIN < time && time < SUNRISE_END) // 夜明け前タイム
        {
            float rate = (time - SUNRISE_BEGIN) / (SUNRISE_END - SUNRISE_BEGIN);
            material.SetFloat("_DayRatio", rate);
            material.SetFloat("_BurnRatio", 1f - rate);
            material.SetColor("_BurnColor", new Color(0.2f, 0.6f, 1.0f));
        }


        //Debug.LogFormat("{0}", time);
        renderer.material = material;

    }
}
