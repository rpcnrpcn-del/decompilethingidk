Shader "Shader Forge/Avatar/Avatar_RGB_Normal" {
	Properties {
		_MainTex ("Mask_Tex", 2D) = "gray" {}
		_Base_Col ("Base_Col", Vector) = (0,0,0,1)
		_Red_Col ("Red_Col", Vector) = (1,0,0,1)
		_Green_Col ("Green_Col", Vector) = (0,1,0,1)
		_Blue_Col ("Blue_Col", Vector) = (0,0,1,1)
		_Dirt_Col ("Dirt_Col", Vector) = (1,0.9724138,0,1)
		[MaterialToggle] _Dirt_Add ("Dirt_Add", Float) = 0
		_Decal_Tex ("Decal_Tex", 2D) = "white" {}
		_Decal_Col ("Decal_Col", Vector) = (0,1,1,1)
		_Spec_Tex ("Spec_Tex", 2D) = "white" {}
		_Gloss ("Gloss", Range(0, 1)) = 0.8
		_Norm_Tex ("Norm_Tex", 2D) = "bump" {}
		_Norm_Mult ("Norm_Mult", Float) = 1
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

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, float2(input.uv.x, input.uv.y));
			}

			ENDHLSL
		}
	}
	Fallback "Standard (No Culling)"
	//CustomEditor "ShaderForgeMaterialInspector"
}