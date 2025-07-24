Shader "Nature/Terrain/Advanced Bumped (Lite)" {
    Properties {
        _Control ("Control (RGBA)", 2D) = "white" {}
        _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        _Bump3 ("Normalmap 3", 2D) = "bump" {}
        _Bump2 ("Normalmap 2", 2D) = "bump" {}
        _Bump1 ("Normalmap 1", 2D) = "bump" {}
        _Bump0 ("Normalmap 0", 2D) = "bump" {}
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.03, 1)) = 0.078125
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        CGPROGRAM
        #pragma surface surf BlinnPhong vertex:vert fullforwardshadows
        #pragma target 3.0

        sampler2D _Control;
        sampler2D _Splat0;
        sampler2D _Splat1;
        sampler2D _Splat2;
        sampler2D _Splat3;
        sampler2D _Bump0;
        sampler2D _Bump1;
        sampler2D _Bump2;
        sampler2D _Bump3;

        fixed4 _Color;
        fixed4 _SpecularColor;
        half _Shininess;

        struct Input {
            float2 uv_Control;
            float2 uv_Splat0;
            float2 uv_Splat1;
            float2 uv_Splat2;
            float2 uv_Splat3;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.uv_Control = v.texcoord.xy;
            o.uv_Splat0 = v.texcoord.xy * 1.0;
            o.uv_Splat1 = v.texcoord.xy * 1.0;
            o.uv_Splat2 = v.texcoord.xy * 1.0;
            o.uv_Splat3 = v.texcoord.xy * 1.0;
        }

        void surf(Input IN, inout SurfaceOutput o) {
            fixed4 splat_control = tex2D(_Control, IN.uv_Control);

            fixed3 col0 = tex2D(_Splat0, IN.uv_Splat0).rgb;
            fixed3 col1 = tex2D(_Splat1, IN.uv_Splat1).rgb;
            fixed3 col2 = tex2D(_Splat2, IN.uv_Splat2).rgb;
            fixed3 col3 = tex2D(_Splat3, IN.uv_Splat3).rgb;

            fixed3 normal0 = UnpackNormal(tex2D(_Bump0, IN.uv_Splat0));
            fixed3 normal1 = UnpackNormal(tex2D(_Bump1, IN.uv_Splat1));
            fixed3 normal2 = UnpackNormal(tex2D(_Bump2, IN.uv_Splat2));
            fixed3 normal3 = UnpackNormal(tex2D(_Bump3, IN.uv_Splat3));

            fixed weight0 = splat_control.r;
            fixed weight1 = splat_control.g;
            fixed weight2 = splat_control.b;
            fixed weight3 = splat_control.a;
            fixed totalWeight = weight0 + weight1 + weight2 + weight3;
            weight0 /= totalWeight;
            weight1 /= totalWeight;
            weight2 /= totalWeight;
            weight3 /= totalWeight;

            o.Albedo = (col0 * weight0 + col1 * weight1 + col2 * weight2 + col3 * weight3) * _Color.rgb;
            o.Normal = normalize(normal0 * weight0 + normal1 * weight1 + normal2 * weight2 + normal3 * weight3);
            o.Specular = _SpecularColor.r;
            o.Gloss = _Shininess;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
