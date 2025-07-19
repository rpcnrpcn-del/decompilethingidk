Shader "Shader Forge/Leaf (Transparent)"
{
    Properties
    {
        _MainTex ("Albedo (RGB) Alpha (A)", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Gloss ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Occlusion ("Occlusion", Range(0,1)) = 1.0
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _EmissionMap;

        fixed4 _Color;
        half _Gloss;
        half _Metallic;
        half _Occlusion;
        fixed4 _EmissionColor;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_EmissionMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 bump = tex2D(_BumpMap, IN.uv_BumpMap);
            fixed3 emit = tex2D(_EmissionMap, IN.uv_EmissionMap).rgb;

            o.Albedo = albedo.rgb;
            o.Normal = UnpackNormal(bump);
            o.Metallic = _Metallic;
            o.Smoothness = _Gloss;
            o.Occlusion = _Occlusion;
            o.Emission = emit * _EmissionColor.rgb;

            // Use texture alpha as transparency
            o.Alpha = albedo.a;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
