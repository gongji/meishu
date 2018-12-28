Shader "Enviro/CurlNoise" {
	Properties {
		curl_scale ("curl scale", Range(0,50)) = 3
		curl_low ("curl low", Range(-10,10)) = -1
		curl_high ("curl_high", Range(-10,10)) = 3
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass {
		CGPROGRAM
	    #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        #include "/Core/EnviroNoiseCore.cginc"
		#pragma target 3.5

		sampler2D _MainTex;

		   struct VertexInput {
  				half4 vertex : POSITION;
 				float2 uv : TEXCOORD0;	
            };

            struct VertexOutput {
           		float4 position : SV_POSITION;
 				float2 uv : TEXCOORD0;
            };

            VertexOutput vert (VertexInput v) {
 			 	VertexOutput o;
 				o.position = UnityObjectToClipPos(v.vertex);				
 				o.uv = v.uv;
 				return o;
            }
 		
 			float curl_scale = 3.0;
			float curl_low = -0.5;
			float curl_high = 3.0;

			float set_range(float value, float low, float high) {
							return saturate((value - low)/(high - low));
			}

			float3 set_ranges_signed(float3 values, float low, float high) {
				return (values - low)/(high - low);
}

 			float4 frag(VertexInput input) : SV_Target {

 			float3 xyz = float3(input.uv.xy, 0);
		    float3 curl_values = curl_noise(xyz * curl_scale);

		    curl_values = set_ranges_signed(curl_values, curl_low, curl_high);
					
			return float4(encode_curl(curl_values), 0);
			}

	ENDCG
	}
	}
	FallBack "Diffuse"
}
