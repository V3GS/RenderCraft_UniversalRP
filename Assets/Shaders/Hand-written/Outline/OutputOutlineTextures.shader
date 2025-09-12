Shader "V3GS/OutputOutlineTextures"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL macros and functions
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The DeclareDepthTexture.hlsl file contains utilities for sampling the Camera depth texture.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct appdata
            {
                float4  vertexPositionOS    : POSITION;
                half3   normalOS            : NORMAL;
                float2  texcoord             : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4  vertexPositionHCS   : SV_POSITION;
                half3   normalWS            : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                
                OUT.vertexPositionHCS = TransformObjectToHClip(IN.vertexPositionOS);
                // Transform normals to World-space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                return OUT;
            }

             // MRT shader
            struct FragmentOutput
            {
                half4 dest0 : SV_Target0;
                half4 dest1 : SV_Target1;
                half4 dest2 : SV_Target2;
            };

            half4 GetDepth(float4 vertexPositionHCS)
            {
                float2 UV = vertexPositionHCS.xy / _ScaledScreenParams.xy;
                real depth = 0.0;

                // Sample the depth from the Camera depth texture.
                #if UNITY_REVERSED_Z
                    depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                return half4(depth, depth, depth, 1);
            }

            FragmentOutput frag (v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                FragmentOutput output;
                float2 uv = IN.vertexPositionHCS.xy / _ScaledScreenParams.xy;

                output.dest0 = half4(IN.normalWS * 0.5 + 0.5, 1.0);
                output.dest1 = GetDepth(IN.vertexPositionHCS);
                output.dest2 = half4(SampleSceneColor(uv), 1.0);

                return output;
            }
            ENDHLSL
        }
    }
}
