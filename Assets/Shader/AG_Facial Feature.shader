Shader "AG/Facial Feature" {
    Properties {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass {
            Tags { "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend Zero SrcColor
            ZWrite Off
            Cull Off
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.position = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.color = saturate(v.color);
                return o;
            }

            fixed4 frag(v2f inp) : SV_Target {
                fixed4 tex = tex2D(_MainTex, inp.texcoord);
                fixed4 tmp0 = tex * inp.color + fixed4(-1.0, -1.0, -1.0, -1.0);
                fixed alpha = tex.a * inp.color.a;
                fixed4 result = alpha.xxxx * tmp0 + fixed4(1.0, 1.0, 1.0, 1.0);
                return result;
            }
            ENDCG
        }
    }
    FallBack Off
}
