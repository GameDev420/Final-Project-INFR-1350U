Shader "Custom/Lighting/LambertLit"
{
	Properties
	{
		_BaseColor ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}

		Pass
		{
			Name "UniversalForward"

			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM

			// Define which functions to use for vertex/fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			// Declare vars here again so GPU can read it
			// These MATCH the property names (at least the ones we want to connect/define)
			// Types include float, float4, bool Texture2D, float4x4, etc.
			CBUFFER_START(UnityPerMaterial)

			float4 _BaseColor;

			CBUFFER_END

			// Vertex Input
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			}; // END STRUCTS WITH SEMI-COLON

			// Vertex output/fragment input
			struct Varyings
			{
				float4 positionHCS : SV_POSITION; // REQUIRED. Specifically the transformed vertex point (usually through TransformWorldToHClip, can be used with TransformObjectToWorld)
				float3 normalWS : TEXCOORD; // Despite name, semantic used for multi-component data (e.g. vectors, normals, arrays, etc.). Also used with TransformObjectToWorldNormal
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT;

				float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);

				OUT.positionHCS = TransformWorldToHClip(posWS);
				OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target // Tells graphics pipeline to use this for fragment color (in conjuction with #pragma?)
			{
				// Ensure given WS normal is normalized
				float3 n = SafeNormalize(IN.normalWS);

				// Get light multiplier (angular contribution, clamped from 0-1)
				Light mainLight = GetMainLight();
				float nDotL = saturate(dot(n, mainLight.direction));

				// Calculate non-ambient color (just color and main light contribution)
				half3 diffuse =  nDotL * _BaseColor.rgb * mainLight.color.rbg;

				// Sample from scene ambience
				half3 ambient = SampleSH(n) * _BaseColor.rgb;

				return half4(diffuse + ambient, 1.0f);
			}

			ENDHLSL
		}
	}
}