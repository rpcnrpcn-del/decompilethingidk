Shader "FX/Glass/Stained BumpDistort" {
	Properties {
		_BumpAmt ("Distortion", Range(0, 128)) = 10
		_MainTex ("Tint Color (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "QUEUE" = "Transparent" "RenderType" = "Opaque" }
		GrabPass {
		}
		Pass {
			Name "BASE"
			Tags { "LIGHTMODE" = "Always" "QUEUE" = "Transparent" "RenderType" = "Opaque" }
			ZClip Off
			GpuProgramID 41578
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _BumpMap_ST;
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _BumpAmt;
			float4 _GrabTexture_TexelSize;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _GrabTexture;
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.position = tmp0;
                tmp0.xy = tmp0.xy * float2(1.0, -1.0) + tmp0.ww;
                o.texcoord.zw = tmp0.zw;
                o.texcoord.xy = tmp0.xy * float2(0.5, 0.5);
                o.texcoord1.xy = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_BumpMap, inp.texcoord1.xy);
                tmp0.xy = tmp0.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.xy = tmp0.xy * _BumpAmt.xx;
                tmp0.xy = tmp0.xy * _GrabTexture_TexelSize.xy;
                tmp0.z = inp.texcoord.z / _ProjectionParams.y;
                tmp0.z = 1.0 - tmp0.z;
                tmp0.z = tmp0.z * _ProjectionParams.z;
                tmp0.z = max(tmp0.z, 0.0);
                tmp0.xy = tmp0.xy * tmp0.zz + inp.texcoord.xy;
                tmp0.xy = tmp0.xy / inp.texcoord.ww;
                tmp0 = tex2D(_GrabTexture, tmp0.xy);
                tmp1 = tex2D(_MainTex, inp.texcoord2.xy);
                o.sv_target = tmp0 * tmp1;
                return o;
			}
			ENDCG
		}
	}
	SubShader {
		Tags { "QUEUE" = "Transparent" "RenderType" = "Opaque" }
		Pass {
			Name "BASE"
			Tags { "QUEUE" = "Transparent" "RenderType" = "Opaque" }
			Blend DstColor Zero, DstColor Zero
			ZClip Off
			Fog {
				Mode 0
			}
			GpuProgramID 93607
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                o.color = float4(0.0, 0.0, 0.0, 1.0);
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = tex2D(_MainTex, inp.texcoord.xy);
                return o;
			}
			ENDCG
		}
	}
}