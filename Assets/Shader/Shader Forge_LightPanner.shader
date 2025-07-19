Shader "Shader Forge/LightPanner" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader {
        Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
        Pass {
            Name "FORWARD"
            Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "ForwardBase" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
            Blend One One
            ZWrite Off
            Cull Off
            Fog { Mode Off }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float4 _TimeEditor;

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float4 color : COLOR0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.position = mul(UNITY_MATRIX_MVP, v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f inp) : SV_Target {
                float t = _TimeEditor.y + _Time.y;
                float2 uv = float2(t * 0.02, 0.0);
                uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                fixed4 col = tex2D(_MainTex, uv);
                col.rgb = col.rgb * inp.color.rgb;
                col.rgb = col.rgb * _TintColor.rgb;
                fixed4 result;
                result.rgb = col.rgb * 2.0;
                result.a = 1.0;
                return result;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}