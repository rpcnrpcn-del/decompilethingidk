Shader "Custom/Vive Movement Border" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Alpha ("Total Alpha (Transparency)", Float) = 1
		_FadeOutStartTexCoord ("Fade Out Start Texture Coordinate", Float) = 1
		_FadeOutEndTexCoord ("Fade Out End Texture Coordinate", Float) = 1
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
		Pass {
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZClip Off
			ZWrite Off
			GpuProgramID 44666
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Color;
			float _Alpha;
			float _FadeOutStartTexCoord;
			float _FadeOutEndTexCoord;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = v.texcoord1.xy;
                tmp0 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp0 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                o.position = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0.x = inp.texcoord.y > 0.0;
                tmp0.y = inp.texcoord.y < 0.0;
                tmp0.x = tmp0.y - tmp0.x;
                tmp0.x = floor(tmp0.x);
                tmp0.x = tmp0.x * 0.5;
                tmp0.y = inp.texcoord.y >= -inp.texcoord.y;
                tmp0.z = frac(abs(inp.texcoord.y));
                tmp0.y = tmp0.y ? tmp0.z : -tmp0.z;
                tmp0.x = tmp0.x >= tmp0.y;
                tmp0.x = tmp0.x ? 1.0 : 0.0;
                tmp0.y = _Color.w * _Alpha;
                tmp0.x = tmp0.x * tmp0.y;
                tmp0.y = _FadeOutEndTexCoord - _FadeOutStartTexCoord;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.z = inp.texcoord1.y - _FadeOutStartTexCoord;
                tmp0.y = saturate(tmp0.y * tmp0.z);
                tmp0.z = tmp0.y * -2.0 + 3.0;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.y = -tmp0.z * tmp0.y + 1.0;
                o.sv_target.w = tmp0.y * tmp0.x;
                o.sv_target.xyz = _Color.xyz;
                return o;
			}
			ENDCG
		}
	}
}