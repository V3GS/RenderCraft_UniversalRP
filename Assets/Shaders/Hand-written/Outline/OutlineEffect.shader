Shader "V3GS/OutlineEffect"
{
    Properties
    {
        [KeywordEnum(OutlineColor, Outline, Normal, Depth, Color, Mask)]
        _VisualizeOption ("Visualize option", Float) = 0

        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)

        _Scale ("Scale", Range(0, 10)) = 1
        _DepthThreshold ("Depth threshold", Range(0, 100)) = 0.2
        _NormalThreshold ("Normal threshold", Range(0, 1)) = 0.2
        _ColorThreshold ("Color threshold", Range(0, 1)) = 0.2
    }
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off
       Cull Off

       Pass
       {
           Name "OutlineEffectPass"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           // Debug shader variants to visualize the input buffers.
           // If you just want to see the final result, you can remove these keywords and only preserve the logic of the Outline color result
           #pragma shader_feature _VISUALIZEOPTION_OUTLINECOLOR
           #pragma shader_feature _VISUALIZEOPTION_OUTLINE
           #pragma shader_feature _VISUALIZEOPTION_NORMAL
           #pragma shader_feature _VISUALIZEOPTION_DEPTH
           #pragma shader_feature _VISUALIZEOPTION_COLOR
           #pragma shader_feature _VISUALIZEOPTION_MASK

           // These textures were generated in the "OutputOutlineTexturesRendererFeature"
           sampler2D _NormalDepthTexture;
           sampler2D _ColorMaskTexture;

           float4 _OutlineColor;
           float4 _HighlightColor;

           float _Scale;
           float _DepthThreshold;
           float _NormalThreshold;
           float _ColorThreshold;

           // Drawing outlines with depth
           float DepthOutline(float4 normalDepthSamples[4])
           {
               float depthDifference0 = normalDepthSamples[1].a - normalDepthSamples[0].a;
               float depthDifference1 = normalDepthSamples[3].a - normalDepthSamples[2].a;

               float edgeDepth = sqrt(pow(depthDifference0, 2) + pow(depthDifference1, 2)) * 100;
               float depthThreshold = _DepthThreshold * normalDepthSamples[0].a;

               edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

               return edgeDepth;
           }

           // Drawing outlines with normals
           float NormalsOutline(float4 normalDepthSamples[4])
           {
               float3 normalDifference0 = normalDepthSamples[1].rgb - normalDepthSamples[0].rgb;
               float3 normalDifference1 = normalDepthSamples[3].rgb - normalDepthSamples[2].rgb;

               float edgeNormal = sqrt(dot(normalDifference0, normalDifference0) + dot(normalDifference1, normalDifference1));
               edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;

               return edgeNormal;
           }

           // Drawing outlines with color variance
           float GetLuminance(float3 color)
           {
                return color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
           }
           
           float ColorOutline(float4 _ColorMaskTexture[4])
           {
                float luminance0 = GetLuminance(_ColorMaskTexture[0].rgb);
                float luminance1 = GetLuminance(_ColorMaskTexture[1].rgb);
                float luminance2 = GetLuminance(_ColorMaskTexture[2].rgb);
                float luminance3 = GetLuminance(_ColorMaskTexture[3].rgb);

                const float colorDifference0 = luminance1 - luminance2;
                const float colorDifference1 = luminance0 - luminance3;

                float edgeColor = sqrt(dot(colorDifference0, colorDifference0) + dot(colorDifference1, colorDifference1));
                edgeColor = edgeColor > _ColorThreshold ? 1 : 0;

                return edgeColor;
           }

           float4 Frag(Varyings input) : SV_Target0
           {
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

               // sample the texture using the SAMPLE_TEXTURE2D_X_LOD
               float2 uv = input.texcoord.xy;
               half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);

               float4 normalDepthBuffer = tex2D(_NormalDepthTexture, uv);
               float4 colorMaskBuffer = tex2D(_ColorMaskTexture, uv);

               #ifdef _VISUALIZEOPTION_NORMAL
                    // The RGB channels contains the normal buffer
                    color = normalDepthBuffer;
               #endif

               #ifdef _VISUALIZEOPTION_DEPTH
                    // The alpha channel contains the depth buffer
                    color = normalDepthBuffer.a;
               #endif

               #ifdef _VISUALIZEOPTION_COLOR
                    // The RGB channels contains the color buffer
                    color = colorMaskBuffer;
               #endif

               #ifdef _VISUALIZEOPTION_MASK
                     // The alpha channel contains the mask buffer
                    color = colorMaskBuffer.a;
               #endif

               float halfScaleFloor = floor(_Scale * 0.5);
               float halfScaleCeil = ceil(_Scale * 0.5);
               float2 texelSize = (1.0) / float2(_BlitTexture_TexelSize.z, _BlitTexture_TexelSize.w);

               float2 uvs[4];
               uvs[0] = uv - float2(texelSize.x, texelSize.y) * halfScaleFloor; // bottomLeftUV
               uvs[1] = uv + float2(texelSize.x, texelSize.y) * halfScaleCeil; // topRightUV
               uvs[2] = uv + float2(texelSize.x * halfScaleCeil, - texelSize.y * halfScaleFloor); // bottomRightUV
               uvs[3] = uv + float2(-texelSize.x * halfScaleFloor, texelSize.y * halfScaleCeil); // topLeftUV

               float4 normalDepthSamples[4];
               float4 colorMaskSamples[4];

               // The shader compiler will unroll this loop because it uses a fixed index.
               // Therefore, it should not cause a performance impact.
               for (int i = 0; i < 4; i++)
               {
                    normalDepthSamples[i] = tex2D(_NormalDepthTexture, uvs[i]);
                    colorMaskSamples[i] = tex2D(_ColorMaskTexture, uvs[i]);
               }

               float edgeDepth;
               float edgeNormal;
               float edgeColor;

               #ifdef _VISUALIZEOPTION_OUTLINE
                    edgeDepth = DepthOutline(normalDepthSamples);
                    edgeNormal = NormalsOutline(normalDepthSamples);
                    edgeColor = ColorOutline(colorMaskSamples);
                    
                    color = saturate(edgeDepth + edgeNormal + edgeColor);
               #endif

               #ifdef _VISUALIZEOPTION_OUTLINECOLOR
                    edgeDepth = DepthOutline(normalDepthSamples);
                    edgeNormal = NormalsOutline(normalDepthSamples);
                    edgeColor = ColorOutline(colorMaskSamples);

                    float edge = saturate(edgeDepth + edgeNormal + edgeColor);
                    // It's added a highlight color. The color will be blended according to the alpha value of the highlight color
                    color += lerp(color, colorMaskBuffer.a * _HighlightColor, _HighlightColor.a);
                    color = lerp(color, _OutlineColor, edge);
               #endif

               return color;
           }

           ENDHLSL
       }
   }
}
