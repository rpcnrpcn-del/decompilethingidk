Shader "Shader Forge/Avatar/Avatar_RGB" {
	Properties {
		_Mask_Tex ("Mask_Tex", 2D) = "gray" {}
		_Base_Col ("Base_Col", Color) = (0,0,0,1)
		_Red_Col ("Red_Col", Color) = (1,0,0,1)
		_Green_Col ("Green_Col", Color) = (0,1,0,1)
		_Blue_Col ("Blue_Col", Color) = (0,0,1,1)
		_Dirt_Col ("Dirt_Col", Color) = (0,0,0,1)
		[MaterialToggle] _Dirt_Add ("Dirt_Add", Float) = 0
		_Decal_Tex ("Decal_Tex", 2D) = "white" {}
		_Decal_Col ("Decal_Col", Color) = (0,1,1,1)
		_Spec ("Spec", Range(0, 1)) = 0
		_Gloss ("Gloss", Range(0, 1)) = 0.8
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }

			Cull Off
			GpuProgramID 28800
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
			float4 _Mask_Tex_ST;
			float _Gloss;
			float4 _Red_Col;
			float4 _Green_Col;
			float4 _Blue_Col;
			float4 _Base_Col;
			float4 _Decal_Tex_ST;
			float4 _Decal_Col;
			float4 _Dirt_Col;
			float _Spec;
			float _Dirt_Add;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Mask_Tex;
			sampler2D _Decal_Tex;
			
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
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                float4 tmp9;
                float4 tmp10;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp1.w = dot(-tmp1.xyz, inp.texcoord4.xyz);
                tmp1.w = tmp1.w + tmp1.w;
                tmp2.xyz = inp.texcoord4.xyz * -tmp1.www + -tmp1.xyz;
                tmp1.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp1.www * _WorldSpaceLightPos0.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp3.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = _Gloss * 10.0 + 1.0;
                tmp0.w = exp(tmp0.w);
                tmp1.w = 1.0 - _Gloss;
                tmp2.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp2.w) {
                    tmp2.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp4.xyz = inp.texcoord3.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.xxx + tmp4.xyz;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.zzz + tmp4.xyz;
                    tmp4.xyz = tmp4.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp4.xyz = tmp2.www ? tmp4.xyz : inp.texcoord3.xyz;
                    tmp4.xyz = tmp4.xyz - unity_ProbeVolumeMin;
                    tmp4.yzw = tmp4.xyz * unity_ProbeVolumeSizeInv;
                    tmp2.w = tmp4.y * 0.25;
                    tmp3.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp4.y = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp2.w = max(tmp2.w, tmp3.w);
                    tmp4.x = min(tmp4.y, tmp2.w);
                    tmp5 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xzw);
                    tmp6.xyz = tmp4.xzw + float3(0.25, 0.0, 0.0);
                    tmp6 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp6.xyz);
                    tmp4.xyz = tmp4.xzw + float3(0.5, 0.0, 0.0);
                    tmp4 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xyz);
                    tmp7.xyz = inp.texcoord4.xyz;
                    tmp7.w = 1.0;
                    tmp5.x = dot(tmp5, tmp7);
                    tmp5.y = dot(tmp6, tmp7);
                    tmp5.z = dot(tmp4, tmp7);
                } else {
                    tmp4.xyz = inp.texcoord4.xyz;
                    tmp4.w = 1.0;
                    tmp5.x = dot(unity_SHAr, tmp4);
                    tmp5.y = dot(unity_SHAg, tmp4);
                    tmp5.z = dot(unity_SHAb, tmp4);
                }
                tmp4.xyz = tmp5.xyz + inp.texcoord10.xyz;
                tmp4.xyz = max(tmp4.xyz, float3(0.0, 0.0, 0.0));
                tmp2.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp2.w) {
                    tmp2.w = dot(tmp2.xyz, tmp2.xyz);
                    tmp2.w = rsqrt(tmp2.w);
                    tmp5.xyz = tmp2.www * tmp2.xyz;
                    tmp6.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord3.xyz;
                    tmp6.xyz = tmp6.xyz / tmp5.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord3.xyz;
                    tmp7.xyz = tmp7.xyz / tmp5.xyz;
                    tmp8.xyz = tmp5.xyz > float3(0.0, 0.0, 0.0);
                    tmp6.xyz = tmp8.xyz ? tmp6.xyz : tmp7.xyz;
                    tmp2.w = min(tmp6.y, tmp6.x);
                    tmp2.w = min(tmp6.z, tmp2.w);
                    tmp6.xyz = inp.texcoord3.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp5.xyz = tmp5.xyz * tmp2.www + tmp6.xyz;
                } else {
                    tmp5.xyz = tmp2.xyz;
                }
                tmp2.w = tmp1.w * 0.7978846;
                tmp6.xy = -tmp1.ww * float2(0.7, 0.7978846) + float2(1.7, 1.0);
                tmp3.w = tmp1.w * tmp6.x;
                tmp3.w = tmp3.w * 6.0;
                tmp5 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp5.xyz, tmp3.w));
                tmp4.w = unity_SpecCube0_HDR.w == 1.0;
                tmp5.w = log(tmp5.w);
                tmp5.w = tmp5.w * unity_SpecCube0_HDR.y;
                tmp5.w = exp(tmp5.w);
                tmp4.w = tmp4.w ? tmp5.w : 1.0;
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.x;
                tmp6.xzw = tmp5.xyz * tmp4.www;
                tmp5.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp5.w) {
                    tmp5.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp5.w) {
                        tmp5.w = dot(tmp2.xyz, tmp2.xyz);
                        tmp5.w = rsqrt(tmp5.w);
                        tmp7.xyz = tmp2.xyz * tmp5.www;
                        tmp8.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord3.xyz;
                        tmp8.xyz = tmp8.xyz / tmp7.xyz;
                        tmp9.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord3.xyz;
                        tmp9.xyz = tmp9.xyz / tmp7.xyz;
                        tmp10.xyz = tmp7.xyz > float3(0.0, 0.0, 0.0);
                        tmp8.xyz = tmp10.xyz ? tmp8.xyz : tmp9.xyz;
                        tmp5.w = min(tmp8.y, tmp8.x);
                        tmp5.w = min(tmp8.z, tmp5.w);
                        tmp8.xyz = inp.texcoord3.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp2.xyz = tmp7.xyz * tmp5.www + tmp8.xyz;
                    }
                    tmp7 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp2.xyz, tmp3.w));
                    tmp2.x = unity_SpecCube1_HDR.w == 1.0;
                    tmp2.y = log(tmp7.w);
                    tmp2.y = tmp2.y * unity_SpecCube1_HDR.y;
                    tmp2.y = exp(tmp2.y);
                    tmp2.x = tmp2.x ? tmp2.y : 1.0;
                    tmp2.x = tmp2.x * unity_SpecCube1_HDR.x;
                    tmp2.xyz = tmp7.xyz * tmp2.xxx;
                    tmp5.xyz = tmp4.www * tmp5.xyz + -tmp2.xyz;
                    tmp6.xzw = unity_SpecCube0_BoxMin.www * tmp5.xyz + tmp2.xyz;
                }
                tmp2.x = dot(inp.texcoord4.xyz, tmp3.xyz);
                tmp2.y = dot(tmp3.xyz, tmp0.xyz);
                tmp2.xy = max(tmp2.xy, float2(0.0, 0.0));
                tmp1.x = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp1.x = max(tmp1.x, 0.0);
                tmp0.x = dot(inp.texcoord4.xyz, tmp0.xyz);
                tmp0.y = tmp2.x * tmp6.y + tmp2.w;
                tmp0.z = tmp1.x * tmp6.y + tmp2.w;
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
                tmp0.y = tmp0.y * tmp2.x;
                tmp0.xy = tmp0.xz * tmp0.wy;
                tmp0.y = tmp0.y * 0.7853982;
                tmp0.y = max(tmp0.y, 0.0);
                tmp0.x = exp(tmp0.x);
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.xyz = tmp0.xxx * _LightColor0.xyz;
                tmp0.w = 1.0 - tmp2.y;
                tmp1.y = tmp0.w * tmp0.w;
                tmp1.y = tmp1.y * tmp1.y;
                tmp0.w = tmp0.w * tmp1.y;
                tmp1.y = 1.0 - _Spec;
                tmp0.w = tmp1.y * tmp0.w + _Spec;
                tmp1.z = saturate(_Gloss + _Spec);
                tmp2.zw = float2(1.0, 1.00001) - tmp1.xx;
                tmp3.xy = tmp2.zw * tmp2.zw;
                tmp3.xy = tmp3.xy * tmp3.xy;
                tmp2.zw = tmp2.zw * tmp3.xy;
                tmp1.x = tmp1.z - _Spec;
                tmp1.x = tmp2.z * tmp1.x + _Spec;
                tmp3.xyz = tmp1.xxx * tmp6.xzw;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp3.xyz;
                tmp0.w = tmp2.y + tmp2.y;
                tmp0.w = tmp2.y * tmp0.w;
                tmp0.w = tmp0.w * tmp1.w + -0.5;
                tmp1.x = 1.00001 - tmp2.x;
                tmp1.z = tmp1.x * tmp1.x;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.x = tmp1.z * tmp1.x;
                tmp1.x = tmp0.w * tmp1.x + 1.0;
                tmp0.w = tmp0.w * tmp2.w + 1.0;
                tmp0.w = tmp0.w * tmp1.x;
                tmp0.w = tmp2.x * tmp0.w;
                tmp1.xz = inp.texcoord.xy * _Mask_Tex_ST.xy + _Mask_Tex_ST.zw;
                tmp2 = tex2D(_Mask_Tex, tmp1.xz);
                tmp1.xz = inp.texcoord1.xy * _Decal_Tex_ST.xy + _Decal_Tex_ST.zw;
                tmp3 = tex2D(_Decal_Tex, tmp1.xz);
                tmp1.xzw = _Red_Col.xyz - _Base_Col.xyz;
                tmp1.xzw = tmp2.xxx * tmp1.xzw + _Base_Col.xyz;
                tmp5.xyz = _Green_Col.xyz - tmp1.xzw;
                tmp1.xzw = tmp2.yyy * tmp5.xyz + tmp1.xzw;
                tmp5.xyz = _Blue_Col.xyz - tmp1.xzw;
                tmp1.xzw = tmp2.zzz * tmp5.xyz + tmp1.xzw;
                tmp1.xzw = tmp1.xzw - _Decal_Col.xyz;
                tmp1.xzw = tmp3.xyz * tmp1.xzw + _Decal_Col.xyz;
                tmp2.xyz = saturate(tmp2.www + _Dirt_Col.xyz);
                tmp3.xyz = tmp1.xzw * tmp2.xyz;
                tmp2.w = 1.0 - tmp2.w;
                tmp5.xyz = saturate(_Dirt_Col.xyz * tmp2.www + tmp1.xzw);
                tmp1.xzw = -tmp2.xyz * tmp1.xzw + tmp5.xyz;
                tmp1.xzw = _Dirt_Add.xxx * tmp1.xzw + tmp3.xyz;
                tmp1.xyz = tmp1.yyy * tmp1.xzw;
                tmp2.xyz = tmp0.www * _LightColor0.xyz + tmp4.xyz;
                o.sv_target.xyz = tmp2.xyz * tmp1.xyz + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD_DELTA"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "ForwardAdd" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			Blend One One, One One

			Cull Off
			GpuProgramID 123712
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
			float4 _Mask_Tex_ST;
			float _Gloss;
			float4 _Red_Col;
			float4 _Green_Col;
			float4 _Blue_Col;
			float4 _Base_Col;
			float4 _Decal_Tex_ST;
			float4 _Decal_Col;
			float4 _Dirt_Col;
			float _Spec;
			float _Dirt_Add;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _LightTexture0;
			sampler2D _Mask_Tex;
			sampler2D _Decal_Tex;
			
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
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xyz = _Red_Col.xyz - _Base_Col.xyz;
                tmp1.xy = inp.texcoord.xy * _Mask_Tex_ST.xy + _Mask_Tex_ST.zw;
                tmp1 = tex2D(_Mask_Tex, tmp1.xy);
                tmp0.xyz = tmp1.xxx * tmp0.xyz + _Base_Col.xyz;
                tmp2.xyz = _Green_Col.xyz - tmp0.xyz;
                tmp0.xyz = tmp1.yyy * tmp2.xyz + tmp0.xyz;
                tmp2.xyz = _Blue_Col.xyz - tmp0.xyz;
                tmp0.xyz = tmp1.zzz * tmp2.xyz + tmp0.xyz;
                tmp0.xyz = tmp0.xyz - _Decal_Col.xyz;
                tmp1.xy = inp.texcoord1.xy * _Decal_Tex_ST.xy + _Decal_Tex_ST.zw;
                tmp2 = tex2D(_Decal_Tex, tmp1.xy);
                tmp0.xyz = tmp2.xyz * tmp0.xyz + _Decal_Col.xyz;
                tmp0.w = 1.0 - tmp1.w;
                tmp1.xyz = saturate(tmp1.www + _Dirt_Col.xyz);
                tmp2.xyz = saturate(_Dirt_Col.xyz * tmp0.www + tmp0.xyz);
                tmp2.xyz = -tmp1.xyz * tmp0.xyz + tmp2.xyz;
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.xyz = _Dirt_Add.xxx * tmp2.xyz + tmp0.xyz;
                tmp0.w = 1.0 - _Spec;
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = _WorldSpaceLightPos0.www * -inp.texcoord3.xyz + _WorldSpaceLightPos0.xyz;
                tmp1.w = dot(tmp1.xyz, tmp1.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp2.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp1.w = dot(tmp2.xyz, tmp2.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp2.xyz * tmp1.www + tmp1.xyz;
                tmp2.xyz = tmp1.www * tmp2.xyz;
                tmp1.w = dot(inp.texcoord4.xyz, tmp2.xyz);
                tmp2.x = dot(tmp3.xyz, tmp3.xyz);
                tmp2.x = rsqrt(tmp2.x);
                tmp2.xyz = tmp2.xxx * tmp3.xyz;
                tmp2.w = dot(inp.texcoord4.xyz, tmp2.xyz);
                tmp2.x = dot(tmp1.xyz, tmp2.xyz);
                tmp1.x = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp1.xw = max(tmp1.xw, float2(0.0, 0.0));
                tmp1.yz = max(tmp2.xw, float2(0.0, 0.0));
                tmp1.z = log(tmp1.z);
                tmp2.x = 1.0 - _Gloss;
                tmp2.y = tmp2.x * tmp2.x;
                tmp2.y = tmp2.y * tmp2.y;
                tmp2.y = max(tmp2.y, 0.0001);
                tmp2.y = 2.0 / tmp2.y;
                tmp2.y = tmp2.y - 2.0;
                tmp2.y = max(tmp2.y, 0.0001);
                tmp2.z = tmp1.z * tmp2.y;
                tmp2.y = tmp2.y + 2.0;
                tmp2.y = tmp2.y * 0.1591549;
                tmp2.z = exp(tmp2.z);
                tmp2.y = tmp2.y * tmp2.z;
                tmp2.z = tmp2.x * 0.7978846;
                tmp2.w = -tmp2.x * 0.7978846 + 1.0;
                tmp3.x = tmp1.w * tmp2.w + tmp2.z;
                tmp2.z = tmp1.x * tmp2.w + tmp2.z;
                tmp2.z = tmp2.z * tmp3.x + 0.00001;
                tmp2.z = 1.0 / tmp2.z;
                tmp2.z = tmp2.z * 0.25;
                tmp2.z = tmp1.x * tmp2.z;
                tmp2.y = tmp2.y * tmp2.z;
                tmp2.y = tmp2.y * 0.7853982;
                tmp2.y = max(tmp2.y, 0.0);
                tmp2.z = _Gloss * 10.0 + 1.0;
                tmp2.z = exp(tmp2.z);
                tmp1.z = tmp1.z * tmp2.z;
                tmp1.z = exp(tmp1.z);
                tmp2.z = dot(inp.texcoord7.xyz, inp.texcoord7.xyz);
                tmp3 = tex2D(_LightTexture0, tmp2.zz);
                tmp3.xyz = tmp3.xxx * _LightColor0.xyz;
                tmp4.xyz = tmp1.zzz * tmp3.xyz;
                tmp2.yzw = tmp2.yyy * tmp4.xyz;
                tmp2.yzw = tmp2.yzw * _LightColor0.xyz;
                tmp1.zw = float2(1.0, 1.00001) - tmp1.yw;
                tmp3.w = tmp1.z * tmp1.z;
                tmp3.w = tmp3.w * tmp3.w;
                tmp1.z = tmp1.z * tmp3.w;
                tmp0.w = tmp0.w * tmp1.z + _Spec;
                tmp2.yzw = tmp0.www * tmp2.yzw;
                tmp0.w = tmp1.w * tmp1.w;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.w = tmp0.w * tmp1.w;
                tmp1.z = tmp1.y + tmp1.y;
                tmp1.y = tmp1.y * tmp1.z;
                tmp1.y = tmp1.y * tmp2.x + -0.5;
                tmp0.w = tmp1.y * tmp0.w + 1.0;
                tmp1.z = 1.00001 - tmp1.x;
                tmp1.w = tmp1.z * tmp1.z;
                tmp1.w = tmp1.w * tmp1.w;
                tmp1.z = tmp1.w * tmp1.z;
                tmp1.y = tmp1.y * tmp1.z + 1.0;
                tmp0.w = tmp0.w * tmp1.y;
                tmp0.w = tmp1.x * tmp0.w;
                tmp1.xyz = tmp3.xyz * tmp0.www;
                o.sv_target.xyz = tmp1.xyz * tmp0.xyz + tmp2.yzw;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "CanUseSpriteAtlas" = "true" "LIGHTMODE" = "Meta" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }

			Cull Off
			GpuProgramID 181409
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
			float4 _Mask_Tex_ST;
			float _Gloss;
			float4 _Red_Col;
			float4 _Green_Col;
			float4 _Blue_Col;
			float4 _Base_Col;
			float4 _Decal_Tex_ST;
			float4 _Decal_Col;
			float4 _Dirt_Col;
			float _Spec;
			float _Dirt_Add;
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
			sampler2D _Mask_Tex;
			sampler2D _Decal_Tex;
			
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
                float4 tmp1;
                float4 tmp2;
                tmp0.xyz = _Red_Col.xyz - _Base_Col.xyz;
                tmp1.xy = inp.texcoord.xy * _Mask_Tex_ST.xy + _Mask_Tex_ST.zw;
                tmp1 = tex2D(_Mask_Tex, tmp1.xy);
                tmp0.xyz = tmp1.xxx * tmp0.xyz + _Base_Col.xyz;
                tmp2.xyz = _Green_Col.xyz - tmp0.xyz;
                tmp0.xyz = tmp1.yyy * tmp2.xyz + tmp0.xyz;
                tmp2.xyz = _Blue_Col.xyz - tmp0.xyz;
                tmp0.xyz = tmp1.zzz * tmp2.xyz + tmp0.xyz;
                tmp0.xyz = tmp0.xyz - _Decal_Col.xyz;
                tmp1.xy = inp.texcoord1.xy * _Decal_Tex_ST.xy + _Decal_Tex_ST.zw;
                tmp2 = tex2D(_Decal_Tex, tmp1.xy);
                tmp0.xyz = tmp2.xyz * tmp0.xyz + _Decal_Col.xyz;
                tmp0.w = 1.0 - tmp1.w;
                tmp1.xyz = saturate(tmp1.www + _Dirt_Col.xyz);
                tmp2.xyz = saturate(_Dirt_Col.xyz * tmp0.www + tmp0.xyz);
                tmp2.xyz = -tmp1.xyz * tmp0.xyz + tmp2.xyz;
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.xyz = _Dirt_Add.xxx * tmp2.xyz + tmp0.xyz;
                tmp0.w = 1.0 - _Gloss;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.w = tmp0.w * _Spec;
                tmp0.w = tmp0.w * 0.5;
                tmp1.x = 1.0 - _Spec;
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
	Fallback "Standard (No Culling)"
	CustomEditor "ShaderForgeMaterialInspector"
}