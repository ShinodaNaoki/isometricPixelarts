Shader "Custom/DayNightPixelArtsGBuffer"
{
  Properties
  {
    [NoScaleOffset] _DayTex ("Day Texture", 2D) = "white" {}
    [NoScaleOffset] _NightTex ("Night Texture", 2D) = "black" {}
    _Transpalent ("Transpalent Color", Color) = (1,0,1,1)
    _IDColor ("Color fo source object ID", Vector) = (0,0,0,0)
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

      sampler2D _DayTex;
      float4 _DayTex_ST;
      sampler2D _NightTex;
      float4 _NightTex_ST;
      float4 _IDColor;

      float _NightTexEnabled;
      fixed3 _Transpalent;

      void vert (in appdata v, out v2f o)
      {
        o.position = UnityObjectToClipPos(v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.uv = TRANSFORM_TEX(v.uv, _DayTex);
      }

      void frag (in v2f i, out flagout o)
      {
        fixed4 colDay = tex2D(_DayTex, i.uv);
        // stop rendering if it is transpalent color.
        fixed3 diff = abs(colDay.rgb - _Transpalent.rgb);
        if(length(diff) < 0.0001) discard;

        // use 25% brightness of day texture, if night texture is disabled.
        fixed4 colNight = lerp(colDay * 0.25, tex2D(_NightTex, i.uv), _NightTexEnabled);

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