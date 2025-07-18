Shader "AG/RadialCooldownShader" {
	Properties {
		_Color ("Color", Color) = (0,1,0,1)
		_BackgroundColor ("Background Color", Color) = (1,0,0,1)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "ForwardBase" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			ZClip Off
			GpuProgramID 58778
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord : TEXCOORD0;
				float texcoord2 : TEXCOORD2;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _PlaneCenterLocal;
			float4 _PlaneNormalLocal;
			float4 _PlaneForwardLocal;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float _Progress;
			float4 _Color;
			float4 _BackgroundColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D unity_NHxRoughness;
			
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
                tmp0.xyz = v.vertex.yzx - _PlaneCenterLocal.yzx;
                tmp0.w = dot(tmp0.xyz, _PlaneNormalLocal.xyz);
                tmp0.xyz = -_PlaneNormalLocal.yzx * tmp0.www + tmp0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = tmp0.xyz * _PlaneForwardLocal.zxy;
                tmp1.xyz = _PlaneForwardLocal.yzx * tmp0.yzx + -tmp1.xyz;
                tmp0.x = dot(_PlaneForwardLocal.xyz, tmp0.xyz);
                tmp0.y = dot(tmp1.xyz, _PlaneNormalLocal.xyz);
                tmp0.y = tmp0.y < 0.0;
                tmp0.z = abs(tmp0.x) * -0.0187293 + 0.074261;
                tmp0.z = tmp0.z * abs(tmp0.x) + -0.2121144;
                tmp0.z = tmp0.z * abs(tmp0.x) + 1.570729;
                tmp0.w = 1.0 - abs(tmp0.x);
                tmp0.x = tmp0.x < -tmp0.x;
                tmp0.w = sqrt(tmp0.w);
                tmp1.x = tmp0.w * tmp0.z;
                tmp1.x = tmp1.x * -2.0 + 3.141593;
                tmp0.x = tmp0.x ? tmp1.x : 0.0;
                tmp0.x = tmp0.z * tmp0.w + tmp0.x;
                tmp0.z = 6.28318 - tmp0.x;
                tmp0.x = tmp0.y ? tmp0.z : tmp0.x;
                o.texcoord2.x = tmp0.x * 0.1591551;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.texcoord.xyz = tmp0.xyz;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
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
                tmp0.xyz = tmp1.xyz + tmp2.xyz;
                o.texcoord3.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
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
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord1.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(-tmp0.xyz, inp.texcoord.xyz);
                tmp0.w = tmp0.w + tmp0.w;
                tmp1.xyz = inp.texcoord.xyz * -tmp0.www + -tmp0.xyz;
                tmp1 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp1.xyz, 6.0));
                tmp0.w = log(tmp1.w);
                tmp0.w = tmp0.w * unity_SpecCube0_HDR.y;
                tmp0.w = exp(tmp0.w);
                tmp1.w = unity_SpecCube0_HDR.w == 1.0;
                tmp0.w = tmp1.w ? tmp0.w : 1.0;
                tmp0.w = tmp0.w * unity_SpecCube0_HDR.x;
                tmp1.xyz = tmp1.xyz * tmp0.www;
                tmp0.w = dot(inp.texcoord.xyz, inp.texcoord.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * inp.texcoord.xyz;
                tmp0.w = dot(tmp0.xyz, tmp2.xyz);
                tmp1.w = tmp0.w + tmp0.w;
                tmp0.w = saturate(tmp0.w);
                tmp3.y = 1.0 - tmp0.w;
                tmp0.xyz = tmp2.xyz * -tmp1.www + tmp0.xyz;
                tmp0.w = saturate(dot(tmp2.xyz, _WorldSpaceLightPos0.xyz));
                tmp2.xyz = tmp0.www * _LightColor0.xyz;
                tmp3.x = dot(tmp0.xyz, _WorldSpaceLightPos0.xyz);
                tmp0.xy = tmp3.xy * tmp3.xy;
                tmp0.xy = tmp0.xy * tmp0.xy;
                tmp0.y = tmp0.y * 0.0 + 0.04;
                tmp1.xyz = tmp0.yyy * tmp1.xyz;
                tmp0.y = _Progress < inp.texcoord2.x;
                tmp0.y = tmp0.y ? 1.0 : 0.0;
                tmp3.xyz = _BackgroundColor.xyz - _Color.xyz;
                tmp3.xyz = tmp0.yyy * tmp3.xyz + _Color.xyz;
                tmp3.xyz = tmp3.xyz * float3(0.96, 0.96, 0.96);
                tmp1.xyz = inp.texcoord3.xyz * tmp3.xyz + tmp1.xyz;
                tmp0.z = 1.0;
                tmp0 = tex2D(unity_NHxRoughness, tmp0.xz);
                tmp0.xyz = tmp0.xxx * float3(0.64, 0.64, 0.64) + tmp3.xyz;
                o.sv_target.xyz = tmp0.xyz * tmp2.xyz + tmp1.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "ForwardAdd" "RenderType" = "Opaque" }
			Blend One One, One One
			ZClip Off
			ZWrite Off
			GpuProgramID 78119
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord : TEXCOORD0;
				float texcoord2 : TEXCOORD2;
				float3 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _PlaneCenterLocal;
			float4 _PlaneNormalLocal;
			float4 _PlaneForwardLocal;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 unity_WorldToLight;
			float4 _LightColor0;
			float _Progress;
			float4 _Color;
			float4 _BackgroundColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _LightTexture0;
			sampler2D unity_NHxRoughness;
			
			// Keywords: POINT
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = tmp0 + glstate_matrix_mvp._m03_m13_m23_m33;
                tmp0.xyz = v.vertex.yzx - _PlaneCenterLocal.yzx;
                tmp0.w = dot(tmp0.xyz, _PlaneNormalLocal.xyz);
                tmp0.xyz = -_PlaneNormalLocal.yzx * tmp0.www + tmp0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = tmp0.xyz * _PlaneForwardLocal.zxy;
                tmp1.xyz = _PlaneForwardLocal.yzx * tmp0.yzx + -tmp1.xyz;
                tmp0.x = dot(_PlaneForwardLocal.xyz, tmp0.xyz);
                tmp0.y = dot(tmp1.xyz, _PlaneNormalLocal.xyz);
                tmp0.y = tmp0.y < 0.0;
                tmp0.z = abs(tmp0.x) * -0.0187293 + 0.074261;
                tmp0.z = tmp0.z * abs(tmp0.x) + -0.2121144;
                tmp0.z = tmp0.z * abs(tmp0.x) + 1.570729;
                tmp0.w = 1.0 - abs(tmp0.x);
                tmp0.x = tmp0.x < -tmp0.x;
                tmp0.w = sqrt(tmp0.w);
                tmp1.x = tmp0.w * tmp0.z;
                tmp1.x = tmp1.x * -2.0 + 3.141593;
                tmp0.x = tmp0.x ? tmp1.x : 0.0;
                tmp0.x = tmp0.z * tmp0.w + tmp0.x;
                tmp0.z = 6.28318 - tmp0.x;
                tmp0.x = tmp0.y ? tmp0.z : tmp0.x;
                o.texcoord2.x = tmp0.x * 0.1591551;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                o.texcoord.xyz = tmp0.www * tmp0.xyz;
                tmp0.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp0.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp0.xyz;
                tmp0.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp0.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp0.xyz;
                return o;
			}
			// Keywords: POINT
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord1.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = dot(inp.texcoord.xyz, inp.texcoord.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * inp.texcoord.xyz;
                tmp0.w = dot(tmp0.xyz, tmp1.xyz);
                tmp0.w = tmp0.w + tmp0.w;
                tmp0.xyz = tmp1.xyz * -tmp0.www + tmp0.xyz;
                tmp2.xyz = _WorldSpaceLightPos0.xyz - inp.texcoord1.xyz;
                tmp0.w = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp0.x = dot(tmp0.xyz, tmp2.xyz);
                tmp0.y = saturate(dot(tmp1.xyz, tmp2.xyz));
                tmp0.x = tmp0.x * tmp0.x;
                tmp1.x = tmp0.x * tmp0.x;
                tmp1.y = 1.0;
                tmp1 = tex2D(unity_NHxRoughness, tmp1.xy);
                tmp0.x = tmp1.x * 0.64;
                tmp0.z = _Progress < inp.texcoord2.x;
                tmp0.z = tmp0.z ? 1.0 : 0.0;
                tmp1.xyz = _BackgroundColor.xyz - _Color.xyz;
                tmp1.xyz = tmp0.zzz * tmp1.xyz + _Color.xyz;
                tmp0.xzw = tmp1.xyz * float3(0.96, 0.96, 0.96) + tmp0.xxx;
                tmp1.xyz = inp.texcoord1.yyy * unity_WorldToLight._m01_m11_m21;
                tmp1.xyz = unity_WorldToLight._m00_m10_m20 * inp.texcoord1.xxx + tmp1.xyz;
                tmp1.xyz = unity_WorldToLight._m02_m12_m22 * inp.texcoord1.zzz + tmp1.xyz;
                tmp1.xyz = tmp1.xyz + unity_WorldToLight._m03_m13_m23;
                tmp1.x = dot(tmp1.xyz, tmp1.xyz);
                tmp1 = tex2D(_LightTexture0, tmp1.xx);
                tmp1.xyz = tmp1.xxx * _LightColor0.xyz;
                tmp1.xyz = tmp0.yyy * tmp1.xyz;
                o.sv_target.xyz = tmp0.xzw * tmp1.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "DEFERRED"
			Tags { "LIGHTMODE" = "Deferred" "RenderType" = "Opaque" }
			ZClip Off
			GpuProgramID 190234
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord : TEXCOORD0;
				float texcoord2 : TEXCOORD2;
				float3 texcoord1 : TEXCOORD1;
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
			float4 _PlaneCenterLocal;
			float4 _PlaneNormalLocal;
			float4 _PlaneForwardLocal;
			// $Globals ConstantBuffers for Fragment Shader
			float _Progress;
			float4 _Color;
			float4 _BackgroundColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
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
                tmp0.xyz = v.vertex.yzx - _PlaneCenterLocal.yzx;
                tmp0.w = dot(tmp0.xyz, _PlaneNormalLocal.xyz);
                tmp0.xyz = -_PlaneNormalLocal.yzx * tmp0.www + tmp0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = tmp0.xyz * _PlaneForwardLocal.zxy;
                tmp1.xyz = _PlaneForwardLocal.yzx * tmp0.yzx + -tmp1.xyz;
                tmp0.x = dot(_PlaneForwardLocal.xyz, tmp0.xyz);
                tmp0.y = dot(tmp1.xyz, _PlaneNormalLocal.xyz);
                tmp0.y = tmp0.y < 0.0;
                tmp0.z = abs(tmp0.x) * -0.0187293 + 0.074261;
                tmp0.z = tmp0.z * abs(tmp0.x) + -0.2121144;
                tmp0.z = tmp0.z * abs(tmp0.x) + 1.570729;
                tmp0.w = 1.0 - abs(tmp0.x);
                tmp0.x = tmp0.x < -tmp0.x;
                tmp0.w = sqrt(tmp0.w);
                tmp1.x = tmp0.w * tmp0.z;
                tmp1.x = tmp1.x * -2.0 + 3.141593;
                tmp0.x = tmp0.x ? tmp1.x : 0.0;
                tmp0.x = tmp0.z * tmp0.w + tmp0.x;
                tmp0.z = 6.28318 - tmp0.x;
                tmp0.x = tmp0.y ? tmp0.z : tmp0.x;
                o.texcoord2.x = tmp0.x * 0.1591551;
                tmp0.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp0.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp0.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.texcoord.xyz = tmp0.xyz;
                tmp1.xyz = v.vertex.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp1.xyz = unity_ObjectToWorld._m00_m10_m20 * v.vertex.xxx + tmp1.xyz;
                tmp1.xyz = unity_ObjectToWorld._m02_m12_m22 * v.vertex.zzz + tmp1.xyz;
                o.texcoord1.xyz = unity_ObjectToWorld._m03_m13_m23 * v.vertex.www + tmp1.xyz;
                o.texcoord4 = float4(0.0, 0.0, 0.0, 0.0);
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
                tmp0.xyz = tmp1.xyz + tmp2.xyz;
                o.texcoord5.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0.x = _Progress < inp.texcoord2.x;
                tmp0.x = tmp0.x ? 1.0 : 0.0;
                tmp0.yzw = _BackgroundColor.xyz - _Color.xyz;
                tmp0.xyz = tmp0.xxx * tmp0.yzw + _Color.xyz;
                tmp0.xyz = tmp0.xyz * float3(0.96, 0.96, 0.96);
                o.sv_target.xyz = tmp0.xyz;
                tmp0.xyz = tmp0.xyz * inp.texcoord5.xyz;
                o.sv_target3.xyz = exp(-tmp0.xyz);
                o.sv_target.w = 1.0;
                o.sv_target1 = float4(0.04, 0.04, 0.04, 0.0);
                o.sv_target2.xyz = inp.texcoord.xyz * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5);
                o.sv_target2.w = 1.0;
                o.sv_target3.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "LIGHTMODE" = "Meta" "RenderType" = "Opaque" }
			ZClip Off
			Cull Off
			GpuProgramID 253212
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float3 texcoord : TEXCOORD0;
				float texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _PlaneCenterLocal;
			float4 _PlaneNormalLocal;
			float4 _PlaneForwardLocal;
			// $Globals ConstantBuffers for Fragment Shader
			float _Progress;
			float4 _Color;
			float4 _BackgroundColor;
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
                tmp0.xyz = v.vertex.yzx - _PlaneCenterLocal.yzx;
                tmp0.w = dot(tmp0.xyz, _PlaneNormalLocal.xyz);
                tmp0.xyz = -_PlaneNormalLocal.yzx * tmp0.www + tmp0.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = tmp0.xyz * _PlaneForwardLocal.zxy;
                tmp1.xyz = _PlaneForwardLocal.yzx * tmp0.yzx + -tmp1.xyz;
                tmp0.x = dot(_PlaneForwardLocal.xyz, tmp0.xyz);
                tmp0.y = dot(tmp1.xyz, _PlaneNormalLocal.xyz);
                tmp0.y = tmp0.y < 0.0;
                tmp0.z = abs(tmp0.x) * -0.0187293 + 0.074261;
                tmp0.z = tmp0.z * abs(tmp0.x) + -0.2121144;
                tmp0.z = tmp0.z * abs(tmp0.x) + 1.570729;
                tmp0.w = 1.0 - abs(tmp0.x);
                tmp0.x = tmp0.x < -tmp0.x;
                tmp0.w = sqrt(tmp0.w);
                tmp1.x = tmp0.w * tmp0.z;
                tmp1.x = tmp1.x * -2.0 + 3.141593;
                tmp0.x = tmp0.x ? tmp1.x : 0.0;
                tmp0.x = tmp0.z * tmp0.w + tmp0.x;
                tmp0.z = 6.28318 - tmp0.x;
                tmp0.x = tmp0.y ? tmp0.z : tmp0.x;
                o.texcoord1.x = tmp0.x * 0.1591551;
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
                tmp0.x = _Progress < inp.texcoord1.x;
                tmp0.x = tmp0.x ? 1.0 : 0.0;
                tmp0.yzw = _BackgroundColor.xyz - _Color.xyz;
                tmp0.xyz = tmp0.xxx * tmp0.yzw + _Color.xyz;
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