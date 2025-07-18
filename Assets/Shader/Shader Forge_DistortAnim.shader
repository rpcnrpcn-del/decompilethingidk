Shader "Shader Forge/DistortAnim" {
	Properties {
		_Color_Tex ("Color_Tex", 2D) = "white" {}
		_FoamColor ("FoamColor", Color) = (1,1,1,1)
		_SurfaceColor ("SurfaceColor", Color) = (0.503,0.6647686,1,1)
		_DepthColor ("DepthColor", Color) = (0,0.3254902,1,1)
		_EdgeDistance ("EdgeDistance", Float) = 0
		_Distortion ("Distortion", Float) = 1
		_Refraction ("Refraction", 2D) = "bump" {}
		_UV_Scale ("UV_Scale", Float) = 10
		_RefractionPower ("RefractionPower", Float) = 1
		_Specular ("Specular", Float) = 0
		_Roughness ("Roughness", Float) = 0.08
		_Opacity ("Opacity", Float) = 1
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		GrabPass {
		}
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "ForwardBase" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZClip Off
			ZWrite Off
			GpuProgramID 60289
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
				float4 texcoord7 : TEXCOORD7;
				float4 texcoord8 : TEXCOORD8;
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
			float4 _Color_Tex_ST;
			float4 _Refraction_ST;
			float _Distortion;
			float _Specular;
			float _Roughness;
			float _Opacity;
			float _UV_Scale;
			float _EdgeDistance;
			float4 _FoamColor;
			float4 _SurfaceColor;
			float4 _DepthColor;
			float _RefractionPower;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Refraction;
			sampler2D _CameraDepthTexture;
			sampler2D _GrabTexture;
			sampler2D _Color_Tex;
			
			// Keywords: DIRECTIONAL DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
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
                tmp0 = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.position = tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
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
                o.texcoord7 = tmp0;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord8.w = tmp0.w;
                o.texcoord8.xy = tmp1.zz + tmp1.xw;
                tmp0.x = v.vertex.y * glstate_matrix_modelview0._m21;
                tmp0.x = glstate_matrix_modelview0._m20 * v.vertex.x + tmp0.x;
                tmp0.x = glstate_matrix_modelview0._m22 * v.vertex.z + tmp0.x;
                tmp0.x = tmp0.x + glstate_matrix_modelview0._m23;
                o.texcoord8.z = -tmp0.x;
                o.texcoord10 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: DIRECTIONAL DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp)
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
                float4 tmp12;
                float4 tmp13;
                tmp0.x = dot(inp.texcoord4.xyz, inp.texcoord4.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.xyz = tmp0.xxx * inp.texcoord4.xyz;
                tmp1.xy = inp.texcoord7.xy / inp.texcoord7.ww;
                tmp0.w = _ProjectionParams.x * -_ProjectionParams.x;
                tmp2.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp1.www * tmp2.xyz;
                tmp2.w = _TimeEditor.y + _Time.y;
                tmp4.xy = tmp2.ww * float2(0.004, 0.002);
                tmp4.xy = inp.texcoord.xy * _UV_Scale.xx + tmp4.xy;
                tmp4.xy = tmp4.xy * _Refraction_ST.xy + _Refraction_ST.zw;
                tmp4 = tex2D(_Refraction, tmp4.xy);
                tmp5.xy = tmp4.wy + tmp4.wy;
                tmp4.xy = tmp4.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp3.w = dot(tmp4.xy, tmp4.xy);
                tmp3.w = min(tmp3.w, 1.0);
                tmp3.w = 1.0 - tmp3.w;
                tmp5.z = sqrt(tmp3.w);
                tmp5.xyz = tmp5.xyz - float3(1.0, 1.0, 1.0);
                tmp5.xyz = _Distortion.xxx * tmp5.xyz + float3(0.0, 0.0, 1.0);
                tmp6.xyz = tmp5.yyy * inp.texcoord6.xyz;
                tmp5.xyw = tmp5.xxx * inp.texcoord5.xyz + tmp6.xyz;
                tmp0.xyz = tmp5.zzz * tmp0.xyz + tmp5.xyw;
                tmp3.w = dot(tmp0.xyz, tmp0.xyz);
                tmp3.w = rsqrt(tmp3.w);
                tmp5.xyz = tmp0.xyz * tmp3.www;
                tmp0.x = dot(-tmp3.xyz, tmp5.xyz);
                tmp0.x = tmp0.x + tmp0.x;
                tmp0.xyz = tmp5.xyz * -tmp0.xxx + -tmp3.xyz;
                tmp4.zw = inp.texcoord8.xy / inp.texcoord8.ww;
                tmp6 = tex2D(_CameraDepthTexture, tmp4.zw);
                tmp3.w = _ZBufferParams.z * tmp6.x + _ZBufferParams.w;
                tmp3.w = 1.0 / tmp3.w;
                tmp3.w = tmp3.w - _ProjectionParams.y;
                tmp3.w = max(tmp3.w, 0.0);
                tmp4.z = inp.texcoord8.z - _ProjectionParams.y;
                tmp4.z = max(tmp4.z, 0.0);
                tmp1.z = tmp0.w * tmp1.y;
                tmp1.xy = tmp1.xz * float2(0.5, 0.5) + float2(0.5, 0.5);
                tmp1.xy = tmp4.xy * _RefractionPower.xx + tmp1.xy;
                tmp6 = tex2D(_GrabTexture, tmp1.xy);
                tmp0.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * _WorldSpaceLightPos0.xyz;
                tmp2.xyz = tmp2.xyz * tmp1.www + tmp1.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp4.xy = -float2(_Specular.x, _Roughness.x) + float2(1.0, 1.0);
                tmp0.w = tmp4.x * 10.0 + 1.0;
                tmp0.w = exp(tmp0.w);
                tmp1.w = 1.0 - tmp4.x;
                tmp4.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp4.w) {
                    tmp4.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp7.xyz = inp.texcoord3.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp7.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.xxx + tmp7.xyz;
                    tmp7.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.zzz + tmp7.xyz;
                    tmp7.xyz = tmp7.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp7.xyz = tmp4.www ? tmp7.xyz : inp.texcoord3.xyz;
                    tmp7.xyz = tmp7.xyz - unity_ProbeVolumeMin;
                    tmp7.yzw = tmp7.xyz * unity_ProbeVolumeSizeInv;
                    tmp4.w = tmp7.y * 0.25;
                    tmp6.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp7.y = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp4.w = max(tmp4.w, tmp6.w);
                    tmp7.x = min(tmp7.y, tmp4.w);
                    tmp8 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp7.xzw);
                    tmp9.xyz = tmp7.xzw + float3(0.25, 0.0, 0.0);
                    tmp9 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp9.xyz);
                    tmp7.xyz = tmp7.xzw + float3(0.5, 0.0, 0.0);
                    tmp7 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp7.xyz);
                    tmp5.w = 1.0;
                    tmp8.x = dot(tmp8, tmp5);
                    tmp8.y = dot(tmp9, tmp5);
                    tmp8.z = dot(tmp7, tmp5);
                } else {
                    tmp5.w = 1.0;
                    tmp8.x = dot(unity_SHAr, tmp5);
                    tmp8.y = dot(unity_SHAg, tmp5);
                    tmp8.z = dot(unity_SHAb, tmp5);
                }
                tmp7.xyz = tmp8.xyz + inp.texcoord10.xyz;
                tmp7.xyz = max(tmp7.xyz, float3(0.0, 0.0, 0.0));
                tmp4.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp4.w) {
                    tmp4.w = dot(tmp0.xyz, tmp0.xyz);
                    tmp4.w = rsqrt(tmp4.w);
                    tmp8.xyz = tmp0.xyz * tmp4.www;
                    tmp9.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord3.xyz;
                    tmp9.xyz = tmp9.xyz / tmp8.xyz;
                    tmp10.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord3.xyz;
                    tmp10.xyz = tmp10.xyz / tmp8.xyz;
                    tmp11.xyz = tmp8.xyz > float3(0.0, 0.0, 0.0);
                    tmp9.xyz = tmp11.xyz ? tmp9.xyz : tmp10.xyz;
                    tmp4.w = min(tmp9.y, tmp9.x);
                    tmp4.w = min(tmp9.z, tmp4.w);
                    tmp9.xyz = inp.texcoord3.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp8.xyz = tmp8.xyz * tmp4.www + tmp9.xyz;
                } else {
                    tmp8.xyz = tmp0.xyz;
                }
                tmp4.w = tmp1.w * 0.7978846;
                tmp9.xy = -tmp1.ww * float2(0.7, 0.7978846) + float2(1.7, 1.0);
                tmp5.w = tmp1.w * tmp9.x;
                tmp5.w = tmp5.w * 6.0;
                tmp8 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp8.xyz, tmp5.w));
                tmp6.w = unity_SpecCube0_HDR.w == 1.0;
                tmp7.w = log(tmp8.w);
                tmp7.w = tmp7.w * unity_SpecCube0_HDR.y;
                tmp7.w = exp(tmp7.w);
                tmp6.w = tmp6.w ? tmp7.w : 1.0;
                tmp6.w = tmp6.w * unity_SpecCube0_HDR.x;
                tmp9.xzw = tmp8.xyz * tmp6.www;
                tmp7.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp7.w) {
                    tmp7.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp7.w) {
                        tmp7.w = dot(tmp0.xyz, tmp0.xyz);
                        tmp7.w = rsqrt(tmp7.w);
                        tmp10.xyz = tmp0.xyz * tmp7.www;
                        tmp11.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord3.xyz;
                        tmp11.xyz = tmp11.xyz / tmp10.xyz;
                        tmp12.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord3.xyz;
                        tmp12.xyz = tmp12.xyz / tmp10.xyz;
                        tmp13.xyz = tmp10.xyz > float3(0.0, 0.0, 0.0);
                        tmp11.xyz = tmp13.xyz ? tmp11.xyz : tmp12.xyz;
                        tmp7.w = min(tmp11.y, tmp11.x);
                        tmp7.w = min(tmp11.z, tmp7.w);
                        tmp11.xyz = inp.texcoord3.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp0.xyz = tmp10.xyz * tmp7.www + tmp11.xyz;
                    }
                    tmp10 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp0.xyz, tmp5.w));
                    tmp0.x = unity_SpecCube1_HDR.w == 1.0;
                    tmp0.y = log(tmp10.w);
                    tmp0.y = tmp0.y * unity_SpecCube1_HDR.y;
                    tmp0.y = exp(tmp0.y);
                    tmp0.x = tmp0.x ? tmp0.y : 1.0;
                    tmp0.x = tmp0.x * unity_SpecCube1_HDR.x;
                    tmp0.xyz = tmp10.xyz * tmp0.xxx;
                    tmp8.xyz = tmp6.www * tmp8.xyz + -tmp0.xyz;
                    tmp9.xzw = unity_SpecCube0_BoxMin.www * tmp8.xyz + tmp0.xyz;
                }
                tmp0.x = dot(tmp5.xyz, tmp1.xyz);
                tmp0.y = dot(tmp1.xyz, tmp2.xyz);
                tmp0.z = dot(tmp5.xyz, tmp3.xyz);
                tmp0.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                tmp1.x = dot(tmp5.xyz, tmp2.xyz);
                tmp1.y = tmp0.x * tmp9.y + tmp4.w;
                tmp1.z = tmp0.z * tmp9.y + tmp4.w;
                tmp1.y = tmp1.y * tmp1.z + 0.00001;
                tmp1.y = 1.0 / tmp1.y;
                tmp1.y = tmp1.y * 0.25;
                tmp1.z = tmp1.w * tmp1.w;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.xz = max(tmp1.xz, float2(0.0, 0.0001));
                tmp1.z = 2.0 / tmp1.z;
                tmp1.z = tmp1.z - 2.0;
                tmp1.z = max(tmp1.z, 0.0001);
                tmp2.x = tmp1.z + 2.0;
                tmp2.x = tmp2.x * 0.1591549;
                tmp1.x = log(tmp1.x);
                tmp1.z = tmp1.x * tmp1.z;
                tmp1.z = exp(tmp1.z);
                tmp1.z = tmp2.x * tmp1.z;
                tmp1.y = tmp0.x * tmp1.y;
                tmp1.y = tmp1.z * tmp1.y;
                tmp1.y = tmp1.y * 0.7853982;
                tmp1.y = max(tmp1.y, 0.0);
                tmp0.w = tmp0.w * tmp1.x;
                tmp0.w = exp(tmp0.w);
                tmp0.w = tmp1.y * tmp0.w;
                tmp1.xyz = tmp0.www * _LightColor0.xyz;
                tmp0.w = 1.0 - tmp0.y;
                tmp2.x = tmp0.w * tmp0.w;
                tmp2.x = tmp2.x * tmp2.x;
                tmp0.w = tmp0.w * tmp2.x;
                tmp0.w = tmp4.y * tmp0.w + _Specular;
                tmp2.x = saturate(tmp4.x + _Specular);
                tmp2.yz = float2(1.0, 1.00001) - tmp0.zz;
                tmp3.xy = tmp2.yz * tmp2.yz;
                tmp3.xy = tmp3.xy * tmp3.xy;
                tmp2.yz = tmp2.yz * tmp3.xy;
                tmp0.z = tmp2.x - _Specular;
                tmp0.z = tmp2.y * tmp0.z + _Specular;
                tmp3.xyz = tmp0.zzz * tmp9.xzw;
                tmp1.xyz = tmp1.xyz * tmp0.www + tmp3.xyz;
                tmp0.z = tmp0.y + tmp0.y;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.y = tmp0.y * tmp1.w + -0.5;
                tmp0.z = 1.00001 - tmp0.x;
                tmp0.w = tmp0.z * tmp0.z;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.z = tmp0.w * tmp0.z;
                tmp0.z = tmp0.y * tmp0.z + 1.0;
                tmp0.y = tmp0.y * tmp2.z + 1.0;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.yz = inp.texcoord.xy * _Color_Tex_ST.xy + _Color_Tex_ST.zw;
                tmp5 = tex2D(_Color_Tex, tmp0.yz);
                tmp0.y = tmp3.w - tmp4.z;
                tmp0.z = saturate(tmp0.y / _SurfaceColor.w);
                tmp2.xyz = _DepthColor.xyz - _SurfaceColor.xyz;
                tmp2.xyz = tmp0.zzz * tmp2.xyz + _SurfaceColor.xyz;
                tmp0.z = sin(tmp2.w);
                tmp0.z = tmp0.z * 0.01 + _EdgeDistance;
                tmp0.z = tmp0.z + 0.01;
                tmp0.y = saturate(tmp0.y / tmp0.z);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.yzw = tmp0.yyy * _FoamColor.xyz;
                tmp0.yzw = tmp0.yzw * _FoamColor.www;
                tmp0.yzw = tmp2.xyz * tmp5.xyz + tmp0.yzw;
                tmp0.yzw = tmp4.yyy * tmp0.yzw;
                tmp2.xyz = tmp0.xxx * _LightColor0.xyz + tmp7.xyz;
                tmp0.xyz = tmp0.yzw * tmp2.xyz;
                tmp0.xyz = tmp0.xyz * _Opacity.xxx + tmp1.xyz;
                tmp0.xyz = tmp0.xyz - tmp6.xyz;
                o.sv_target.xyz = _Opacity.xxx * tmp0.xyz + tmp6.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "Meta" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 105699
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
				float4 texcoord4 : TEXCOORD4;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float unity_OneOverOutputBoost;
			float unity_MaxOutputValue;
			float4 _TimeEditor;
			float4 _Color_Tex_ST;
			float _Specular;
			float _Roughness;
			float _EdgeDistance;
			float4 _FoamColor;
			float4 _SurfaceColor;
			float4 _DepthColor;
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
			sampler2D _CameraDepthTexture;
			sampler2D _Color_Tex;
			
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
                tmp0 = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.position = tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord4.w = tmp0.w;
                o.texcoord4.xy = tmp1.zz + tmp1.xw;
                tmp0.x = v.vertex.y * glstate_matrix_modelview0._m21;
                tmp0.x = glstate_matrix_modelview0._m20 * v.vertex.x + tmp0.x;
                tmp0.x = glstate_matrix_modelview0._m22 * v.vertex.z + tmp0.x;
                tmp0.x = tmp0.x + glstate_matrix_modelview0._m23;
                o.texcoord4.z = -tmp0.x;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xy = inp.texcoord4.xy / inp.texcoord4.ww;
                tmp0 = tex2D(_CameraDepthTexture, tmp0.xy);
                tmp0.x = _ZBufferParams.z * tmp0.x + _ZBufferParams.w;
                tmp0.x = 1.0 / tmp0.x;
                tmp0.x = tmp0.x - _ProjectionParams.y;
                tmp0.y = inp.texcoord4.z - _ProjectionParams.y;
                tmp0.xy = max(tmp0.xy, float2(0.0, 0.0));
                tmp0.x = tmp0.x - tmp0.y;
                tmp0.y = _TimeEditor.y + _Time.y;
                tmp0.y = sin(tmp0.y);
                tmp0.y = tmp0.y * 0.01 + _EdgeDistance;
                tmp0.y = tmp0.y + 0.01;
                tmp0.y = saturate(tmp0.x / tmp0.y);
                tmp0.x = saturate(tmp0.x / _SurfaceColor.w);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.yzw = tmp0.yyy * _FoamColor.xyz;
                tmp0.yzw = tmp0.yzw * _FoamColor.www;
                tmp1.xyz = _DepthColor.xyz - _SurfaceColor.xyz;
                tmp1.xyz = tmp0.xxx * tmp1.xyz + _SurfaceColor.xyz;
                tmp2.xy = inp.texcoord.xy * _Color_Tex_ST.xy + _Color_Tex_ST.zw;
                tmp2 = tex2D(_Color_Tex, tmp2.xy);
                tmp0.xyz = tmp1.xyz * tmp2.xyz + tmp0.yzw;
                tmp0.w = _Roughness * _Roughness;
                tmp0.w = tmp0.w * _Specular;
                tmp0.w = tmp0.w * 0.5;
                tmp1.x = 1.0 - _Specular;
                tmp0.xyz = tmp0.xyz * tmp1.xxx + tmp0.www;
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
	Fallback "FX/Glass/Stained BumpDistort"
	CustomEditor "ShaderForgeMaterialInspector"
}