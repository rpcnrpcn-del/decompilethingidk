Shader "ProBuilder/Standard Vertex Color" {
	Properties {
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base Color", 2D) = "white" {}
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Gloss ("Gloss", Range(0, 1)) = 0.8
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			GpuProgramID 16727
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
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
			float4 _Color;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float _Metallic;
			float _Gloss;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _MainTex;
			
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
                o.texcoord1.zw = v.texcoord2.xy;
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
                tmp0.x = dot(inp.texcoord4.xyz, inp.texcoord4.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.xyz = tmp0.xxx * inp.texcoord4.xyz;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp1.xyz;
                tmp3.xy = inp.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
                tmp3 = tex2D(_BumpMap, tmp3.xy);
                tmp3.xy = tmp3.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp3.xy, tmp3.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp1.w = sqrt(tmp1.w);
                tmp3.yzw = tmp3.yyy * inp.texcoord6.xyz;
                tmp3.xyz = tmp3.xxx * inp.texcoord5.xyz + tmp3.yzw;
                tmp0.xyz = tmp1.www * tmp0.xyz + tmp3.xyz;
                tmp1.w = dot(tmp0.xyz, tmp0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp0.xyz * tmp1.www;
                tmp0.x = dot(-tmp2.xyz, tmp3.xyz);
                tmp0.x = tmp0.x + tmp0.x;
                tmp0.xyz = tmp3.xyz * -tmp0.xxx + -tmp2.xyz;
                tmp1.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp1.www * _WorldSpaceLightPos0.xyz;
                tmp1.xyz = tmp1.xyz * tmp0.www + tmp4.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = 1.0 - _Gloss;
                tmp1.w = 1.0 - tmp0.w;
                tmp2.w = sqrt(tmp1.w);
                tmp4.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp4.w) {
                    tmp4.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp5.xyz = inp.texcoord3.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp5.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.xxx + tmp5.xyz;
                    tmp5.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord3.zzz + tmp5.xyz;
                    tmp5.xyz = tmp5.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp5.xyz = tmp4.www ? tmp5.xyz : inp.texcoord3.xyz;
                    tmp5.xyz = tmp5.xyz - unity_ProbeVolumeMin;
                    tmp5.yzw = tmp5.xyz * unity_ProbeVolumeSizeInv;
                    tmp4.w = tmp5.y * 0.25;
                    tmp5.y = unity_ProbeVolumeParams.z * 0.5;
                    tmp6.x = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp4.w = max(tmp4.w, tmp5.y);
                    tmp5.x = min(tmp6.x, tmp4.w);
                    tmp6 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp5.xzw);
                    tmp7.xyz = tmp5.xzw + float3(0.25, 0.0, 0.0);
                    tmp7 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp7.xyz);
                    tmp5.xyz = tmp5.xzw + float3(0.5, 0.0, 0.0);
                    tmp5 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp5.xyz);
                    tmp3.w = 1.0;
                    tmp6.x = dot(tmp6, tmp3);
                    tmp6.y = dot(tmp7, tmp3);
                    tmp6.z = dot(tmp5, tmp3);
                } else {
                    tmp3.w = 1.0;
                    tmp6.x = dot(unity_SHAr, tmp3);
                    tmp6.y = dot(unity_SHAg, tmp3);
                    tmp6.z = dot(unity_SHAb, tmp3);
                }
                tmp5.xyz = tmp6.xyz + inp.texcoord10.xyz;
                tmp5.xyz = max(tmp5.xyz, float3(0.0, 0.0, 0.0));
                tmp3.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp3.w) {
                    tmp3.w = dot(tmp0.xyz, tmp0.xyz);
                    tmp3.w = rsqrt(tmp3.w);
                    tmp6.xyz = tmp0.xyz * tmp3.www;
                    tmp7.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord3.xyz;
                    tmp7.xyz = tmp7.xyz / tmp6.xyz;
                    tmp8.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord3.xyz;
                    tmp8.xyz = tmp8.xyz / tmp6.xyz;
                    tmp9.xyz = tmp6.xyz > float3(0.0, 0.0, 0.0);
                    tmp7.xyz = tmp9.xyz ? tmp7.xyz : tmp8.xyz;
                    tmp3.w = min(tmp7.y, tmp7.x);
                    tmp3.w = min(tmp7.z, tmp3.w);
                    tmp7.xyz = inp.texcoord3.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp6.xyz = tmp6.xyz * tmp3.www + tmp7.xyz;
                } else {
                    tmp6.xyz = tmp0.xyz;
                }
                tmp3.w = -tmp2.w * 0.7 + 1.7;
                tmp2.w = tmp2.w * tmp3.w;
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
                        tmp4.w = dot(tmp0.xyz, tmp0.xyz);
                        tmp4.w = rsqrt(tmp4.w);
                        tmp8.xyz = tmp0.xyz * tmp4.www;
                        tmp9.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord3.xyz;
                        tmp9.xyz = tmp9.xyz / tmp8.xyz;
                        tmp10.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord3.xyz;
                        tmp10.xyz = tmp10.xyz / tmp8.xyz;
                        tmp11.xyz = tmp8.xyz > float3(0.0, 0.0, 0.0);
                        tmp9.xyz = tmp11.xyz ? tmp9.xyz : tmp10.xyz;
                        tmp4.w = min(tmp9.y, tmp9.x);
                        tmp4.w = min(tmp9.z, tmp4.w);
                        tmp9.xyz = inp.texcoord3.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp0.xyz = tmp8.xyz * tmp4.www + tmp9.xyz;
                    }
                    tmp8 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp0.xyz, tmp2.w));
                    tmp0.x = unity_SpecCube1_HDR.w == 1.0;
                    tmp0.y = log(tmp8.w);
                    tmp0.y = tmp0.y * unity_SpecCube1_HDR.y;
                    tmp0.y = exp(tmp0.y);
                    tmp0.x = tmp0.x ? tmp0.y : 1.0;
                    tmp0.x = tmp0.x * unity_SpecCube1_HDR.x;
                    tmp0.xyz = tmp8.xyz * tmp0.xxx;
                    tmp6.xyz = tmp3.www * tmp6.xyz + -tmp0.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMin.www * tmp6.xyz + tmp0.xyz;
                }
                tmp0.x = dot(tmp3.xyz, tmp4.xyz);
                tmp0.y = dot(tmp4.xyz, tmp1.xyz);
                tmp0.xy = max(tmp0.xy, float2(0.0, 0.0));
                tmp4.xy = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp4 = tex2D(_MainTex, tmp4.xy);
                tmp4.xyz = tmp4.xyz * _Color.xyz;
                tmp6.xyz = tmp4.xyz * inp.color.xyz;
                tmp4.xyz = tmp4.xyz * inp.color.xyz + float3(-0.04, -0.04, -0.04);
                tmp4.xyz = _Metallic.xxx * tmp4.xyz + float3(0.04, 0.04, 0.04);
                tmp0.z = -_Metallic * 0.96 + 0.96;
                tmp6.xyz = tmp0.zzz * tmp6.xyz;
                tmp0.z = 1.0 - tmp0.z;
                tmp2.x = dot(tmp3.xyz, tmp2.xyz);
                tmp2.x = max(tmp2.x, 0.0);
                tmp1.x = dot(tmp3.xyz, tmp1.xyz);
                tmp1.x = max(tmp1.x, 0.0);
                tmp1.y = 1.0 - tmp1.w;
                tmp1.z = tmp2.x * tmp1.y + tmp1.w;
                tmp1.y = tmp0.x * tmp1.y + tmp1.w;
                tmp1.y = tmp1.y * tmp2.x;
                tmp1.y = tmp0.x * tmp1.z + tmp1.y;
                tmp1.y = tmp1.y + 0.00001;
                tmp1.y = 0.5 / tmp1.y;
                tmp1.z = tmp1.w * tmp1.w;
                tmp2.y = tmp1.x * tmp1.z + -tmp1.x;
                tmp1.x = tmp2.y * tmp1.x + 1.0;
                tmp1.z = tmp1.z * 0.3183099;
                tmp1.x = tmp1.x * tmp1.x + 0.0000001;
                tmp1.x = tmp1.z / tmp1.x;
                tmp1.y = tmp0.x * tmp1.y;
                tmp1.x = tmp1.x * tmp1.y;
                tmp1.x = tmp1.x * 0.7853982;
                tmp1.x = tmp0.x * tmp1.x;
                tmp1.x = max(tmp1.x, 0.0);
                tmp1.xyz = tmp1.xxx * _LightColor0.xyz;
                tmp2.y = 1.0 - tmp0.y;
                tmp2.z = tmp2.y * tmp2.y;
                tmp2.z = tmp2.z * tmp2.z;
                tmp2.y = tmp2.y * tmp2.z;
                tmp3.xyz = float3(1.0, 1.0, 1.0) - tmp4.xyz;
                tmp2.yzw = tmp3.xyz * tmp2.yyy + tmp4.xyz;
                tmp0.z = saturate(tmp0.z + tmp0.w);
                tmp0.w = 1.0 - tmp2.x;
                tmp2.x = tmp0.w * tmp0.w;
                tmp2.x = tmp2.x * tmp2.x;
                tmp0.w = tmp0.w * tmp2.x;
                tmp3.xyz = tmp0.zzz - tmp4.xyz;
                tmp3.xyz = tmp0.www * tmp3.xyz + tmp4.xyz;
                tmp3.xyz = tmp3.xyz * tmp7.xyz;
                tmp1.xyz = tmp1.xyz * tmp2.yzw + tmp3.xyz;
                tmp0.z = tmp0.y + tmp0.y;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.z = 1.0 - tmp0.x;
                tmp2.x = tmp0.z * tmp0.z;
                tmp2.x = tmp2.x * tmp2.x;
                tmp0.z = tmp0.z * tmp2.x;
                tmp0.y = tmp0.y * tmp1.w + -0.5;
                tmp0.z = tmp0.y * tmp0.z + 1.0;
                tmp0.y = tmp0.y * tmp0.w + 1.0;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.xyz = tmp0.xxx * _LightColor0.xyz + tmp5.xyz;
                o.sv_target.xyz = tmp0.xyz * tmp6.xyz + tmp1.xyz;
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
			GpuProgramID 130971
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
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
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			float _Metallic;
			float _Gloss;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BumpMap;
			sampler2D _LightTexture0;
			sampler2D _MainTex;
			
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
                o.texcoord1.zw = v.texcoord2.xy;
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
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0.x = dot(inp.texcoord4.xyz, inp.texcoord4.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.xyz = tmp0.xxx * inp.texcoord4.xyz;
                tmp1.xy = inp.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
                tmp1 = tex2D(_BumpMap, tmp1.xy);
                tmp1.xy = tmp1.wy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp2.xyz = tmp1.yyy * inp.texcoord6.xyz;
                tmp2.xyz = tmp1.xxx * inp.texcoord5.xyz + tmp2.xyz;
                tmp0.w = dot(tmp1.xy, tmp1.xy);
                tmp0.w = min(tmp0.w, 1.0);
                tmp0.w = 1.0 - tmp0.w;
                tmp0.w = sqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz + tmp2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp1.xyz;
                tmp1.w = dot(tmp0.xyz, tmp2.xyz);
                tmp1.w = max(tmp1.w, 0.0);
                tmp2.x = 1.0 - _Gloss;
                tmp2.x = 1.0 - tmp2.x;
                tmp2.y = 1.0 - tmp2.x;
                tmp2.z = tmp1.w * tmp2.y + tmp2.x;
                tmp3.xyz = _WorldSpaceLightPos0.www * -inp.texcoord3.xyz + _WorldSpaceLightPos0.xyz;
                tmp2.w = dot(tmp3.xyz, tmp3.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp3.xyz = tmp2.www * tmp3.xyz;
                tmp2.w = dot(tmp0.xyz, tmp3.xyz);
                tmp2.w = max(tmp2.w, 0.0);
                tmp2.y = tmp2.w * tmp2.y + tmp2.x;
                tmp2.y = tmp1.w * tmp2.y;
                tmp1.w = 1.0 - tmp1.w;
                tmp2.y = tmp2.w * tmp2.z + tmp2.y;
                tmp2.y = tmp2.y + 0.00001;
                tmp2.y = 0.5 / tmp2.y;
                tmp2.y = tmp2.y * tmp2.w;
                tmp1.xyz = tmp1.xyz * tmp0.www + tmp3.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.x = dot(tmp0.xyz, tmp1.xyz);
                tmp0.y = dot(tmp3.xyz, tmp1.xyz);
                tmp0.xy = max(tmp0.xy, float2(0.0, 0.0));
                tmp0.z = tmp2.x * tmp2.x;
                tmp0.w = tmp0.x * tmp0.z + -tmp0.x;
                tmp0.x = tmp0.w * tmp0.x + 1.0;
                tmp0.x = tmp0.x * tmp0.x + 0.0000001;
                tmp0.z = tmp0.z * 0.3183099;
                tmp0.x = tmp0.z / tmp0.x;
                tmp0.x = tmp0.x * tmp2.y;
                tmp0.x = tmp0.x * 0.7853982;
                tmp0.x = tmp2.w * tmp0.x;
                tmp0.x = max(tmp0.x, 0.0);
                tmp0.z = dot(inp.texcoord7.xyz, inp.texcoord7.xyz);
                tmp3 = tex2D(_LightTexture0, tmp0.zz);
                tmp1.xyz = tmp3.xxx * _LightColor0.xyz;
                tmp0.xzw = tmp0.xxx * tmp1.xyz;
                tmp0.xzw = tmp0.xzw * _LightColor0.xyz;
                tmp2.y = 1.0 - tmp0.y;
                tmp2.z = tmp2.y * tmp2.y;
                tmp2.z = tmp2.z * tmp2.z;
                tmp2.y = tmp2.y * tmp2.z;
                tmp3.xy = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp3 = tex2D(_MainTex, tmp3.xy);
                tmp3.xyz = tmp3.xyz * _Color.xyz;
                tmp4.xyz = tmp3.xyz * inp.color.xyz + float3(-0.04, -0.04, -0.04);
                tmp3.xyz = tmp3.xyz * inp.color.xyz;
                tmp4.xyz = _Metallic.xxx * tmp4.xyz + float3(0.04, 0.04, 0.04);
                tmp5.xyz = float3(1.0, 1.0, 1.0) - tmp4.xyz;
                tmp4.xyz = tmp5.xyz * tmp2.yyy + tmp4.xyz;
                tmp0.xzw = tmp0.xzw * tmp4.xyz;
                tmp2.y = tmp0.y + tmp0.y;
                tmp0.y = tmp0.y * tmp2.y;
                tmp0.y = tmp0.y * tmp2.x + -0.5;
                tmp2.x = tmp1.w * tmp1.w;
                tmp2.x = tmp2.x * tmp2.x;
                tmp1.w = tmp1.w * tmp2.x;
                tmp1.w = tmp0.y * tmp1.w + 1.0;
                tmp2.x = 1.0 - tmp2.w;
                tmp2.y = tmp2.x * tmp2.x;
                tmp2.y = tmp2.y * tmp2.y;
                tmp2.x = tmp2.x * tmp2.y;
                tmp0.y = tmp0.y * tmp2.x + 1.0;
                tmp0.y = tmp1.w * tmp0.y;
                tmp0.y = tmp2.w * tmp0.y;
                tmp1.xyz = tmp1.xyz * tmp0.yyy;
                tmp0.y = -_Metallic * 0.96 + 0.96;
                tmp2.xyz = tmp0.yyy * tmp3.xyz;
                o.sv_target.xyz = tmp1.xyz * tmp2.xyz + tmp0.xzw;
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
			GpuProgramID 142510
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord3 : TEXCOORD3;
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
			float4 _Color;
			float4 _MainTex_ST;
			float _Metallic;
			float _Gloss;
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
                o.texcoord1.zw = v.texcoord2.xy;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.color = v.color;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp0.xyz = tmp0.xyz * _Color.xyz;
                tmp1.xyz = tmp0.xyz * inp.color.xyz + float3(-0.04, -0.04, -0.04);
                tmp0.xyz = tmp0.xyz * inp.color.xyz;
                tmp1.xyz = _Metallic.xxx * tmp1.xyz + float3(0.04, 0.04, 0.04);
                tmp1.xyz = tmp1.xyz * _Gloss.xxx;
                tmp1.xyz = tmp1.xyz * _Gloss.xxx;
                tmp1.xyz = tmp1.xyz * float3(0.5, 0.5, 0.5);
                tmp0.w = -_Metallic * 0.96 + 0.96;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp1.xyz;
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