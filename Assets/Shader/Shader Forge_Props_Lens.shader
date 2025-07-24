Shader "Shader Forge/Props/Lens" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _NormalTex ("Normal Texture", 2D) = "bump" {}
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (1,1,1,1)
        _Gloss ("Glossiness", Range(0,1)) = 0.5
        _Distort ("Distortion Strength", Range(0,1)) = 0.1
        _Fresnel ("Fresnel Power", Range(0,5)) = 1.0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;
        sampler2D _NormalTex;
        fixed4 _Tint;
        half _Gloss;
        half _Distort;
        half _Fresnel;

        struct Input {
            float2 uv_MainTex;
            float2 uv_MaskTex;
            float2 uv_NormalTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Tint;
            fixed4 m = tex2D(_MaskTex, IN.uv_MaskTex);

            fixed3 normalSample = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
            o.Normal = normalSample;

            half NdotV = saturate(dot(normalize(o.Normal), normalize(IN.viewDir)));
            half fresnelTerm = pow((1.0h - NdotV), _Fresnel);

            o.Albedo = c.rgb * m.r;
            o.Metallic = 0.0;
            o.Smoothness = _Gloss;
            o.Emission = c.rgb * fresnelTerm * m.g;
            o.Alpha = c.a * m.a;
        }
        ENDCG
    }
    Fallback Off
}
