Shader "Unlit/SimpleWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.2, 0.5, 0.7, 0.8)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _FoamTex ("Foam Texture", 2D) = "white" {}
        _FoamSpeed ("Foam Movement Speed", Vector) = (0.1, 0.1, 0, 0)
        _EdgeWidth ("Edge Width", Range(0, 5)) = 1
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float4 fogFactor : TEXCOORD2;
            };

            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FoamColor;
                float2 _FoamSpeed;
                float _EdgeWidth;
                float _FoamAmount;
                float4 _FoamTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Transform position and normal to world space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _FoamTex);
                
                // Calculate view direction in world space
                float3 positionWS = vertexInput.positionWS;
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
                output.viewDir = viewDirWS;
                
                // Fog
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Calculate edge foam using fresnel
                float fresnel = saturate(1 - dot(normalize(input.viewDir), float3(0, 1, 0)));
                float edgeFoam = pow(fresnel, (5 - _EdgeWidth));
                
                // Moving foam particles
                float2 foamUV = input.uv + _Time.y * _FoamSpeed;
                float foam = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, foamUV).r;
                foam = saturate(foam * _FoamAmount);
                
                // Combine edge foam and particles
                float finalFoam = saturate(edgeFoam + foam);
                
                // Final color
                float4 finalColor = lerp(_Color, _FoamColor, finalFoam);
                
                // Apply fog
                float fogFactor = input.fogFactor.x;
                finalColor.rgb = MixFog(finalColor.rgb, fogFactor);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
