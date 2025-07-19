Shader "Fixed/GrenadeToolFinal"
{
    Properties
    {
        _MainTex ("Decal (RGBA)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Color ("Base Color Tint", Color) = (1,0,0,1)
        _Metallic ("Metallic", Range(0,1)) = 1
        _Gloss ("Gloss", Range(0,1)) = 0.8
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        Cull Back      // ? normal frontface culling
        ZWrite On      // ? depth writes, proper lighting
        Blend One Zero // ? fully opaque

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        fixed4 _EmissionColor;
        float _Metallic;
        float _Gloss;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 decal = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 normal = tex2D(_BumpMap, IN.uv_BumpMap);

            // Blend decal color with base tint
            fixed3 finalColor = lerp(_Color.rgb, decal.rgb, decal.a);

            o.Albedo = finalColor;
            o.Normal = UnpackNormal(normal);
            o.Metallic = _Metallic;
            o.Smoothness = _Gloss;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = 1; // fully opaque
        }
        ENDCG
    }

    FallBack "Diffuse"
}
