Shader "DigitalSalmon.Fade/Clock" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		[MaterialToggle]_Invert("[Band] Invert]", Float) = 1
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Anchor("[Clock] Anchor", Vector) = (0.5,0.5,0,0)
		_Angle("[Clock] Initial Angle (Degrees)", Float) = 0
		_Softness("[Clock] Softness", Float) = 1		
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

			uniform fixed _Invert;
			uniform float4 _Anchor;
			uniform float _Angle;
			uniform float _Softness;

			fixed4 effect(ds_v2f i) : SV_Target 
			{			
				REMAP_SOFTNESS

				float2 domain = i.uv; 

				domain -= _Anchor.xy;				
				domain.x *= _ScreenParams.x / _ScreenParams.y;					
				domain = rotateAround(domain, 0, _Angle + 90);				

				float field = radial01(domain);		

				if (_Invert) field = 1 - field;

				float delta = remapDelta(_Delta, _Softness);

				float clockMask = smoothstep(delta - _Softness, delta + _Softness, field);

				fixed4 col = tex2D(_MainTex, i.uv);

				return lerp(col, _BaseColor, 1-clockMask);

				return field;



			}
			ENDCG
		}
	}
}
