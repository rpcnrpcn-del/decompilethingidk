Shader "Shader Forge/Avatar/Avatar_RGB_Normal"
{
    Properties
    {
        _MainTex ("Mask_Tex", 2D) = "gray" {}
        _Base_Col ("Base_Col", Color) = (0,0,0,1)
        _Red_Col ("Red_Col", Color) = (1,0,0,1)
        _Green_Col ("Green_Col", Color) = (0,1,0,1)
        _Blue_Col ("Blue_Col", Color) = (0,0,1,1)
        _Dirt_Col ("Dirt_Col", Color) = (1,0.9724138,0,1)
        [MaterialToggle] _Dirt_Add ("Dirt_Add", Float) = 0
        _Decal_Tex ("Decal_Tex", 2D) = "white" {}
        _Decal_Col ("Decal_Col", Color) = (0,1,1,1)
        _Spec_Tex ("Spec_Tex", 2D) = "white" {}
        _Gloss ("Gloss", Range(0,1)) = 0.8
        _Norm_Tex ("Norm_Tex", 2D) = "bump" {}
        _Norm_Mult ("Norm_Mult", Float) = 1
        _Outline ("Outline", Float) = 0.012
        _Outline_Col ("Outline_Col", Color) = (0.4392157,0.9254903,0.3019608,1)
        _Fresnel ("Fresnel", Float) = 1.75
        _Highlight ("Highlight", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _Decal_Tex;
        sampler2D _Spec_Tex;
        sampler2D _Norm_Tex;

        fixed4 _Base_Col;
        fixed4 _Red_Col;
        fixed4 _Green_Col;
        fixed4 _Blue_Col;
        fixed4 _Dirt_Col;
        float _Dirt_Add;
        fixed4 _Decal_Col;
        half _Gloss;
        half _Norm_Mult;
        fixed4 _Outline_Col;
        float _Outline;
        float _Fresnel;
        float _Highlight;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Decal_Tex;
            float2 uv_Spec_Tex;
            float2 uv_Norm_Tex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample main mask
            fixed4 mask = tex2D(_MainTex, IN.uv_MainTex);

            // Channel-mask based color
            fixed3 baseRGB = mask.r * _Red_Col.rgb +
                             mask.g * _Green_Col.rgb +
                             mask.b * _Blue_Col.rgb;

            fixed3 finalColor = lerp(_Base_Col.rgb, baseRGB, mask.a);

            // Optional dirt overlay
            if (_Dirt_Add > 0.5)
            {
                finalColor = lerp(finalColor, _Dirt_Col.rgb, 0.5);
            }
            
            // Check if we have a decal
            fixed4 decalSample = tex2D(_Decal_Tex, IN.uv_Decal_Tex);
            bool hasDecal = (decalSample.rgb.r < 0.99 || decalSample.rgb.g < 0.99 || decalSample.rgb.b < 0.99 || decalSample.a < 0.99);
            if (hasDecal && decalSample.a > 0.001) {
                fixed3 decalFinal = decalSample.rgb * _Decal_Col.rgb;
                baseRGB = lerp(baseRGB, decalFinal, decalSample.a);
            }

            /*// Decal overlay
            fixed4 decalSample = tex2D(_Decal_Tex, IN.uv_Decal_Tex);
            // Only blend where decal alpha > 0
            finalColor = lerp(finalColor, _Decal_Col.rgb * decalSample.rgb, decalSample.a);*/

            // Specular texture controls gloss
            fixed4 spec = tex2D(_Spec_Tex, IN.uv_Spec_Tex);

            // Normal map
            fixed4 nrm = tex2D(_Norm_Tex, IN.uv_Norm_Tex);
            o.Normal = UnpackNormal(nrm) * _Norm_Mult;

            // Output
            o.Albedo = finalColor;
            o.Smoothness = _Gloss * spec.r;
            o.Metallic = 0;
            o.Occlusion = 1;
            o.Emission = 0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
