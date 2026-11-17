Shader "Custom/RimLighting"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        //[MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _RimColor("Rim Color", Color) = (0, 0.5, 0.5, 1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL; // Object space normal
                float4 tangentOS : TANGENT; // Tangent space for rim light calculations
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 viewDirWS : TEXCOORD0; // View direction in world space
                float3 normalWS : TEXCOORD1; // World space normal
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor; // Base color property
                float4 _RimColor; // Rim color property
                float _RimPower; // Rim power property
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Transform object space position to homogeneous clip-space position
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Transform object space normal to world space
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                // Calculate the view direction in world space (from the camera to the surface)
                float3 worldPosWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - worldPosWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize the world space normal and view direction
                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(IN.viewDirWS);
                
                // Rim lighting calculation (using dot product between normal and view direction)
                half rimFactor = 1.0 - saturate(dot(viewDirWS, normalWS));
                // ^ 1.0 - inverses the operation so it's on the edges, not from the center, can try without to demonstrate

                half rimLighting = pow(rimFactor, _RimPower);
                // ^ Power creates the gradient effect

                // Combine rim lighting color with the base color
                half3 finalColor = _BaseColor.rgb + _RimColor.rgb * rimLighting;
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
