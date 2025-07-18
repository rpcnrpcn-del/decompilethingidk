Shader "Shader Forge/Grass" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Texture ("Texture", 2D) = "white" {}
		_OpacityPower ("OpacityPower", Float) = 2
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" }
		Pass {
			Name "FORWARD"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "ForwardBase" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 24598
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
				float4 texcoord10 : TEXCOORD10;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _Color;
			float4 _Texture_ST;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture;
			
			// Keywords: DIRECTIONAL DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.texcoord4.xyz = tmp0.xyz;
                tmp1.xyz = v.tangent.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.tangent.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.tangent.zzz + tmp1.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                o.texcoord5.xyz = tmp1.xyz;
                tmp2.xyz = tmp0.zxy * tmp1.yzx;
                tmp0.xyz = tmp0.yzx * tmp1.zxy + -tmp2.xyz;
                tmp0.xyz = tmp0.xyz * v.tangent.www;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord6.xyz = tmp0.www * tmp0.xyz;
                o.texcoord10 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: DIRECTIONAL DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.w = tmp0.w - 0.5;
                tmp0.w = tmp0.w < 0.0;
                if (tmp0.w) {
                    discard;
                }
                tmp0.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * _WorldSpaceLightPos0.xyz;
                tmp0.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp0.w) {
                    tmp0.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp2.xyz = inp.texcoord3.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp2.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.xxx + tmp2.xyz;
                    tmp2.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.zzz + tmp2.xyz;
                    tmp2.xyz = tmp2.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp2.xyz = tmp0.www ? tmp2.xyz : inp.texcoord3.xyz;
                    tmp2.xyz = tmp2.xyz - unity_ProbeVolumeMin;
                    tmp2.yzw = tmp2.xyz * unity_ProbeVolumeSizeInv;
                    tmp0.w = tmp2.y * 0.25;
                    tmp1.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp2.y = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp0.w = max(tmp0.w, tmp1.w);
                    tmp2.x = min(tmp2.y, tmp0.w);
                    tmp3 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp2.xzw);
                    tmp4.xyz = tmp2.xzw + float3(0.25, 0.0, 0.0);
                    tmp4 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xyz);
                    tmp2.xyz = tmp2.xzw + float3(0.5, 0.0, 0.0);
                    tmp2 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp2.xyz);
                    tmp5.xyz = inp.texcoord4.xyz;
                    tmp5.w = 1.0;
                    tmp3.x = dot(tmp3, tmp5);
                    tmp3.y = dot(tmp4, tmp5);
                    tmp3.z = dot(tmp2, tmp5);
                } else {
                    tmp2.xyz = inp.texcoord4.xyz;
                    tmp2.w = 1.0;
                    tmp3.x = dot(unity_SHAr, tmp2);
                    tmp3.y = dot(unity_SHAg, tmp2);
                    tmp3.z = dot(unity_SHAb, tmp2);
                }
                tmp2.xyz = tmp3.xyz + inp.texcoord10.xyz;
                tmp2.xyz = max(tmp2.xyz, float3(0.0, 0.0, 0.0));
                tmp0.w = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.xyz = tmp0.xyz * _Color.xyz;
                tmp1.xyz = tmp0.www * _LightColor0.xyz + tmp2.xyz;
                o.sv_target.xyz = tmp0.xyz * tmp1.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD_DELTA"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "ForwardAdd" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZClip Off
			Cull Off
			GpuProgramID 113979
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
				float3 texcoord7 : TEXCOORD7;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4x4 unity_WorldToLight;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _Color;
			float4 _Texture_ST;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture;
			sampler2D _LightTexture0;
			
			// Keywords: POINT DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.texcoord3 = tmp0;
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                o.texcoord4.xyz = tmp1.xyz;
                tmp2.xyz = v.tangent.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp2.xyz = unity_ObjectToWorld._m00_m10_m20 * v.tangent.xxx + tmp2.xyz;
                tmp2.xyz = unity_ObjectToWorld._m02_m12_m22 * v.tangent.zzz + tmp2.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                o.texcoord5.xyz = tmp2.xyz;
                tmp3.xyz = tmp1.zxy * tmp2.yzx;
                tmp1.xyz = tmp1.yzx * tmp2.zxy + -tmp3.xyz;
                tmp1.xyz = tmp1.xyz * v.tangent.www;
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                o.texcoord6.xyz = tmp1.www * tmp1.xyz;
                tmp1.xyz = tmp0.yyy * unity_WorldToLight._m01_m11_m21;
                tmp1.xyz = unity_WorldToLight._m00_m10_m20 * tmp0.xxx + tmp1.xyz;
                tmp0.xyz = unity_WorldToLight._m02_m12_m22 * tmp0.zzz + tmp1.xyz;
                o.texcoord7.xyz = unity_WorldToLight._m03_m13_m23 * tmp0.www + tmp0.xyz;
                return o;
			}
			// Keywords: POINT DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.w = tmp0.w - 0.5;
                tmp0.xyz = tmp0.xyz * _Color.xyz;
                tmp0.w = tmp0.w < 0.0;
                if (tmp0.w) {
                    discard;
                }
                tmp1.xyz = _WorldSpaceLightPos0.www * -inp.texcoord3.xyz + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp0.w = max(tmp0.w, 0.0);
                tmp1.x = dot(inp.texcoord7.xyz, inp.texcoord7.xyz);
                tmp1 = tex2D(_LightTexture0, tmp1.xx);
                tmp1.xyz = tmp1.xxx * _LightColor0.xyz;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                o.sv_target.xyz = tmp0.xyz * tmp1.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "SHADOWCASTER"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Offset 1, 1
			GpuProgramID 134550
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float2 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Texture_ST;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture;
			
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                tmp1.x = unity_LightShadowBias.x / tmp0.w;
                tmp1.x = min(tmp1.x, 0.0);
                tmp1.x = max(tmp1.x, -1.0);
                tmp0.z = tmp0.z + tmp1.x;
                tmp1.x = min(tmp0.w, tmp0.z);
                o.position.xyw = tmp0.xyw;
                tmp0.x = tmp1.x - tmp0.z;
                o.position.z = unity_LightShadowBias.y * tmp0.x + tmp0.z;
                o.texcoord1.xy = v.texcoord.xy;
                o.texcoord2.xy = v.texcoord1.xy;
                o.texcoord3.xy = v.texcoord2.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.texcoord4 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                tmp0.xy = inp.texcoord1.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.x = tmp0.w - 0.5;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                o.sv_target = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "Meta" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 210897
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float unity_OneOverOutputBoost;
			float unity_MaxOutputValue;
			float4 _Color;
			float4 _Texture_ST;
			// Custom ConstantBuffers for Vertex Shader
			CBUFFER_START(UnityMetaPass)
				bool4 unity_MetaVertexControl;
			CBUFFER_END
			// Custom ConstantBuffers for Fragment Shader
			CBUFFER_START(UnityMetaPass)
				bool4 unity_MetaFragmentControl;
			CBUFFER_END
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture;
			
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = v.vertex.z > 0.0;
                tmp0.z = tmp0.x ? 0.0001 : 0.0;
                tmp0.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                tmp0.xyz = unity_MetaVertexControl.xxx ? tmp0.xyz : v.vertex.xyz;
                tmp0.w = tmp0.z > 0.0;
                tmp1.z = tmp0.w ? 0.0001 : 0.0;
                tmp1.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                tmp0.xyz = unity_MetaVertexControl.yyy ? tmp1.xyz : tmp0.xyz;
                tmp1 = tmp0.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                tmp0.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.xyz = tmp0.xyz * _Color.xyz;
                tmp0.xyz = log(tmp0.xyz);
                tmp0.w = saturate(unity_OneOverOutputBoost);
                tmp0.xyz = tmp0.xyz * tmp0.www;
                tmp0.xyz = exp(tmp0.xyz);
                tmp0.xyz = min(tmp0.xyz, unity_MaxOutputValue.xxx);
                tmp0.w = 1.0;
                tmp0 = unity_MetaFragmentControl ? tmp0 : float4(0.0, 0.0, 0.0, 0.0);
                o.sv_target = unity_MetaFragmentControl ? float4(0.0, 0.0, 0.0, 0.0235294) : tmp0;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Standard"
	CustomEditor "ShaderForgeMaterialInspector"
}