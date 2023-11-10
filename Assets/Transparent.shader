Shader "Custom/Transparent"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Tags { "ForceNoShadowCasting" = "True" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vertexFunction
            #pragma fragment fragmentFunction
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vertexFunction(appdata IN)
            {
                v2f OUT;

                OUT.position = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;

                return OUT;
            }

            fixed4 fragmentFunction(v2f IN) : SV_TARGET
            {
                fixed4 pixelColor = tex2D(_MainTex, IN.uv);

                // Check if the pixel is approximately white
                if (pixelColor.r > 0.7 && pixelColor.g > 0.7 && pixelColor.b > 0.7)
                {
                    pixelColor.a = 0; // make pixel transparent
                }

                return pixelColor;
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
