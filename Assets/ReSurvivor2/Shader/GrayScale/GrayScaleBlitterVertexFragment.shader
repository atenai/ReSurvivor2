Shader "Hidden/GrayScaleBlitterVertexFragment"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		Cull Off
		ZWrite Off

		Pass
		{
			Name "ColorBlitPass"

			HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			#pragma vertex vert
			#pragma fragment frag

			// 0〜1: 1=フルカラー, 0=完全グレー
			float HP = 1.0f;

			struct GrayscaleAttributes
			{
				uint vertexID : SV_VertexID;
			};

			struct GrayscaleVaryings
			{
				float2 uv : TEXCOORD0;
				float4 positionHCS : SV_POSITION;
			};

			GrayscaleVaryings vert(GrayscaleAttributes IN)
			{
				GrayscaleVaryings OUT;
				OUT.positionHCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
				OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
				return OUT;
			}

			half4 frag(GrayscaleVaryings IN) : SV_Target
			{
				float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.uv);

				half gray = dot((half3)col.rgb, half3(0.299h, 0.587h, 0.114h));
				half t = saturate(1.0h - (half)HP);

				half3 outRgb = lerp((half3)col.rgb, half3(gray, gray, gray), t);
				return half4(outRgb, (half)col.a);
			}

			ENDHLSL
		}
	}
}
