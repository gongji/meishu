Shader "DigitalSalmon.Fade/BandWipe" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Tiling("[Band] Tiling", Float) = 1
		_Angle("[Band] Angle (Degrees)", Float) = 0
		_Softness("[Band] Softness", Float) = 1		
		_DomainStretch("[Band] Domain Stretch", Float) = 0
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

			float _Tiling;
			float _Angle;
			float _Softness;
			float _DomainStretch;

			fixed4 effect(ds_v2f i) : SV_Target
			{		
				REMAP_SOFTNESS

				float2 domain = i.uv; 
				domain -= 0.5;
				domain.x *= _ScreenParams.x/ _ScreenParams.y;
				domain = rotateAround(domain, 0, _Angle);
				domain += 0.5;

				float band = floor(frac((domain * _Tiling) * 2)*2);
				float bandDomain = lerp(domain.y, 1 - domain.y, band);

				float delta = remapDelta(_Delta, _Softness + _DomainStretch);
				float mask = smoothstep(delta + _Softness, delta - _Softness, bandDomain);

				return lerp(tex2D(_MainTex, i.uv), _BaseColor, mask);
			}
			ENDCG
		}
	}
}
