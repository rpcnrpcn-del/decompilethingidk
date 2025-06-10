Shader "Shader Forge/DistortAnim" {
	Properties {
		_Color_Tex ("Color_Tex", 2D) = "white" {}
		_FoamColor ("FoamColor", Vector) = (1,1,1,1)
		_SurfaceColor ("SurfaceColor", Vector) = (0.503,0.6647686,1,1)
		_DepthColor ("DepthColor", Vector) = (0,0.3254902,1,1)
		_EdgeDistance ("EdgeDistance", Float) = 0
		_Distortion ("Distortion", Float) = 1
		_Refraction ("Refraction", 2D) = "bump" {}
		_UV_Scale ("UV_Scale", Float) = 10
		_RefractionPower ("RefractionPower", Float) = 1
		_Specular ("Specular", Float) = 0
		_Roughness ("Roughness", Float) = 0.08
		_Opacity ("Opacity", Float) = 1
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
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
	Fallback "FX/Glass/Stained BumpDistort"
	//CustomEditor "ShaderForgeMaterialInspector"
}