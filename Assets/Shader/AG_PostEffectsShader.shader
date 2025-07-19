Shader "AG/PostEffectsShader" {
	Properties {
	}
	SubShader {
		Tags { "QUEUE" = "Overlay+100" }
		Pass {
			Tags { "QUEUE" = "Overlay+100" }
			Blend DstColor Zero, DstColor Zero
			ZTest Always
			ZWrite Off
			GpuProgramID 15695
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float _Fade;
			float4 _GradientVector;
			float _VignetteRadius;
			float _VignetteSoftness;
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
                tmp0.xy = inp.texcoord.xy - float2(0.5, 0.5);
                tmp0.x = dot(tmp0.xy, tmp0.xy);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = min(tmp0.x, 1.0);
                tmp0.y = _VignetteRadius - _VignetteSoftness;
                tmp0.x = tmp0.x - tmp0.y;
                tmp0.y = _VignetteRadius - tmp0.y;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.x = saturate(tmp0.y * tmp0.x);
                tmp0.y = tmp0.x * -2.0 + 3.0;
                tmp0.x = tmp0.x * tmp0.x;
                tmp0.x = -tmp0.y * tmp0.x + 1.0;
                tmp0.y = inp.texcoord.x - _GradientVector.x;
                tmp0.zw = _GradientVector.yw - _GradientVector.xz;
                tmp0.z = 1.0 / tmp0.z;
                tmp0.y = saturate(tmp0.z * tmp0.y);
                tmp0.z = tmp0.y * -2.0 + 3.0;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.y = tmp0.y * tmp0.w + _GradientVector.z;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.x = tmp0.x * _Fade;
                o.sv_target = saturate(tmp0.xxxx);
                return o;
			}
			ENDCG
		}
	}
}