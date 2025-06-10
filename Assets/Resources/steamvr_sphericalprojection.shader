Shader "Custom/SteamVR_SphericalProjection" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_N ("N (normal of plane)", Vector) = (0,0,0,0)
		_Phi0 ("Phi0", Float) = 0
		_Phi1 ("Phi1", Float) = 1
		_Theta0 ("Theta0", Float) = 0
		_Theta1 ("Theta1", Float) = 1
		_UAxis ("uAxis", Vector) = (0,0,0,0)
		_VAxis ("vAxis", Vector) = (0,0,0,0)
		_UOrigin ("uOrigin", Vector) = (0,0,0,0)
		_VOrigin ("vOrigin", Vector) = (0,0,0,0)
		_UScale ("uScale", Float) = 1
		_VScale ("vScale", Float) = 1
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
}