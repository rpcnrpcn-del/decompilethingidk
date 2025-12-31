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
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			GpuProgramID 48467
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
				float4 texcoord9 : TEXCOORD9;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
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
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp1 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp0 = tmp1.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp0 = unity_MatrixVP._m00_m10_m20_m30 * tmp1.xxxx + tmp0;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp1.zzzz + tmp0;
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp1.wwww + tmp0;
                o.position = tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp2.x = dot(v.normal.xyz, unity_WorldToObject._m00_m10_m20);
                tmp2.y = dot(v.normal.xyz, unity_WorldToObject._m01_m11_m21);
                tmp2.z = dot(v.normal.xyz, unity_WorldToObject._m02_m12_m22);
                tmp0.z = dot(tmp2.xyz, tmp2.xyz);
                tmp0.z = rsqrt(tmp0.z);
                tmp2.xyz = tmp0.zzz * tmp2.xyz;
                o.texcoord4.xyz = tmp2.xyz;
                tmp3.xyz = v.tangent.yyy * unity_ObjectToWorld._m01_m11_m21;
                tmp3.xyz = unity_ObjectToWorld._m00_m10_m20 * v.tangent.xxx + tmp3.xyz;
                tmp3.xyz = unity_ObjectToWorld._m02_m12_m22 * v.tangent.zzz + tmp3.xyz;
                tmp0.z = dot(tmp3.xyz, tmp3.xyz);
                tmp0.z = rsqrt(tmp0.z);
                tmp3.xyz = tmp0.zzz * tmp3.xyz;
                o.texcoord5.xyz = tmp3.xyz;
                tmp4.xyz = tmp2.zxy * tmp3.yzx;
                tmp2.xyz = tmp2.yzx * tmp3.zxy + -tmp4.xyz;
                tmp2.xyz = tmp2.xyz * v.tangent.www;
                tmp0.z = dot(tmp2.xyz, tmp2.xyz);
                tmp0.z = rsqrt(tmp0.z);
                o.texcoord6.xyz = tmp0.zzz * tmp2.xyz;
                tmp0.z = tmp1.y * unity_MatrixV._m21;
                tmp0.z = unity_MatrixV._m20 * tmp1.x + tmp0.z;
                tmp0.z = unity_MatrixV._m22 * tmp1.z + tmp0.z;
                tmp0.z = unity_MatrixV._m23 * tmp1.w + tmp0.z;
                o.texcoord7.z = -tmp0.z;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord7.w = tmp0.w;
                o.texcoord7.xy = tmp1.zz + tmp1.xw;
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
                float4 tmp6;
                float4 tmp7;
                float4 tmp8;
                float4 tmp9;
                float4 tmp10;
                float4 tmp11;
                float4 tmp12;
                tmp0.x = dot(inp.texcoord4.xyz, inp.texcoord4.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp0.xyz = tmp0.xxx * inp.texcoord4.xyz;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp1.xyz;
                tmp3.xy = _Time.yy * float2(0.006, 0.003);
                tmp3.xy = inp.texcoord.xy * _UV_Scale.xx + tmp3.xy;
                tmp3.xy = tmp3.xy * _Refraction_ST.xy + _Refraction_ST.zw;
                tmp3 = tex2D(_Refraction, tmp3.xy);
                tmp3.x = tmp3.w * tmp3.x;
                tmp4.xy = tmp3.xy + tmp3.xy;
                tmp3.xy = tmp3.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1.w = dot(tmp3.xy, tmp3.xy);
                tmp1.w = min(tmp1.w, 1.0);
                tmp1.w = 1.0 - tmp1.w;
                tmp4.z = sqrt(tmp1.w);
                tmp4.xyz = tmp4.xyz - float3(1.0, 1.0, 1.0);
                tmp4.xyz = _Distortion.xxx * tmp4.xyz + float3(0.0, 0.0, 1.0);
                tmp5.xyz = tmp4.yyy * inp.texcoord6.xyz;
                tmp4.xyw = tmp4.xxx * inp.texcoord5.xyz + tmp5.xyz;
                tmp0.xyz = tmp4.zzz * tmp0.xyz + tmp4.xyw;
                tmp1.w = dot(tmp0.xyz, tmp0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp0.xyz = tmp0.xyz * tmp1.www;
                tmp1.w = dot(-tmp2.xyz, tmp0.xyz);
                tmp1.w = tmp1.w + tmp1.w;
                tmp4.xyz = tmp0.xyz * -tmp1.www + -tmp2.xyz;
                tmp3.zw = inp.texcoord7.xy / inp.texcoord7.ww;
                tmp5 = tex2D(_CameraDepthTexture, tmp3.zw);
                tmp1.w = _ZBufferParams.z * tmp5.x + _ZBufferParams.w;
                tmp1.w = 1.0 / tmp1.w;
                tmp1.w = tmp1.w - _ProjectionParams.y;
                tmp1.w = max(tmp1.w, 0.0);
                tmp2.w = inp.texcoord7.z - _ProjectionParams.y;
                tmp2.w = max(tmp2.w, 0.0);
                tmp3.xy = tmp3.xy * _RefractionPower.xx + tmp3.zw;
                tmp3 = tex2D(_GrabTexture, tmp3.xy);
                tmp3.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp3.w = rsqrt(tmp3.w);
                tmp5.xyz = tmp3.www * _WorldSpaceLightPos0.xyz;
                tmp1.xyz = tmp1.xyz * tmp0.www + tmp5.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp6.xy = -float2(_Specular.x, _Roughness.x) + float2(1.0, 1.0);
                tmp0.w = _Roughness * _Roughness;
                tmp6.zw = float2(1.0, 1.0) - tmp6.xy;
                tmp3.w = unity_SpecCube0_ProbePosition.w > 0.0;
                if (tmp3.w) {
                    tmp3.w = dot(tmp4.xyz, tmp4.xyz);
                    tmp3.w = rsqrt(tmp3.w);
                    tmp7.xyz = tmp3.www * tmp4.xyz;
                    tmp8.xyz = unity_SpecCube0_BoxMax.xyz - inp.texcoord3.xyz;
                    tmp8.xyz = tmp8.xyz / tmp7.xyz;
                    tmp9.xyz = unity_SpecCube0_BoxMin.xyz - inp.texcoord3.xyz;
                    tmp9.xyz = tmp9.xyz / tmp7.xyz;
                    tmp10.xyz = tmp7.xyz > float3(0.0, 0.0, 0.0);
                    tmp8.xyz = tmp10.xyz ? tmp8.xyz : tmp9.xyz;
                    tmp3.w = min(tmp8.y, tmp8.x);
                    tmp3.w = min(tmp8.z, tmp3.w);
                    tmp8.xyz = inp.texcoord3.xyz - unity_SpecCube0_ProbePosition.xyz;
                    tmp7.xyz = tmp7.xyz * tmp3.www + tmp8.xyz;
                } else {
                    tmp7.xyz = tmp4.xyz;
                }
                tmp3.w = -tmp6.z * 0.7 + 1.7;
                tmp3.w = tmp3.w * tmp6.z;
                tmp3.w = tmp3.w * 6.0;
                tmp7 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp7.xyz, tmp3.w));
                tmp4.w = tmp7.w - 1.0;
                tmp4.w = unity_SpecCube0_HDR.w * tmp4.w + 1.0;
                tmp4.w = log(tmp4.w);
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.y;
                tmp4.w = exp(tmp4.w);
                tmp4.w = tmp4.w * unity_SpecCube0_HDR.x;
                tmp8.xyz = tmp7.xyz * tmp4.www;
                tmp5.w = unity_SpecCube0_BoxMin.w < 0.99999;
                if (tmp5.w) {
                    tmp5.w = unity_SpecCube1_ProbePosition.w > 0.0;
                    if (tmp5.w) {
                        tmp5.w = dot(tmp4.xyz, tmp4.xyz);
                        tmp5.w = rsqrt(tmp5.w);
                        tmp9.xyz = tmp4.xyz * tmp5.www;
                        tmp10.xyz = unity_SpecCube1_BoxMax.xyz - inp.texcoord3.xyz;
                        tmp10.xyz = tmp10.xyz / tmp9.xyz;
                        tmp11.xyz = unity_SpecCube1_BoxMin.xyz - inp.texcoord3.xyz;
                        tmp11.xyz = tmp11.xyz / tmp9.xyz;
                        tmp12.xyz = tmp9.xyz > float3(0.0, 0.0, 0.0);
                        tmp10.xyz = tmp12.xyz ? tmp10.xyz : tmp11.xyz;
                        tmp5.w = min(tmp10.y, tmp10.x);
                        tmp5.w = min(tmp10.z, tmp5.w);
                        tmp10.xyz = inp.texcoord3.xyz - unity_SpecCube1_ProbePosition.xyz;
                        tmp4.xyz = tmp9.xyz * tmp5.www + tmp10.xyz;
                    }
                    tmp9 = UNITY_SAMPLE_TEXCUBE_SAMPLER(unity_SpecCube0, unity_SpecCube0, float4(tmp4.xyz, tmp3.w));
                    tmp3.w = tmp9.w - 1.0;
                    tmp3.w = unity_SpecCube1_HDR.w * tmp3.w + 1.0;
                    tmp3.w = log(tmp3.w);
                    tmp3.w = tmp3.w * unity_SpecCube1_HDR.y;
                    tmp3.w = exp(tmp3.w);
                    tmp3.w = tmp3.w * unity_SpecCube1_HDR.x;
                    tmp4.xyz = tmp9.xyz * tmp3.www;
                    tmp7.xyz = tmp4.www * tmp7.xyz + -tmp4.xyz;
                    tmp8.xyz = unity_SpecCube0_BoxMin.www * tmp7.xyz + tmp4.xyz;
                }
                tmp3.w = dot(tmp0.xyz, tmp5.xyz);
                tmp3.w = max(tmp3.w, 0.0);
                tmp4.x = min(tmp3.w, 1.0);
                tmp4.y = saturate(dot(tmp5.xyz, tmp1.xyz));
                tmp4.zw = inp.texcoord.xy * _Color_Tex_ST.xy + _Color_Tex_ST.zw;
                tmp5 = tex2D(_Color_Tex, tmp4.zw);
                tmp1.w = tmp1.w - tmp2.w;
                tmp2.w = saturate(tmp1.w / _SurfaceColor.w);
                tmp7.xyz = _DepthColor.xyz - _SurfaceColor.xyz;
                tmp7.xyz = tmp2.www * tmp7.xyz + _SurfaceColor.xyz;
                tmp2.w = sin(_Time.y);
                tmp2.w = tmp2.w * 0.01 + _EdgeDistance;
                tmp2.w = tmp2.w + 0.01;
                tmp1.w = saturate(tmp1.w / tmp2.w);
                tmp1.w = 1.0 - tmp1.w;
                tmp9.xyz = tmp1.www * _FoamColor.xyz;
                tmp9.xyz = tmp9.xyz * _FoamColor.www;
                tmp5.xyz = tmp7.xyz * tmp5.xyz + tmp9.xyz;
                tmp5.xyz = tmp6.yyy * tmp5.xyz;
                tmp1.w = dot(tmp0.xyz, tmp2.xyz);
                tmp0.x = saturate(dot(tmp0.xyz, tmp1.xyz));
                tmp0.y = -_Roughness * _Roughness + 1.0;
                tmp0.z = abs(tmp1.w) * tmp0.y + tmp0.w;
                tmp0.y = tmp4.x * tmp0.y + tmp0.w;
                tmp0.y = tmp0.y * abs(tmp1.w);
                tmp0.y = tmp4.x * tmp0.z + tmp0.y;
                tmp0.y = tmp0.y + 0.00001;
                tmp0.y = 0.5 / tmp0.y;
                tmp0.z = tmp0.w * tmp0.w;
                tmp1.x = tmp0.x * tmp0.z + -tmp0.x;
                tmp0.x = tmp1.x * tmp0.x + 1.0;
                tmp0.z = tmp0.z * 0.3183099;
                tmp0.x = tmp0.x * tmp0.x + 0.0000001;
                tmp0.x = tmp0.z / tmp0.x;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.x = tmp0.x * 3.141593;
                tmp0.x = tmp4.x * tmp0.x;
                tmp0.x = max(tmp0.x, 0.0);
                tmp0.y = tmp0.w * tmp0.w + 1.0;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.z = dot(float3(_Distortion.x, _Specular.x, _Roughness.x), float3(_Distortion.x, _Specular.x, _Roughness.x));
                tmp0.z = tmp0.z != 0.0;
                tmp0.z = tmp0.z ? 1.0 : 0.0;
                tmp0.x = tmp0.z * tmp0.x;
                tmp0.xzw = tmp0.xxx * _LightColor0.xyz;
                tmp1.x = 1.0 - tmp4.y;
                tmp1.y = tmp1.x * tmp1.x;
                tmp1.y = tmp1.y * tmp1.y;
                tmp1.x = tmp1.x * tmp1.y;
                tmp1.x = tmp6.y * tmp1.x + _Specular;
                tmp1.y = saturate(tmp6.w + tmp6.x);
                tmp1.z = 1.0 - abs(tmp1.w);
                tmp1.w = tmp1.z * tmp1.z;
                tmp1.w = tmp1.w * tmp1.w;
                tmp1.z = tmp1.z * tmp1.w;
                tmp1.y = tmp1.y - _Specular;
                tmp1.y = tmp1.z * tmp1.y + _Specular;
                tmp2.xyz = tmp1.yyy * tmp8.xyz;
                tmp2.xyz = tmp0.yyy * tmp2.xyz;
                tmp0.xyz = tmp0.xzw * tmp1.xxx + tmp2.xyz;
                tmp0.w = tmp4.y + tmp4.y;
                tmp0.w = tmp4.y * tmp0.w;
                tmp1.x = 1.0 - tmp3.w;
                tmp1.y = tmp1.x * tmp1.x;
                tmp1.y = tmp1.y * tmp1.y;
                tmp1.x = tmp1.x * tmp1.y;
                tmp0.w = tmp0.w * tmp6.z + -0.5;
                tmp1.x = tmp0.w * tmp1.x + 1.0;
                tmp0.w = tmp0.w * tmp1.z + 1.0;
                tmp0.w = tmp0.w * tmp1.x;
                tmp0.w = tmp3.w * tmp0.w;
                tmp1.xyz = tmp0.www * _LightColor0.xyz;
                tmp0.w = 1.0 - tmp6.w;
                tmp2.xyz = tmp0.www * tmp5.xyz;
                tmp1.xyz = tmp1.xyz * tmp2.xyz;
                tmp0.xyz = tmp1.xyz * _Opacity.xxx + tmp0.xyz;
                tmp0.xyz = tmp0.xyz - tmp3.xyz;
                o.sv_target.xyz = _Opacity.xxx * tmp0.xyz + tmp3.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "META"
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "META" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Cull Off
			GpuProgramID 81372
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
			
			// Keywords: SHADOWS_DEPTH
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
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp0 = tmp0 + unity_MatrixVP._m03_m13_m23_m33;
                o.position = tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                o.texcoord3 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                tmp1 = tmp1 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp0.z = tmp1.y * unity_MatrixV._m21;
                tmp0.z = unity_MatrixV._m20 * tmp1.x + tmp0.z;
                tmp0.z = unity_MatrixV._m22 * tmp1.z + tmp0.z;
                tmp0.z = unity_MatrixV._m23 * tmp1.w + tmp0.z;
                o.texcoord4.z = -tmp0.z;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord4.w = tmp0.w;
                o.texcoord4.xy = tmp1.zz + tmp1.xw;
                return o;
			}
			// Keywords: SHADOWS_DEPTH
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
                tmp0.y = sin(_Time.y);
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
                o.sv_target = unity_MetaFragmentControl ? float4(0.0, 0.0, 0.0, 1.0) : tmp0;
                return o;
			}
			ENDCG
		}
	}
	Fallback "AG/Shadow Caster"
	CustomEditor "ShaderForgeMaterialInspector"
}