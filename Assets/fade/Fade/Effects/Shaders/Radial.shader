Shader "DigitalSalmon.Fade/Radial" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		[MaterialToggle]_Invert("[Base] Invert]", Float) = 1
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Anchor("[Radial] Anchor", Vector) = (0.5,0.5,0,0)
		_Softness("[Radial] Softness", Float) = 1
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

			uniform float _Softness;
			uniform float4 _Anchor;
			uniform fixed _Invert;

			fixed4 effect(ds_v2f i) : SV_Target
			{	
				REMAP_SOFTNESS				

				float2 domain = i.uv; 

				domain -= 0.5;				
				domain.x *= _ScreenParams.x/ _ScreenParams.y;				
				domain += 0.5;

				domain -= _Anchor.xy;
				
				if (_Invert) _Delta = 1 - _Delta;

				float delta = remapDelta(_Delta/2, _Softness);
				
				float boost = length(_Anchor.xy-0.5) * 2;

				float field = sdSphere(domain, delta*(1.1 + boost));

				float radialMask = smoothstep(delta + (_Softness), delta - (_Softness), field);

				fixed4 col = tex2D(_MainTex, i.uv);

				if (_Invert) radialMask = 1 - radialMask;

				return lerp(col, _BaseColor, radialMask );
			}
			ENDCG
		}
	}
}
