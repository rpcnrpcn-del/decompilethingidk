Shader "Hidden/TerrainEngine/Splatmap/Lightmap-AddPass-Advanced-Lite Bumped" {
	Properties {
		_Depth ("Blend Depth", Range(0.001, 1)) = 0.1
		_Control ("Control (RGBA)", 2D) = "black" {}
		_Splat3 ("Layer 3 (A)", 2D) = "black" {}
		_Splat2 ("Layer 2 (B)", 2D) = "black" {}
		_Splat1 ("Layer 1 (G)", 2D) = "black" {}
		_Splat0 ("Layer 0 (R)", 2D) = "black" {}
		_Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		_Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		_Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		_Normal0 ("Normal 0 (R)", 2D) = "bump" {}
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
}