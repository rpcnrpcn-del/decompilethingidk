Shader "Shader Forge/Grass" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" }
        LOD 200
        Cull Back
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord;
            o.color = v.color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Metallic = 0.0;
            o.Smoothness = 0.5;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
