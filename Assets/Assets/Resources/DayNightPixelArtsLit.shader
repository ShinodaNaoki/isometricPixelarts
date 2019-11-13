Shader "Hidden/DayNightPixelArtsLit"
{
SubShader {
// Pass 1: Lighting pass
//  LDR case - Lighting encoded into a subtractive ARGB8 buffer
//  HDR case - Lighting additively blended into floating point buffer
Pass {
    ZWrite Off
    //Blend [_SrcBlend] [_DstBlend]
    BlendOp Max

CGPROGRAM
#pragma target 3.0
#pragma vertex vert_deferred
#pragma fragment frag
#pragma multi_compile_lightpass
#pragma multi_compile ___ UNITY_HDR_ON

#pragma exclude_renderers nomrt

#include "UnityCG.cginc"
#include "UnityDeferredLibrary.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardBRDF.cginc"

sampler2D _CameraGBufferTexture0;
sampler2D _CameraGBufferTexture1;
sampler2D _CameraGBufferTexture2;
sampler2D _CameraGBufferTexture3;
sampler2D _DepthTexture;

inline float _depthEdge(float2 uv, float base, float x, float y) {
  float2 uv2 = float2(uv.x + x / _ScreenParams.x, uv.y + y / _ScreenParams.y);
  half4 pix = tex2D (_CameraGBufferTexture0, uv2);
  return smoothstep(0, 0.1, pix.a - base);
}

float CalcEdgeStrength (float d, float2 uv)
{
    float e1 = max(_depthEdge(uv, d, +1, 0), _depthEdge(uv, d, -1, 0));
    float e2 = max(_depthEdge(uv, d, 0, +1), _depthEdge(uv, d, 0, -1));
    
    return max(e1, e2) * 0.75;
}

half4 CalculateLight (unity_v2f_deferred i)
{
    float2 uv = i.uv.xy / i.uv.w;
    half4 colDay = tex2D (_CameraGBufferTexture0, uv);
    half4 normal = tex2D (_CameraGBufferTexture1, uv);
    half4 idCol = tex2D (_CameraGBufferTexture2, uv);
    half3 colNight = tex2D (_CameraGBufferTexture3, uv);    

    float edge =  1 - CalcEdgeStrength (colDay.a, uv);   
    
    // apply color burn effect to the day color.
    fixed3 colBurn = (1 - _LightColor .rgb) * _LightColor.a;
    colDay = fixed4(colDay.rgb - colBurn, 1) * edge;
    return half4(lerp(colNight, colDay.rgb, 1 - _LightColor.a), 1);
}

#ifdef UNITY_HDR_ON
half4
#else
fixed4
#endif
frag (unity_v2f_deferred i) : SV_Target
{
    half4 c = CalculateLight(i);
    #ifdef UNITY_HDR_ON
    return c;
    #else
    return exp2(-c);
    #endif
}

ENDCG
}


// Pass 2: Final decode pass.
// Used only with HDR off, to decode the logarithmic buffer into the main RT
Pass {
    ZTest Always Cull Off ZWrite Off
    Stencil {
        ref [_StencilNonBackground]
        readmask [_StencilNonBackground]
        // Normally just comp would be sufficient, but there's a bug and only front face stencil state is set (case 583207)
        compback equal
        compfront equal
    }

CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#pragma exclude_renderers nomrt

#include "UnityCG.cginc"

sampler2D _LightBuffer;
struct v2f {
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

v2f vert (float4 vertex : POSITION, float2 texcoord : TEXCOORD0)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(vertex);
    o.texcoord = texcoord.yx;
#ifdef UNITY_SINGLE_PASS_STEREO
    o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0f);
#endif
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    return -log2(tex2D(_LightBuffer, i.texcoord));
}
ENDCG
}

}
Fallback Off
}