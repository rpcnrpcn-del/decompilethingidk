Shader "Custom/LCDShader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_EmissionStrength ("Emission Strength", Range(0, 1)) = 0.5
		_PixelEffect ("PixelEffect", Range(0, 1)) = 0.18
		_ScanlineEffect ("ScanlineAlpha", Range(0, 1)) = 0.326
		_ScanSpeed ("Scanline Speed", Range(0, 100)) = 5.9
		_NumPixelsX ("LCD X Resolution", Float) = 128
		_NumPixelsY ("LCD Y Resolution", Float) = 9.5
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
	Fallback "Diffuse"
}