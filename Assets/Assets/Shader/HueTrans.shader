Shader "Custom/HueTrans"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}

        _RedTransfar ("RED Transfer Color", Color) = (1,0,0,1)
        _GreenTransfar ("GREEN Transfer Color", Color) = (0,1,0,1)
        _BlueTransfar ("BLUE Transfer Color", Color) = (0,0,1,1)
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed3 _RedTransfar;
            fixed3 _GreenTransfar;
            fixed3 _BlueTransfar;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            inline fixed4 transferColor(in fixed4 srcCol) {
                // test srcCol if the two of r,g,b are 0.
                float d1 = (1 - srcCol.r) * (1 - srcCol.g) * (1 - srcCol.b);
                float d2 = srcCol.r + srcCol.g + srcCol.b;
                float flag = step(d1 + d2, 1);

                fixed3 rgb = lerp(
                    srcCol.rgb,
                    _RedTransfar * srcCol.r + _GreenTransfar * srcCol.g + _BlueTransfar * srcCol.b,
                    flag );

                fixed3 white = fixed3(1,1,1);
                flag = (1 - flag) * step(1, srcCol.r) * step(1, 1- abs(srcCol.g - srcCol.b));
                rgb = lerp(
                    rgb,
                    lerp(white, _RedTransfar, srcCol.r - srcCol.g),
                    flag);

                flag = (1 - flag) * step(1, srcCol.g) * step(1, 1- abs(srcCol.b - srcCol.r));
                rgb = lerp(
                    rgb,
                    lerp(white, _GreenTransfar, srcCol.g - srcCol.b),
                    flag);

                flag = (1 - flag) * step(1, srcCol.b) * step(1, 1- abs(srcCol.r - srcCol.g));
                rgb = lerp(
                    rgb,
                    lerp(white, _BlueTransfar, srcCol.b - srcCol.r),
                    flag);
                return fixed4(rgb,1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // stop rendering if it is transpalent.
                if(col.a < 0.0001) discard;

                // apply hue transform.
                col =  transferColor(col);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
