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
				float4 pos = GetFullScreenTriangleVertexPosition(IN.vertexID);
				float2 uv = GetFullScreenTriangleTexCoord(IN.vertexID);
				
				OUT.positionHCS = pos;
				OUT.uv = uv;
				return OUT;
			}

			TEXTURE2D(_CameraOpaqueTexture);
			SAMPLER(sampler_CameraOpaqueTexture);

			half4 frag(GrayscaleVaryings IN) : SV_Target
			{
				float4 col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, IN.uv);
				half gray = dot(col.rgb, half3(0.299, 0.587, 0.114));
				return half4(gray.rrr, col.a);
			}
			ENDHLSL
		}
	}
}