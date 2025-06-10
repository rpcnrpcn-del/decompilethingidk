Shader "Shader Forge/Avatar/Avatar_RGB" {
	Properties {
		_Mask_Tex ("Mask_Tex", 2D) = "gray" {}
		_Base_Col ("Base_Col", Vector) = (0,0,0,1)
		_Red_Col ("Red_Col", Vector) = (1,0,0,1)
		_Green_Col ("Green_Col", Vector) = (0,1,0,1)
		_Blue_Col ("Blue_Col", Vector) = (0,0,1,1)
		_Dirt_Col ("Dirt_Col", Vector) = (0,0,0,1)
		[MaterialToggle] _Dirt_Add ("Dirt_Add", Float) = 0
		_Decal_Tex ("Decal_Tex", 2D) = "white" {}
		_Decal_Col ("Decal_Col", Vector) = (0,1,1,1)
		_Spec ("Spec", Range(0, 1)) = 0
		_Gloss ("Gloss", Range(0, 1)) = 0.8
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
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

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Standard (No Culling)"
	//CustomEditor "ShaderForgeMaterialInspector"
}