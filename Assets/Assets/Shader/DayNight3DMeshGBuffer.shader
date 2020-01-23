Shader "Custom/DayNight3DMeshGBuffer"
{
  Properties
  {
    _MainTex ("Day Texture", 2D) = "white" {}
    _IDColor ("Color for source object ID", Vector) = (0,0,0,0)
  }
  SubShader
  {
    Pass
    {
      Stencil
      {
        Comp Always
        Pass Replace
        Ref 128
      }

      Tags{ "LightMode" = "Deferred" }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float4 position : SV_POSITION;
        float3 normal : NORMAL;
        float2 uv : TEXCOORD0;
      };

      struct flagout
      {
        float4 gBuffer0 : SV_TARGET0;
        float4 gBuffer1 : SV_TARGET1;
        float4 gBuffer2 : SV_TARGET2;
        float4 gBuffer3 : SV_TARGET3;
        float depth: SV_DEPTH;
      };

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float4 _IDColor;

      void vert (in appdata v, out v2f o)
      {        
        o.position = UnityObjectToClipPos(v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
      }

      void frag (in v2f i, out flagout o)
      {
         half NdotL = max(0, dot (i.normal, _WorldSpaceLightPos0));
        
        fixed4 colDay = tex2D(_MainTex, i.uv) * lerp(NdotL, 1, 0.75);

        // use 25% brightness of day texture, if night texture is disabled.
        fixed4 colNight = colDay * 0.25;

        o.gBuffer0 = fixed4(colDay.rgb, i.position.z);
        o.gBuffer3 = fixed4(colNight.rgb, 0.1);
        o.gBuffer2 = float4(i.normal, 0) * 0.5 + float4(0.5, 0.5, 0.5, 0);
        o.gBuffer1 = _IDColor;
        o.depth = i.position.z;
      }

      ENDCG
    }
  }
}