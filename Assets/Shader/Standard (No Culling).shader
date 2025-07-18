Shader "Standard (No Culling)" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
	}
	SubShader {
		LOD 200
		Tags { "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			LOD 200
			Tags { "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 31758
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord6 : TEXCOORD6;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float _Glossiness;
			float _Metallic;
			float4 _Color;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.texcoord1.xyz = tmp0.xyz;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                tmp0.w = tmp0.y * tmp0.y;
                tmp0.w = tmp0.x * tmp0.x + -tmp0.w;
                tmp1 = tmp0.yzzx * tmp0.xyzz;
                tmp0.x = dot(unity_SHBr, tmp1);
                tmp0.y = dot(unity_SHBg, tmp1);
                tmp0.z = dot(unity_SHBb, tmp1);
                o.texcoord3.xyz = unity_SHC.xyz * tmp0.www + tmp0.xyz;
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
                return o;
			}
			// Keywords: DIRECTIONAL
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
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2 = tex2D(_MainTex, inp.texcoord.xy);
                tmp3.xyz = tmp2.xyz * _Color.xyz;
                tmp1.w = 1.0 - _Glossiness;
                tmp2.w = dot(-tmp1.xyz, inp.texcoord1.xyz);
                tmp2.w = tmp2.w + tmp2.w;
                tmp4.xyz = inp.texcoord1.xyz * -tmp2.www + -tmp1.xyz;
                tmp2.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp2.w) {
                    tmp2.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp5.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp5.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp5.xyz;
                    tmp5.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp5.xyz;
                    tmp5.xyz = tmp5.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp5.xyz = tmp2.www ? tmp5.xyz : inp.texcoord2.xyz;
                    tmp5.xyz = tmp5.xyz - unity_ProbeVolumeMin;
                    tmp5.yzw = tmp5.xyz * unity_ProbeVolumeSizeInv;
                    tmp2.w = tmp5.y * 0.25;
                    tmp3.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp4.w = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp2.w = max(tmp2.w, tmp3.w);
                    tmp5.x = min(tmp4.w, tmp2.w);
                    tmp6 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp5.xzw);
                    tmp7.xyz = tmp5.xzw + float3(0.25, 0.0, 0.0);
                    tmp7 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp7.xyz);
                    tmp5.xyz = tmp5.xzw + float3(0.5, 0.0, 0.0);
                    tmp5 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp5.xyz);
                    tmp8.xyz = inp.texcoord1.xyz;
                    tmp8.w = 1.0;
                    tmp6.x = dot(tmp6, tmp8);
                    tmp6.y = dot(tmp7, tmp8);
                    tmp6.z = dot(tmp5, tmp8);
                } else {
                    tmp5.xyz = inp.texcoord1.xyz;
                    tmp5.w = 1.0;
                    tmp6.x = dot(unity_SHAr, tmp5);
                    tmp6.y = dot(unity_SHAg, tmp5);
                    tmp6.z = dot(unity_SHAb, tmp5);
                }
                tmp5.xyz = tmp6.xyz + inp.texcoord3.xyz;
                tmp5.xyz = max(tmp5.xyz, float3(0.0, 0.0, 0.0));
                tmp2.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp2.w) {
                    tmp2.w = dot(tmp4.xyz, tmp4.xyz);
                    tmp2.w = rsqrt(tmp2.w);
                    tmp6.xyz = tmp2.www * tmp4.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord2.xyz;
                    tmp7.xyz = tmp7.xyz / tmp6.xyz;
                    tmp8.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord2.xyz;
                    tmp8.xyz = tmp8.xyz / tmp6.xyz;
                    tmp9.xyz = tmp6.xyz > float3(0.0, 0.0, 0.0);
                    tmp7.xyz = tmp9.xyz ? tmp7.xyz : tmp8.xyz;
                    tmp2.w = min(tmp7.y, tmp7.x);
                    tmp2.w = min(tmp7.z, tmp2.w);
                    tmp7.xyz = inp.texcoord2.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp6.xyz = tmp6.xyz * tmp2.www + tmp7.xyz;
                } else {
                    tmp6.xyz = tmp4.xyz;
                }
                tmp2.w = -tmp1.w * 0.7 + 1.7;
                tmp2.w = tmp1.w * tmp2.w;
                tmp2.w = tmp2.w * 6.0;
                tmp6 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp6.xyz, tmp2.w));
                tmp3.w = unity_SpecCube0_HDR.w == 1.0;
                tmp4.w = log(tmp6.w);
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.y;
                tmp4.w = exp(tmp4.w);
                tmp3.w = tmp3.w ? tmp4.w : 1.0;
                tmp3.w = tmp3.w * unity_SpecCube0_HDR.x;
                tmp7.xyz = tmp6.xyz * tmp3.www;
                tmp4.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp4.w) {
                    tmp4.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp4.w) {
                        tmp4.w = dot(tmp4.xyz, tmp4.xyz);
                        tmp4.w = rsqrt(tmp4.w);
                        tmp8.xyz = tmp4.www * tmp4.xyz;
                        tmp9.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord2.xyz;
                        tmp9.xyz = tmp9.xyz / tmp8.xyz;
                        tmp10.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord2.xyz;
                        tmp10.xyz = tmp10.xyz / tmp8.xyz;
                        tmp11.xyz = tmp8.xyz > float3(0.0, 0.0, 0.0);
                        tmp9.xyz = tmp11.xyz ? tmp9.xyz : tmp10.xyz;
                        tmp4.w = min(tmp9.y, tmp9.x);
                        tmp4.w = min(tmp9.z, tmp4.w);
                        tmp9.xyz = inp.texcoord2.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp4.xyz = tmp8.xyz * tmp4.www + tmp9.xyz;
                    }
                    tmp4 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp4.xyz, tmp2.w));
                    tmp2.w = unity_SpecCube1_HDR.w == 1.0;
                    tmp4.w = log(tmp4.w);
                    tmp4.w = tmp4.w * unity_SpecCube1_HDR.y;
                    tmp4.w = exp(tmp4.w);
                    tmp2.w = tmp2.w ? tmp4.w : 1.0;
                    tmp2.w = tmp2.w * unity_SpecCube1_HDR.x;
                    tmp4.xyz = tmp4.xyz * tmp2.www;
                    tmp6.xyz = tmp3.www * tmp6.xyz + -tmp4.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMin.www * tmp6.xyz + tmp4.xyz;
                }
                tmp2.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp4.xyz = tmp2.www * inp.texcoord1.xyz;
                tmp2.xyz = tmp2.xyz * _Color.xyz + float3(-0.04, -0.04, -0.04);
                tmp2.xyz = _Metallic.xxx * tmp2.xyz + float3(0.04, 0.04, 0.04);
                tmp2.w = -_Metallic * 0.96 + 0.96;
                tmp3.xyz = tmp2.www * tmp3.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(tmp4.xyz, tmp1.xyz);
                tmp1.x = saturate(dot(tmp4.xyz, _WorldSpaceLightPos0.xyz));
                tmp1.y = saturate(dot(tmp4.xyz, tmp0.xyz));
                tmp0.x = saturate(dot(_WorldSpaceLightPos0.xyz, tmp0.xyz));
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = dot(tmp0.xy, tmp1.xy);
                tmp0.y = tmp0.y - 0.5;
                tmp0.z = 1.0 - tmp1.x;
                tmp1.z = tmp0.z * tmp0.z;
                tmp1.z = tmp1.z * tmp1.z;
                tmp0.z = tmp0.z * tmp1.z;
                tmp0.z = tmp0.y * tmp0.z + 1.0;
                tmp1.z = 1.0 - abs(tmp0.w);
                tmp3.w = tmp1.z * tmp1.z;
                tmp3.w = tmp3.w * tmp3.w;
                tmp1.z = tmp1.z * tmp3.w;
                tmp0.y = tmp0.y * tmp1.z + 1.0;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.z = tmp1.w * tmp1.w;
                tmp1.w = -tmp1.w * tmp1.w + 1.0;
                tmp3.w = abs(tmp0.w) * tmp1.w + tmp0.z;
                tmp1.w = tmp1.x * tmp1.w + tmp0.z;
                tmp0.w = abs(tmp0.w) * tmp1.w;
                tmp0.w = tmp1.x * tmp3.w + tmp0.w;
                tmp0.w = tmp0.w + 0.00001;
                tmp0.w = 0.5 / tmp0.w;
                tmp1.w = tmp0.z * tmp0.z;
                tmp3.w = tmp1.y * tmp1.w + -tmp1.y;
                tmp1.y = tmp3.w * tmp1.y + 1.0;
                tmp1.w = tmp1.w * 0.3183099;
                tmp1.y = tmp1.y * tmp1.y + 0.0000001;
                tmp1.y = tmp1.w / tmp1.y;
                tmp0.w = tmp0.w * tmp1.y;
                tmp0.w = tmp0.w * 3.141593;
                tmp0.yw = tmp1.xx * tmp0.yw;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * tmp0.z + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.x = dot(tmp2.xyz, tmp2.xyz);
                tmp1.x = tmp1.x != 0.0;
                tmp1.x = tmp1.x ? 1.0 : 0.0;
                tmp0.w = tmp0.w * tmp1.x;
                tmp1.x = _Glossiness - tmp2.w;
                tmp1.x = saturate(tmp1.x + 1.0);
                tmp4.xyz = _LightColor0.xyz * tmp0.yyy + tmp5.xyz;
                tmp5.xyz = tmp0.www * _LightColor0.xyz;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp6.xyz = float3(1.0, 1.0, 1.0) - tmp2.xyz;
                tmp0.xyw = tmp6.xyz * tmp0.xxx + tmp2.xyz;
                tmp0.xyw = tmp0.xyw * tmp5.xyz;
                tmp0.xyw = tmp3.xyz * tmp4.xyz + tmp0.xyw;
                tmp3.xyz = tmp7.xyz * tmp0.zzz;
                tmp1.xyw = tmp1.xxx - tmp2.xyz;
                tmp1.xyz = tmp1.zzz * tmp1.xyw + tmp2.xyz;
                o.sv_target.xyz = tmp3.xyz * tmp1.xyz + tmp0.xyw;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			LOD 200
			Tags { "LIGHTMODE" = "ForwardAdd" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZClip Off
			ZWrite Off
			Cull Off
			GpuProgramID 119209
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 unity_WorldToLight;
			float4 _LightColor0;
			float _Glossiness;
			float _Metallic;
			float4 _Color;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _LightTexture0;
			
			// Keywords: POINT
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord1.xyz = tmp0.www * tmp0.xyz;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: POINT
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = _WorldSpaceLightPos0.xyz - inp.texcoord2.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp1.xyz * tmp0.www + tmp0.xyz;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp0.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp3.xyz = tmp0.www * inp.texcoord1.xyz;
                tmp0.w = saturate(dot(tmp3.xyz, tmp2.xyz));
                tmp1.w = saturate(dot(tmp1.xyz, tmp2.xyz));
                tmp1.x = saturate(dot(tmp3.xyz, tmp1.xyz));
                tmp0.x = dot(tmp3.xyz, tmp0.xyz);
                tmp0.y = 1.0 - _Glossiness;
                tmp0.z = tmp0.y * tmp0.y;
                tmp1.y = tmp0.z * tmp0.z;
                tmp1.z = tmp0.w * tmp1.y + -tmp0.w;
                tmp0.w = tmp1.z * tmp0.w + 1.0;
                tmp0.w = tmp0.w * tmp0.w + 0.0000001;
                tmp1.y = tmp1.y * 0.3183099;
                tmp0.w = tmp1.y / tmp0.w;
                tmp1.y = -tmp0.y * tmp0.y + 1.0;
                tmp1.z = abs(tmp0.x) * tmp1.y + tmp0.z;
                tmp0.z = tmp1.x * tmp1.y + tmp0.z;
                tmp0.z = tmp0.z * abs(tmp0.x);
                tmp0.x = 1.0 - abs(tmp0.x);
                tmp0.z = tmp1.x * tmp1.z + tmp0.z;
                tmp0.z = tmp0.z + 0.00001;
                tmp0.z = 0.5 / tmp0.z;
                tmp0.z = tmp0.w * tmp0.z;
                tmp0.z = tmp0.z * 3.141593;
                tmp0.z = tmp1.x * tmp0.z;
                tmp0.z = max(tmp0.z, 0.0);
                tmp2 = tex2D(_MainTex, inp.texcoord.xy);
                tmp3.xyz = tmp2.xyz * _Color.xyz + float3(-0.04, -0.04, -0.04);
                tmp2.xyz = tmp2.xyz * _Color.xyz;
                tmp3.xyz = _Metallic.xxx * tmp3.xyz + float3(0.04, 0.04, 0.04);
                tmp0.w = dot(tmp3.xyz, tmp3.xyz);
                tmp0.w = tmp0.w != 0.0;
                tmp0.w = tmp0.w ? 1.0 : 0.0;
                tmp0.z = tmp0.w * tmp0.z;
                tmp4.xyz = inp.texcoord2.yyy * unity_WorldToLight._m01_m11_m21;
                tmp4.xyz = unity_WorldToLight._m00_m10_m20 * inp.texcoord2.xxx + tmp4.xyz;
                tmp4.xyz = unity_WorldToLight._m02_m12_m22 * inp.texcoord2.zzz + tmp4.xyz;
                tmp4.xyz = tmp4.xyz + unity_WorldToLight._m03_m13_m23;
                tmp0.w = dot(tmp4.xyz, tmp4.xyz);
                tmp4 = tex2D(_LightTexture0, tmp0.ww);
                tmp4.xyz = tmp4.xxx * _LightColor0.xyz;
                tmp5.xyz = tmp0.zzz * tmp4.xyz;
                tmp0.z = 1.0 - tmp1.w;
                tmp0.w = tmp1.w * tmp1.w;
                tmp0.y = dot(tmp0.xy, tmp0.xy);
                tmp0.y = tmp0.y - 0.5;
                tmp0.w = tmp0.z * tmp0.z;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.z = tmp0.z * tmp0.w;
                tmp1.yzw = float3(1.0, 1.0, 1.0) - tmp3.xyz;
                tmp1.yzw = tmp1.yzw * tmp0.zzz + tmp3.xyz;
                tmp1.yzw = tmp1.yzw * tmp5.xyz;
                tmp0.z = tmp0.x * tmp0.x;
                tmp0.z = tmp0.z * tmp0.z;
                tmp0.x = tmp0.x * tmp0.z;
                tmp0.x = tmp0.y * tmp0.x + 1.0;
                tmp0.z = 1.0 - tmp1.x;
                tmp0.w = tmp0.z * tmp0.z;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.y = tmp0.y * tmp0.z + 1.0;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.x = tmp1.x * tmp0.x;
                tmp0.xyz = tmp0.xxx * tmp4.xyz;
                tmp0.w = -_Metallic * 0.96 + 0.96;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                o.sv_target.xyz = tmp2.xyz * tmp0.xyz + tmp1.yzw;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "DEFERRED"
			LOD 200
			Tags { "LIGHTMODE" = "Deferred" "RenderType" = "Opaque" }
			ZClip Off
			Cull Off
			GpuProgramID 149290
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float4 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
				float4 sv_target1 : SV_Target1;
				float4 sv_target2 : SV_Target2;
				float4 sv_target3 : SV_Target3;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _Glossiness;
			float _Metallic;
			float4 _Color;
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
                float4 tmp1;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.texcoord1.xyz = tmp0.xyz;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                o.texcoord2.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                o.texcoord4 = float4(0.0, 0.0, 0.0, 0.0);
                tmp0.w = tmp0.y * tmp0.y;
                tmp0.w = tmp0.x * tmp0.x + -tmp0.w;
                tmp1 = tmp0.yzzx * tmp0.xyzz;
                tmp0.x = dot(unity_SHBr, tmp1);
                tmp0.y = dot(unity_SHBg, tmp1);
                tmp0.z = dot(unity_SHBb, tmp1);
                o.texcoord5.xyz = unity_SHC.xyz * tmp0.www + tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1.xyz = tmp0.xyz * _Color.xyz;
                tmp0.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp0.w) {
                    tmp0.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp2.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp2.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp2.xyz;
                    tmp2.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp2.xyz;
                    tmp2.xyz = tmp2.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp2.xyz = tmp0.www ? tmp2.xyz : inp.texcoord2.xyz;
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
                    tmp5.xyz = inp.texcoord1.xyz;
                    tmp5.w = 1.0;
                    tmp3.x = dot(tmp3, tmp5);
                    tmp3.y = dot(tmp4, tmp5);
                    tmp3.z = dot(tmp2, tmp5);
                } else {
                    tmp2.xyz = inp.texcoord1.xyz;
                    tmp2.w = 1.0;
                    tmp3.x = dot(unity_SHAr, tmp2);
                    tmp3.y = dot(unity_SHAg, tmp2);
                    tmp3.z = dot(unity_SHAb, tmp2);
                }
                tmp2.xyz = tmp3.xyz + inp.texcoord5.xyz;
                tmp2.xyz = max(tmp2.xyz, float3(0.0, 0.0, 0.0));
                tmp0.xyz = tmp0.xyz * _Color.xyz + float3(-0.04, -0.04, -0.04);
                o.sv_target1.xyz = _Metallic.xxx * tmp0.xyz + float3(0.04, 0.04, 0.04);
                tmp0.x = -_Metallic * 0.96 + 0.96;
                tmp0.xyz = tmp0.xxx * tmp1.xyz;
                tmp1.xyz = tmp2.xyz * tmp0.xyz;
                o.sv_target3.xyz = exp(-tmp1.xyz);
                o.sv_target.xyz = tmp0.xyz;
                o.sv_target.w = 1.0;
                o.sv_target1.w = _Glossiness;
                o.sv_target2.xyz = inp.texcoord1.xyz * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5);
                o.sv_target2.w = 1.0;
                o.sv_target3.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			LOD 200
			Tags { "LIGHTMODE" = "Meta" "RenderType" = "Opaque" }
			ZClip Off
			Cull Off
			GpuProgramID 253452
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Color;
			float unity_OneOverOutputBoost;
			float unity_MaxOutputValue;
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
			sampler2D _MainTex;
			
			// Keywords: 
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
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
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
	Fallback "Diffuse"
}