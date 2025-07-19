Shader "Shader Forge/Avatar_Hair" {
    Properties {
        _Col_Tex ("Col_Tex", 2D) = "white" {}
        _Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1)
        _Dirt_Mult ("Dirt_Mult", Float) = 0.27
        _Dirt_Exp ("Dirt_Exp", Float) = 1.59
        _Spec_Tex ("Spec_Tex", 2D) = "white" {}
        _Gloss ("Gloss", Range(0, 1)) = 0.561
        _BumpMap ("Normal_Tex", 2D) = "bump" {}
        _Normal_Mult ("Normal_Mult", Float) = 0.38
        _BumpMap2 ("Normal_Detail", 2D) = "bump" {}
        _Normal_Detail_Mult ("Normal_Detail_Mult", Float) = 0.72
        _Outline ("Outline", Float) = 0.012
        _Outline_Col ("Outline_Col", Color) = (0.4392157,0.9254903,0.3019608,1)
        _Fresnel ("Fresnel", Float) = 1.75
        _Highlight ("Highlight", Float) = 0
    }
    SubShader {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back

        Pass {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Col_Tex;
            float4 _Col_Tex_ST;
            fixed4 _Color;
            float _Dirt_Mult;
            float _Dirt_Exp;
            sampler2D _Spec_Tex;
            float4 _Spec_Tex_ST;
            half _Gloss;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
            float _Normal_Mult;
            sampler2D _BumpMap2;
            float4 _BumpMap2_ST;
            float _Normal_Detail_Mult;
            float _Outline;
            fixed4 _Outline_Col;
            float _Fresnel;
            float _Highlight;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Col_Tex);
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldTangent = normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz));
                float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w;
                o.worldNormal = worldNormal;
                o.tangentDir = worldTangent;
                o.bitangentDir = worldBitangent;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 albedo = tex2D(_Col_Tex, i.uv) * _Color;
                float3 N = normalize(i.worldNormal);

                float3 n1 = UnpackNormal(tex2D(_BumpMap, i.uv)) * _Normal_Mult;
                float3 n2 = UnpackNormal(tex2D(_BumpMap2, i.uv)) * _Normal_Detail_Mult;
                float3 nCombined = normalize(n1 + n2);

                float3x3 TBN = float3x3(i.tangentDir, i.bitangentDir, i.worldNormal);
                N = normalize(mul(nCombined, TBN));

                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 H = normalize(L + V);

                float NdotL = saturate(dot(N, L));
                float specSample = tex2D(_Spec_Tex, i.uv).r;
                float specular = pow(saturate(dot(N, H)), _Gloss * 128.0) * specSample;

                float3 lighting = (albedo.rgb * NdotL) + (specular * _Highlight);
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo.rgb;
                float3 finalRGB = lighting + ambient;

                return fixed4(finalRGB, albedo.a);
            }
            ENDCG
        }
    }
    FallBack "Standard"
}