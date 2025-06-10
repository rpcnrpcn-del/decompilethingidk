Shader "Shader Forge/Props/Lens" {
	Properties {
		_Color ("Color", Vector) = (0,0,0,1)
		_Gloss ("Gloss", Range(0, 1)) = 1
		_Specular ("Specular", 2D) = "white" {}
		_Normal ("Normal", 2D) = "bump" {}
		_Fresnel ("Fresnel", Float) = 3.2
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
	Fallback "Standard (Specular setup)"
	//CustomEditor "ShaderForgeMaterialInspector"
}