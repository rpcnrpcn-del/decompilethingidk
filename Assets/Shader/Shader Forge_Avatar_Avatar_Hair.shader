Shader "Shader Forge/Avatar_Hair" {
    Properties {
        _Col_Tex ("Col_Tex", 2D) = "white" {}
        _Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1)
        _Spec_Tex ("Spec_Tex", 2D) = "white" {}
        _Gloss ("Gloss", Range(0,1)) = 0.5
        _BumpMap ("Normal_Tex", 2D) = "bump" {}
        _Normal_Mult ("Normal_Mult", Float) = 1.0
        _BumpMap2 ("Normal_Detail", 2D) = "bump" {}
        _Normal_Detail_Mult ("Normal_Detail_Mult", Float) = 1.0
        _Highlight ("Highlight", Float) = 1.0
    }
    SubShader {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        Cull Back
        ZWrite On
        Lighting On

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _Col_Tex;
        fixed4 _Color;
        sampler2D _BumpMap;
        float _Normal_Mult;
        sampler2D _BumpMap2;
        float _Normal_Detail_Mult;
        sampler2D _Spec_Tex;
        half _Gloss;
        half _Highlight;

        struct Input {
            float2 uv_Col_Tex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 albedo = tex2D(_Col_Tex, IN.uv_Col_Tex) * _Color;

            float3 n1 = UnpackNormal(tex2D(_BumpMap, IN.uv_Col_Tex)) * _Normal_Mult;
            float3 n2 = UnpackNormal(tex2D(_BumpMap2, IN.uv_Col_Tex)) * _Normal_Detail_Mult;
            float3 nCombined = normalize(n1 + n2);

            o.Albedo = albedo.rgb;
            o.Normal = nCombined;
            o.Smoothness = _Gloss;
            o.Metallic = 0.0;
            // Removed incorrect usage of o.Specular, Standard lighting uses Metallic/Smoothness
        }
        ENDCG
    }
    FallBack "Diffuse"
}