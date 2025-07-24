Shader "Shader Forge/DistortAnim" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _DistortTex ("Distortion Texture", 2D) = "bump" {}
        _DistortStrength ("Distortion Strength", Range(0,1)) = 0.1
        _Speed ("Animation Speed", Float) = 1.0
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _DistortTex;
            float4 _MainTex_ST;
            float4 _DistortTex_ST;
            fixed4 _Color;
            float _DistortStrength;
            float _Speed;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvDistort : TEXCOORD1;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvDistort = TRANSFORM_TEX(v.uv, _DistortTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Time-based animation for distortion
                float2 animOffset = float2(_Time.y * _Speed, _Time.y * _Speed);
                float2 distortSample = tex2D(_DistortTex, i.uvDistort + animOffset).rg * 2 - 1;
                float2 distortedUV = i.uv + distortSample * _DistortStrength;
                fixed4 col = tex2D(_MainTex, distortedUV) * _Color;
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}