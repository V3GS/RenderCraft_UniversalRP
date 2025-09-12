Shader "V3GS/OutlineEffect"
{
    Properties
    {
        [KeywordEnum(OutlineColor, Outline, Normal, Depth, Color)]
        _VisualizeOption ("Visualize option", Float) = 0
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

               return color;
           }

           ENDHLSL
       }
   }
}
