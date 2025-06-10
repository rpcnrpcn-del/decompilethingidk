Shader "Shader Forge/Avatar/Avatar_Hair" {
	Properties {
		_Col_Tex ("Col_Tex", 2D) = "white" {}
		_Color ("Color", Vector) = (0.5019608,0.5019608,0.5019608,1)
		_Dirt_Mult ("Dirt_Mult", Float) = 0.27
		_Dirt_Exp ("Dirt_Exp", Float) = 1.59
		_Spec_Tex ("Spec_Tex", 2D) = "white" {}
		_Gloss ("Gloss", Range(0, 1)) = 0.561
		_BumpMap ("Normal_Tex", 2D) = "bump" {}
		_Normal_Mult ("Normal_Mult", Float) = 0.38
		_BumpMap2 ("Normal_Detail", 2D) = "bump" {}
		_Normal_Detail_Mult ("Normal_Detail_Mult", Float) = 0.72
		_Outline ("Outline", Float) = 0.012
		_Outline_Col ("Outline_Col", Vector) = (0.4392157,0.9254903,0.3019608,1)
		_Fresnel ("Fresnel", Float) = 1.75
		_Highlight ("Highlight", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_MatrixMVP;

			struct Vertex_Stage_Input
			{
				float3 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixMVP, float4(input.pos, 1.0));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}