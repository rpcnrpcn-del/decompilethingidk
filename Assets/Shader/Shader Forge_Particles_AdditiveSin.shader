Shader "Shader Forge/Particles/AdditiveSin" {
	Properties {
		_Texture ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0.5,0.5,0.5,1)
		_Wavelength ("Wavelength", Float) = 1
		_Speed ("Speed", Float) = 1
		_Amplitude ("Amplitude", Float) = 1
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "ForwardBase" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZClip Off
			ZWrite Off
			GpuProgramID 28207
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 color : COLOR0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _TimeEditor;
			float _Wavelength;
			float _Speed;
			float _Amplitude;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Texture_ST;
			float4 _Color;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Texture;
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0.x = _TimeEditor.y + _Time.y;
                tmp0.x = tmp0.x * _Speed;
                tmp0.x = v.texcoord1.y * _Wavelength + tmp0.x;
                tmp0.x = sin(tmp0.x);
                tmp0.x = tmp0.x * _Amplitude;
                tmp0.x = tmp0.x * v.color.x;
                tmp0.xyz = unity_WorldToObject._m01_m11_m21 * tmp0.xxx + v.vertex.xyz;
                tmp1 = tmp0.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                o.color = v.color;
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.xyz = tmp0.xyz * _Color.xyz;
                o.sv_target.xyz = tmp0.xyz + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
	CustomEditor "ShaderForgeMaterialInspector"
}