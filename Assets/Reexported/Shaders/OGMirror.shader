Shader "AG/Mirror (old)" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[HideInInspector] _ReflectionTex ("", 2D) = "white" {}
		[HideInInspector] _CameraPosition ("", Vector) = (0,0,0,0)
	}
	SubShader {
		LOD 100
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "RenderType" = "Opaque" }
			GpuProgramID 57643
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 position : SV_POSITION0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_ST;
			float4 _CameraPosition;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _ReflectionTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp0.x = _CameraPosition.y * unity_MatrixV._m01;
                tmp0.x = unity_MatrixV._m00 * _CameraPosition.x + tmp0.x;
                tmp0.x = unity_MatrixV._m02 * _CameraPosition.z + tmp0.x;
                tmp0.x = tmp0.x + unity_MatrixV._m03;
                tmp0.y = tmp0.x > 0.001;
                tmp0.x = tmp0.x < -0.001;
                tmp1 = v.vertex.yyyy * glstate_matrix_mvp._m01_m11_m21_m31;
                tmp1 = glstate_matrix_mvp._m00_m10_m20_m30 * v.vertex.xxxx + tmp1;
                tmp1 = glstate_matrix_mvp._m02_m12_m22_m32 * v.vertex.zzzz + tmp1;
                tmp1 = glstate_matrix_mvp._m03_m13_m23_m33 * v.vertex.wwww + tmp1;
                tmp0.z = tmp1.y * _ProjectionParams.x;
                tmp2.w = tmp0.z * 0.5;
                tmp2.xz = tmp1.xw * float2(0.5, 0.5);
                tmp0.zw = tmp2.zz + tmp2.xw;
                tmp2.x = tmp0.z * 0.5 + tmp2.z;
                tmp0.x = tmp0.x ? tmp2.x : tmp0.z;
                tmp0.z = tmp0.z * 0.5;
                o.texcoord1.y = tmp0.w;
                o.texcoord1.x = tmp0.y ? tmp0.z : tmp0.x;
                o.texcoord1.zw = tmp1.zw;
                o.position = tmp1;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = inp.texcoord1.xy / inp.texcoord1.ww;
                tmp0 = tex2D(_ReflectionTex, tmp0.xy);
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                o.sv_target = tmp0 * tmp1;
                return o;
			}
			ENDCG
		}
	}
}