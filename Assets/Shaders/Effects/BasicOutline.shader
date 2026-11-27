Shader "Custom/Effects/BasicOutline"
{
	Properties
	{
		[Header(Toggles)]
		[Space]
		[Toggle] _UseOutline ("Use Outline", Float) = 1
		[Space]
		[Toggle] _UseAlbedo ("Use Albedo", Float) = 1
		[Toggle] _UseNormal ("Use Normal", Float) = 1
		[Toggle] _UseLambert ("Use Lambert", Float) = 1
		[Toggle] _UseAmbient ("Use Ambient", Float) = 1
		[Toggle] _UseSpecular ("Use Specular", Float) = 1
		[Toggle] _UseShadows ("Use Shadows", Float) = 1
		[Toggle] _UseFog ("Use Fog", Float) = 1
		[Toggle] _UseAdditionalLights("Use Additional Lights", Float) = 1

		[Space]
		[Header(Outline)]
		[Space]
		[HDR] _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineWidth ("Outline Width", Float) = 1
		[Toggle] _OutlineFog("Outline Fog", Float) = 1
		_OutlinePulseSpeed("Outline Pulse Speed", Float) = 0.4
		_OutlinePulseMin("Outline Pulse Min", Range(0, 5)) = 0
		_OutlinePulseMax("Outline Pulse Max", Range(0, 5)) = 1

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
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			Name "UniversalForward"

			Tags { "LightMode" = "UniversalForward" "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

			HLSLPROGRAM

			// Define which functions to use for vertex/fragment shaders
			#pragma vertex vert
			#pragma fragment frag

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
			float _UseAdditionalLights;

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

				// Declare commonly used variables here so they can be used and modified across muliple features
				float3 normalWS = (float3)0;
				half3 albedo = half3(1, 1, 1); // used as a base color even if "use albedo" is false. Found that using 'final color' for additional lights didn't work in areas not hit by main light
				Light mainLight = GetMainLight();

				float nDotL = -1.0;
				float shadow = 0.0;

				if (_UseAlbedo == 1)
				{
					albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;
					finalColor *= albedo;
				}

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

				if (_UseLambert == 1)
				{
					// Get light multiplier (angular contribution, clamped from 0-1)
					nDotL = ValidateNDotL(nDotL, normalWS, mainLight.direction);

					// Calculate non-ambient color (just color, texture, and main light contribution)
					finalColor = nDotL * mainLight.color.rgb * albedo;
				}

				if (_UseSpecular == 1)
				{
					half3 lightDir = normalize(mainLight.direction);

					half3 reflectDir = reflect(-lightDir, normalWS);

					half3 viewDir = normalize(IN.viewDirWS);
					half specFactor = pow(saturate(dot(reflectDir, viewDir)), max(_Shininess, 1));
					//half3 specular = _SpecColor.rgb * specFactor;
					half3 specular = mainLight.color.rgb * specFactor; // I like using the light color instead :)

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

					// Populates light information from the shadow coordinates
					mainLight = GetMainLight(shadowCoord);

					nDotL = ValidateNDotL(nDotL, normalWS, mainLight.direction);

					// Account for URP shadow fade
					float shadowAttenuation = lerp(mainLight.shadowAttenuation, 1.0, GetMainLightShadowFade(IN.positionWS));

					// Apply shadow strength
					// Pretty sure it's correct for the main light shadow to be multiplied with the current color, but additional lights are added to the current color..?
					// Also not sure if distance attenuation applies here, since it's likely a directional light
					shadow += shadowAttenuation * mainLight.distanceAttenuation * nDotL;

					// ^ Shadow Docs
					// https://docs.unity.com/en-us/engine/6000.0/manual/materials-and-shaders/shaders/writing-custom-shaders-urp/use-built-in-shader-methods/shadows
				}

				// Additional Lights
				if (_UseAdditionalLights == 1)
				{
					// Sources:
					// https://docs.unity.com/en-us/engine/6000.0/manual/lighting-overview/lighting/custom-lighting/use-built-in-shader-methods-additional-lights-fplus
					// https://discussions.unity.com/t/how-to-get-point-light-shadows-working-in-custom-urp-hlsl-shader-6000-2-0f1/1685050/2

					// Required for additional lights in Forward+
					InputData inputData = (InputData)0;
					inputData.positionWS = IN.positionWS;
					inputData.normalWS = normalWS;
					inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
					inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);

					uint pixelLightCount = GetAdditionalLightsCount();

					// Loop through all additional lights affecting this fragment
					LIGHT_LOOP_BEGIN(pixelLightCount)
					
						Light light = GetAdditionalLight(lightIndex, IN.positionWS, half4(1, 1, 1, 1));

						float dotProduct = saturate(dot(normalWS, light.direction));

						// Lambert
						if (_UseLambert)
						{
							finalColor += light.distanceAttenuation * dotProduct * light.color * albedo;
						}

						// Specular
						if (_UseSpecular && light.shadowAttenuation > 0.0)
						{
							half3 viewDir = GetWorldSpaceNormalizeViewDir(IN.positionWS);
							half3 reflectDir = reflect(-light.direction, normalWS);
							half specValue = pow(saturate(dot(reflectDir, viewDir)), max(_Shininess, 1));
							finalColor += specValue * light.color.rgb * light.shadowAttenuation * light.distanceAttenuation; // No specular if covered in shadow (though maybe could add multiply by like 0.25 if so to fake indirect lighting?)
						}

						// Shadows
						if (_UseShadows == 1)
						{
							// Apply shadow fade (since I don't believe it's calculated automatically in GetAdditionalLight)
							float shadowAttenuation = lerp(light.shadowAttenuation, 1.0, GetAdditionalLightShadowFade(IN.positionWS));

							// Source:
							// https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
							// ^ took way too long to find radiance formula, but at least I now know how useful the URP source code is.
							shadow += shadowAttenuation * light.distanceAttenuation * dotProduct;
						}

					LIGHT_LOOP_END
				}

				// Apply shadows to final color
				//finalColor *= lerp(1.0, shadow, _ShadowStrength);
				if (_UseShadows == 1) finalColor *= saturate(lerp(1.0, shadow, _ShadowStrength));

				// Applied after Lambert and shadow calculations since ambient lighting happens regardless
				if (_UseAmbient == 1)
				{
					// Sample from scene ambience
					finalColor += SampleSH(normalWS) * albedo;
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

		// OUTLINE PASS
		Pass
		{
			Name "Outline"

			Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

			Cull Front

			HLSLPROGRAM

			// Define which functions to use for vertex/fragment shaders
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fog

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			CBUFFER_START(UnityPerMaterial)

			float _UseOutline;
			float4 _OutlineColor;
			float _OutlineWidth;

			float _OutlinePulseSpeed;
			float _OutlinePulseMin;
			float _OutlinePulseMax;

			float _UseFog;
			float _OutlineFog;

			CBUFFER_END

			// Vertex Input
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float3 positionWS : TEXCOORD0;
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				
				float width = max(_OutlineWidth, 0.0);
				width *= lerp(_OutlinePulseMin, _OutlinePulseMax, (sin(_Time.y * _OutlinePulseSpeed) + 1.0) * 0.5);
				
				// Not the most robust solution; simply renders extrudes the vertex along its negative normal (behind the mesh)
				// Should be fairly cheap for our game though, as only a couple of objects can have an outline at any given time
				if (width > 1.0 && _UseOutline == 1)
					OUT.positionHCS = TransformWorldToHClip(TransformObjectToWorld(IN.positionOS.xyz + normalize(IN.normalOS) * -width));
				else
					OUT.positionHCS = TransformWorldToHClip(TransformObjectToWorld(IN.positionOS.xyz));

				// ^ largest downside is probably being per-vertex instead of per-pixel, potentially problematic for low-poly meshes AND/OR meshes near screen edges (outline is affected by perspective warping)

				OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half4 finalColor = _OutlineColor;

				if (_UseFog == 1 && _OutlineFog == 1)
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

				return finalColor;
			}

			ENDHLSL
		}
	}
}