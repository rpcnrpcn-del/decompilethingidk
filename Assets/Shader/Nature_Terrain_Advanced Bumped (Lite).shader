Shader "Nature/Terrain/Advanced Bumped (Lite)" {
	Properties {
		_Depth ("Blend Depth", Range(0.001, 1)) = 0.1
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "black" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "black" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "black" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "black" {}
		[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SplatCount" = "4" }
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "ForwardBase" "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" "SplatCount" = "4" }
			ZClip Off
			GpuProgramID 4443
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
				float4 texcoord9 : TEXCOORD9;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Control_ST;
			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float _Depth;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Control;
			sampler2D _Splat0;
			sampler2D _Splat1;
			sampler2D _Splat2;
			sampler2D _Splat3;
			sampler2D _Normal0;
			sampler2D _Normal1;
			sampler2D _Normal2;
			sampler2D _Normal3;
			
			// Keywords: DIRECTIONAL
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
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _Control_ST.xy + _Control_ST.zw;
                o.texcoord.zw = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
                o.texcoord1.xy = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
                o.texcoord1.zw = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                o.texcoord3.w = tmp0.x;
                tmp0.xw = v.normal.zx * float2(0.0, 1.0);
                tmp0.xw = v.normal.yz * float2(1.0, 0.0) + -tmp0.xw;
                tmp1.xyz = tmp0.www * unity_ObjectToWorld._m11_m21_m01;
                tmp1.xyz = unity_ObjectToWorld._m10_m20_m00 * tmp0.xxx + tmp1.xyz;
                tmp0.x = dot(tmp1.xyz, tmp1.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                o.texcoord3.x = tmp1.z;
                tmp2.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp2.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp2.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp2 = tmp0.xxxx * tmp2.xyzz;
                tmp3.xyz = tmp1.xyz * tmp2.wxy;
                tmp3.xyz = tmp2.ywx * tmp1.yzx + -tmp3.xyz;
                tmp3.xyz = tmp3.xyz * -unity_WorldTransformParams.www;
                o.texcoord3.y = tmp3.x;
                o.texcoord3.z = tmp2.x;
                o.texcoord4.x = tmp1.x;
                o.texcoord5.x = tmp1.y;
                o.texcoord4.w = tmp0.y;
                o.texcoord5.w = tmp0.z;
                o.texcoord4.y = tmp3.y;
                o.texcoord5.y = tmp3.z;
                o.texcoord4.z = tmp2.y;
                o.texcoord5.z = tmp2.w;
                tmp0.x = tmp2.y * tmp2.y;
                tmp0.x = tmp2.x * tmp2.x + -tmp0.x;
                tmp1 = tmp2.ywzx * tmp2;
                tmp2.x = dot(unity_SHBr, tmp1);
                tmp2.y = dot(unity_SHBg, tmp1);
                tmp2.z = dot(unity_SHBb, tmp1);
                o.texcoord6.xyz = unity_SHC.xyz * tmp0.xxx + tmp2.xyz;
                o.texcoord9 = float4(0.0, 0.0, 0.0, 0.0);
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
                tmp0 = tex2D(_Control, inp.texcoord.xy);
                tmp1 = tex2D(_Splat0, inp.texcoord.zw);
                tmp2 = tex2D(_Splat1, inp.texcoord1.xy);
                tmp3 = tex2D(_Splat2, inp.texcoord1.zw);
                tmp4 = tex2D(_Splat3, inp.texcoord2.xy);
                tmp5.x = tmp0.x + tmp1.w;
                tmp5.y = tmp0.y + tmp2.w;
                tmp5.z = tmp0.z + tmp3.w;
                tmp5.w = tmp0.w + tmp4.w;
                tmp1.w = max(tmp5.w, tmp5.z);
                tmp1.w = max(tmp1.w, tmp5.y);
                tmp1.w = max(tmp1.w, tmp5.x);
                tmp5 = tmp5 - tmp1.wwww;
                tmp5 = tmp5 + _Depth.xxxx;
                tmp5 = max(tmp5, float4(0.0, 0.0, 0.0, 0.0));
                tmp1.w = dot(tmp5, float4(1.0, 1.0, 1.0, 1.0));
                tmp5 = tmp5 / tmp1.wwww;
                tmp2.xyz = tmp2.xyz * tmp5.yyy;
                tmp1.xyz = tmp5.xxx * tmp1.xyz + tmp2.xyz;
                tmp1.xyz = tmp5.zzz * tmp3.xyz + tmp1.xyz;
                tmp1.xyz = tmp5.www * tmp4.xyz + tmp1.xyz;
                tmp2 = tex2D(_Normal0, inp.texcoord.zw);
                tmp3 = tex2D(_Normal1, inp.texcoord1.xy);
                tmp2.xz = tmp3.wy * tmp5.yy;
                tmp2.xy = tmp5.xx * tmp2.wy + tmp2.xz;
                tmp3 = tex2D(_Normal2, inp.texcoord1.zw);
                tmp2.xy = tmp5.zz * tmp3.wy + tmp2.xy;
                tmp3 = tex2D(_Normal3, inp.texcoord2.xy);
                tmp2.xy = tmp5.ww * tmp3.wy + tmp2.xy;
                tmp0.x = dot(tmp0, float4(1.0, 1.0, 1.0, 1.0));
                tmp0.yz = tmp2.xy - float2(0.5, 0.5);
                tmp0.yz = tmp0.xx * tmp0.yz + float2(0.5, 0.5);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                tmp0.xy = tmp0.yz * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.w = dot(tmp0.xy, tmp0.xy);
                tmp0.w = min(tmp0.w, 1.0);
                tmp0.w = 1.0 - tmp0.w;
                tmp0.z = sqrt(tmp0.w);
                tmp2.x = dot(inp.texcoord3.xyz, tmp0.xyz);
                tmp2.y = dot(inp.texcoord4.xyz, tmp0.xyz);
                tmp2.z = dot(inp.texcoord5.xyz, tmp0.xyz);
                tmp0.x = unity_ProbeVolumeParams.x == 1.0;
                if (tmp0.x) {
                    tmp0.x = unity_ProbeVolumeParams.y == 1.0;
                    tmp0.yzw = inp.texcoord4.www * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp0.yzw = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.www + tmp0.yzw;
                    tmp0.yzw = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord5.www + tmp0.yzw;
                    tmp0.yzw = tmp0.yzw + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp3.y = inp.texcoord3.w;
                    tmp3.z = inp.texcoord4.w;
                    tmp3.w = inp.texcoord5.w;
                    tmp0.xyz = tmp0.xxx ? tmp0.yzw : tmp3.yzw;
                    tmp0.xyz = tmp0.xyz - unity_ProbeVolumeMin;
                    tmp0.yzw = tmp0.xyz * unity_ProbeVolumeSizeInv;
                    tmp0.y = tmp0.y * 0.25;
                    tmp1.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp3.x = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp0.y = max(tmp0.y, tmp1.w);
                    tmp0.x = min(tmp3.x, tmp0.y);
                    tmp3 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp0.xzw);
                    tmp4.xyz = tmp0.xzw + float3(0.25, 0.0, 0.0);
                    tmp4 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xyz);
                    tmp0.xyz = tmp0.xzw + float3(0.5, 0.0, 0.0);
                    tmp0 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp0.xyz);
                    tmp2.w = 1.0;
                    tmp3.x = dot(tmp3, tmp2);
                    tmp3.y = dot(tmp4, tmp2);
                    tmp3.z = dot(tmp0, tmp2);
                } else {
                    tmp2.w = 1.0;
                    tmp3.x = dot(unity_SHAr, tmp2);
                    tmp3.y = dot(unity_SHAg, tmp2);
                    tmp3.z = dot(unity_SHAb, tmp2);
                }
                tmp0.xyz = tmp3.xyz + inp.texcoord6.xyz;
                tmp0.w = dot(tmp2.xyz, _WorldSpaceLightPos0.xyz);
                tmp0 = max(tmp0, float4(0.0, 0.0, 0.0, 0.0));
                tmp2.xyz = tmp1.xyz * _LightColor0.xyz;
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                o.sv_target.xyz = tmp2.xyz * tmp0.www + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "ForwardAdd" "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SplatCount" = "4" }
			Blend One One, One One
			ZClip Off
			ZWrite Off
			GpuProgramID 80752
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Control_ST;
			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 unity_WorldToLight;
			float4 _LightColor0;
			float _Depth;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Control;
			sampler2D _Splat0;
			sampler2D _Splat1;
			sampler2D _Splat2;
			sampler2D _Splat3;
			sampler2D _Normal0;
			sampler2D _Normal1;
			sampler2D _Normal2;
			sampler2D _Normal3;
			sampler2D _LightTexture0;
			
			// Keywords: POINT
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _Control_ST.xy + _Control_ST.zw;
                o.texcoord.zw = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
                o.texcoord1.xy = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
                o.texcoord1.zw = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
                tmp0.xy = v.normal.zx * float2(0.0, 1.0);
                tmp0.xy = v.normal.yz * float2(1.0, 0.0) + -tmp0.xy;
                tmp0.yzw = tmp0.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp0.xyz = unity_ObjectToWorld._m10_m20_m00 * tmp0.xxx + tmp0.yzw;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp2.xyz = tmp1.zxy * tmp0.yzx + -tmp2.xyz;
                tmp2.xyz = tmp2.xyz * -unity_WorldTransformParams.www;
                o.texcoord3.y = tmp2.x;
                o.texcoord3.x = tmp0.z;
                o.texcoord3.z = tmp1.y;
                o.texcoord4.x = tmp0.x;
                o.texcoord5.x = tmp0.y;
                o.texcoord4.z = tmp1.z;
                o.texcoord5.z = tmp1.x;
                o.texcoord4.y = tmp2.y;
                o.texcoord5.y = tmp2.z;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord6.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
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
                float4 tmp6;
                tmp0.xyz = inp.texcoord6.yyy * unity_WorldToLight._m01_m11_m21;
                tmp0.xyz = unity_WorldToLight._m00_m10_m20 * inp.texcoord6.xxx + tmp0.xyz;
                tmp0.xyz = unity_WorldToLight._m02_m12_m22 * inp.texcoord6.zzz + tmp0.xyz;
                tmp0.xyz = tmp0.xyz + unity_WorldToLight._m03_m13_m23;
                tmp0.x = dot(tmp0.xyz, tmp0.xyz);
                tmp0 = tex2D(_LightTexture0, tmp0.xx);
                tmp0.xyz = tmp0.xxx * _LightColor0.xyz;
                tmp1 = tex2D(_Splat2, inp.texcoord1.zw);
                tmp2 = tex2D(_Control, inp.texcoord.xy);
                tmp3.z = tmp1.w + tmp2.z;
                tmp4 = tex2D(_Splat3, inp.texcoord2.xy);
                tmp3.w = tmp2.w + tmp4.w;
                tmp0.w = max(tmp3.w, tmp3.z);
                tmp5 = tex2D(_Splat1, inp.texcoord1.xy);
                tmp3.y = tmp2.y + tmp5.w;
                tmp0.w = max(tmp0.w, tmp3.y);
                tmp6 = tex2D(_Splat0, inp.texcoord.zw);
                tmp3.x = tmp2.x + tmp6.w;
                tmp1.w = dot(tmp2, float4(1.0, 1.0, 1.0, 1.0));
                tmp0.w = max(tmp0.w, tmp3.x);
                tmp2 = tmp3 - tmp0.wwww;
                tmp2 = tmp2 + _Depth.xxxx;
                tmp2 = max(tmp2, float4(0.0, 0.0, 0.0, 0.0));
                tmp0.w = dot(tmp2, float4(1.0, 1.0, 1.0, 1.0));
                tmp2 = tmp2 / tmp0.wwww;
                tmp3.xyz = tmp5.xyz * tmp2.yyy;
                tmp3.xyz = tmp2.xxx * tmp6.xyz + tmp3.xyz;
                tmp1.xyz = tmp2.zzz * tmp1.xyz + tmp3.xyz;
                tmp1.xyz = tmp2.www * tmp4.xyz + tmp1.xyz;
                tmp1.xyz = tmp1.www * tmp1.xyz;
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp3 = tex2D(_Normal1, inp.texcoord1.xy);
                tmp1.xy = tmp2.yy * tmp3.wy;
                tmp3 = tex2D(_Normal0, inp.texcoord.zw);
                tmp1.xy = tmp2.xx * tmp3.wy + tmp1.xy;
                tmp3 = tex2D(_Normal2, inp.texcoord1.zw);
                tmp1.xy = tmp2.zz * tmp3.wy + tmp1.xy;
                tmp3 = tex2D(_Normal3, inp.texcoord2.xy);
                tmp1.xy = tmp2.ww * tmp3.wy + tmp1.xy;
                tmp1.xy = tmp1.xy - float2(0.5, 0.5);
                tmp1.xy = tmp1.ww * tmp1.xy + float2(0.5, 0.5);
                tmp1.xy = tmp1.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.w = dot(tmp1.xy, tmp1.xy);
                tmp0.w = min(tmp0.w, 1.0);
                tmp0.w = 1.0 - tmp0.w;
                tmp1.z = sqrt(tmp0.w);
                tmp2.x = dot(inp.texcoord3.xyz, tmp1.xyz);
                tmp2.y = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp2.z = dot(inp.texcoord5.xyz, tmp1.xyz);
                tmp1.xyz = _WorldSpaceLightPos0.xyz - inp.texcoord6.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(tmp2.xyz, tmp1.xyz);
                tmp0.w = max(tmp0.w, 0.0);
                o.sv_target.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "PREPASS"
			Tags { "LIGHTMODE" = "PrePassBase" "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SplatCount" = "4" }
			ZClip Off
			GpuProgramID 137522
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 texcoord5 : TEXCOORD5;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Control_ST;
			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _Depth;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Control;
			sampler2D _Splat0;
			sampler2D _Splat1;
			sampler2D _Splat2;
			sampler2D _Splat3;
			sampler2D _Normal0;
			sampler2D _Normal1;
			sampler2D _Normal2;
			sampler2D _Normal3;
			
			// Keywords: 
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
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _Control_ST.xy + _Control_ST.zw;
                o.texcoord.zw = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
                o.texcoord1.xy = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
                o.texcoord1.zw = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
                tmp0.xy = v.normal.zx * float2(0.0, 1.0);
                tmp0.xy = v.normal.yz * float2(1.0, 0.0) + -tmp0.xy;
                tmp0.yzw = tmp0.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp0.xyz = unity_ObjectToWorld._m10_m20_m00 * tmp0.xxx + tmp0.yzw;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp2.xyz = tmp1.zxy * tmp0.yzx + -tmp2.xyz;
                tmp2.xyz = tmp2.xyz * -unity_WorldTransformParams.www;
                o.texcoord3.y = tmp2.x;
                tmp3.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp3.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp3.xyz;
                o.texcoord3.w = tmp3.x;
                o.texcoord3.x = tmp0.z;
                o.texcoord3.z = tmp1.y;
                o.texcoord4.x = tmp0.x;
                o.texcoord5.x = tmp0.y;
                o.texcoord4.z = tmp1.z;
                o.texcoord5.z = tmp1.x;
                o.texcoord4.w = tmp3.y;
                o.texcoord5.w = tmp3.z;
                o.texcoord4.y = tmp2.y;
                o.texcoord5.y = tmp2.z;
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
                tmp0 = tex2D(_Splat2, inp.texcoord1.zw);
                tmp1 = tex2D(_Control, inp.texcoord.xy);
                tmp0.z = tmp0.w + tmp1.z;
                tmp2 = tex2D(_Splat3, inp.texcoord2.xy);
                tmp0.w = tmp1.w + tmp2.w;
                tmp2.x = max(tmp0.w, tmp0.z);
                tmp3 = tex2D(_Splat1, inp.texcoord1.xy);
                tmp0.y = tmp1.y + tmp3.w;
                tmp2.x = max(tmp2.x, tmp0.y);
                tmp3 = tex2D(_Splat0, inp.texcoord.zw);
                tmp0.x = tmp1.x + tmp3.w;
                tmp1.x = dot(tmp1, float4(1.0, 1.0, 1.0, 1.0));
                tmp1.y = max(tmp2.x, tmp0.x);
                tmp0 = tmp0 - tmp1.yyyy;
                tmp0 = tmp0 + _Depth.xxxx;
                tmp0 = max(tmp0, float4(0.0, 0.0, 0.0, 0.0));
                tmp1.y = dot(tmp0, float4(1.0, 1.0, 1.0, 1.0));
                tmp0 = tmp0 / tmp1.yyyy;
                tmp2 = tex2D(_Normal1, inp.texcoord1.xy);
                tmp1.yz = tmp0.yy * tmp2.wy;
                tmp2 = tex2D(_Normal0, inp.texcoord.zw);
                tmp0.xy = tmp0.xx * tmp2.wy + tmp1.yz;
                tmp2 = tex2D(_Normal2, inp.texcoord1.zw);
                tmp0.xy = tmp0.zz * tmp2.wy + tmp0.xy;
                tmp2 = tex2D(_Normal3, inp.texcoord2.xy);
                tmp0.xy = tmp0.ww * tmp2.wy + tmp0.xy;
                tmp0.xy = tmp0.xy - float2(0.5, 0.5);
                tmp0.xy = tmp1.xx * tmp0.xy + float2(0.5, 0.5);
                tmp0.xy = tmp0.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.w = dot(tmp0.xy, tmp0.xy);
                tmp0.w = min(tmp0.w, 1.0);
                tmp0.w = 1.0 - tmp0.w;
                tmp0.z = sqrt(tmp0.w);
                tmp1.x = dot(inp.texcoord3.xyz, tmp0.xyz);
                tmp1.y = dot(inp.texcoord4.xyz, tmp0.xyz);
                tmp1.z = dot(inp.texcoord5.xyz, tmp0.xyz);
                o.sv_target.xyz = tmp1.xyz * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5);
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "PREPASS"
			Tags { "LIGHTMODE" = "PrePassFinal" "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SplatCount" = "4" }
			ZClip Off
			ZWrite Off
			GpuProgramID 209687
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Control_ST;
			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _Depth;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Control;
			sampler2D _Splat0;
			sampler2D _Splat1;
			sampler2D _Splat2;
			sampler2D _Splat3;
			sampler2D _LightBuffer;
			
			// Keywords: 
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
                tmp0 = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.position = tmp0;
                o.texcoord.xy = v.texcoord.xy * _Control_ST.xy + _Control_ST.zw;
                o.texcoord.zw = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
                o.texcoord1.xy = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
                o.texcoord1.zw = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                o.texcoord3.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord4.zw = tmp0.zw;
                o.texcoord4.xy = tmp1.zz + tmp1.xw;
                o.texcoord5 = float4(0.0, 0.0, 0.0, 0.0);
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.x = tmp0.y * tmp0.y;
                tmp1.x = tmp0.x * tmp0.x + -tmp1.x;
                tmp2 = tmp0.yzzx * tmp0.xyzz;
                tmp3.x = dot(unity_SHBr, tmp2);
                tmp3.y = dot(unity_SHBg, tmp2);
                tmp3.z = dot(unity_SHBb, tmp2);
                tmp1.xyz = unity_SHC.xyz * tmp1.xxx + tmp3.xyz;
                tmp0.w = 1.0;
                tmp2.x = dot(unity_SHAr, tmp0);
                tmp2.y = dot(unity_SHAg, tmp0);
                tmp2.z = dot(unity_SHAb, tmp0);
                o.texcoord6.xyz = tmp1.xyz + tmp2.xyz;
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
                tmp0 = tex2D(_Splat2, inp.texcoord1.zw);
                tmp1 = tex2D(_Control, inp.texcoord.xy);
                tmp2.z = tmp0.w + tmp1.z;
                tmp3 = tex2D(_Splat3, inp.texcoord2.xy);
                tmp2.w = tmp1.w + tmp3.w;
                tmp0.w = max(tmp2.w, tmp2.z);
                tmp4 = tex2D(_Splat1, inp.texcoord1.xy);
                tmp2.y = tmp1.y + tmp4.w;
                tmp0.w = max(tmp0.w, tmp2.y);
                tmp5 = tex2D(_Splat0, inp.texcoord.zw);
                tmp2.x = tmp1.x + tmp5.w;
                tmp1.x = dot(tmp1, float4(1.0, 1.0, 1.0, 1.0));
                tmp0.w = max(tmp0.w, tmp2.x);
                tmp2 = tmp2 - tmp0.wwww;
                tmp2 = tmp2 + _Depth.xxxx;
                tmp2 = max(tmp2, float4(0.0, 0.0, 0.0, 0.0));
                tmp0.w = dot(tmp2, float4(1.0, 1.0, 1.0, 1.0));
                tmp2 = tmp2 / tmp0.wwww;
                tmp1.yzw = tmp4.xyz * tmp2.yyy;
                tmp1.yzw = tmp2.xxx * tmp5.xyz + tmp1.yzw;
                tmp0.xyz = tmp2.zzz * tmp0.xyz + tmp1.yzw;
                tmp0.xyz = tmp2.www * tmp3.xyz + tmp0.xyz;
                tmp0.xyz = tmp1.xxx * tmp0.xyz;
                tmp1.xy = inp.texcoord4.xy / inp.texcoord4.ww;
                tmp1 = tex2D(_LightBuffer, tmp1.xy);
                tmp1.xyz = log(tmp1.xyz);
                tmp1.xyz = inp.texcoord6.xyz - tmp1.xyz;
                o.sv_target.xyz = tmp0.xyz * tmp1.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "DEFERRED"
			Tags { "LIGHTMODE" = "Deferred" "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SplatCount" = "4" }
			ZClip Off
			GpuProgramID 286400
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 texcoord5 : TEXCOORD5;
				float4 texcoord6 : TEXCOORD6;
				float3 texcoord7 : TEXCOORD7;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
				float4 sv_target1 : SV_Target1;
				float4 sv_target2 : SV_Target2;
				float4 sv_target3 : SV_Target3;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Control_ST;
			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _Depth;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Control;
			sampler2D _Splat0;
			sampler2D _Splat1;
			sampler2D _Splat2;
			sampler2D _Splat3;
			sampler2D _Normal0;
			sampler2D _Normal1;
			sampler2D _Normal2;
			sampler2D _Normal3;
			
			// Keywords: 
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
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy * _Control_ST.xy + _Control_ST.zw;
                o.texcoord.zw = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
                o.texcoord1.xy = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
                o.texcoord1.zw = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                o.texcoord3.w = tmp0.x;
                tmp0.xw = v.normal.zx * float2(0.0, 1.0);
                tmp0.xw = v.normal.yz * float2(1.0, 0.0) + -tmp0.xw;
                tmp1.xyz = tmp0.www * unity_ObjectToWorld._m11_m21_m01;
                tmp1.xyz = unity_ObjectToWorld._m10_m20_m00 * tmp0.xxx + tmp1.xyz;
                tmp0.x = dot(tmp1.xyz, tmp1.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                o.texcoord3.x = tmp1.z;
                tmp2.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp2.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp2.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp2 = tmp0.xxxx * tmp2.xyzz;
                tmp3.xyz = tmp1.xyz * tmp2.wxy;
                tmp3.xyz = tmp2.ywx * tmp1.yzx + -tmp3.xyz;
                tmp3.xyz = tmp3.xyz * -unity_WorldTransformParams.www;
                o.texcoord3.y = tmp3.x;
                o.texcoord3.z = tmp2.x;
                o.texcoord4.x = tmp1.x;
                o.texcoord5.x = tmp1.y;
                o.texcoord4.w = tmp0.y;
                o.texcoord5.w = tmp0.z;
                o.texcoord4.y = tmp3.y;
                o.texcoord5.y = tmp3.z;
                o.texcoord4.z = tmp2.y;
                o.texcoord5.z = tmp2.w;
                o.texcoord6 = float4(0.0, 0.0, 0.0, 0.0);
                tmp0.x = tmp2.y * tmp2.y;
                tmp0.x = tmp2.x * tmp2.x + -tmp0.x;
                tmp1 = tmp2.ywzx * tmp2;
                tmp2.x = dot(unity_SHBr, tmp1);
                tmp2.y = dot(unity_SHBg, tmp1);
                tmp2.z = dot(unity_SHBb, tmp1);
                o.texcoord7.xyz = unity_SHC.xyz * tmp0.xxx + tmp2.xyz;
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
                tmp0 = tex2D(_Control, inp.texcoord.xy);
                tmp1 = tex2D(_Splat0, inp.texcoord.zw);
                tmp2 = tex2D(_Splat1, inp.texcoord1.xy);
                tmp3 = tex2D(_Splat2, inp.texcoord1.zw);
                tmp4 = tex2D(_Splat3, inp.texcoord2.xy);
                tmp5.x = tmp0.x + tmp1.w;
                tmp5.y = tmp0.y + tmp2.w;
                tmp5.z = tmp0.z + tmp3.w;
                tmp5.w = tmp0.w + tmp4.w;
                tmp1.w = max(tmp5.w, tmp5.z);
                tmp1.w = max(tmp1.w, tmp5.y);
                tmp1.w = max(tmp1.w, tmp5.x);
                tmp5 = tmp5 - tmp1.wwww;
                tmp5 = tmp5 + _Depth.xxxx;
                tmp5 = max(tmp5, float4(0.0, 0.0, 0.0, 0.0));
                tmp1.w = dot(tmp5, float4(1.0, 1.0, 1.0, 1.0));
                tmp5 = tmp5 / tmp1.wwww;
                tmp2.xyz = tmp2.xyz * tmp5.yyy;
                tmp1.xyz = tmp5.xxx * tmp1.xyz + tmp2.xyz;
                tmp1.xyz = tmp5.zzz * tmp3.xyz + tmp1.xyz;
                tmp1.xyz = tmp5.www * tmp4.xyz + tmp1.xyz;
                tmp2 = tex2D(_Normal0, inp.texcoord.zw);
                tmp3 = tex2D(_Normal1, inp.texcoord1.xy);
                tmp2.xz = tmp3.wy * tmp5.yy;
                tmp2.xy = tmp5.xx * tmp2.wy + tmp2.xz;
                tmp3 = tex2D(_Normal2, inp.texcoord1.zw);
                tmp2.xy = tmp5.zz * tmp3.wy + tmp2.xy;
                tmp3 = tex2D(_Normal3, inp.texcoord2.xy);
                tmp2.xy = tmp5.ww * tmp3.wy + tmp2.xy;
                tmp0.x = dot(tmp0, float4(1.0, 1.0, 1.0, 1.0));
                tmp0.yz = tmp2.xy - float2(0.5, 0.5);
                tmp0.yz = tmp0.xx * tmp0.yz + float2(0.5, 0.5);
                tmp1.xyz = tmp0.xxx * tmp1.xyz;
                tmp0.xy = tmp0.yz * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.w = dot(tmp0.xy, tmp0.xy);
                tmp0.w = min(tmp0.w, 1.0);
                tmp0.w = 1.0 - tmp0.w;
                tmp0.z = sqrt(tmp0.w);
                tmp2.x = dot(inp.texcoord3.xyz, tmp0.xyz);
                tmp2.y = dot(inp.texcoord4.xyz, tmp0.xyz);
                tmp2.z = dot(inp.texcoord5.xyz, tmp0.xyz);
                tmp0.x = unity_ProbeVolumeParams.x == 1.0;
                if (tmp0.x) {
                    tmp0.x = unity_ProbeVolumeParams.y == 1.0;
                    tmp0.yzw = inp.texcoord4.www * unity_ProbeVolumeWorldToObject._m01_m11_m21;
                    tmp0.yzw = unity_ProbeVolumeWorldToObject._m00_m10_m20 * inp.texcoord3.www + tmp0.yzw;
                    tmp0.yzw = unity_ProbeVolumeWorldToObject._m02_m12_m22 * inp.texcoord5.www + tmp0.yzw;
                    tmp0.yzw = tmp0.yzw + unity_ProbeVolumeWorldToObject._m03_m13_m23;
                    tmp3.y = inp.texcoord3.w;
                    tmp3.z = inp.texcoord4.w;
                    tmp3.w = inp.texcoord5.w;
                    tmp0.xyz = tmp0.xxx ? tmp0.yzw : tmp3.yzw;
                    tmp0.xyz = tmp0.xyz - unity_ProbeVolumeMin;
                    tmp0.yzw = tmp0.xyz * unity_ProbeVolumeSizeInv;
                    tmp0.y = tmp0.y * 0.25;
                    tmp1.w = unity_ProbeVolumeParams.z * 0.5;
                    tmp3.x = -unity_ProbeVolumeParams.z * 0.5 + 0.25;
                    tmp0.y = max(tmp0.y, tmp1.w);
                    tmp0.x = min(tmp3.x, tmp0.y);
                    tmp3 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp0.xzw);
                    tmp4.xyz = tmp0.xzw + float3(0.25, 0.0, 0.0);
                    tmp4 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp4.xyz);
                    tmp0.xyz = tmp0.xzw + float3(0.5, 0.0, 0.0);
                    tmp0 = UNITY_SAMPLE_TEX3D_SAMPLER(unity_ProbeVolumeSH, unity_ProbeVolumeSH, tmp0.xyz);
                    tmp2.w = 1.0;
                    tmp3.x = dot(tmp3, tmp2);
                    tmp3.y = dot(tmp4, tmp2);
                    tmp3.z = dot(tmp0, tmp2);
                } else {
                    tmp2.w = 1.0;
                    tmp3.x = dot(unity_SHAr, tmp2);
                    tmp3.y = dot(unity_SHAg, tmp2);
                    tmp3.z = dot(unity_SHAb, tmp2);
                }
                tmp0.xyz = tmp3.xyz + inp.texcoord7.xyz;
                tmp0.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                o.sv_target2.xyz = tmp2.xyz * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5);
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                o.sv_target3.xyz = exp(-tmp0.xyz);
                o.sv_target.xyz = tmp1.xyz;
                o.sv_target.w = 1.0;
                o.sv_target1 = float4(0.0, 0.0, 0.0, 0.0);
                o.sv_target2.w = 1.0;
                o.sv_target3.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "LIGHTMODE" = "Meta" "QUEUE" = "Geometry-100" "RenderType" = "Opaque" "SplatCount" = "4" }
			ZClip Off
			Cull Off
			GpuProgramID 343128
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 texcoord5 : TEXCOORD5;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Control_ST;
			float4 _Splat0_ST;
			float4 _Splat1_ST;
			float4 _Splat2_ST;
			float4 _Splat3_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float _Depth;
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
			sampler2D _Control;
			sampler2D _Splat0;
			sampler2D _Splat1;
			sampler2D _Splat2;
			sampler2D _Splat3;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
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
                o.texcoord.xy = v.texcoord.xy * _Control_ST.xy + _Control_ST.zw;
                o.texcoord.zw = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
                o.texcoord1.xy = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
                o.texcoord1.zw = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
                o.texcoord2.xy = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
                tmp0.xy = v.normal.zx * float2(0.0, 1.0);
                tmp0.xy = v.normal.yz * float2(1.0, 0.0) + -tmp0.xy;
                tmp0.yzw = tmp0.yyy * unity_ObjectToWorld._m11_m21_m01;
                tmp0.xyz = unity_ObjectToWorld._m10_m20_m00 * tmp0.xxx + tmp0.yzw;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.y = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp1.z = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp1.x = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp2.xyz = tmp1.zxy * tmp0.yzx + -tmp2.xyz;
                tmp2.xyz = tmp2.xyz * -unity_WorldTransformParams.www;
                o.texcoord3.y = tmp2.x;
                tmp3.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp3.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp3.xyz;
                o.texcoord3.w = tmp3.x;
                o.texcoord3.x = tmp0.z;
                o.texcoord3.z = tmp1.y;
                o.texcoord4.x = tmp0.x;
                o.texcoord5.x = tmp0.y;
                o.texcoord4.z = tmp1.z;
                o.texcoord5.z = tmp1.x;
                o.texcoord4.w = tmp3.y;
                o.texcoord5.w = tmp3.z;
                o.texcoord4.y = tmp2.y;
                o.texcoord5.y = tmp2.z;
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
                tmp0 = tex2D(_Splat2, inp.texcoord1.zw);
                tmp1 = tex2D(_Control, inp.texcoord.xy);
                tmp2.z = tmp0.w + tmp1.z;
                tmp3 = tex2D(_Splat3, inp.texcoord2.xy);
                tmp2.w = tmp1.w + tmp3.w;
                tmp0.w = max(tmp2.w, tmp2.z);
                tmp4 = tex2D(_Splat1, inp.texcoord1.xy);
                tmp2.y = tmp1.y + tmp4.w;
                tmp0.w = max(tmp0.w, tmp2.y);
                tmp5 = tex2D(_Splat0, inp.texcoord.zw);
                tmp2.x = tmp1.x + tmp5.w;
                tmp1.x = dot(tmp1, float4(1.0, 1.0, 1.0, 1.0));
                tmp0.w = max(tmp0.w, tmp2.x);
                tmp2 = tmp2 - tmp0.wwww;
                tmp2 = tmp2 + _Depth.xxxx;
                tmp2 = max(tmp2, float4(0.0, 0.0, 0.0, 0.0));
                tmp0.w = dot(tmp2, float4(1.0, 1.0, 1.0, 1.0));
                tmp2 = tmp2 / tmp0.wwww;
                tmp1.yzw = tmp4.xyz * tmp2.yyy;
                tmp1.yzw = tmp2.xxx * tmp5.xyz + tmp1.yzw;
                tmp0.xyz = tmp2.zzz * tmp0.xyz + tmp1.yzw;
                tmp0.xyz = tmp2.www * tmp3.xyz + tmp0.xyz;
                tmp0.xyz = tmp1.xxx * tmp0.xyz;
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
	Fallback "Nature/Terrain/Diffuse"
}