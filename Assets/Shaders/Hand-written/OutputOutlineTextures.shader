Shader "V3GS/OutputOutlineTextures"
{
    Properties
    {

    }
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

            struct appdata
            {
                float4  vertexPositionOS    : POSITION;
                half3   normalOS            : NORMAL;
            };

            struct v2f
            {
                float4  vertexPositionHCS   : SV_POSITION;
                half3   normalWS            : TEXCOORD0;
            };

            v2f vert (appdata IN)
            {
                v2f OUT;
                
                OUT.vertexPositionHCS = TransformObjectToHClip(IN.vertexPositionOS);
                // Transform normals to World-space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

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
                half4 color = 0;

                // Visualize normals
                //color.rgb = IN.normalWS * 0.5 + 0.5;

                // Visualize depth
                color = GetDepth(IN.vertexPositionHCS);

                return color;
            }
            ENDHLSL
        }
    }
}
