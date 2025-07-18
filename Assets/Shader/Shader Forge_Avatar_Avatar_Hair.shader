Shader "Shader Forge/Avatar/Avatar_Hair" {
	Properties {
		_Col_Tex ("Col_Tex", 2D) = "white" {}
		_Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1)
		_Dirt_Mult ("Dirt_Mult", Float) = 0.27
		_Dirt_Exp ("Dirt_Exp", Float) = 1.59
		_Spec_Tex ("Spec_Tex", 2D) = "white" {}
		_Gloss ("Gloss", Range(0, 1)) = 0.561
		_BumpMap ("Normal_Tex", 2D) = "bump" {}
		_Normal_Mult ("Normal_Mult", Float) = 0.38
		_BumpMap2 ("Normal_Detail", 2D) = "bump" {}
		_Normal_Detail_Mult ("Normal_Detail_Mult", Float) = 0.72
		_Outline ("Outline", Float) = 0.012
		_Outline_Col ("Outline_Col", Color) = (0.4392157,0.9254903,0.3019608,1)
		_Fresnel ("Fresnel", Float) = 1.75
		_Highlight ("Highlight", Float) = 0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Name "OUTLINE"
			Tags { "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Front
			GpuProgramID 40173
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _TimeEditor;
			float _Highlight;
			float _Outline;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Outline_Col;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = _TimeEditor.w + _Time.w;
                tmp0.x = sin(tmp0.x);
                tmp0.x = tmp0.x * 0.3 + 1.3;
                tmp0.x = tmp0.x * _Outline;
                tmp0.x = tmp0.x * _Highlight;
                tmp0.y = dot(v.vertex, v.vertex);
                tmp0.y = rsqrt(tmp0.y);
                tmp0.yzw = tmp0.yyy * v.vertex.xyz;
                tmp0.xyz = tmp0.yzw * tmp0.xxx + v.vertex.xyz;
                tmp1 = tmp0.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord1.xy;
                o.texcoord1.xy = v.texcoord2.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.texcoord2 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                o.sv_target.xyz = _Outline_Col.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 98231
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
				float4 color : COLOR0;
				float4 texcoord10 : TEXCOORD10;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _TimeEditor;
			float4 _Color;
			float4 _Col_Tex_ST;
			float4 _BumpMap_ST;
			float _Gloss;
			float _Dirt_Exp;
			float4 _Spec_Tex_ST;
			float _Normal_Mult;
			float4 _BumpMap2_ST;
			float _Normal_Detail_Mult;
			float _Dirt_Mult;
			float _Highlight;
			float4 _Outline_Col;
			float _Fresnel;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _BumpMap2;
			sampler2D _Spec_Tex;
			sampler2D _Col_Tex;
			
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
                o.color = v.color;
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
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                float4 tmp9;
                float4 tmp10;
                float4 tmp11;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2.xy = inp.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
                tmp2 = tex2D(_BumpMap, tmp2.xy);
                tmp2.xy = tmp2.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp2.xy, tmp2.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp3.z = sqrt(tmp1.w);
                tmp2.zw = inp.texcoord.xy * _BumpMap2_ST.xy + _BumpMap2_ST.zw;
                tmp4 = tex2D(_BumpMap2, tmp2.zw);
                tmp2.zw = tmp4.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp2.xy, tmp2.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp4.z = sqrt(tmp1.w);
                tmp3.xy = tmp2.xy * _Normal_Mult.xx;
                tmp3.xyz = tmp3.xyz + float3(0.0, 0.0, 1.0);
                tmp4.xy = tmp2.zw * _Normal_Detail_Mult.xx;
                tmp2.xyz = tmp4.xyz * float3(-1.0, -1.0, 1.0);
                tmp1.w = dot(tmp3.xyz, tmp2.xyz);
                tmp2.xyz = tmp1.www * tmp3.xyz;
                tmp2.xyz = tmp2.xyz / tmp3.zzz;
                tmp2.xyz = -tmp4.xyz * float3(-1.0, -1.0, 1.0) + tmp2.xyz;
                tmp3.xyz = tmp2.yyy * inp.texcoord6.xyz;
                tmp2.xyw = tmp2.xxx * inp.texcoord5.xyz + tmp3.xyz;
                tmp2.xyz = tmp2.zzz * inp.texcoord4.xyz + tmp2.xyw;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp1.w = dot(-tmp1.xyz, tmp2.xyz);
                tmp1.w = tmp1.w + tmp1.w;
                tmp3.xyz = tmp2.xyz * -tmp1.www + -tmp1.xyz;
                tmp1.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp1.www * _WorldSpaceLightPos0.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp4.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = _Gloss * 10.0 + 1.0;
                tmp0.w = exp(tmp0.w);
                tmp1.w = 1.0 - _Gloss;
                tmp3.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp3.w) {
                    tmp3.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp5.xyz = inp.texcoord3.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp5.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.xxx + tmp5.xyz;
                    tmp5.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.zzz + tmp5.xyz;
                    tmp5.xyz = tmp5.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp5.xyz = tmp3.www ? tmp5.xyz : inp.texcoord3.xyz;
                    tmp5.xyz = tmp5.xyz - unity_ProbeVolumeMin;
                    tmp5.yzw = tmp5.xyz * unity_ProbeVolumeSizeInv;
                    tmp3.w = tmp5.y * 0.25;
                    tmp4.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp5.y = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp3.w = max(tmp3.w, tmp4.w);
                    tmp5.x = min(tmp5.y, tmp3.w);
                    tmp6 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp5.xzw);
                    tmp7.xyz = tmp5.xzw + float3(0.25, 0.0, 0.0);
                    tmp7 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp7.xyz);
                    tmp5.xyz = tmp5.xzw + float3(0.5, 0.0, 0.0);
                    tmp5 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp5.xyz);
                    tmp2.w = 1.0;
                    tmp6.x = dot(tmp6, tmp2);
                    tmp6.y = dot(tmp7, tmp2);
                    tmp6.z = dot(tmp5, tmp2);
                } else {
                    tmp2.w = 1.0;
                    tmp6.x = dot(unity_SHAr, tmp2);
                    tmp6.y = dot(unity_SHAg, tmp2);
                    tmp6.z = dot(unity_SHAb, tmp2);
                }
                tmp5.xyz = tmp6.xyz + inp.texcoord10.xyz;
                tmp5.xyz = max(tmp5.xyz, float3(0.0, 0.0, 0.0));
                tmp2.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp2.w) {
                    tmp2.w = dot(tmp3.xyz, tmp3.xyz);
                    tmp2.w = rsqrt(tmp2.w);
                    tmp6.xyz = tmp2.www * tmp3.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord3.xyz;
                    tmp7.xyz = tmp7.xyz / tmp6.xyz;
                    tmp8.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord3.xyz;
                    tmp8.xyz = tmp8.xyz / tmp6.xyz;
                    tmp9.xyz = tmp6.xyz > float3(0.0, 0.0, 0.0);
                    tmp7.xyz = tmp9.xyz ? tmp7.xyz : tmp8.xyz;
                    tmp2.w = min(tmp7.y, tmp7.x);
                    tmp2.w = min(tmp7.z, tmp2.w);
                    tmp7.xyz = inp.texcoord3.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp6.xyz = tmp6.xyz * tmp2.www + tmp7.xyz;
                } else {
                    tmp6.xyz = tmp3.xyz;
                }
                tmp2.w = tmp1.w * 0.7978846;
                tmp7.xy = -tmp1.ww * float2(0.7, 0.7978846) + float2(1.7, 1.0);
                tmp3.w = tmp1.w * tmp7.x;
                tmp3.w = tmp3.w * 6.0;
                tmp6 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp6.xyz, tmp3.w));
                tmp4.w = unity_SpecCube0_HDR.w == 1.0;
                tmp5.w = log(tmp6.w);
                tmp5.w = tmp5.w * unity_SpecCube0_HDR.y;
                tmp5.w = exp(tmp5.w);
                tmp4.w = tmp4.w ? tmp5.w : 1.0;
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.x;
                tmp7.xzw = tmp6.xyz * tmp4.www;
                tmp5.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp5.w) {
                    tmp5.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp5.w) {
                        tmp5.w = dot(tmp3.xyz, tmp3.xyz);
                        tmp5.w = rsqrt(tmp5.w);
                        tmp8.xyz = tmp3.xyz * tmp5.www;
                        tmp9.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord3.xyz;
                        tmp9.xyz = tmp9.xyz / tmp8.xyz;
                        tmp10.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord3.xyz;
                        tmp10.xyz = tmp10.xyz / tmp8.xyz;
                        tmp11.xyz = tmp8.xyz > float3(0.0, 0.0, 0.0);
                        tmp9.xyz = tmp11.xyz ? tmp9.xyz : tmp10.xyz;
                        tmp5.w = min(tmp9.y, tmp9.x);
                        tmp5.w = min(tmp9.z, tmp5.w);
                        tmp9.xyz = inp.texcoord3.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp3.xyz = tmp8.xyz * tmp5.www + tmp9.xyz;
                    }
                    tmp3 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp3.xyz, tmp3.w));
                    tmp5.w = unity_SpecCube1_HDR.w == 1.0;
                    tmp3.w = log(tmp3.w);
                    tmp3.w = tmp3.w * unity_SpecCube1_HDR.y;
                    tmp3.w = exp(tmp3.w);
                    tmp3.w = tmp5.w ? tmp3.w : 1.0;
                    tmp3.w = tmp3.w * unity_SpecCube1_HDR.x;
                    tmp3.xyz = tmp3.xyz * tmp3.www;
                    tmp6.xyz = tmp4.www * tmp6.xyz + -tmp3.xyz;
                    tmp7.xzw = unity_SpecCube0_BoxMin.www * tmp6.xyz + tmp3.xyz;
                }
                tmp3.x = dot(tmp2.xyz, tmp4.xyz);
                tmp3.y = dot(tmp4.xyz, tmp0.xyz);
                tmp3.xy = max(tmp3.xy, float2(0.0, 0.0));
                tmp3.zw = inp.texcoord.xy * _Spec_Tex_ST.xy + _Spec_Tex_ST.zw;
                tmp4 = tex2D(_Spec_Tex, tmp3.zw);
                tmp3.z = max(tmp4.y, tmp4.x);
                tmp3.z = max(tmp4.z, tmp3.z);
                tmp1.x = dot(tmp2.xyz, tmp1.xyz);
                tmp1.x = max(tmp1.x, 0.0);
                tmp0.x = dot(tmp2.xyz, tmp0.xyz);
                tmp0.y = tmp3.x * tmp7.y + tmp2.w;
                tmp0.z = tmp1.x * tmp7.y + tmp2.w;
                tmp0.y = tmp0.y * tmp0.z + 0.00001;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.y = tmp0.y * 0.25;
                tmp0.z = tmp1.w * tmp1.w;
                tmp0.z = tmp0.z * tmp0.z;
                tmp0.xz = max(tmp0.xz, float2(0.0, 0.0001));
                tmp0.z = 2.0 / tmp0.z;
                tmp0.z = tmp0.z - 2.0;
                tmp0.z = max(tmp0.z, 0.0001);
                tmp1.y = tmp0.z + 2.0;
                tmp1.y = tmp1.y * 0.1591549;
                tmp0.x = log(tmp0.x);
                tmp0.z = tmp0.x * tmp0.z;
                tmp0.z = exp(tmp0.z);
                tmp0.z = tmp1.y * tmp0.z;
                tmp0.y = tmp0.y * tmp3.x;
                tmp0.xy = tmp0.xz * tmp0.wy;
                tmp0.y = tmp0.y * 0.7853982;
                tmp0.y = max(tmp0.y, 0.0);
                tmp0.x = exp(tmp0.x);
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.xyz = tmp0.xxx * _LightColor0.xyz;
                tmp0.w = 1.0 - tmp3.y;
                tmp1.y = tmp0.w * tmp0.w;
                tmp1.y = tmp1.y * tmp1.y;
                tmp0.w = tmp0.w * tmp1.y;
                tmp2.xyz = float3(1.0, 1.0, 1.0) - tmp4.xyz;
                tmp2.xyz = tmp2.xyz * tmp0.www + tmp4.xyz;
                tmp0.w = saturate(tmp3.z + _Gloss);
                tmp6.xyz = tmp7.xzw * inp.color.xxx;
                tmp1.xy = float2(1.0, 1.00001) - tmp1.xx;
                tmp7.xy = tmp1.xy * tmp1.xy;
                tmp7.xy = tmp7.xy * tmp7.xy;
                tmp1.yz = tmp1.xy * tmp7.xy;
                tmp7.xyz = tmp0.www - tmp4.xyz;
                tmp4.xyz = tmp1.yyy * tmp7.xyz + tmp4.xyz;
                tmp4.xyz = tmp4.xyz * tmp6.xyz;
                tmp0.xyz = tmp0.xyz * tmp2.xyz + tmp4.xyz;
                tmp0.w = tmp3.y + tmp3.y;
                tmp0.w = tmp3.y * tmp0.w;
                tmp0.w = tmp0.w * tmp1.w + -0.5;
                tmp1.y = 1.00001 - tmp3.x;
                tmp1.w = tmp1.y * tmp1.y;
                tmp1.w = tmp1.w * tmp1.w;
                tmp1.y = tmp1.w * tmp1.y;
                tmp1.y = tmp0.w * tmp1.y + 1.0;
                tmp0.w = tmp0.w * tmp1.z + 1.0;
                tmp0.w = tmp0.w * tmp1.y;
                tmp0.w = tmp3.x * tmp0.w;
                tmp1.yz = inp.texcoord.xy * _Col_Tex_ST.xy + _Col_Tex_ST.zw;
                tmp2 = tex2D(_Col_Tex, tmp1.yz);
                tmp1.yzw = _Color.xyz * _Dirt_Mult.xxx;
                tmp3.xyw = log(inp.color.xyz);
                tmp3.xyw = tmp3.xyw * _Dirt_Exp.xxx;
                tmp3.xyw = exp(tmp3.xyw);
                tmp4.xyz = -_Dirt_Mult.xxx * _Color.xyz + _Color.xyz;
                tmp1.yzw = tmp3.xyw * tmp4.xyz + tmp1.yzw;
                tmp1.yzw = tmp1.yzw * tmp2.xyz;
                tmp2.x = 1.0 - tmp3.z;
                tmp1.yzw = tmp1.yzw * tmp2.xxx;
                tmp2.xyz = tmp0.www * _LightColor0.xyz + tmp5.xyz;
                tmp0.w = _TimeEditor.w + _Time.w;
                tmp0.w = sin(tmp0.w);
                tmp0.w = tmp0.w * 0.3 + 1.3;
                tmp1.x = log(tmp1.x);
                tmp1.x = tmp1.x * _Fresnel;
                tmp1.x = exp(tmp1.x);
                tmp1.x = tmp1.x * 0.81;
                tmp3.xyz = tmp1.xxx * _Outline_Col.xyz;
                tmp3.xyz = tmp0.www * tmp3.xyz;
                tmp0.xyz = tmp2.xyz * tmp1.yzw + tmp0.xyz;
                o.sv_target.xyz = tmp3.xyz * _Highlight.xxx + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD_DELTA"
			Tags { "LIGHTMODE" = "ForwardAdd" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZClip Off
			Cull Off
			GpuProgramID 149227
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
				float4 color : COLOR0;
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
			float4 _Col_Tex_ST;
			float4 _BumpMap_ST;
			float _Gloss;
			float _Dirt_Exp;
			float4 _Spec_Tex_ST;
			float _Normal_Mult;
			float4 _BumpMap2_ST;
			float _Normal_Detail_Mult;
			float _Dirt_Mult;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _BumpMap2;
			sampler2D _LightTexture0;
			sampler2D _Spec_Tex;
			sampler2D _Col_Tex;
			
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
                o.color = v.color;
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
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xy = inp.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
                tmp0 = tex2D(_BumpMap, tmp0.xy);
                tmp0.xy = tmp0.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.z = dot(tmp0.xy, tmp0.xy);
                tmp1.xy = tmp0.xy * _Normal_Mult.xx;
                tmp0.x = min(tmp0.z, 1.0);
                tmp0.x = 1.0 - tmp0.x;
                tmp1.z = sqrt(tmp0.x);
                tmp0.xyz = tmp1.xyz + float3(0.0, 0.0, 1.0);
                tmp1.xy = inp.texcoord.xy * _BumpMap2_ST.xy + _BumpMap2_ST.zw;
                tmp1 = tex2D(_BumpMap2, tmp1.xy);
                tmp1.xy = tmp1.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.w = dot(tmp1.xy, tmp1.xy);
                tmp1.xy = tmp1.xy * _Normal_Detail_Mult.xx;
                tmp0.w = min(tmp0.w, 1.0);
                tmp0.w = 1.0 - tmp0.w;
                tmp1.z = sqrt(tmp0.w);
                tmp2.xyz = tmp1.xyz * float3(-1.0, -1.0, 1.0);
                tmp0.w = dot(tmp0.xyz, tmp2.xyz);
                tmp0.xyw = tmp0.www * tmp0.xyz;
                tmp0.xyz = tmp0.xyw / tmp0.zzz;
                tmp0.xyz = -tmp1.xyz * float3(-1.0, -1.0, 1.0) + tmp0.xyz;
                tmp1.xyz = tmp0.yyy * inp.texcoord6.xyz;
                tmp0.xyw = tmp0.xxx * inp.texcoord5.xyz + tmp1.xyz;
                tmp0.xyz = tmp0.zzz * inp.texcoord4.xyz + tmp0.xyw;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = _WorldSpaceLightPos0.www * -inp.texcoord3.xyz + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp3.xyz = tmp2.xyz * tmp0.www + tmp1.xyz;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp2.xyz);
                tmp1.w = dot(tmp3.xyz, tmp3.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp2.xyz = tmp1.www * tmp3.xyz;
                tmp1.w = dot(tmp0.xyz, tmp2.xyz);
                tmp0.x = dot(tmp0.xyz, tmp1.xyz);
                tmp0.y = dot(tmp1.xyz, tmp2.xyz);
                tmp0.xyw = max(tmp0.xyw, float3(0.0, 0.0, 0.0));
                tmp0.z = max(tmp1.w, 0.0);
                tmp0.z = log(tmp0.z);
                tmp1.x = 1.0 - _Gloss;
                tmp1.y = tmp1.x * tmp1.x;
                tmp1.y = tmp1.y * tmp1.y;
                tmp1.y = max(tmp1.y, 0.0001);
                tmp1.y = 2.0 / tmp1.y;
                tmp1.y = tmp1.y - 2.0;
                tmp1.y = max(tmp1.y, 0.0001);
                tmp1.z = tmp0.z * tmp1.y;
                tmp1.y = tmp1.y + 2.0;
                tmp1.y = tmp1.y * 0.1591549;
                tmp1.z = exp(tmp1.z);
                tmp1.y = tmp1.y * tmp1.z;
                tmp1.z = tmp1.x * 0.7978846;
                tmp1.w = -tmp1.x * 0.7978846 + 1.0;
                tmp2.x = tmp0.w * tmp1.w + tmp1.z;
                tmp1.z = tmp0.x * tmp1.w + tmp1.z;
                tmp1.z = tmp1.z * tmp2.x + 0.00001;
                tmp1.z = 1.0 / tmp1.z;
                tmp1.z = tmp1.z * 0.25;
                tmp1.z = tmp0.x * tmp1.z;
                tmp1.y = tmp1.y * tmp1.z;
                tmp1.y = tmp1.y * 0.7853982;
                tmp1.y = max(tmp1.y, 0.0);
                tmp1.z = _Gloss * 10.0 + 1.0;
                tmp1.z = exp(tmp1.z);
                tmp0.z = tmp0.z * tmp1.z;
                tmp0.z = exp(tmp0.z);
                tmp1.z = dot(inp.texcoord7.xyz, inp.texcoord7.xyz);
                tmp2 = tex2D(_LightTexture0, tmp1.zz);
                tmp2.xyz = tmp2.xxx * _LightColor0.xyz;
                tmp3.xyz = tmp0.zzz * tmp2.xyz;
                tmp1.yzw = tmp1.yyy * tmp3.xyz;
                tmp1.yzw = tmp1.yzw * _LightColor0.xyz;
                tmp0.zw = float2(1.0, 1.00001) - tmp0.yw;
                tmp2.w = tmp0.z * tmp0.z;
                tmp2.w = tmp2.w * tmp2.w;
                tmp0.z = tmp0.z * tmp2.w;
                tmp3.xy = inp.texcoord.xy * _Spec_Tex_ST.xy + _Spec_Tex_ST.zw;
                tmp3 = tex2D(_Spec_Tex, tmp3.xy);
                tmp4.xyz = float3(1.0, 1.0, 1.0) - tmp3.xyz;
                tmp4.xyz = tmp4.xyz * tmp0.zzz + tmp3.xyz;
                tmp1.yzw = tmp1.yzw * tmp4.xyz;
                tmp0.z = tmp0.y + tmp0.y;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.y = tmp0.y * tmp1.x + -0.5;
                tmp0.z = tmp0.w * tmp0.w;
                tmp0.z = tmp0.z * tmp0.z;
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.z = tmp0.y * tmp0.z + 1.0;
                tmp0.w = 1.00001 - tmp0.x;
                tmp1.x = tmp0.w * tmp0.w;
                tmp1.x = tmp1.x * tmp1.x;
                tmp0.w = tmp0.w * tmp1.x;
                tmp0.y = tmp0.y * tmp0.w + 1.0;
                tmp0.y = tmp0.z * tmp0.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.xyz = tmp2.xyz * tmp0.xxx;
                tmp0.w = max(tmp3.y, tmp3.x);
                tmp0.w = max(tmp3.z, tmp0.w);
                tmp0.w = 1.0 - tmp0.w;
                tmp2.xyz = log(inp.color.xyz);
                tmp2.xyz = tmp2.xyz * _Dirt_Exp.xxx;
                tmp2.xyz = exp(tmp2.xyz);
                tmp3.xyz = _Color.xyz * _Dirt_Mult.xxx;
                tmp4.xyz = -_Dirt_Mult.xxx * _Color.xyz + _Color.xyz;
                tmp2.xyz = tmp2.xyz * tmp4.xyz + tmp3.xyz;
                tmp3.xy = inp.texcoord.xy * _Col_Tex_ST.xy + _Col_Tex_ST.zw;
                tmp3 = tex2D(_Col_Tex, tmp3.xy);
                tmp2.xyz = tmp2.xyz * tmp3.xyz;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                o.sv_target.xyz = tmp0.xyz * tmp2.xyz + tmp1.yzw;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "LIGHTMODE" = "Meta" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 229561
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
				float4 color : COLOR0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float unity_OneOverOutputBoost;
			float unity_MaxOutputValue;
			float unity_UseLinearSpace;
			float4 _TimeEditor;
			float4 _Color;
			float4 _Col_Tex_ST;
			float _Gloss;
			float _Dirt_Exp;
			float4 _Spec_Tex_ST;
			float _Dirt_Mult;
			float _Highlight;
			float4 _Outline_Col;
			float _Fresnel;
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
			sampler2D _Col_Tex;
			sampler2D _Spec_Tex;
			
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
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord4.xyz = tmp0.www * tmp0.xyz;
                o.color = v.color;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xyz = log(inp.color.xyz);
                tmp0.xyz = tmp0.xyz * _Dirt_Exp.xxx;
                tmp0.xyz = exp(tmp0.xyz);
                tmp1.xyz = _Color.xyz * _Dirt_Mult.xxx;
                tmp2.xyz = -_Dirt_Mult.xxx * _Color.xyz + _Color.xyz;
                tmp0.xyz = tmp0.xyz * tmp2.xyz + tmp1.xyz;
                tmp1.xy = inp.texcoord.xy * _Col_Tex_ST.xy + _Col_Tex_ST.zw;
                tmp1 = tex2D(_Col_Tex, tmp1.xy);
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp1.xy = inp.texcoord.xy * _Spec_Tex_ST.xy + _Spec_Tex_ST.zw;
                tmp1 = tex2D(_Spec_Tex, tmp1.xy);
                tmp0.w = max(tmp1.y, tmp1.x);
                tmp0.w = max(tmp1.z, tmp0.w);
                tmp0.w = 1.0 - tmp0.w;
                tmp1.w = 1.0 - _Gloss;
                tmp1.w = tmp1.w * tmp1.w;
                tmp1.xyz = tmp1.xyz * tmp1.www;
                tmp1.xyz = tmp1.xyz * float3(0.5, 0.5, 0.5);
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp1.xyz;
                tmp0.xyz = log(tmp0.xyz);
                tmp0.w = saturate(unity_OneOverOutputBoost);
                tmp0.xyz = tmp0.xyz * tmp0.www;
                tmp0.xyz = exp(tmp0.xyz);
                tmp0.xyz = min(tmp0.xyz, unity_MaxOutputValue.xxx);
                tmp0.w = 1.0;
                tmp0 = unity_MetaFragmentControl ? tmp0 : float4(0.0, 0.0, 0.0, 0.0);
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp1.x = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp1.x = max(tmp1.x, 0.0);
                tmp1.x = 1.0 - tmp1.x;
                tmp1.x = log(tmp1.x);
                tmp1.x = tmp1.x * _Fresnel;
                tmp1.x = exp(tmp1.x);
                tmp1.x = tmp1.x * 0.81;
                tmp1.xyz = tmp1.xxx * _Outline_Col.xyz;
                tmp1.w = _TimeEditor.w + _Time.w;
                tmp1.w = sin(tmp1.w);
                tmp1.w = tmp1.w * 0.3 + 1.3;
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp1.xyz = tmp1.xyz * _Highlight.xxx;
                tmp2.xyz = tmp1.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp2.xyz = tmp1.xyz * tmp2.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp2.xyz = tmp1.xyz * tmp2.xyz;
                tmp1.w = unity_UseLinearSpace != 0.0;
                tmp1.xyz = tmp1.www ? tmp1.xyz : tmp2.xyz;
                tmp1.xyz = tmp1.xyz * float3(0.0103093, 0.0103093, 0.0103093);
                tmp1.w = max(tmp1.y, tmp1.x);
                tmp2.x = max(tmp1.z, 0.02);
                tmp1.w = max(tmp1.w, tmp2.x);
                tmp1.w = tmp1.w * 255.0;
                tmp1.w = ceil(tmp1.w);
                tmp2.w = tmp1.w * 0.0039216;
                tmp2.xyz = tmp1.xyz / tmp2.www;
                o.sv_target = unity_MetaFragmentControl ? tmp2 : tmp0;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ShaderForgeMaterialInspector"
}