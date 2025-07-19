/*Shader "AG/InvisibleWall" {
	Properties {
		_ImpactColor ("Impact Color", Color) = (0,0,1,1)
	}
	SubShader {
		Pass {
			LOD 200
			Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend One One, One One
			ZClip Off
			GpuProgramID 53260
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 sv_position : SV_Position0;
				float3 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _ImpactColor;
			float4 _ImpactPoint0;
			float4 _ImpactPoint1;
			float4 _ImpactPoint2;
			float4 _ImpactPoint3;
			float4 _ImpactPoint4;
			float4 _ImpactPoint5;
			float4 _ImpactPoint6;
			float4 _ImpactPoint7;
			float _ImpactPointAlpha0;
			float _ImpactPointAlpha1;
			float _ImpactPointAlpha2;
			float _ImpactPointAlpha3;
			float _ImpactPointAlpha4;
			float _ImpactPointAlpha5;
			float _ImpactPointAlpha6;
			float _ImpactPointAlpha7;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.sv_position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xyz = _ImpactPoint1.xyz - inp.texcoord.xyz;
                tmp0.x = dot(tmp0.xyz, tmp0.xyz);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = saturate(tmp0.x / _ImpactPoint1.w);
                tmp0.x = 1.0 - tmp0.x;
                tmp0 = tmp0.xxxx * _ImpactColor;
                tmp0 = tmp0 * _ImpactPointAlpha1.xxxx;
                tmp1.xyz = _ImpactPoint0.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint0.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                tmp0 = tmp1 * _ImpactPointAlpha0.xxxx + tmp0;
                tmp1.xyz = _ImpactPoint2.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint2.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                tmp0 = tmp1 * _ImpactPointAlpha2.xxxx + tmp0;
                tmp1.xyz = _ImpactPoint3.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint3.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                tmp0 = tmp1 * _ImpactPointAlpha3.xxxx + tmp0;
                tmp1.xyz = _ImpactPoint4.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint4.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                tmp0 = tmp1 * _ImpactPointAlpha4.xxxx + tmp0;
                tmp1.xyz = _ImpactPoint5.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint5.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                tmp0 = tmp1 * _ImpactPointAlpha5.xxxx + tmp0;
                tmp1.xyz = _ImpactPoint6.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint6.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                tmp0 = tmp1 * _ImpactPointAlpha6.xxxx + tmp0;
                tmp1.xyz = _ImpactPoint7.xyz - inp.texcoord.xyz;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = saturate(tmp1.x / _ImpactPoint7.w);
                tmp1.x = 1.0 - tmp1.x;
                tmp1 = tmp1.xxxx * _ImpactColor;
                o.sv_target = tmp1 * _ImpactPointAlpha7.xxxx + tmp0;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}*/
Shader "AG/InvisibleWall" {
    Properties {
        _ImpactColor ("Impact Color", Color) = (0,0,1,1)
    }
    SubShader {
        Tags { "QUEUE"="Transparent" "RenderType"="Transparent" }
        Pass {
            LOD 200
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _ImpactColor;
            float4 _ImpactPoint0;
            float4 _ImpactPoint1;
            float4 _ImpactPoint2;
            float4 _ImpactPoint3;
            float4 _ImpactPoint4;
            float4 _ImpactPoint5;
            float4 _ImpactPoint6;
            float4 _ImpactPoint7;
            float _ImpactPointAlpha0;
            float _ImpactPointAlpha1;
            float _ImpactPointAlpha2;
            float _ImpactPointAlpha3;
            float _ImpactPointAlpha4;
            float _ImpactPointAlpha5;
            float _ImpactPointAlpha6;
            float _ImpactPointAlpha7;

            v2f vert (appdata v) {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = world.xyz;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }

            float4 ComputeImpact(float3 wPos, float4 impactPoint, float alpha) {
                float d = distance(wPos, impactPoint.xyz);
                float intensity = saturate(1.0 - (d / impactPoint.w));
                return _ImpactColor * intensity * alpha;
            }

            float4 frag (v2f i) : SV_Target {
                float4 col = 0;
                col += ComputeImpact(i.worldPos, _ImpactPoint0, _ImpactPointAlpha0);
                col += ComputeImpact(i.worldPos, _ImpactPoint1, _ImpactPointAlpha1);
                col += ComputeImpact(i.worldPos, _ImpactPoint2, _ImpactPointAlpha2);
                col += ComputeImpact(i.worldPos, _ImpactPoint3, _ImpactPointAlpha3);
                col += ComputeImpact(i.worldPos, _ImpactPoint4, _ImpactPointAlpha4);
                col += ComputeImpact(i.worldPos, _ImpactPoint5, _ImpactPointAlpha5);
                col += ComputeImpact(i.worldPos, _ImpactPoint6, _ImpactPointAlpha6);
                col += ComputeImpact(i.worldPos, _ImpactPoint7, _ImpactPointAlpha7);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
