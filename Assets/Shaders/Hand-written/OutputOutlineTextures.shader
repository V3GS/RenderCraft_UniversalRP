Shader "V3GS/OutputOutlineTextures"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4  vertexPositionOS    : POSITION;
                half3   normalOS            : NORMAL;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                float4  vertexPositionHCS   : SV_POSITION;
                half3   normalWS            : TEXCOORD0;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertexPositionHCS = TransformObjectToHClip(IN.vertexPositionOS);
                // Transform normals to World-space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                //OUT.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return OUT;
            }

            half4 frag (v2f IN) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, IN.uv);
                //return col;

                half4 color = 0;

                color.rgb = IN.normalWS * 0.5 + 0.5;
                return color;
            }
            ENDHLSL
        }
    }
}
