Shader "Hidden/ProBuilder/VertexPicker" {
	Properties {
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "ProBuilderPicker" = "VertexPass" "RenderType" = "Opaque" }
		Pass {
			Name "VERTICES"
			Tags { "IGNOREPROJECTOR" = "true" "ProBuilderPicker" = "VertexPass" "RenderType" = "Opaque" }
			ZClip Off
			Cull Off
			Offset 10, -1
			GpuProgramID 52208
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
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
                tmp0 = v.vertex.yyyy * glstate_matrix_modelview0._m01_m11_m21_m31;
                tmp0 = glstate_matrix_modelview0._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = glstate_matrix_modelview0._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = glstate_matrix_modelview0._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                tmp0.xyz = tmp0.xyz * float3(0.99, 0.99, 0.99);
                tmp1 = tmp0.yyyy * glstate_matrix_projection._m01_m11_m21_m31;
                tmp1 = glstate_matrix_projection._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = glstate_matrix_projection._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                tmp0 = glstate_matrix_projection._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.xy = tmp0.xy / tmp0.ww;
                tmp0.xy = tmp0.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                tmp1.xy = v.texcoord1.xy * float2(3.5, 3.5);
                tmp0.xy = tmp0.xy * _ScreenParams.xy + tmp1.xy;
                tmp0.xy = tmp0.xy / _ScreenParams.xy;
                tmp0.xy = tmp0.xy - float2(0.5, 0.5);
                tmp0.xy = tmp0.ww * tmp0.xy;
                o.position.xy = tmp0.xy + tmp0.xy;
                tmp0.x = 1.0 - glstate_matrix_projection._m33;
                o.position.z = -tmp0.x * 0.01 + tmp0.z;
                o.position.w = tmp0.w;
                o.texcoord.xy = v.texcoord.xy;
                o.color = v.color;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = inp.color;
                return o;
			}
			ENDCG
		}
	}
}