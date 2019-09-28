Shader "Custom/PseudoSpriteAdv"
{
    Properties
    {
        [NoScaleOffset] _DayTex ("Day Texture", 2D) = "white" {}
        [NoScaleOffset] _NightTex ("Night Texture", 2D) = "black" {}
        [MaterialToggle] _NightTexEnabled ("Night Texture Enabled", Float) = 1
        _DayRatio ("Day Night Mixture Ration", Range (0.0, 1.0)) = 0.5
        _Transpalent ("Transpalent Color", Color) = (1,0,1,1)
        _BurnColor ("Burn Color", Color) = (1,0.7,0,1)
        _BurnRatio ("Burn Ration", Range (0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _DayTex;
            float4 _DayTex_ST;
            sampler2D _NightTex;
            float4 _NightTex_ST;

            float _NightTexEnabled;
            fixed3 _Transpalent;
            float _DayRatio;
            
            fixed3 _BurnColor;
            float _BurnRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DayTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 colDay = tex2D(_DayTex, i.uv);

                // stop rendering if it is transpalent color.
                fixed3 diff = abs(colDay.rgb - _Transpalent.rgb);
                if(length(diff) < 0.0001) discard;

                // use 25% brightness of day texture, if night texture is disabled.
                fixed4 colNight = lerp(colDay * 0.25, tex2D(_NightTex, i.uv), _NightTexEnabled);

                // apply color burn effect to the day color.
                fixed3 colBurn = (1 - _BurnColor.rgb) * _BurnRatio;
                colDay = fixed4(colDay.rgb - colBurn,1);
                //colDay = fixed4(lerp(colDay.rgb, _BurnColor.rgb, _BurnRatio),1); // 通常
                //colDay = fixed4( lerp(colDay.rgb, _BurnColor.rgb * colDay.rgb, _BurnRatio),1); // 乗算

                // mixing day and night by ratio, unless it lowers night color.
                fixed4 col = max(lerp(colNight, colDay, _DayRatio), colNight);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
