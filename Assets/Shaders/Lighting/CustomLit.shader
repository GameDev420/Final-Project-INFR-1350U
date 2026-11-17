Shader "Custom/Lighting/Lit"
{
	Properties
	{
		[Header(Toggles)]
		[Space]
		// Toggle attribute turns Float into a checkbox in Inspector
		// 0 is false, 1 is true
		// Also looked into Shader Features, but that gets pretty bloated with more than a handful of shader varients
		// ^ (since it essentially compiles a separate shader for each path afaik, making total shaders 2^X, which would be at LEAST 16 varients for the different lighting models)
		[Toggle] _UseAlbedo ("Use Albedo", Float) = 1
		[Toggle] _UseNormal ("Use Normal", Float) = 1
		[Toggle] _UseLambert ("Use Lambert", Float) = 1
		[Toggle] _UseAmbient ("Use Ambient", Float) = 1
		[Toggle] _UseSpecular ("Use Specular", Float) = 1
		[Toggle] _UseShadows ("Use Shadows", Float) = 1
		[Toggle] _UseFog ("Use Fog", Float) = 1

		[Space]
		[Header(Albedo)]
		[Space]
		_BaseColor ("Base Color", Color) = (1, 1, 1, 1)
		_BaseMap ("Base Map", 2D) = "white" {}

		[Space]
		[Header(Normal)]
		[Space]
		_NormalStrength ("Normal Strength", Range(0, 10)) = 1
		[NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}

		[Space]
		[Header(Specular)]
		[Space]
		_SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
		_Shininess ("Shininess", Float) = 1 // specular exponent

		[Space]
		[Header(Shadows)]
		[Space]
		_ShadowStrength ("Shadow Strength", Range(0, 1)) = 1

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

			Tags { "LightMode" = "UniversalForward" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

			HLSLPROGRAM

			// Define which functions to use for vertex/fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile_fog

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			// Declare texture reference and sampler (?)
			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);
			float4 _BaseMap_ST; // Tiling and offset (combined with TRANSFORM_TEX, Unity does the rest)

			TEXTURE2D(_NormalMap);
			SAMPLER(sampler_NormalMap); // TODO: see if you can use a single sampler for both albedo and normal maps

			// Declare vars here again so GPU can read it
			// These MATCH the property names (at least the ones we want to connect/define)
			// Types include float, float4, bool Texture2D, float4x4, etc.
			CBUFFER_START(UnityPerMaterial)

			// Toggles
			float _UseAlbedo;
			float _UseNormal;
			float _UseLambert;
			float _UseAmbient;
			float _UseSpecular;
			float _UseShadows;
			float _UseFog;

			// Albedo
			float4 _BaseColor; // Inspector-editable (from properties, can only access in shader bc of this?)

			// Normal
			float _NormalStrength;

			// Specular
			float4 _SpecColor;
			float _Shininess;

			// Shadows
			float _ShadowStrength;

			CBUFFER_END

			// Vertex Input
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 uv : TEXCOORD0; // Texture UV coordinates
				float4 tangentOS : TANGENT; // Tangent for normal mapping

			}; // END STRUCTS WITH SEMI-COLON

			// Vertex output/fragment input
			struct Varyings
			{
				float3 positionWS : TEXCOORD5; // World Space position (for shadows) 
				float4 positionHCS : SV_POSITION; // REQUIRED. Specifically the transformed vertex point (usually through TransformWorldToHClip, can be used with TransformObjectToWorld)
				float3 normalWS : TEXCOORD1; // Despite name, semantic used for multi-component data (e.g. vectors, normals, arrays, etc.). Also used with TransformObjectToWorldNormal
				float3 tangentWS : TEXCOORD2; // World Space tangent (for normal map)
				float3 bitangentWS : TEXCOORD3; // World Space bitangent (for normal map)
				float3 viewDirWS : TEXCOORD4; // Used for Specular
				float2 uv : TEXCOORD0;
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT;

				OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

				OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);

				OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
				OUT.tangentWS = normalize(TransformObjectToWorldNormal(IN.tangentOS.xyz));
				OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w;
				
				// Compute world-space view direction (dir from vertex to camera?)
				OUT.viewDirWS = normalize(GetCameraPositionWS() - OUT.positionWS);
				
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target // Tells graphics pipeline to use this for fragment color (in conjuction with #pragma?)
			{
				half3 finalColor = half3(1, 1, 1);

				if (_UseAlbedo == 1)
				{
					finalColor = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
				}

				
				half3 normalWS = (half3)0;

				// Use normal map if enabled
				if (_UseNormal)
				{
					// Sample and unpack normal map
					half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
					
					// Apply normal strength
					normalTS.xy *= _NormalStrength;
					
					// Transform tangent space normal to world space
					half3x3 TBN = half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS);
					normalWS = normalize(mul(normalTS, TBN)); // matrix multiplication
				}
				// Otherwise, use face normal from vertex shader
				// Also ensure given WS normal is normalized
				else normalWS = SafeNormalize(IN.normalWS);

				// Applied before lambert to prevent direct lighting from influencing ambient color
				// since we multiply harmonics by final color, which would include lambert lighting if it was applied first ?
				if (_UseAmbient == 1)
				{
					// Sample from scene ambience
					finalColor += SampleSH(normalWS) * finalColor;
				}
				
				if (_UseLambert == 1)
				{
					// Get light multiplier (angular contribution, clamped from 0-1)
					Light mainLight = GetMainLight();
					half nDotL = saturate(dot(normalWS, mainLight.direction));

					// Calculate non-ambient color (just color, texture, and main light contribution)
					finalColor *= nDotL * mainLight.color.rbg;
				}

				// TODO: fix specular on backfaces (w/ normal map)
				if (_UseSpecular == 1)
				{
					Light mainLight = GetMainLight();
					half3 lightDir = normalize(mainLight.direction);

					half3 reflectDir = reflect(-lightDir, normalWS);

					half3 viewDir = normalize(IN.viewDirWS);
					half specFactor = pow(saturate(dot(reflectDir, viewDir)), max(_Shininess, 1));
					half3 specular = _SpecColor.rgb * specFactor;

					finalColor += specular;
				}

				if (_UseShadows == 1)
				{
					float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
					Light mainLight = GetMainLight(shadowCoord);

					// Apply shadow strength
					finalColor *= lerp(1.0, mainLight.shadowAttenuation, _ShadowStrength);
				}

				if (_UseFog == 1)
				{
					// Found via:
					// https://discussions.unity.com/t/how-to-compute-fog-in-hlsl-on-urp/943637/4
					#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					float viewZ = -TransformWorldToView(IN.positionWS).z;
					float nearZ0ToFarZ = max(viewZ - _ProjectionParams.y, 0);
					float density = 1.0f - ComputeFogIntensity(ComputeFogFactorZ0ToFar(nearZ0ToFarZ));

					// Apply fog
					finalColor = lerp(finalColor, unity_FogColor, density);
					#endif
				}

				return half4(finalColor, 1.0f);
			}

			ENDHLSL
		}
	}
}