Shader "V3GS/OutputOutlineTextures"
{
    Properties
    {
        [KeywordEnum(Normal, Depth, Color)]
        _VisualizeOption ("Visualize option", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _VISUALIZEOPTION_NORMAL
            #pragma shader_feature _VISUALIZEOPTION_DEPTH
            #pragma shader_feature _VISUALIZEOPTION_COLOR

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
                float2  uv                  : TEXCOORD0;
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

                OUT.uv = OUT.vertexPositionHCS.xy / OUT.vertexPositionHCS.w;
                OUT.uv = (OUT.uv * 0.5 + 0.5);

                #ifdef UNITY_UV_STARTS_AT_TOP
                    OUT.uv.y = 1 - OUT.uv.y;
                #endif

                return OUT;
            }

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

                return half4(depth, 0, 0, 1);
            }

            half4 frag (v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half4 color = 0;

                // Visualize normals
                #ifdef _VISUALIZEOPTION_NORMAL
                    color.rgb = IN.normalWS * 0.5 + 0.5;
                #endif

                // Visualize depth
                #ifdef _VISUALIZEOPTION_DEPTH
                    color = GetDepth(IN.vertexPositionHCS);
                #endif
                
                // Visualize color
                #ifdef _VISUALIZEOPTION_COLOR
                    color = half4(SampleSceneColor(IN.uv), 1.0);
                #endif

                return color;
            }
            ENDHLSL
        }
    }
}
