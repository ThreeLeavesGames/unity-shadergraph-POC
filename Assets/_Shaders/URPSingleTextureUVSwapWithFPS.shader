Shader "Custom/URPSingleTextureUVSwapWithFPS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Direction ("Direction", Int) = 0  // 0: Down, 1: Up, 2: Left, 3: Right
        _PartType ("Part Type", Int) = 0   // 0: Head, 1: Body
        [Toggle] _UseAlpha("Use Alpha Blending", Float) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_UseAlpha]
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Universal Render Pipeline requirements
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                int _Direction;
                int _PartType;
                float _UseAlpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Constants matching the original code
            static const float TEXTURE_WIDTH = 256.0;
            static const float TEXTURE_HEIGHT = 128.0;
            static const float PART_HEIGHT = 64.0;

            // Function to convert pixel coordinates to UV coordinates
            float2 PixelsToUV(float2 pixels)
            {
                return float2(pixels.x / TEXTURE_WIDTH, pixels.y / TEXTURE_HEIGHT);
            }

            // Function to get UV offset based on direction and part type
            float2 GetUVOffset()
            {
                float2 offset = float2(0, 0);
                
                // Calculate base offset for part type (head or body)
                offset.y = _PartType * PART_HEIGHT;  // 0 for head, 64 for body
                
                // Calculate offset based on direction
                offset.x = _Direction * PART_HEIGHT;  // 0, 64, 128, or 192
                
                return offset;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Transform position from object to clip space
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                // Get the base UV offset for current direction and part
                float2 uvOffset = GetUVOffset();
                
                // Convert original UV (0-1) to pixel space relative to sprite section
                float2 pixelCoord = input.uv * PART_HEIGHT;
                
                // Add offset and convert back to UV space
                float2 finalUV = PixelsToUV(pixelCoord + uvOffset);
                
                output.uv = finalUV;
                
                // Calculate fog
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample texture
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Apply fog
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
}

}