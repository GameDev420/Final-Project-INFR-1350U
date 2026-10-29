Shader "Custom/Lighting/LambertLit"
{
	Properties
	{
		[Header(Toggles)]
		[Space]
		// Toggle attribute turns Float into a checkbox in Inspector
		// 0 is false, 1 is true
		// Also looked into Shader Features, but that gets pretty bloated with more than a handful of shader varients
		// ^ (since it essentially compiles a separate shader for each path afaik, making total shaders 2^X, which would be at LEAST 16 varients for the different lighting models)
		[Toggle] _UseAlbedo("Use Albedo", Float) = 1
		[Toggle] _UseLambert ("Use Lambert", Float) = 1
		[Toggle] _UseAmbient ("Use Ambient", Float) = 1

		[Space]
		[Header(Albedo)]
		[Space]
		_BaseColor ("Base Color", Color) = (1, 1, 1, 1)
		_BaseMap ("Base Map", 2D) = "white" {}
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

			Tags { "LightMode" = "UniversalForward" "RenderPipeline" = "UniversalPipeline" }

			HLSLPROGRAM

			// Define which functions to use for vertex/fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			// Declare texture reference and sampler (?)
			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);
			float4 _BaseMap_ST; // Tiling and offset (combined with TRANSFORM_TEX, Unity does the rest)

			// Declare vars here again so GPU can read it
			// These MATCH the property names (at least the ones we want to connect/define)
			// Types include float, float4, bool Texture2D, float4x4, etc.
			CBUFFER_START(UnityPerMaterial)

			// Toggles
			float _UseAlbedo;
			float _UseLambert;
			float _UseAmbient;

			// Albedo
			float4 _BaseColor; // Inspector-editable (from properties, can only access in shader bc of this?)

			CBUFFER_END

			// Vertex Input
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 uv : TEXCOORD0; // Texture UV coordinates

			}; // END STRUCTS WITH SEMI-COLON

			// Vertex output/fragment input
			struct Varyings
			{
				float4 positionHCS : SV_POSITION; // REQUIRED. Specifically the transformed vertex point (usually through TransformWorldToHClip, can be used with TransformObjectToWorld)
				float3 normalWS : TEXCOORD1; // Despite name, semantic used for multi-component data (e.g. vectors, normals, arrays, etc.). Also used with TransformObjectToWorldNormal
				float2 uv : TEXCOORD0;
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT;

				float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);

				OUT.positionHCS = TransformWorldToHClip(posWS);
				OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target // Tells graphics pipeline to use this for fragment color (in conjuction with #pragma?)
			{
				half3 finalColor = half3(1, 1, 1);

				if (_UseAlbedo == 1)
				{
					// SAMPLE_TEXTURE2D returns a half4, so I'm guessing .rgb just converts to half3.
					finalColor = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
				}

				// Ensure given WS normal is normalized
				float3 n = SafeNormalize(IN.normalWS);
				
				if (_UseLambert == 1)
				{
					// Get light multiplier (angular contribution, clamped from 0-1)
					Light mainLight = GetMainLight();
					float nDotL = saturate(dot(n, mainLight.direction));

					// Calculate non-ambient color (just color, texture, and main light contribution)
					finalColor *= nDotL * mainLight.color.rbg;
				}

				if (_UseAmbient == 1)
				{
					// Sample from scene ambience
					// multiplying by (1 - nDotL) does nothing here
					finalColor += SampleSH(n) * finalColor;
				}

				return half4(finalColor, 1.0f);
			}

			ENDHLSL
		}
	}
}