// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Effect/BuildingTransparent"
{
	Properties
	{
	_Color("Base color", Color) = (1, 1, 1, 1)
	_MainTex("Texture", 2D) = "white" {}

	_Emission("Emission", Float) = 2.0

		_TransparentColor("Transparent Color", Color) = (1, 1, 1, 1)
		_TransparentDistance("Distance", Float) = 0.0
		_TransparentThickness("Thickness", Range(0.0, 100.0)) = 1.5
}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Opaque" }
		LOD 200

			Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
				//ZTest LEqual
				ZWrite Off
				Cull Off

				CGPROGRAM
#pragma vertex MainVS
#pragma fragment MainPS
#pragma target 3.0
#pragma multi_compile_fog
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"

#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

				uniform float4 _Color;
			uniform sampler2D _MainTex;

			uniform float _Emission;

			uniform float4 _TransparentColor;
			uniform float _TransparentDistance;
			uniform float _TransparentThickness;

			//#pragma surface surf Lambert
			struct InputSurface
			{
				float4 texcoord;
				float3 worldPos;
			};

			half surf(InputSurface input, inout SurfaceOutputStandard output)
			{
				half result = -1.0;
				half4 color = 0;
				if (input.worldPos.y <= _TransparentDistance)
				{
					color = _TransparentColor;
					result = 1.0;
				}

				output.Albedo = output.Emission * color.rgb;
				output.Alpha = color.a;
				return result;
			}

			// vertex-to-fragment interpolation data
#ifdef LIGHTMAP_OFF
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 texcoord : TEXCOORD1;
				fixed3 normal : TEXCOORD2;
#if UNITY_SHOULD_SAMPLE_SH
				half3 sh : TEXCOORD3; // SH
#endif
				SHADOW_COORDS(4)
					UNITY_FOG_COORDS(5)
#if SHADER_TARGET >= 30
					float4 lmap : TEXCOORD6;
#endif
			};
#endif

#ifndef LIGHTMAP_OFF
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 texcoord : TEXCOORD1;
				fixed3 normal : TEXCOORD2;
				float4 lmap : TEXCOORD3;
				SHADOW_COORDS(4)
					UNITY_FOG_COORDS(5)
#ifdef DIRLIGHTMAP_COMBINED
					fixed3 tSpace0 : TEXCOORD6;
				fixed3 tSpace1 : TEXCOORD7;
				fixed3 tSpace2 : TEXCOORD8;
#endif
			};
#endif

			float4 _MainTex_ST;

			half4 LightingStandard2(v2f input, SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
			{
				s.Normal = normalize(s.Normal);

				half oneMinusReflectivity;
				half3 specColor;
				s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

				// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
				// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
				half outputAlpha;
				s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

				half4 c;// = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
				c.rgb = s.Albedo;
				c.rgb *= tex2D(_MainTex, input.texcoord);
				c.a = 0.2;
				//c.rgb += UNITY_BRDF_GI(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
				//c.a = outputAlpha;
				return c;
			}

			// vertex shader
			v2f MainVS(appdata_full v)
			{
				v2f output;
				UNITY_INITIALIZE_OUTPUT(v2f, output);
				output.pos = UnityObjectToClipPos(v.vertex);
				output.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
#if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
#endif
#if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
				output.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				output.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				output.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
#endif
				output.worldPos = worldPos;
				output.normal = worldNormal;

#ifndef DYNAMICLIGHTMAP_OFF
				output.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
#ifndef LIGHTMAP_OFF
				output.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

				// SH/ambient and vertex lights
#ifdef LIGHTMAP_OFF
#if UNITY_SHOULD_SAMPLE_SH
#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
				output.sh = 0.0;
#elif (SHADER_TARGET < 30)
				output.sh = ShadeSH9(float4(worldNormal, 1.0));
#else
				output.sh = ShadeSH3Order(half4(worldNormal, 1.0));
#endif
				// Add approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
				//光照
				output.sh += Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, worldPos, worldNormal);
#endif
#endif
#endif // LIGHTMAP_OFF

				TRANSFER_SHADOW(output); // pass shadow coordinates to pixel shader
				UNITY_TRANSFER_FOG(output, output.pos); // pass fog coordinates to pixel shader
				return output;
			}

			// fragment shader
			fixed4 MainPS(v2f input) : SV_Target
			{
				// prepare and unpack data
				InputSurface surfInput;
				UNITY_INITIALIZE_OUTPUT(InputSurface, surfInput);
				surfInput.texcoord = input.texcoord;
				surfInput.worldPos = input.worldPos;

				float3 worldPos = input.worldPos;
#ifndef USING_DIRECTIONAL_LIGHT
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
#else
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;
#endif
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

#ifdef UNITY_COMPILER_HLSL
				SurfaceOutputStandard surfaceOutput = (SurfaceOutputStandard)0;
#else
				SurfaceOutputStandard surfaceOutput;
#endif
				surfaceOutput.Albedo = 0.0;
				surfaceOutput.Emission = _Emission;
				surfaceOutput.Alpha = 0.0;
				surfaceOutput.Occlusion = 1.0;

				fixed3 normalWorldVertex = fixed3(0, 0, 1);
				surfaceOutput.Normal = input.normal;
				normalWorldVertex = input.normal;

				// call surface function
				if (surf(surfInput, surfaceOutput) < 0.5)
					discard;

				// compute lighting & shadowing factor
				UNITY_LIGHT_ATTENUATION(atten, input, worldPos)
					fixed4 color = 0;

				// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
#if !defined(LIGHTMAP_ON)
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				gi.light.ndotl = LambertTerm(surfaceOutput.Normal, gi.light.dir);
#endif

				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = input.lmap;
#else
				giInput.lightmapUV = 0.0;
#endif

#if UNITY_SHOULD_SAMPLE_SH
				// 环境光
				giInput.ambient = input.sh;
				//giInput.ambient.rgb = 0.0;
#else
				giInput.ambient.rgb = 0.0;
#endif

				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif

#if UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
#endif

				LightingStandard_GI(surfaceOutput, giInput, gi);
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				// realtime lighting: call lighting function
				color += LightingStandard2(input, surfaceOutput, worldViewDir, gi);
				//color += LightingStandard(surfaceOutput, worldViewDir, gi);
				UNITY_APPLY_FOG(input.fogCoord, color); // apply fog
				return color;
			}
			ENDCG
		}

		Pass
		{
			Name "BACKWARD"
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
				ZTest LEqual
				ZWrite On
				Cull Back

				CGPROGRAM
#pragma vertex MainVS
#pragma fragment MainPS
#pragma target 3.0
#pragma multi_compile_fog
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"

#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

				uniform float4 _Color;
			uniform sampler2D _MainTex;

			uniform float _Emission;

			uniform float4 _TransparentColor;
			uniform float _TransparentDistance;
			uniform float _TransparentThickness;

			//#pragma surface surf Lambert
			struct InputSurface
			{
				float4 texcoord;
				float3 worldPos;
			};

			half surf(InputSurface input, inout SurfaceOutputStandard output)
			{
				half result = 1.0;
				half4 color = 0;
				if (input.worldPos.y <= _TransparentDistance)
				{
					result = -1.0;
				}
				else
				{
					half4 texColor = tex2D(_MainTex, input.texcoord) * _Color;
					float lerp = clamp((input.worldPos.y - _TransparentDistance) / _TransparentThickness, 0.0, 1.0);
					color = (1 - lerp) * _TransparentColor + lerp * texColor;
				}

				output.Albedo = output.Emission * color.rgb;
				output.Alpha = color.a;
				return result;
			}

			// vertex-to-fragment interpolation data
#ifdef LIGHTMAP_OFF
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 texcoord : TEXCOORD1;
				fixed3 normal : TEXCOORD2;
#if UNITY_SHOULD_SAMPLE_SH
				half3 sh : TEXCOORD3; // SH
#endif
				SHADOW_COORDS(4)
					UNITY_FOG_COORDS(5)
#if SHADER_TARGET >= 30
					float4 lmap : TEXCOORD6;
#endif
			};
#endif

#ifndef LIGHTMAP_OFF
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 texcoord : TEXCOORD1;
				fixed3 normal : TEXCOORD2;
				float4 lmap : TEXCOORD3;
				SHADOW_COORDS(4)
					UNITY_FOG_COORDS(5)
#ifdef DIRLIGHTMAP_COMBINED
					fixed3 tSpace0 : TEXCOORD6;
				fixed3 tSpace1 : TEXCOORD7;
				fixed3 tSpace2 : TEXCOORD8;
#endif
			};
#endif

			float4 _MainTex_ST;

			// vertex shader
			v2f MainVS(appdata_full v)
			{
				v2f output;
				UNITY_INITIALIZE_OUTPUT(v2f, output);
				output.pos = UnityObjectToClipPos(v.vertex);
				output.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
#if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
#endif
#if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
				output.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				output.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				output.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
#endif
				output.worldPos = worldPos;
				output.normal = worldNormal;

#ifndef DYNAMICLIGHTMAP_OFF
				output.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
#ifndef LIGHTMAP_OFF
				output.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

				// SH/ambient and vertex lights
#ifdef LIGHTMAP_OFF
#if UNITY_SHOULD_SAMPLE_SH
#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
				output.sh = 0.0;
#elif (SHADER_TARGET < 30)
				output.sh = ShadeSH9(float4(worldNormal, 1.0));
#else
				output.sh = ShadeSH3Order(half4(worldNormal, 1.0));
#endif
				// Add approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
				// 光照
				output.sh += Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, worldPos, worldNormal);
#endif
#endif
#endif // LIGHTMAP_OFF

				TRANSFER_SHADOW(output); // pass shadow coordinates to pixel shader
				UNITY_TRANSFER_FOG(output, output.pos); // pass fog coordinates to pixel shader
				return output;
			}

			// fragment shader
			fixed4 MainPS(v2f input) : SV_Target
			{
				// prepare and unpack data
				InputSurface surfInput;
				UNITY_INITIALIZE_OUTPUT(InputSurface, surfInput);
				surfInput.texcoord = input.texcoord;
				surfInput.worldPos = input.worldPos;

				float3 worldPos = input.worldPos;
#ifndef USING_DIRECTIONAL_LIGHT
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
#else
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;
#endif
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

#ifdef UNITY_COMPILER_HLSL
				SurfaceOutputStandard surfaceOutput = (SurfaceOutputStandard)0;
#else
				SurfaceOutputStandard surfaceOutput;
#endif
				surfaceOutput.Albedo = 0.0;
				surfaceOutput.Emission = _Emission;
				surfaceOutput.Alpha = 0.0;
				surfaceOutput.Occlusion = 1.0;

				fixed3 normalWorldVertex = fixed3(0, 0, 1);
				surfaceOutput.Normal = input.normal;
				normalWorldVertex = input.normal;

				// call surface function
				if (surf(surfInput, surfaceOutput) < 0.5)
					discard;

				// compute lighting & shadowing factor
				UNITY_LIGHT_ATTENUATION(atten, input, worldPos)
					fixed4 color = 0;

				// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
#if !defined(LIGHTMAP_ON)
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				gi.light.ndotl = LambertTerm(surfaceOutput.Normal, gi.light.dir);
#endif

				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = input.lmap;
#else
				giInput.lightmapUV = 0.0;
#endif

#if UNITY_SHOULD_SAMPLE_SH
				// 环境光
				giInput.ambient = input.sh;
#else
				giInput.ambient.rgb = 0.0;
#endif

				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif

#if UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
#endif

				LightingStandard_GI(surfaceOutput, giInput, gi);

				// realtime lighting: call lighting function
				color += LightingStandard(surfaceOutput, worldViewDir, gi);
				UNITY_APPLY_FOG(input.fogCoord, color); // apply fog
				//UNITY_OPAQUE_ALPHA(color.a);
				return color;
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
