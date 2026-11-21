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
		// Global tags here?
		// (we don't need any for this, since we specify them in each pass)

		Pass
		{
			Name "UniversalForward"

			Tags { "LightMode" = "UniversalForward" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

			HLSLPROGRAM

			// Define which functions to use for vertex/fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _FORWARD_PLUS

			// TODO: research shadows for additional lights (hopefully similar to main light where Unity does most of the calculations at this stage)
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			
			#pragma multi_compile_fog

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl" // Should be included in Core.hlsl

			// Declare texture reference and sampler
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
			float4 _BaseColor;

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
				OUT.tangentWS = normalize(TransformObjectToWorldDir(IN.tangentOS.xyz)); // Tangent is a direction, not a normal
				// Also accounts for negatively-scaling objects
				OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w * GetOddNegativeScale();
				
				// Compute world-space view direction (dir from vertex to camera?)
				OUT.viewDirWS = normalize(GetCameraPositionWS() - OUT.positionWS);
				
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

				// Example implementation for receiving shadows on slides
				// Probably better performance (i.e. per-vertex shadow coord calculation), but created immensely incorrect shadows (rings around surface below camera)
				//VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);
				//OUT.shadowCoords = TransformWorldToShadowCoord(positions.positionWS);
				//OUT.shadowCoords = GetShadowCoord(positions);

				return OUT;
			}

			// Checks whether nDotL has been calculated,and if not, calculates it using the given normal and light direction.
			// While I'm sure basic vector math is really fast on the GPU, hopefully this approach is more efficient than potentially calculating it multiple times.
			half ValidateNDotL(half nDotL, half3 normal, half3 lightDir)
			{
				if (nDotL < 0.0) nDotL = saturate(dot(normal, lightDir));
				return nDotL;
			}

			half4 frag(Varyings IN) : SV_Target // Tells graphics pipeline to use this for fragment color (in conjuction with #pragma?)
			{
				half3 finalColor = half3(1, 1, 1);

				if (_UseAlbedo == 1)
				{
					finalColor = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
				}

				
				// Declare commonly used variables here so they can be used and modified across muliple features
				float3 normalWS = (float3)0;
				
				half nDotL = -1.0;

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
					finalColor += SampleSH(normalWS);// * finalColor;
				}
				
				if (_UseLambert == 1)
				{
					// Get light multiplier (angular contribution, clamped from 0-1)
					Light mainLight = GetMainLight();
					
					nDotL = ValidateNDotL(nDotL, normalWS, mainLight.direction);

					// Calculate non-ambient color (just color, texture, and main light contribution)
					finalColor *= nDotL * mainLight.color.rgb;
				}

				if (_UseSpecular == 1)
				{
					Light mainLight = GetMainLight();
					half3 lightDir = normalize(mainLight.direction);

					half3 reflectDir = reflect(-lightDir, normalWS);

					half3 viewDir = normalize(IN.viewDirWS);
					half specFactor = pow(saturate(dot(reflectDir, viewDir)), max(_Shininess, 1));
					half3 specular = _SpecColor.rgb * specFactor;

					nDotL = ValidateNDotL(nDotL, normalWS, lightDir);

					// Apply specular reflections to fragment color
					// Found that without multiplying by nDotL, specular reflections would appear even on the back side of objects (where nDotL is negative)
					finalColor += specular * nDotL;
				}

				if (_UseShadows == 1)
				{
					// Probably worse performance than per-vertex shadow coord calculation, but this should be more accurate and avoid the weird shadow rings with the per-vertex approach.
					// I think this is because the vertex approach interpolates based on the current fragment? Or is literally just the same shadow coord for the entire triangle (still heavily inaccurate)
					float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

					// Probably more efficient that getting both the main light and shadow attenuation if we only need one (via GetMainLight(shadowCoord) )
					// (unless we need light reference for color or something)
					float shadowAttenuation = MainLightRealtimeShadow(shadowCoord);

					// Account for URP shadow fade
					shadowAttenuation = lerp(shadowAttenuation, 1.0, GetMainLightShadowFade(IN.positionWS));

					// Apply shadow strength
					finalColor *= lerp(1.0, shadowAttenuation, _ShadowStrength);

					// ^ Shadow Docs
					// https://docs.unity.com/en-us/engine/6000.0/manual/materials-and-shaders/shaders/writing-custom-shaders-urp/use-built-in-shader-methods/shadows
				}

				// Additional Lights
				if (false)
				{
					// Sources:
					// https://docs.unity.com/en-us/engine/6000.0/manual/lighting-overview/lighting/custom-lighting/use-built-in-shader-methods-additional-lights-fplus
					// https://discussions.unity.com/t/how-to-get-point-light-shadows-working-in-custom-urp-hlsl-shader-6000-2-0f1/1685050/2

					// Required for additional lights in Forward+
					InputData inputData = (InputData)0;
					inputData.positionWS = IN.positionWS;
					inputData.normalWS = IN.normalWS;
					inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
					inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);

					// Represents how much this fragment is in shadow
					// i.e. 0 = fully lit, 1 = fully in shadow
					float shadow = 0.0;

					uint pixelLightCount = GetAdditionalLightsCount();
					LIGHT_LOOP_BEGIN(pixelLightCount)
						Light light = GetAdditionalLight(lightIndex, IN.positionWS);

						// Pretty sure light.direction is already calculated here (relative to the current fragment)
						half dotProduct = saturate(dot(normalWS, light.direction));
						finalColor *= dotProduct * light.color.rgb;

						shadow += AdditionalLightRealtimeShadow(lightIndex, IN.positionWS, light.direction);
					LIGHT_LOOP_END
					
					if (pixelLightCount > 0) shadow /= pixelLightCount;
					else shadow = 1.0;
					
					finalColor *= lerp(1.0, shadow, _ShadowStrength);
				}

				if (_UseFog == 1)
				{
					// Found via:
					// https://discussions.unity.com/t/how-to-compute-fog-in-hlsl-on-urp/943637/4

					#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					float viewZ = -TransformWorldToView(IN.positionWS).z;
					float nearZ0ToFarZ = max(viewZ - _ProjectionParams.y, 0);
					float density = 1.0f - ComputeFogIntensity(ComputeFogFactorZ0ToFar(nearZ0ToFarZ));

					// Apply fog to fragment color
					// Simple lerp since density calculation handles the fog linear/exponential mode from my understanding
					finalColor = lerp(finalColor, unity_FogColor, density);
					#endif
				}

				return half4(finalColor, 1.0f);
			}

			ENDHLSL
		}
		

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull Back

			HLSLPROGRAM

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			//#pragma multi_compile_shadowcaster
			
			// URP shadow caster?
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
			};

			float3 _LightDirection;
			float3 _LightPosition;
			

			Varyings vertShadowCaster(Attributes IN)
			{
				Varyings OUT;
				
				float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

				// Reference:
				// https://discussions.unity.com/t/urp-custom-lighting-shadows-not-working-with-cascades/1718739/2

				#if defined( _CASTING_PUNCTUAL_LIGHT_SHADOW)
				// Calculate direction of additional lights
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
				// Directional light
				float3 lightDirectionWS = _LightDirection;
				#endif

				// Apply shadow bias according to light settings
				positionWS = ApplyShadowBias
				(
					positionWS, // world space position
					TransformObjectToWorldNormal(IN.normalOS), // world space normal
					lightDirectionWS
				);

				OUT.positionHCS = TransformWorldToHClip(positionWS);
				OUT.positionHCS = ApplyShadowClamping(OUT.positionHCS);

				return OUT;
			}

			half4 fragShadowCaster(Varyings IN) : SV_Target
			{
				return half4(0, 0, 0, 1);
			}

			ENDHLSL
		}
	}
}