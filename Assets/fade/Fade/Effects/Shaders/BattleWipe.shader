Shader "DigitalSalmon.Fade/BattleWipe" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5	
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Angle("[Wipe] Angle", Float) = 1
		_Frequency("[Wipe] Frequency", Float) = 1
		_Amplitude("[Wipe] Amplitude", Float) =1
		_Softness("[Wipe] Softness", Float) = 1
		_DomainStretch("[Domain] Stretch", Float) = 1
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

			uniform float _Angle;
			uniform float _Frequency;
			uniform float _Amplitude;
			uniform float _Softness;
			uniform float _DomainStretch;

			fixed4 effect(ds_v2f i) : SV_Target
			{
				REMAP_SOFTNESS

				_Amplitude *= 0.01;
				 
				float2 domain = i.uv;
				domain = rotateAround(domain, 0.5, _Angle);

				float offset = _Amplitude * simplex((domain.y * _Frequency)-_Frequency/2);
				float delta = remapDelta(_Delta + offset, _Softness + (_Amplitude) +_DomainStretch);
				

				float mask = smoothstep(delta - _Softness, delta + _Softness, i.uv.x);


				fixed4 col = tex2D(_MainTex, i.uv);

				return lerp(_BaseColor, col, mask);
			}
			ENDCG
		}
	}
}
