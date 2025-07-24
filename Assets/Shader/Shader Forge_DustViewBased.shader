Shader "Shader Forge/DustViewBased" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Texture ("Texture", 2D) = "white" {}
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "ForwardBase" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			GpuProgramID 63079
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 color : COLOR0;
				float4 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
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
                tmp0 = v.vertex.yyyy * UNITY_MATRIX_MVP._m01_m11_m21_m31;
                tmp0 = UNITY_MATRIX_MVP._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = UNITY_MATRIX_MVP._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = UNITY_MATRIX_MVP._m03_m13_m23_m33 * v.vertex.wwww + tmp0;
                o.position = tmp0;
                o.texcoord.xy = v.texcoord.xy;
                tmp1 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp1 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                o.texcoord1 = unity_ObjectToWorld._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                o.color = v.color;
                tmp0.y = tmp0.y * _ProjectionParams.x;
                tmp1.xzw = tmp0.xwy * float3(0.5, 0.5, 0.5);
                o.texcoord2.w = tmp0.w;
                o.texcoord2.xy = tmp1.zz + tmp1.xw;
                tmp0.x = v.vertex.y * glstate_matrix_modelview0._m21;
                tmp0.x = glstate_matrix_modelview0._m20 * v.vertex.x + tmp0.x;
                tmp0.x = glstate_matrix_modelview0._m22 * v.vertex.z + tmp0.x;
                tmp0.x = tmp0.x + glstate_matrix_modelview0._m23;
                o.texcoord2.z = -tmp0.x;
                return o;
			}
			// Keywords: DIRECTIONAL
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0.xy = inp.texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
                tmp0 = tex2D(_Texture, tmp0.xy);
                tmp0.xyz = tmp0.xyz * inp.color.www;
                tmp0.yzw = tmp0.xyz * _Color.xyz;
                o.sv_target.xyz = tmp0.yzw * _Color.www;
                tmp0.y = saturate(inp.texcoord2.z - _ProjectionParams.y);
                o.sv_target.w = tmp0.y * tmp0.x;
                return o;
			}
			ENDCG
		}
	}
	CustomEditor "ShaderForgeMaterialInspector"
}