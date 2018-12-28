Shader "DigitalSalmon.Fade/Rubicks" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Tiling("[Band] Tiling", Float) = 1
		_Angle("[Band] Angle (Degrees)", Float) = 0	
		_DomainStretch("[Band] Domain Stretch", Float) = 0
		_Amplitude("[Noise] Amplitude", Float) = 1

	}
	SubShader {
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			CGPROGRAM
			#pragma vertex ds_vert_img
			#pragma fragment effect

			#include "UnityCG.cginc"
			#include "DigitalSalmon.Fade.cginc"	
			#include "DigitalSalmon.Simplex.cginc"	

			float _Tiling;
			float _Angle;
			float _DomainStretch;
			float _Amplitude;

			fixed4 effect(ds_v2f i) : SV_Target
			{		

				float2 domain = i.uv; 

				domain = rotateAround(domain, 0.5, _Angle);

				float band = floor(domain * _Tiling)/_Tiling;

				float2 offset = float2(sin(_Angle*DEG2RAD), cos(_Angle*DEG2RAD));
				float offsetNoiseStrength = (simplex(band*172.2) + 1) / 2;
				offsetNoiseStrength *= abs(_Amplitude);

				float delta = _Delta * _DomainStretch;
				delta -= offsetNoiseStrength;

				
				float2 sampleUv = i.uv + offset * max(0, delta);

				float mask = (sampleUv.x > 0) && (sampleUv.y > 0) && (sampleUv.x < 1) && (sampleUv.y < 1);
				
				return lerp(_BaseColor,tex2D(_MainTex, sampleUv), mask);
			}
			ENDCG
		}
	}
}
