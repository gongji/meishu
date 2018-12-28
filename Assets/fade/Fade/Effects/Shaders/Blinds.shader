Shader "DigitalSalmon.Fade/Blinds" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Tiling("[Band] Tiling", Float) = 1
		_Angle("[Band] Angle (Degrees)", Float) = 0
		_Softness("[Band] Softness", Float) = 1		
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

			uniform float _Tiling;
			uniform float _Angle;
			uniform float _Softness;

			fixed4 effect(ds_v2f i) : SV_Target
			{		
				REMAP_SOFTNESS

				float2 domain = i.uv; 
				domain -= 0.5;
				domain.x *= _ScreenParams.x/ _ScreenParams.y;
				domain = rotateAround(domain, 0, _Angle);

				float field = frac((domain.x * _Tiling) * 2);					
				float delta = remapDelta(_Delta/2, _Softness);
				float blindMask = opUnion(smoothstep(0.5 - delta - _Softness, 0.5 - delta, field), smoothstep(0.5 + delta+ _Softness, 0.5 + delta, field));

				return lerp(tex2D(_MainTex, i.uv), _BaseColor, blindMask);			
			}
			ENDCG
		}
	}
}
