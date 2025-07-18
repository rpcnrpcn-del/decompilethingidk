Shader "Custom/LCDShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _EmissionStrength ("Emission Strength", Range(0,1)) = 0.5
        _PixelEffect ("PixelEffect", Range(0,1)) = 0.18
        _ScanlineEffect ("ScanlineAlpha", Range(0,1)) = 0.326
        _ScanSpeed ("Scanline Speed", Range(0,100)) = 5.9
        _NumPixelsX ("LCD X Resolution", Float) = 128
        _NumPixelsY ("LCD Y Resolution", Float) = 9.5
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

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Glossiness;
            float _EmissionStrength;
            float _PixelEffect;
            float _ScanlineEffect;
            float _ScanSpeed;
            float _NumPixelsX;
            float _NumPixelsY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // emulate pixelation
                float2 resolution = float2(_NumPixelsX, _NumPixelsY);
                float2 pixelUV = floor(i.uv * resolution) / resolution;

                fixed4 col = tex2D(_MainTex, pixelUV);

                // apply pixel effect factor
                col.rgb = lerp(col.rgb, col.rgb * _EmissionStrength, _PixelEffect);

                // scanline effect using sine wave with adjustable speed
                float scanline = 0.5 + 0.5 * sin((i.uv.y * resolution.y) + (_Time.y * _ScanSpeed));
                col.rgb *= lerp(1.0, scanline, _ScanlineEffect);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
