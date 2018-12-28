Shader "DigitalSalmon/Sky" {
   Properties {
		
		// Sun
        _SunColor							("[Sun] Color", Color) = (1,1,1,1)
        _SunStrength						("[Sun] Strength", Float ) = 2
		_SunSize							("[Sun] Size", Float ) = 1
        _SunSoftness						("[Sun] Softness", Float ) = 3

		// Sky
        _SkyZenithColor						("[Sky] Zenith color", Color) = (0,0.5019608,1,1)
        _SkyAzimuthColor					("[Sky] Azimuth Color", Color) = (0.682353,0.8352942,0.9254903,1)
        _SkyNadirColor						("[Sky] Nadir Color", Color) = (0.1372549,0.1372549,0.1607843,1)

        _SkyUpperExponent					("[Sky] Upper Exponent", Float ) = 2
		_SkyUpperMultiplier					("[Sky] Upper Multiplier", Float ) = 1
		
		_SkyLowerExponent					("[Sky] Lower Exponent", Float ) = 6
        _SkyLowerMultiplier					("[Sky] Lower Multiplier", Float ) = 0.8
       
		// Halo
        _HaloColor							("[Halo] Color", Color) = (1,1,1,1)
        _HaloSize							("[Halo] Size", Float ) = 15
        _HaloSoftness						("[Halo] Softness", Float ) = 20
        _HaloAzimuthExponent						("[Halo] Exponent", Float ) = 3
        _HaloStrength						("[Halo] Strength", Float ) = 0.5

		// Horizon Halo
        _HorizonHaloHorizonHaloColor		("[HorizonHalo] Horizon Halo Color", Color) = (1,1,1,1)
        _HorizonHaloHorizonHaloSize			("[HorizonHalo] Horizon Halo Size", Float ) = 0.5
        _HorizonHaloHorizonHaloExponent		("[HorizonHalo] Horizon Halo Exponent", Float ) = 0.5
        _HorizonHaloHorizonHaloIntensity	("[HorizonHalo] Horizon Halo Intensity", Float ) = 0.2 

		// Debug
		[Enum(All,0, Sun,1, Sky,2, Halo,3, HorizonHalo,4)] _DebugView("[Debug] View Pass", Float) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Background"
            "RenderType"="Opaque"
            "PreviewType"="Skybox"
        }
        Pass {
			ZWrite Off
			Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
          
            #include "UnityCG.cginc"

            #pragma target 3.0

			// Directional light
            uniform float4 _LightColor0;

			// Sun
			uniform float _SunSize;
            uniform float _SunSoftness;
            uniform float4 _SunColor;
            uniform float _SunStrength;

			// Sky
            uniform float4 _SkyZenithColor;
            uniform float4 _SkyAzimuthColor;
            uniform float4 _SkyNadirColor;
            uniform float _SkyUpperExponent;
            uniform float _SkyLowerExponent;
			uniform float _SkyUpperMultiplier;
            uniform float _SkyLowerMultiplier;
				
			// Halo
			uniform float _HaloStrength;
            uniform float4 _HaloColor;
            uniform float _HaloSize;
			uniform float _HaloSoftness;
            uniform float _HaloAzimuthExponent;

			// Horizon Halo
			uniform float _HorizonHaloHorizonHaloSize;
            uniform float _HorizonHaloHorizonHaloExponent;
            uniform float4 _HorizonHaloHorizonHaloColor;
            uniform float _HorizonHaloHorizonHaloIntensity;
			
			// Developer
			uniform int _DebugView;


			// Custom smoothing sampling distributed amongst the sign threshold.
			float sampleSmoothSigned(float field, float smoothing) {
				smoothing /= 100;
				return smoothstep(smoothing / 2, -smoothing / 2, field);
			}
			// Custom smoothing sampling.
			float sampleSmooth(float field, float smoothing) {
				smoothing /= 100;
				return smoothstep(0, -smoothing, field);
			}

			// 1D Radius
			float sdSphere(float domain, float radius) {
				return length(domain) - radius;
			}
            float BottomLerpAlpha( float x , float expo ){
            return clamp((1-pow((1-(clamp(x,-100,0)*-1)),expo)),0,1);
            }
            
            float TopLerpAlpha( float x , float expo ){
            return 1-clamp(pow((1-clamp(x,0,100)),expo),0,1);
            }
            
            float SS_Sun( float x , float size , float softness ){
            float normSize = pow(size * 0.01,6);
            softness = abs(softness);
            return (1-(smoothstep(normSize - (normSize*softness), normSize + (normSize*softness), x)));
            }
            
            
            float SS_Halo( float x , float size , float softness ){
            softness = abs(softness);
            size = size * 0.01;
            return 1-smoothstep(size - ((softness*size)/4), size + ((softness*size)), 1-x);
            }
            
            
            float SS_TopBotMult( float x ){
            return smoothstep(-0.003,0.003,x);
            }
            
        
            float SS_HorizonHalo( float x , float threshold ){
            return 1-smoothstep(-threshold,threshold,x);
            }
            
            
            float SS_SunBrightness( float sun , float brightness ){
            return sun*brightness;
            }
            
			// Vertex Input
            struct VertexInput {
                float4 vertex : POSITION;
            };

			// Vertex Output
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
            };

			// Vertex Program
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }

			// Fragment Program
            float4 frag(VertexOutput i) : COLOR {
				
				const float TOP_BOTTOM_ALPHA_SMOOTHING = 2;

				// Unity variables.
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
				float sunField = clamp(0,1,1-dot(viewDirection, -lightDirection));
				
				// Sky field.
                float heightField = dot(viewDirection,float3(0,-1,0));

				float topLerpAlpha = TopLerpAlpha(heightField, _SkyUpperExponent);
				float bottomLerpAlpha = BottomLerpAlpha(heightField, _SkyLowerExponent);
				float topBottomAlpha = 1-sampleSmoothSigned(heightField, TOP_BOTTOM_ALPHA_SMOOTHING);

				float3 skyTopColor = lerp(_SkyAzimuthColor, _SkyZenithColor, topLerpAlpha);
				float3 skyColor = lerp (skyTopColor, _SkyNadirColor, bottomLerpAlpha);

				float topBottomMult = lerp (_SkyLowerMultiplier, _SkyUpperMultiplier, topBottomAlpha);

				skyColor *= topBottomMult;

				if (_DebugView == 2) return float4(skyColor,1);

				// Sun
				float sunMask = sampleSmooth(sdSphere(sunField, _SunSize / 100), _SunSoftness);
				float3 sunColor = sunMask * _SunColor * _SunStrength;

				if (_DebugView == 1) return float4(sunColor,1);

				sunColor *= topBottomAlpha;
			

				// Horizon Halo.

				float horizonHaloField = abs(heightField) - _HorizonHaloHorizonHaloSize;
				float horizonMask = 1-pow(1-sampleSmooth(horizonHaloField, 100 * _HorizonHaloHorizonHaloSize), _HorizonHaloHorizonHaloExponent);;
				float3 horizonColor = horizonMask * _HorizonHaloHorizonHaloColor * _HorizonHaloHorizonHaloIntensity;
			
				if (_DebugView == 4) return float4(horizonColor, 1);


				// Halo

				// Get a trigonometrically remapped height field. 0 -> 1.
				float horizonField = abs(sin((heightField/2)*3.14159));
				// Apply a power curve.
				horizonField = 1-pow(1-horizonField, _HaloAzimuthExponent);

				float haloSmoothing = lerp(_HaloSoftness * 4, _HaloSoftness, horizonField);
				float haloSize = lerp(_HaloSize*3, _HaloSize, horizonField);
				float sizeMul = 1.0 / 100;
				float haloField = sdSphere(sunField, haloSize * sizeMul);				
				
				float haloMask = sampleSmooth(haloField, haloSmoothing);

				//haloMask *= topBottomMult;
				haloMask *= _HaloStrength;
				float3 haloColor = haloMask * _HaloColor;
//
				if (_DebugView == 3) return float4(haloColor,1);// * _HaloStrength; 

				haloColor *= topBottomAlpha;

				

				return float4(skyColor + horizonColor + sunColor + haloColor, 1);

            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
