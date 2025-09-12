Shader "V3GS/OutlineEffect"
{
    Properties
    {
        [KeywordEnum(OutlineColor, Outline, Normal, Depth, Color)]
        _VisualizeOption ("Visualize option", Float) = 0

        _Scale ("Scale", Range(0, 10)) = 1
        _DepthThreshold ("Depth threshold", Range(0, 100)) = 0.2
    }
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off

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

           float _Scale;
           float _DepthThreshold;

           // Drawing outlines with depth
           float DepthOutline(float2 uv)
           {
               float halfScaleFloor = floor(_Scale * 0.5);
               float halfScaleCeil = ceil(_Scale * 0.5);
               float2 texel = (1.0) / float2(_BlitTexture_TexelSize.z, _BlitTexture_TexelSize.w);

               float2 bottomLeftUV = uv - float2(texel.x, texel.y) * halfScaleFloor;
               float2 topRightUV = uv + float2(texel.x, texel.y) * halfScaleCeil;
               float2 bottomRightUV = uv + float2(texel.x * halfScaleCeil, - texel.y * halfScaleFloor);
               float2 topLeftUV = uv + float2(-texel.x * halfScaleFloor, texel.y * halfScaleCeil);

               float4 depth0 = tex2D(_DepthTexture, bottomLeftUV).r;
               float4 depth1 = tex2D(_DepthTexture, topRightUV).r;
               float4 depth2 = tex2D(_DepthTexture, bottomRightUV).r;
               float4 depth3 = tex2D(_DepthTexture, topLeftUV).r;

               float depthFiniteDifference0 = depth1 - depth0;
               float depthFiniteDifference1 = depth3 - depth2;

               float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
               float depthThreshold = _DepthThreshold * depth0;

               edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

               return edgeDepth;
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

               #ifdef _VISUALIZEOPTION_OUTLINE
                    color = DepthOutline(uv);
               #endif

               return color;
           }

           ENDHLSL
       }
   }
}
