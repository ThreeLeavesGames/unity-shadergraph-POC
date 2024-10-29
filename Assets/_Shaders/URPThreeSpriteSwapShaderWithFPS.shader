Shader "Custom/URPThreeSpriteSwapShaderWithFPS"
{
    Properties
    {
        _MainTex1 ("Sprite 1", 2D) = "white" {}
        _MainTex2 ("Sprite 2", 2D) = "white" {}
        _MainTex3 ("Sprite 3", 2D) = "white" {}
        _FPS ("Frames Per Second", Float) = 1.0  // How fast to swap the textures (1 FPS by default)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex1);
            SAMPLER(sampler_MainTex1);
            TEXTURE2D(_MainTex2);
            SAMPLER(sampler_MainTex2);
            TEXTURE2D(_MainTex3);
            SAMPLER(sampler_MainTex3);
            float _FPS;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Sample all textures
                half4 tex1 = SAMPLE_TEXTURE2D(_MainTex1, sampler_MainTex1, IN.uv);
                half4 tex2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, IN.uv);
                half4 tex3 = SAMPLE_TEXTURE2D(_MainTex3, sampler_MainTex3, IN.uv);

                // Calculate time based on FPS (Frames Per Second)
                float time = _Time.y;  // Time in seconds from Unity's built-in _Time variable

                // Calculate frame duration (how many seconds per frame)
                float frameDuration = 1.0 / _FPS;

                // Calculate which frame we're currently on (modulo 3 for 3 textures)
                int frame = (int)(time / frameDuration) % 3;

                // Return the appropriate texture based on the frame
                if (frame == 0)
                {
                    return tex1;  // Show Sprite 1
                }
                else if (frame == 1)
                {
                    return tex2;  // Show Sprite 2
                }
                else
                {
                    return tex3;  // Show Sprite 3
                }
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
