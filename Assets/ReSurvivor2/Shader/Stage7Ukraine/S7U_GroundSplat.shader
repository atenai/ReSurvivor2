// Stage7 (Ukrainian trench field) ground: soil base + three layers picked by the mesh vertex colour
// (R = forest litter, G = mud, B = dry grass). Layers are blended by height so the borders look natural.
// The trench floor (world y close to 0) is made wet: darker and glossier.
Shader "ReSurvivor2/Stage7Ukraine/GroundSplat"
{
    Properties
    {
        _SoilTex("Soil (base)", 2D) = "white" {}
        _SoilNormal("Soil normal", 2D) = "bump" {}
        _LitterTex("Forest litter (R)", 2D) = "white" {}
        _LitterNormal("Forest litter normal", 2D) = "bump" {}
        _MudTex("Mud (G)", 2D) = "white" {}
        _MudNormal("Mud normal", 2D) = "bump" {}
        _GrassTex("Dry grass (B)", 2D) = "white" {}
        _GrassNormal("Dry grass normal", 2D) = "bump" {}
        _SoilTint("Soil tint", Color) = (0.8, 0.74, 0.68, 1)
        _LitterTint("Litter tint", Color) = (0.72, 0.62, 0.5, 1)
        _MudTint("Mud tint", Color) = (0.34, 0.28, 0.24, 1)
        _GrassTint("Grass tint", Color) = (0.95, 0.78, 0.45, 1)
        _LayerTiling("Tiling soil / litter / mud / grass", Vector) = (1, 1, 1, 1)
        _LayerSmoothness("Smoothness soil / litter / mud / grass", Vector) = (0.08, 0.05, 0.3, 0.05)
        _NormalStrength("Normal strength", Range(0, 2)) = 1
        _HeightBlend("Height blend amount", Range(0, 1)) = 0.6
        _BlendSharpness("Blend sharpness", Range(0.01, 1)) = 0.2
        _WetHeight("Wet below this world height", Float) = 0.25
        _WetDarken("Wet darken", Range(0, 1)) = 0.55
        _WetSmoothness("Wet smoothness", Range(0, 1)) = 0.62
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _SoilTex_ST;
        half4 _SoilTint;
        half4 _LitterTint;
        half4 _MudTint;
        half4 _GrassTint;
        float4 _LayerTiling;
        half4 _LayerSmoothness;
        half _NormalStrength;
        half _HeightBlend;
        half _BlendSharpness;
        float _WetHeight;
        half _WetDarken;
        half _WetSmoothness;
    CBUFFER_END
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_SoilTex);      SAMPLER(sampler_SoilTex);
            TEXTURE2D(_SoilNormal);   SAMPLER(sampler_SoilNormal);
            TEXTURE2D(_LitterTex);
            TEXTURE2D(_LitterNormal);
            TEXTURE2D(_MudTex);
            TEXTURE2D(_MudNormal);
            TEXTURE2D(_GrassTex);
            TEXTURE2D(_GrassNormal);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 tangentWS : TEXCOORD3;
                half4 color : TEXCOORD4;
                half fogFactor : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = nrm.normalWS;
                real sign = input.tangentOS.w * GetOddNegativeScale();
                output.tangentWS = half4(nrm.tangentWS, sign);
                output.uv = TRANSFORM_TEX(input.uv, _SoilTex);
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            half Luma(half3 c)
            {
                return dot(c, half3(0.299h, 0.587h, 0.114h));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uvS = input.uv * _LayerTiling.x;
                float2 uvL = input.uv * _LayerTiling.y;
                float2 uvM = input.uv * _LayerTiling.z;
                float2 uvG = input.uv * _LayerTiling.w;

                half4 cS = SAMPLE_TEXTURE2D(_SoilTex, sampler_SoilTex, uvS);
                half4 cL = SAMPLE_TEXTURE2D(_LitterTex, sampler_SoilTex, uvL);
                half4 cM = SAMPLE_TEXTURE2D(_MudTex, sampler_SoilTex, uvM);
                half4 cG = SAMPLE_TEXTURE2D(_GrassTex, sampler_SoilTex, uvG);

                half3 m = saturate(input.color.rgb);
                half4 w = half4(saturate(1.0h - m.r - m.g - m.b), m.r, m.g, m.b);
                // heights: soil and grass carry a height map in alpha, the others use their luminance
                half4 h = half4(cS.a, Luma(cL.rgb) * 1.4h, Luma(cM.rgb) * 1.2h, cG.a);
                half4 t = w + h * _HeightBlend * step(0.001h, w);
                half mx = max(max(t.x, t.y), max(t.z, t.w)) - _BlendSharpness;
                half4 b = max(t - mx, 0.0h);
                b /= max(b.x + b.y + b.z + b.w, 1e-4h);

                half3 albedo = cS.rgb * _SoilTint.rgb * b.x + cL.rgb * _LitterTint.rgb * b.y
                             + cM.rgb * _MudTint.rgb * b.z + cG.rgb * _GrassTint.rgb * b.w;
                half smooth = dot(_LayerSmoothness, b);

                half3 nS = UnpackNormal(SAMPLE_TEXTURE2D(_SoilNormal, sampler_SoilNormal, uvS));
                half3 nL = UnpackNormal(SAMPLE_TEXTURE2D(_LitterNormal, sampler_SoilNormal, uvL));
                half3 nM = UnpackNormal(SAMPLE_TEXTURE2D(_MudNormal, sampler_SoilNormal, uvM));
                half3 nG = UnpackNormal(SAMPLE_TEXTURE2D(_GrassNormal, sampler_SoilNormal, uvG));
                half3 nTS = nS * b.x + nL * b.y + nM * b.z + nG * b.w;
                nTS.xy *= _NormalStrength;
                nTS = normalize(nTS);

                // wet trench floor
                half wet = saturate(1.0h - input.positionWS.y / max(_WetHeight, 1e-3));
                albedo *= lerp(1.0h, _WetDarken, wet);
                smooth = lerp(smooth, _WetSmoothness, wet);

                float sgn = input.tangentWS.w;
                float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                half3x3 tbn = half3x3(input.tangentWS.xyz, bitangent, input.normalWS.xyz);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = NormalizeNormalPerPixel(TransformTangentToWorld(nTS, tbn));
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);
                inputData.tangentToWorld = tbn;

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo;
                surface.metallic = 0;
                surface.specular = 0;
                surface.smoothness = smooth;
                surface.normalTS = nTS;
                surface.occlusion = 1;
                surface.alpha = 1;

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 ShadowVert(ShadowAttributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            half4 ShadowFrag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 DepthVert(DepthAttributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half DepthFrag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
