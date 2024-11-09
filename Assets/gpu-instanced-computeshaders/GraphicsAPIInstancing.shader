Shader "Custom/GraphicsAPIInstancing"
{
   SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
             // Make sure we enable instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

           
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct InstanceData
            {
                float4 color;
            };

            StructuredBuffer<InstanceData> _InstanceData;
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;  // Initialize to avoid warning
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                uint instanceID = input.instanceID;
                InstanceData data = _InstanceData[instanceID];
                
                float4 positionOS = input.positionOS;                
                output.positionCS = TransformObjectToHClip(positionOS.xyz);
                output.color = data.color;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return input.color;
            }
            ENDHLSL
        }
    }
}