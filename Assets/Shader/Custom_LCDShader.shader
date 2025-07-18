Shader "Custom/LCDShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_EmissionStrength ("Emission Strength", Range(0, 1)) = 0.5
		_PixelEffect ("PixelEffect", Range(0, 1)) = 0.18
		_ScanlineEffect ("ScanlineAlpha", Range(0, 1)) = 0.326
		_ScanSpeed ("Scanline Speed", Range(0, 100)) = 5.9
		_NumPixelsX ("LCD X Resolution", Float) = 128
		_NumPixelsY ("LCD Y Resolution", Float) = 9.5
	}
	SubShader {
		LOD 200
		Tags { "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			LOD 200
			Tags { "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			GpuProgramID 35927
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
			float _PixelEffect;
			float _ScanlineEffect;
			float _ScanSpeed;
			float _EmissionStrength;
			int _NumPixelsX;
			int _NumPixelsY;
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
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2 = tex2D(_MainTex, inp.texcoord.xy);
                tmp3.xy = tmp3.xy * inp.texcoord.xy;
                tmp1.w = tmp3.x * 3.0;
                null = tmp1.w / tmp1.w;
                tmp2.w = _Time.x * _ScanSpeed + tmp3.y;
                tmp2.w = uint1(tmp2.w) & uint1(0.0);
                tmp3.x = 1.0 - _PixelEffect;
                tmp1.w = floor(tmp1.w);
                tmp3.yz = tmp1.ww == float2(0.0, 2.0);
                tmp4.xyz = tmp3.yyy ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                tmp1.w = tmp3.z ? 1.0 : 0.0;
                tmp3.yzw = tmp3.yyy ? float3(0.0, -1.0, 1.0) : float3(-1.0, 0.0, 1.0);
                tmp3.yzw = tmp1.www * tmp3.yzw + tmp4.xyz;
                tmp3.xyz = max(tmp3.yzw, tmp3.xxx);
                tmp2.xyz = tmp2.xyz * tmp3.xyz;
                tmp1.w = floor(tmp2.w);
                tmp1.w = tmp1.w == 0.0;
                tmp1.w = tmp1.w ? 1.0 : 0.0;
                tmp1.w = tmp1.w * _ScanlineEffect;
                tmp2.xyz = tmp1.www * -tmp2.xyz + tmp2.xyz;
                tmp1.w = 1.0 - _Glossiness;
                tmp2.w = dot(-tmp1.xyz, inp.texcoord1.xyz);
                tmp2.w = tmp2.w + tmp2.w;
                tmp3.xyz = inp.texcoord1.xyz * -tmp2.www + -tmp1.xyz;
                tmp2.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp2.w) {
                    tmp2.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp4.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp4.xyz;
                    tmp4.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp4.xyz;
                    tmp4.xyz = tmp4.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp4.xyz = tmp2.www ? tmp4.xyz : inp.texcoord2.xyz;
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
                    tmp7.xyz = inp.texcoord1.xyz;
                    tmp7.w = 1.0;
                    tmp5.x = dot(tmp5, tmp7);
                    tmp5.y = dot(tmp6, tmp7);
                    tmp5.z = dot(tmp4, tmp7);
                } else {
                    tmp4.xyz = inp.texcoord1.xyz;
                    tmp4.w = 1.0;
                    tmp5.x = dot(unity_SHAr, tmp4);
                    tmp5.y = dot(unity_SHAg, tmp4);
                    tmp5.z = dot(unity_SHAb, tmp4);
                }
                tmp4.xyz = tmp5.xyz + inp.texcoord3.xyz;
                tmp4.xyz = max(tmp4.xyz, float3(0.0, 0.0, 0.0));
                tmp2.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp2.w) {
                    tmp2.w = dot(tmp3.xyz, tmp3.xyz);
                    tmp2.w = rsqrt(tmp2.w);
                    tmp5.xyz = tmp2.www * tmp3.xyz;
                    tmp6.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord2.xyz;
                    tmp6.xyz = tmp6.xyz / tmp5.xyz;
                    tmp7.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord2.xyz;
                    tmp7.xyz = tmp7.xyz / tmp5.xyz;
                    tmp8.xyz = tmp5.xyz > float3(0.0, 0.0, 0.0);
                    tmp6.xyz = tmp8.xyz ? tmp6.xyz : tmp7.xyz;
                    tmp2.w = min(tmp6.y, tmp6.x);
                    tmp2.w = min(tmp6.z, tmp2.w);
                    tmp6.xyz = inp.texcoord2.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp5.xyz = tmp5.xyz * tmp2.www + tmp6.xyz;
                } else {
                    tmp5.xyz = tmp3.xyz;
                }
                tmp2.w = -tmp1.w * 0.7 + 1.7;
                tmp2.w = tmp1.w * tmp2.w;
                tmp2.w = tmp2.w * 6.0;
                tmp5 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp5.xyz, tmp2.w));
                tmp3.w = unity_SpecCube0_HDR.w == 1.0;
                tmp4.w = log(tmp5.w);
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.y;
                tmp4.w = exp(tmp4.w);
                tmp3.w = tmp3.w ? tmp4.w : 1.0;
                tmp3.w = tmp3.w * unity_SpecCube0_HDR.x;
                tmp6.xyz = tmp5.xyz * tmp3.www;
                tmp4.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp4.w) {
                    tmp4.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp4.w) {
                        tmp4.w = dot(tmp3.xyz, tmp3.xyz);
                        tmp4.w = rsqrt(tmp4.w);
                        tmp7.xyz = tmp3.xyz * tmp4.www;
                        tmp8.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord2.xyz;
                        tmp8.xyz = tmp8.xyz / tmp7.xyz;
                        tmp9.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord2.xyz;
                        tmp9.xyz = tmp9.xyz / tmp7.xyz;
                        tmp10.xyz = tmp7.xyz > float3(0.0, 0.0, 0.0);
                        tmp8.xyz = tmp10.xyz ? tmp8.xyz : tmp9.xyz;
                        tmp4.w = min(tmp8.y, tmp8.x);
                        tmp4.w = min(tmp8.z, tmp4.w);
                        tmp8.xyz = inp.texcoord2.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp3.xyz = tmp7.xyz * tmp4.www + tmp8.xyz;
                    }
                    tmp7 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp3.xyz, tmp2.w));
                    tmp2.w = unity_SpecCube1_HDR.w == 1.0;
                    tmp3.x = log(tmp7.w);
                    tmp3.x = tmp3.x * unity_SpecCube1_HDR.y;
                    tmp3.x = exp(tmp3.x);
                    tmp2.w = tmp2.w ? tmp3.x : 1.0;
                    tmp2.w = tmp2.w * unity_SpecCube1_HDR.x;
                    tmp3.xyz = tmp7.xyz * tmp2.www;
                    tmp5.xyz = tmp3.www * tmp5.xyz + -tmp3.xyz;
                    tmp6.xyz = unity_SpecCube0_BoxMin.www * tmp5.xyz + tmp3.xyz;
                }
                tmp2.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp2.w = rsqrt(tmp2.w);
                tmp3.xyz = tmp2.www * inp.texcoord1.xyz;
                tmp5.xyz = tmp2.xyz * float3(0.96, 0.96, 0.96);
                tmp0.xyz = tmp0.xyz * tmp0.www + _WorldSpaceLightPos0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(tmp3.xyz, tmp1.xyz);
                tmp1.x = saturate(dot(tmp3.xyz, _WorldSpaceLightPos0.xyz));
                tmp1.y = saturate(dot(tmp3.xyz, tmp0.xyz));
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
                tmp2.w = tmp1.z * tmp1.z;
                tmp2.w = tmp2.w * tmp2.w;
                tmp1.z = tmp1.z * tmp2.w;
                tmp0.y = tmp0.y * tmp1.z + 1.0;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.z = tmp1.w * tmp1.w;
                tmp1.w = -tmp1.w * tmp1.w + 1.0;
                tmp2.w = abs(tmp0.w) * tmp1.w + tmp0.z;
                tmp1.w = tmp1.x * tmp1.w + tmp0.z;
                tmp0.w = abs(tmp0.w) * tmp1.w;
                tmp0.w = tmp1.x * tmp2.w + tmp0.w;
                tmp0.w = tmp0.w + 0.00001;
                tmp0.w = 0.5 / tmp0.w;
                tmp1.w = tmp0.z * tmp0.z;
                tmp2.w = tmp1.y * tmp1.w + -tmp1.y;
                tmp1.y = tmp2.w * tmp1.y + 1.0;
                tmp1.w = tmp1.w * 0.3183099;
                tmp1.y = tmp1.y * tmp1.y + 0.0000001;
                tmp1.y = tmp1.w / tmp1.y;
                tmp0.w = tmp0.w * tmp1.y;
                tmp0.w = tmp0.w * 3.141593;
                tmp0.yw = tmp1.xx * tmp0.yw;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * tmp0.z + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.x = saturate(_Glossiness + 0.04);
                tmp3.xyz = _LightColor0.xyz * tmp0.yyy + tmp4.xyz;
                tmp4.xyz = tmp0.www * _LightColor0.xyz;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.x = tmp0.x * 0.96 + 0.04;
                tmp0.xyw = tmp0.xxx * tmp4.xyz;
                tmp0.xyw = tmp5.xyz * tmp3.xyz + tmp0.xyw;
                tmp3.xyz = tmp6.xyz * tmp0.zzz;
                tmp0.z = tmp1.x - 0.04;
                tmp0.z = tmp1.z * tmp0.z + 0.04;
                tmp0.xyz = tmp3.xyz * tmp0.zzz + tmp0.xyw;
                o.sv_target.xyz = tmp2.xyz * _EmissionStrength.xxx + tmp0.xyz;
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
			GpuProgramID 86350
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
			float _PixelEffect;
			float _ScanlineEffect;
			float _ScanSpeed;
			int _NumPixelsX;
			int _NumPixelsY;
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
                tmp0.xy = tmp0.xy * inp.texcoord.xy;
                tmp0.x = tmp0.x * 3.0;
                tmp0.y = _Time.x * _ScanSpeed + tmp0.y;
                tmp0.y = uint1(tmp0.y) & uint1(0.0);
                tmp0.y = floor(tmp0.y);
                tmp0.y = tmp0.y == 0.0;
                tmp0.y = tmp0.y ? 1.0 : 0.0;
                tmp0.y = tmp0.y * _ScanlineEffect;
                null = tmp0.x / tmp0.x;
                tmp0.x = floor(tmp0.x);
                tmp0.xz = tmp0.xx == float2(0.0, 2.0);
                tmp1.xyz = tmp0.xxx ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                tmp0.z = tmp0.z ? 1.0 : 0.0;
                tmp2.xyz = tmp0.xxx ? float3(0.0, -1.0, 1.0) : float3(-1.0, 0.0, 1.0);
                tmp0.xzw = tmp0.zzz * tmp2.xyz + tmp1.xyz;
                tmp1.x = 1.0 - _PixelEffect;
                tmp0.xzw = max(tmp0.xzw, tmp1.xxx);
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.xzw = tmp0.xzw * tmp1.xyz;
                tmp0.xyz = tmp0.yyy * -tmp0.xzw + tmp0.xzw;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord2.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = _WorldSpaceLightPos0.xyz - inp.texcoord2.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp3.xyz = tmp2.xyz * tmp0.www + tmp1.xyz;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp0.w = dot(tmp3.xyz, tmp3.xyz);
                tmp0.w = max(tmp0.w, 0.001);
                tmp0.w = rsqrt(tmp0.w);
                tmp3.xyz = tmp0.www * tmp3.xyz;
                tmp0.w = dot(inp.texcoord1.xyz, inp.texcoord1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp4.xyz = tmp0.www * inp.texcoord1.xyz;
                tmp0.w = saturate(dot(tmp4.xyz, tmp3.xyz));
                tmp1.w = saturate(dot(tmp2.xyz, tmp3.xyz));
                tmp2.x = saturate(dot(tmp4.xyz, tmp2.xyz));
                tmp1.x = dot(tmp4.xyz, tmp1.xyz);
                tmp1.y = 1.0 - _Glossiness;
                tmp1.z = tmp1.y * tmp1.y;
                tmp2.y = tmp1.z * tmp1.z;
                tmp2.z = tmp0.w * tmp2.y + -tmp0.w;
                tmp0.w = tmp2.z * tmp0.w + 1.0;
                tmp0.w = tmp0.w * tmp0.w + 0.0000001;
                tmp2.y = tmp2.y * 0.3183099;
                tmp0.w = tmp2.y / tmp0.w;
                tmp2.y = -tmp1.y * tmp1.y + 1.0;
                tmp2.z = abs(tmp1.x) * tmp2.y + tmp1.z;
                tmp1.z = tmp2.x * tmp2.y + tmp1.z;
                tmp1.z = tmp1.z * abs(tmp1.x);
                tmp1.x = 1.0 - abs(tmp1.x);
                tmp1.z = tmp2.x * tmp2.z + tmp1.z;
                tmp1.z = tmp1.z + 0.00001;
                tmp1.z = 0.5 / tmp1.z;
                tmp0.w = tmp0.w * tmp1.z;
                tmp0 = tmp0 * float4(0.96, 0.96, 0.96, 3.141593);
                tmp0.w = tmp2.x * tmp0.w;
                tmp0.w = max(tmp0.w, 0.0);
                tmp2.yzw = inp.texcoord2.yyy * unity_WorldToLight._m01_m11_m21;
                tmp2.yzw = unity_WorldToLight._m00_m10_m20 * inp.texcoord2.xxx + tmp2.yzw;
                tmp2.yzw = unity_WorldToLight._m02_m12_m22 * inp.texcoord2.zzz + tmp2.yzw;
                tmp2.yzw = tmp2.yzw + unity_WorldToLight._m03_m13_m23;
                tmp1.z = dot(tmp2.xyz, tmp2.xyz);
                tmp3 = tex2D(_LightTexture0, tmp1.zz);
                tmp2.yzw = tmp3.xxx * _LightColor0.xyz;
                tmp3.xyz = tmp0.www * tmp2.yzw;
                tmp0.w = 1.0 - tmp1.w;
                tmp1.z = tmp0.w * tmp0.w;
                tmp1.z = tmp1.z * tmp1.z;
                tmp0.w = tmp0.w * tmp1.z;
                tmp0.w = tmp0.w * 0.96 + 0.04;
                tmp3.xyz = tmp0.www * tmp3.xyz;
                tmp0.w = tmp1.x * tmp1.x;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.w = tmp1.x * tmp0.w;
                tmp1.x = tmp1.w + tmp1.w;
                tmp1.x = tmp1.w * tmp1.x;
                tmp1.x = tmp1.x * tmp1.y + -0.5;
                tmp0.w = tmp1.x * tmp0.w + 1.0;
                tmp1.y = 1.0 - tmp2.x;
                tmp1.z = tmp1.y * tmp1.y;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.y = tmp1.y * tmp1.z;
                tmp1.x = tmp1.x * tmp1.y + 1.0;
                tmp0.w = tmp0.w * tmp1.x;
                tmp0.w = tmp2.x * tmp0.w;
                tmp1.xyz = tmp0.www * tmp2.yzw;
                o.sv_target.xyz = tmp0.xyz * tmp1.xyz + tmp3.xyz;
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
			GpuProgramID 157650
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
			float _PixelEffect;
			float _ScanlineEffect;
			float _ScanSpeed;
			float _EmissionStrength;
			int _NumPixelsX;
			int _NumPixelsY;
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
                tmp0 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1.xy = tmp1.xy * inp.texcoord.xy;
                tmp0.w = tmp1.x * 3.0;
                null = tmp0.w / tmp0.w;
                tmp1.x = _Time.x * _ScanSpeed + tmp1.y;
                tmp1.x = uint1(tmp1.x) & uint1(0.0);
                tmp1.y = 1.0 - _PixelEffect;
                tmp0.w = floor(tmp0.w);
                tmp1.zw = tmp0.ww == float2(0.0, 2.0);
                tmp2.xyz = tmp1.zzz ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                tmp0.w = tmp1.w ? 1.0 : 0.0;
                tmp3.xyz = tmp1.zzz ? float3(0.0, -1.0, 1.0) : float3(-1.0, 0.0, 1.0);
                tmp2.xyz = tmp0.www * tmp3.xyz + tmp2.xyz;
                tmp1.yzw = max(tmp1.yyy, tmp2.xyz);
                tmp0.xyz = tmp0.xyz * tmp1.yzw;
                tmp0.w = floor(tmp1.x);
                tmp0.w = tmp0.w == 0.0;
                tmp0.w = tmp0.w ? 1.0 : 0.0;
                tmp0.w = tmp0.w * _ScanlineEffect;
                tmp0.xyz = tmp0.www * -tmp0.xyz + tmp0.xyz;
                tmp0.w = unity_ProbeVolumeParams.x == 1.0;
                if (tmp0.w) {
                    tmp0.w = unity_ProbeVolumeParams.y == 1.0;
                    tmp1.xyz = inp.texcoord2.yyy * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp1.xyz = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord2.xxx + tmp1.xyz;
                    tmp1.xyz = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord2.zzz + tmp1.xyz;
                    tmp1.xyz = tmp1.xyz + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp1.xyz = tmp0.www ? tmp1.xyz : inp.texcoord2.xyz;
                    tmp1.xyz = tmp1.xyz - unity_ProbeVolumeMin;
                    tmp1.yzw = tmp1.xyz * unity_ProbeVolumeSizeInv;
                    tmp0.w = tmp1.y * 0.25;
                    tmp1.y = unity_ProbeVolumeParams.z * 0.5;
                    tmp2.x = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp0.w = max(tmp0.w, tmp1.y);
                    tmp1.x = min(tmp2.x, tmp0.w);
                    tmp2 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp1.xzw);
                    tmp3.xyz = tmp1.xzw + float3(0.25, 0.0, 0.0);
                    tmp3 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp3.xyz);
                    tmp1.xyz = tmp1.xzw + float3(0.5, 0.0, 0.0);
                    tmp1 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp1.xyz);
                    tmp4.xyz = inp.texcoord1.xyz;
                    tmp4.w = 1.0;
                    tmp2.x = dot(tmp2, tmp4);
                    tmp2.y = dot(tmp3, tmp4);
                    tmp2.z = dot(tmp1, tmp4);
                } else {
                    tmp1.xyz = inp.texcoord1.xyz;
                    tmp1.w = 1.0;
                    tmp2.x = dot(unity_SHAr, tmp1);
                    tmp2.y = dot(unity_SHAg, tmp1);
                    tmp2.z = dot(unity_SHAb, tmp1);
                }
                tmp1.xyz = tmp2.xyz + inp.texcoord5.xyz;
                tmp1.xyz = max(tmp1.xyz, float3(0.0, 0.0, 0.0));
                tmp2.xyz = tmp0.xyz * float3(0.96, 0.96, 0.96);
                tmp1.xyz = tmp1.xyz * tmp2.xyz;
                tmp0.xyz = tmp0.xyz * _EmissionStrength.xxx + tmp1.xyz;
                o.sv_target3.xyz = exp(-tmp0.xyz);
                o.sv_target.xyz = tmp2.xyz;
                o.sv_target.w = 1.0;
                o.sv_target1.xyz = float3(0.04, 0.04, 0.04);
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
			GpuProgramID 240163
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
			float _PixelEffect;
			float _ScanlineEffect;
			float _ScanSpeed;
			float _EmissionStrength;
			int _NumPixelsX;
			int _NumPixelsY;
			float unity_OneOverOutputBoost;
			float unity_MaxOutputValue;
			float unity_UseLinearSpace;
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
                float4 tmp1;
                float4 tmp2;
                tmp0.xy = tmp0.xy * inp.texcoord.xy;
                tmp0.x = tmp0.x * 3.0;
                tmp0.y = _Time.x * _ScanSpeed + tmp0.y;
                tmp0.y = uint1(tmp0.y) & uint1(0.0);
                tmp0.y = floor(tmp0.y);
                tmp0.y = tmp0.y == 0.0;
                tmp0.y = tmp0.y ? 1.0 : 0.0;
                tmp0.y = tmp0.y * _ScanlineEffect;
                null = tmp0.x / tmp0.x;
                tmp0.x = floor(tmp0.x);
                tmp0.xz = tmp0.xx == float2(0.0, 2.0);
                tmp1.xyz = tmp0.xxx ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
                tmp0.z = tmp0.z ? 1.0 : 0.0;
                tmp2.xyz = tmp0.xxx ? float3(0.0, -1.0, 1.0) : float3(-1.0, 0.0, 1.0);
                tmp0.xzw = tmp0.zzz * tmp2.xyz + tmp1.xyz;
                tmp1.x = 1.0 - _PixelEffect;
                tmp0.xzw = max(tmp0.xzw, tmp1.xxx);
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.xzw = tmp0.xzw * tmp1.xyz;
                tmp0.xyz = tmp0.yyy * -tmp0.xzw + tmp0.xzw;
                tmp1.xyz = log(tmp0.xyz);
                tmp0.xyz = tmp0.xyz * _EmissionStrength.xxx;
                tmp0.w = saturate(unity_OneOverOutputBoost);
                tmp1.xyz = tmp1.xyz * tmp0.www;
                tmp1.xyz = exp(tmp1.xyz);
                tmp1.xyz = min(tmp1.xyz, unity_MaxOutputValue.xxx);
                tmp1.w = 1.0;
                tmp1 = unity_MetaFragmentControl ? tmp1 : float4(0.0, 0.0, 0.0, 0.0);
                tmp2.xyz = tmp0.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp2.xyz = tmp0.xyz * tmp2.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp2.xyz = tmp0.xyz * tmp2.xyz;
                tmp0.w = unity_UseLinearSpace != 0.0;
                tmp0.xyz = tmp0.www ? tmp0.xyz : tmp2.xyz;
                tmp0.xyz = tmp0.xyz * float3(0.0103093, 0.0103093, 0.0103093);
                tmp0.w = max(tmp0.y, tmp0.x);
                tmp2.x = max(tmp0.z, 0.02);
                tmp0.w = max(tmp0.w, tmp2.x);
                tmp0.w = tmp0.w * 255.0;
                tmp0.w = ceil(tmp0.w);
                tmp2.w = tmp0.w * 0.0039216;
                tmp2.xyz = tmp0.xyz / tmp2.www;
                o.sv_target = unity_MetaFragmentControl ? tmp2 : tmp1;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}