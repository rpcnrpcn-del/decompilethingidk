Shader "Shader Forge/PBRDecal" {
    Properties {
        _Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1)
        _MainTex ("Base Color", 2D) = "white" {}
        _Gloss ("Gloss", Range(0,1)) = 0.8
        _DecalTex ("Decal", 2D) = "white" {}
        _SpecTex ("Spec", 2D) = "white" {}
        _NormalTex ("Normal", 2D) = "bump" {}
        _Normal ("Normal", Float) = 1
        _EmissionColor ("EmissionColor", Color) = (0,0,0,1)
        _DecalScale ("Decal Scale", Range(0.1, 5.0)) = 1.25 // adjusted scale to better match reference size
        [HideInInspector]_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _DecalTex;
        sampler2D _SpecTex;
        sampler2D _NormalTex;
        fixed4 _Color;
        half _Gloss;
        half _Normal;
        fixed4 _EmissionColor;
        float _DecalScale;

        struct Input {
            float2 uv_MainTex;
            float2 uv2_DecalTex; // use secondary UV channel for decals to match original setup
            float2 uv_SpecTex;
            float2 uv_NormalTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 baseCol = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            // Apply scaling to decal UVs for size adjustment
            float2 scaledDecalUV = (IN.uv2_DecalTex - 0.5) / _DecalScale + 0.5;
            fixed4 decalCol = tex2D(_DecalTex, scaledDecalUV);
            baseCol.rgb = lerp(baseCol.rgb, decalCol.rgb, decalCol.a);

            o.Albedo = baseCol.rgb;
            o.Metallic = tex2D(_SpecTex, IN.uv_SpecTex).r;
            o.Smoothness = _Gloss;
            o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex)) * _Normal;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = baseCol.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}