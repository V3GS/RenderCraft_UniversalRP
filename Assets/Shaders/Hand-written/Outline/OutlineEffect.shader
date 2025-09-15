Shader "V3GS/OutlineEffect"
{
    Properties
    {
        [KeywordEnum(OutlineColor, Outline, Normal, Depth, Color)]
        _VisualizeOption ("Visualize option", Float) = 0

        _OutlineColor ("Outline Color", Color) = (1,1,1,1)

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

           // These textures were generated in the "OutputOutlineTexturesRendererFeature"
           sampler2D _NormalTexture;
           sampler2D _DepthTexture;
           sampler2D _ColorTexture;

           float4 _OutlineColor;

           float _Scale;
           float _DepthThreshold;
           float _NormalThreshold;
           float _ColorThreshold;

           // Drawing outlines with depth
           float DepthOutline(float2 uvs[4])
           {
               float depth0 = tex2D(_DepthTexture, uvs[0]).r;
               float depth1 = tex2D(_DepthTexture, uvs[1]).r;
               float depth2 = tex2D(_DepthTexture, uvs[2]).r;
               float depth3 = tex2D(_DepthTexture, uvs[3]).r;

               float depthDifference0 = depth1 - depth0;
               float depthDifference1 = depth3 - depth2;

               float edgeDepth = sqrt(pow(depthDifference0, 2) + pow(depthDifference1, 2)) * 100;
               float depthThreshold = _DepthThreshold * depth0;

               edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

               return edgeDepth;
           }

           // Drawing outlines with normals
           float NormalsOutline(float2 uvs[4])
           {
               float3 normal0 = tex2D(_NormalTexture, uvs[0]).rgb;
               float3 normal1 = tex2D(_NormalTexture, uvs[1]).rgb;
               float3 normal2 = tex2D(_NormalTexture, uvs[2]).rgb;
               float3 normal3 = tex2D(_NormalTexture, uvs[3]).rgb;

               float3 normalDifference0 = normal1 - normal0;
               float3 normalDifference1 = normal3 - normal2;

               float edgeNormal = sqrt(dot(normalDifference0, normalDifference0) + dot(normalDifference1, normalDifference1));
               edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;

               return edgeNormal;
           }

           // Drawing outlines with color variance
           float GetLuminance(float3 color)
           {
                return color.r * 0.3 + color.g * 0.59 + color.b * 0.11;
           }
           
           float ColorOutline(float2 uvs[4])
           {
                float3 color0 = tex2D(_ColorTexture, uvs[0]).rgb;
                float3 color1 = tex2D(_ColorTexture, uvs[1]).rgb;
                float3 color2 = tex2D(_ColorTexture, uvs[2]).rgb;
                float3 color3 = tex2D(_ColorTexture, uvs[3]).rgb;

                float luminance0 = GetLuminance(color0);
                float luminance1 = GetLuminance(color1);
                float luminance2 = GetLuminance(color2);
                float luminance3 = GetLuminance(color3);

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

               float4 normalBuffer = tex2D(_NormalTexture, uv);
               float4 depthBuffer = tex2D(_DepthTexture, uv);
               float4 colorBuffer = tex2D(_ColorTexture, uv);

               #ifdef _VISUALIZEOPTION_NORMAL
                    color = normalBuffer;
               #endif

               #ifdef _VISUALIZEOPTION_DEPTH
                    color = depthBuffer;
               #endif

               #ifdef _VISUALIZEOPTION_COLOR
                    color = colorBuffer;
               #endif

               float halfScaleFloor = floor(_Scale * 0.5);
               float halfScaleCeil = ceil(_Scale * 0.5);
               float2 texelSize = (1.0) / float2(_BlitTexture_TexelSize.z, _BlitTexture_TexelSize.w);

               float2 uvs[4];
               uvs[0] = uv - float2(texelSize.x, texelSize.y) * halfScaleFloor; // bottomLeftUV
               uvs[1] = uv + float2(texelSize.x, texelSize.y) * halfScaleCeil; // topRightUV
               uvs[2] = uv + float2(texelSize.x * halfScaleCeil, - texelSize.y * halfScaleFloor); // bottomRightUV
               uvs[3] = uv + float2(-texelSize.x * halfScaleFloor, texelSize.y * halfScaleCeil); // topLeftUV

               float edgeDepth;
               float edgeNormal;
               float edgeColor;

               #ifdef _VISUALIZEOPTION_OUTLINE
                    edgeDepth = DepthOutline(uvs);
                    edgeNormal = NormalsOutline(uvs);
                    edgeColor = ColorOutline(uvs);
                    
                    color = saturate(edgeDepth + edgeNormal + edgeColor);
               #endif

               #ifdef _VISUALIZEOPTION_OUTLINECOLOR
                    edgeDepth = DepthOutline(uvs);
                    edgeNormal = NormalsOutline(uvs);
                    edgeColor = ColorOutline(uvs);

                    float edge = saturate(edgeDepth + edgeNormal + edgeColor);
                    color = lerp(color, _OutlineColor, edge);
               #endif

               return color;
           }

           ENDHLSL
       }
   }
}
