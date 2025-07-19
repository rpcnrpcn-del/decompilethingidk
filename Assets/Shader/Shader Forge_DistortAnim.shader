Shader "Shader Forge/DistortAnim"
{
    Properties
    {
        _MainTex ("MainTex (RGB)", 2D) = "white" {}
        _NormalMap ("NormalMap", 2D) = "bump" {}
        _DistortionStrength ("Distortion Strength", Range(0,1)) = 0.1
        _UVSpeed ("UV Speed", Vector) = (0.1, 0, 0, 0)
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300

        GrabPass { }

        Pass
        {
            Name "DISTORT"
            Tags { "LightMode"="Always" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            float _DistortionStrength;
            float4 _UVSpeed;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uvMain : TEXCOORD1;
                float2 uvNormal : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // MainTex UV
                o.uvMain = TRANSFORM_TEX(v.texcoord, _MainTex);

                // NormalMap UV with scrolling
                float2 scrollUV = v.texcoord + _Time.y * _UVSpeed.xy;
                o.uvNormal = TRANSFORM_TEX(scrollUV, _NormalMap);

                // Grab texture position
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Distortion from normal map
                fixed3 normalSample = UnpackNormal(tex2D(_NormalMap, i.uvNormal));
                float2 distortion = normalSample.rg * _DistortionStrength;

                // Grab screen with distortion offset
                float2 grabUV = (i.grabPos.xy / i.grabPos.w) + distortion;
                fixed4 grabCol = tex2D(_GrabTexture, grabUV);

                // Mask & alpha from main texture
                fixed4 mask = tex2D(_MainTex, i.uvMain);

                // Final color
                return grabCol * _Color * mask.a;
            }
            ENDCG
        }
    }

    FallBack Off
}
