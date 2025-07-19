Shader "Shader Forge/Avatar/Avatar_RGB_Normal" {
    Properties {
        _MainTex ("Mask_Tex", 2D) = "white" {}
        _Base_Col ("Base_Col", Color) = (1,1,1,1)
        _Red_Col ("Red_Col", Color) = (1,0,0,1)
        _Green_Col ("Green_Col", Color) = (0,1,0,1)
        _Blue_Col ("Blue_Col", Color) = (0,0,1,1)
        _Dirt_Col ("Dirt_Col", Color) = (1,0.9724138,0,1)
        [MaterialToggle] _Dirt_Add ("Dirt_Add", Float) = 0
        _Decal_Tex ("Decal_Tex", 2D) = "white" {}
        _Decal_Col ("Decal_Col", Color) = (1,1,1,1)
        _Spec_Tex ("Spec_Tex", 2D) = "white" {}
        _Gloss ("Gloss", Range(0, 1)) = 0.8
        _Norm_Tex ("Norm_Tex", 2D) = "bump" {}
        _Norm_Mult ("Norm_Mult", Float) = 1
        _Outline ("Outline", Float) = 0.012
        _Outline_Col ("Outline_Col", Color) = (0.4392157,0.9254903,0.3019608,1)
        _Fresnel ("Fresnel", Float) = 1.75
        _Highlight ("Highlight", Float) = 0
    }
    SubShader {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _Red_Col;
        float4 _Green_Col;
        float4 _Blue_Col;
        float4 _Dirt_Col;
        float _Dirt_Add;
        sampler2D _Decal_Tex;
        float4 _Decal_Col;
        sampler2D _Spec_Tex;
        float _Gloss;
        sampler2D _Norm_Tex;
        float _Norm_Mult;

        struct Input {
            float2 uv_MainTex;
            float2 uv_Decal_Tex;
            float2 uv_Spec_Tex;
            float2 uv_Norm_Tex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 mask = tex2D(_MainTex, IN.uv_MainTex);
            fixed3 channelMix = mask.r * _Red_Col.rgb + mask.g * _Green_Col.rgb + mask.b * _Blue_Col.rgb;
            fixed3 baseCol = channelMix;

            if (_Dirt_Add > 0.5) {
                baseCol = lerp(baseCol, _Dirt_Col.rgb, _Dirt_Col.a);
            }

            fixed4 decalSample = tex2D(_Decal_Tex, IN.uv_Decal_Tex);
            // Check if decal texture is effectively the default (white)
            bool hasDecal = (decalSample.rgb.r < 0.99 || decalSample.rgb.g < 0.99 || decalSample.rgb.b < 0.99 || decalSample.a < 0.99);
            if (hasDecal && decalSample.a > 0.001) {
                fixed3 decalFinal = decalSample.rgb * _Decal_Col.rgb;
                baseCol = lerp(baseCol, decalFinal, decalSample.a);
            }

            o.Albedo = baseCol;
            o.Metallic = tex2D(_Spec_Tex, IN.uv_Spec_Tex).r;
            o.Smoothness = _Gloss;
            fixed4 nrm = tex2D(_Norm_Tex, IN.uv_Norm_Tex);
            o.Normal = UnpackNormal(nrm) * _Norm_Mult;
            o.Alpha = 1.0;
        }
        ENDCG

        Pass {
            Name "OUTLINE"
            Cull Front
            ZWrite Off
            ZTest Less
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Outline;
            float4 _Outline_Col;
            float _Highlight;
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f {
                float4 pos : SV_POSITION;
            };
            v2f vert(appdata v) {
                v2f o;
                float3 norm = normalize(v.normal);
                o.pos = UnityObjectToClipPos(v.vertex + float4(norm * _Outline, 0));
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                if (_Highlight <= 0.0) {
                    return fixed4(0,0,0,0);
                }
                return fixed4(_Outline_Col.rgb, _Outline_Col.a);
            }
            ENDCG
        }
    }
    Fallback "Standard"
    CustomEditor "ShaderForgeMaterialInspector"
}