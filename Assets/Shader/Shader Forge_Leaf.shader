Shader "Shader Forge/Leaf" {
	Properties {
		_Color ("Color", Color) = (1,0.9724138,0,1)
		_Texture ("Texture", 2D) = "white" {}
		_OpacityPower ("OpacityPower", Float) = 2
		_EmitPower ("EmitPower", Float) = 0
		_DiffPower ("DiffPower", Float) = 0.8
		_Spec ("Spec", Float) = 0
		_Gloss ("Gloss", Float) = 0
		_DiffAmbLight ("DiffAmbLight", Float) = 0
		_Speed ("Speed", Float) = 1
		_Amplitude ("Amplitude", Float) = 0.05
		_WaveLength ("WaveLength", Float) = 1
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" }
		Pass {
			Name "FORWARD"
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "ForwardBase" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 19230
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
				float4 texcoord8 : TEXCOORD8;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _TimeEditor;
			float _Amplitude;
			float _Speed;
			float _WaveLength;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _Color;
			float4 _Texture_ST;
			float _OpacityPower;
			float _EmitPower;
			float _Spec;
			float _Gloss;
			float _DiffAmbLight;
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
                tmp0.x = v.vertex.y * unity_ObjectToWorld._m01;
                tmp0.x = unity_ObjectToWorld._m00 * v.vertex.x + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m02 * v.vertex.z + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m03 * v.vertex.w + tmp0.x;
                tmp0.y = _TimeEditor.y + _Time.y;
                tmp0.y = tmp0.y * _Speed;
                tmp0.x = tmp0.x * _WaveLength + tmp0.y;
                tmp0.x = sin(tmp0.x);
                tmp0.xyz = _Amplitude.xxx * tmp0.xxx + v.vertex.xyz;
                tmp1 = tmp0.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp1 = tmp0.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
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
                o.texcoord8 = float4(0.0, 0.0, 0.0, 0.0);
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
                tmp0.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp0.xyz;
                tmp2.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp2 = tex2D(_Texture, tmp2.xy);
                tmp3.xyz = ddx(inp.texcoord3.zxy);
                tmp1.w = dot(tmp3.xyz, tmp3.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp1.www * tmp3.xyz;
                tmp4.xyz = ddy(inp.texcoord3.yzx);
                tmp1.w = dot(tmp4.xyz, tmp4.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp4.xyz = tmp1.www * tmp4.xyz;
                tmp5.xyz = tmp3.xyz * tmp4.xyz;
                tmp3.xyz = tmp3.zxy * tmp4.yzx + -tmp5.xyz;
                tmp1.w = dot(tmp1.xyz, tmp3.xyz);
                tmp2.w = tmp2.w * _OpacityPower;
                tmp1.w = tmp2.w * abs(tmp1.w) + -0.5;
                tmp1.w = tmp1.w < 0.0;
                if (tmp1.w) {
                    discard;
                }
                tmp1.w = dot(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz);
                tmp1.w = rsqrt(tmp1.w);
                tmp3.xyz = tmp1.www * _WorldSpaceLightPos0.xyz;
                tmp0.xyz = tmp0.xyz * tmp0.www + tmp3.xyz;
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp0.w = 1.0 - _Gloss;
                tmp1.w = tmp0.w * 10.0 + 1.0;
                tmp1.w = exp(tmp1.w);
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
                tmp4.xyz = tmp5.xyz + inp.texcoord8.xyz;
                tmp4.xyz = max(tmp4.xyz, float3(0.0, 0.0, 0.0));
                tmp2.w = dot(inp.texcoord4.xyz, tmp3.xyz);
                tmp2.w = max(tmp2.w, 0.0);
                tmp3.x = dot(tmp3.xyz, tmp0.xyz);
                tmp3.x = max(tmp3.x, 0.0);
                tmp2.xyz = tmp2.xyz * _Color.xyz;
                tmp3.yzw = tmp2.xyz * _Spec.xxx;
                tmp4.w = max(tmp3.z, tmp3.y);
                tmp4.w = max(tmp3.w, tmp4.w);
                tmp1.x = dot(inp.texcoord4.xyz, tmp1.xyz);
                tmp1.x = max(tmp1.x, 0.0);
                tmp0.x = dot(inp.texcoord4.xyz, tmp0.xyz);
                tmp0.y = 1.0 - tmp0.w;
                tmp0.z = tmp0.y * 0.7978846;
                tmp0.w = -tmp0.y * 0.7978846 + 1.0;
                tmp1.y = tmp2.w * tmp0.w + tmp0.z;
                tmp0.z = tmp1.x * tmp0.w + tmp0.z;
                tmp0.z = tmp1.y * tmp0.z + 0.00001;
                tmp0.z = 1.0 / tmp0.z;
                tmp0.z = tmp0.z * 0.25;
                tmp0.w = tmp0.y * tmp0.y;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.xw = max(tmp0.xw, float2(0.0, 0.0001));
                tmp0.w = 2.0 / tmp0.w;
                tmp0.w = tmp0.w - 2.0;
                tmp0.w = max(tmp0.w, 0.0001);
                tmp1.y = tmp0.w + 2.0;
                tmp1.y = tmp1.y * 0.1591549;
                tmp0.x = log(tmp0.x);
                tmp0.w = tmp0.x * tmp0.w;
                tmp0.w = exp(tmp0.w);
                tmp0.w = tmp1.y * tmp0.w;
                tmp0.z = tmp0.z * tmp2.w;
                tmp0.z = tmp0.w * tmp0.z;
                tmp0.z = tmp0.z * 0.7853982;
                tmp0.z = max(tmp0.z, 0.0);
                tmp0.x = tmp0.x * tmp1.w;
                tmp0.x = exp(tmp0.x);
                tmp0.x = tmp0.z * tmp0.x;
                tmp0.xzw = tmp0.xxx * _LightColor0.xyz;
                tmp1.y = 1.0 - tmp3.x;
                tmp1.z = tmp1.y * tmp1.y;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.y = tmp1.y * tmp1.z;
                tmp5.xyz = -_Spec.xxx * tmp2.xyz + float3(1.0, 1.0, 1.0);
                tmp1.yzw = tmp5.xyz * tmp1.yyy + tmp3.yzw;
                tmp0.xzw = tmp0.xzw * tmp1.yzw;
                tmp1.y = tmp3.x + tmp3.x;
                tmp1.y = tmp3.x * tmp1.y;
                tmp0.y = tmp1.y * tmp0.y + -0.5;
                tmp1.y = 1.00001 - tmp2.w;
                tmp1.z = tmp1.y * tmp1.y;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.y = tmp1.z * tmp1.y;
                tmp1.y = tmp0.y * tmp1.y + 1.0;
                tmp1.x = 1.00001 - tmp1.x;
                tmp1.z = tmp1.x * tmp1.x;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.x = tmp1.z * tmp1.x;
                tmp0.y = tmp0.y * tmp1.x + 1.0;
                tmp0.y = tmp0.y * tmp1.y;
                tmp0.y = tmp2.w * tmp0.y;
                tmp1.xyz = tmp4.xyz + _DiffAmbLight.xxx;
                tmp1.w = 1.0 - tmp4.w;
                tmp3.xyz = tmp1.www * tmp2.xyz;
                tmp1.xyz = tmp0.yyy * _LightColor0.xyz + tmp1.xyz;
                tmp0.xyz = tmp1.xyz * tmp3.xyz + tmp0.xzw;
                o.sv_target.xyz = tmp2.xyz * _EmitPower.xxx + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "FORWARD_DELTA"
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "ForwardAdd" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" }
			Blend One One, One One
			ZClip Off
			Cull Off
			GpuProgramID 70366
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
			float4 _TimeEditor;
			float _Amplitude;
			float _Speed;
			float _WaveLength;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _LightColor0;
			float4 _Color;
			float4 _Texture_ST;
			float _OpacityPower;
			float _Spec;
			float _Gloss;
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
                tmp0.x = v.vertex.y * unity_ObjectToWorld._m01;
                tmp0.x = unity_ObjectToWorld._m00 * v.vertex.x + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m02 * v.vertex.z + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m03 * v.vertex.w + tmp0.x;
                tmp0.y = _TimeEditor.y + _Time.y;
                tmp0.y = tmp0.y * _Speed;
                tmp0.x = tmp0.x * _WaveLength + tmp0.y;
                tmp0.x = sin(tmp0.x);
                tmp0.xyz = _Amplitude.xxx * tmp0.xxx + v.vertex.xyz;
                tmp1 = tmp0.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp1 = tmp0.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
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
                float4 tmp5;
                tmp0.xyz = ddx(inp.texcoord3.zxy);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = ddy(inp.texcoord3.yzx);
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.xyz = tmp0.zxy * tmp1.yzx + -tmp2.xyz;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord3.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp2.xyz = tmp0.www * tmp1.xyz;
                tmp0.x = dot(tmp2.xyz, tmp0.xyz);
                tmp0.y = dot(inp.texcoord4.xyz, tmp2.xyz);
                tmp2.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp2 = tex2D(_Texture, tmp2.xy);
                tmp0.z = tmp2.w * _OpacityPower;
                tmp2.xyz = tmp2.xyz * _Color.xyz;
                tmp0.x = tmp0.z * abs(tmp0.x) + -0.5;
                tmp0.x = tmp0.x < 0.0;
                if (tmp0.x) {
                    discard;
                }
                tmp3.xyz = _WorldSpaceLightPos0.www * -inp.texcoord3.xyz + _WorldSpaceLightPos0.xyz;
                tmp0.x = dot(tmp3.xyz, tmp3.xyz);
                tmp0.x = rsqrt(tmp0.x);
                tmp3.xyz = tmp0.xxx * tmp3.xyz;
                tmp0.xzw = tmp1.xyz * tmp0.www + tmp3.xyz;
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.x = rsqrt(tmp1.x);
                tmp0.xzw = tmp0.xzw * tmp1.xxx;
                tmp1.x = dot(inp.texcoord4.xyz, tmp0.xyz);
                tmp0.x = dot(tmp3.xyz, tmp0.xyz);
                tmp0.z = dot(inp.texcoord4.xyz, tmp3.xyz);
                tmp0.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                tmp0.w = max(tmp1.x, 0.0);
                tmp0.w = log(tmp0.w);
                tmp1.x = 1.0 - _Gloss;
                tmp1.y = 1.0 - tmp1.x;
                tmp1.x = tmp1.x * 10.0 + 1.0;
                tmp1.x = exp(tmp1.x);
                tmp1.x = tmp0.w * tmp1.x;
                tmp1.x = exp(tmp1.x);
                tmp1.z = tmp1.y * tmp1.y;
                tmp1.z = tmp1.z * tmp1.z;
                tmp1.z = max(tmp1.z, 0.0001);
                tmp1.z = 2.0 / tmp1.z;
                tmp1.z = tmp1.z - 2.0;
                tmp1.z = max(tmp1.z, 0.0001);
                tmp0.w = tmp0.w * tmp1.z;
                tmp1.z = tmp1.z + 2.0;
                tmp1.z = tmp1.z * 0.1591549;
                tmp0.w = exp(tmp0.w);
                tmp0.w = tmp1.z * tmp0.w;
                tmp1.z = tmp1.y * 0.7978846;
                tmp1.w = -tmp1.y * 0.7978846 + 1.0;
                tmp2.w = tmp0.y * tmp1.w + tmp1.z;
                tmp1.z = tmp0.z * tmp1.w + tmp1.z;
                tmp1.z = tmp1.z * tmp2.w + 0.00001;
                tmp1.z = 1.0 / tmp1.z;
                tmp1.z = tmp1.z * 0.25;
                tmp1.z = tmp0.z * tmp1.z;
                tmp0.w = tmp0.w * tmp1.z;
                tmp0.w = tmp0.w * 0.7853982;
                tmp0.w = max(tmp0.w, 0.0);
                tmp1.z = dot(inp.texcoord7.xyz, inp.texcoord7.xyz);
                tmp3 = tex2D(_LightTexture0, tmp1.zz);
                tmp3.xyz = tmp3.xxx * _LightColor0.xyz;
                tmp1.xzw = tmp1.xxx * tmp3.xyz;
                tmp1.xzw = tmp0.www * tmp1.xzw;
                tmp1.xzw = tmp1.xzw * _LightColor0.xyz;
                tmp0.yw = float2(1.00001, 1.0) - tmp0.yx;
                tmp2.w = tmp0.w * tmp0.w;
                tmp2.w = tmp2.w * tmp2.w;
                tmp0.w = tmp0.w * tmp2.w;
                tmp4.xyz = -_Spec.xxx * tmp2.xyz + float3(1.0, 1.0, 1.0);
                tmp5.xyz = tmp2.xyz * _Spec.xxx;
                tmp4.xyz = tmp4.xyz * tmp0.www + tmp5.xyz;
                tmp1.xzw = tmp1.xzw * tmp4.xyz;
                tmp0.w = max(tmp5.y, tmp5.x);
                tmp0.w = max(tmp5.z, tmp0.w);
                tmp0.w = 1.0 - tmp0.w;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp0.w = tmp0.y * tmp0.y;
                tmp0.w = tmp0.w * tmp0.w;
                tmp0.y = tmp0.w * tmp0.y;
                tmp0.w = tmp0.x + tmp0.x;
                tmp0.x = tmp0.x * tmp0.w;
                tmp0.x = tmp0.x * tmp1.y + -0.5;
                tmp0.y = tmp0.x * tmp0.y + 1.0;
                tmp0.w = 1.00001 - tmp0.z;
                tmp1.y = tmp0.w * tmp0.w;
                tmp1.y = tmp1.y * tmp1.y;
                tmp0.w = tmp0.w * tmp1.y;
                tmp0.x = tmp0.x * tmp0.w + 1.0;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.x = tmp0.z * tmp0.x;
                tmp0.xyz = tmp3.xyz * tmp0.xxx;
                o.sv_target.xyz = tmp0.xyz * tmp2.xyz + tmp1.xzw;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			Name "SHADOWCASTER"
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "SHADOWCASTER" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Offset 1, 1
			GpuProgramID 139564
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
			float4 _TimeEditor;
			float _Amplitude;
			float _Speed;
			float _WaveLength;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Texture_ST;
			float _OpacityPower;
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
                tmp0.x = v.vertex.y * unity_ObjectToWorld._m01;
                tmp0.x = unity_ObjectToWorld._m00 * v.vertex.x + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m02 * v.vertex.z + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m03 * v.vertex.w + tmp0.x;
                tmp0.y = _TimeEditor.y + _Time.y;
                tmp0.y = tmp0.y * _Speed;
                tmp0.x = tmp0.x * _WaveLength + tmp0.y;
                tmp0.x = sin(tmp0.x);
                tmp0.xyz = _Amplitude.xxx * tmp0.xxx + v.vertex.xyz;
                tmp1 = tmp0.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp1 = tmp1 + glstate_matrix_mvp._m03_m13_m23_m33;
                tmp0.w = unity_LightShadowBias.x / tmp1.w;
                tmp0.w = min(tmp0.w, 0.0);
                tmp0.w = max(tmp0.w, -1.0);
                tmp0.w = tmp0.w + tmp1.z;
                tmp1.z = min(tmp1.w, tmp0.w);
                o.position.xyw = tmp1.xyw;
                tmp1.x = tmp1.z - tmp0.w;
                o.position.z = unity_LightShadowBias.y * tmp1.x + tmp0.w;
                o.texcoord1.xy = v.texcoord.xy;
                o.texcoord2.xy = v.texcoord1.xy;
                o.texcoord3.xy = v.texcoord2.xy;
                tmp1 = tmp0.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.texcoord4 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                return o;
			}
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			fout frag(v2f inp, float facing: VFACE)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xyz = ddx(inp.texcoord4.zxy);
                tmp0.w = dot(tmp0.xyz, tmp0.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                tmp1.xyz = ddy(inp.texcoord4.yzx);
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp2.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.xyz = tmp0.zxy * tmp1.yzx + -tmp2.xyz;
                tmp1.xyz = _WorldSpaceCameraPos - inp.texcoord4.xyz;
                tmp0.w = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.w);
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.x = dot(tmp1.xyz, tmp0.xyz);
                tmp0.yz = inp.texcoord1.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp1 = tex2D(_Texture, tmp0.yz);
                tmp0.y = tmp1.w * _OpacityPower;
                tmp0.x = tmp0.y * abs(tmp0.x) + -0.5;
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
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "Meta" "QUEUE" = "AlphaTest+50" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			ZClip Off
			Cull Off
			GpuProgramID 255441
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
			float4 _TimeEditor;
			float _Amplitude;
			float _Speed;
			float _WaveLength;
			// $Globals ConstantBuffers for Fragment Shader
			float unity_OneOverOutputBoost;
			float unity_MaxOutputValue;
			float unity_UseLinearSpace;
			float4 _Color;
			float4 _Texture_ST;
			float _EmitPower;
			float _Spec;
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
			sampler2D _Texture;
			
			// Keywords: SHADOWS_DEPTH DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF DIRLIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = v.vertex.y * unity_ObjectToWorld._m01;
                tmp0.x = unity_ObjectToWorld._m00 * v.vertex.x + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m02 * v.vertex.z + tmp0.x;
                tmp0.x = unity_ObjectToWorld._m03 * v.vertex.w + tmp0.x;
                tmp0.y = _TimeEditor.y + _Time.y;
                tmp0.y = tmp0.y * _Speed;
                tmp0.x = tmp0.x * _WaveLength + tmp0.y;
                tmp0.x = sin(tmp0.x);
                tmp0.xyz = _Amplitude.xxx * tmp0.xxx + v.vertex.xyz;
                tmp0.w = tmp0.z > 0.0;
                tmp1.z = tmp0.w ? 0.0001 : 0.0;
                tmp1.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                tmp1.xyz = unity_MetaVertexControl.xxx ? tmp1.xyz : tmp0.xyz;
                tmp0.w = tmp1.z > 0.0;
                tmp2.z = tmp0.w ? 0.0001 : 0.0;
                tmp2.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                tmp1.xyz = unity_MetaVertexControl.yyy ? tmp2.xyz : tmp1.xyz;
                tmp2 = tmp1.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp2 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp1.xxxx + tmp2;
                tmp1 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp1.zzzz + tmp2;
                o.position = tmp1 + glstate_matrix_mvp._m03_m13_m23_m33;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.texcoord2.xy = v.texcoord2.xy;
                tmp1 = tmp0.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
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
                tmp0.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.xyz = tmp0.xyz * _Color.xyz;
                tmp1.xyz = tmp0.xyz * _Spec.xxx;
                tmp0.w = max(tmp1.y, tmp1.x);
                tmp0.w = max(tmp1.z, tmp0.w);
                tmp0.w = 1.0 - tmp0.w;
                tmp1.w = _Gloss * _Gloss;
                tmp1.xyz = tmp1.xyz * tmp1.www;
                tmp1.xyz = tmp1.xyz * float3(0.5, 0.5, 0.5);
                tmp1.xyz = tmp0.xyz * tmp0.www + tmp1.xyz;
                tmp0.xyz = tmp0.xyz * _EmitPower.xxx;
                tmp1.xyz = log(tmp1.xyz);
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
	Fallback "Standard"
	CustomEditor "ShaderForgeMaterialInspector"
}