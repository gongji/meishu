Shader "DigitalSalmon.Fade/Star" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		[MaterialToggle]_Invert("[Base] Invert]", Float) = 1
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Anchor("[Star] Anchor", Vector) = (0.5,0.5,0,0)
		_Softness("[Star] Softness", Float) = 1
		_StarSides("[Star] Sides", Float) = 1
		_Angle("[Star] Angle", Float) = 0
		_Index("[Star] Indent", Float) = 0.1
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
			uniform int _StarSides;
			uniform float _Angle;
			uniform float _Index;

			fixed4 effect(ds_v2f i) : SV_Target
			{
				REMAP_SOFTNESS

				_StarSides = max(5, _StarSides);

				float2 domain = i.uv; 
					
				domain -= _Anchor.xy;
				domain.x *= _ScreenParams.x/ _ScreenParams.y;	
				domain = rotateAround(domain, 0, _Angle);								
				
				if (_Invert) _Delta = 1 - _Delta;

				float delta = remapDelta(_Delta*1.7, _Softness);				
				
				float field = sdStar(domain, _StarSides, _Index);

				float radialMask = smoothstep(delta + (_Softness), delta - (_Softness), field);

				fixed4 col = tex2D(_MainTex, i.uv);

				if (_Invert) radialMask = 1 - radialMask;

				return lerp(col, _BaseColor, radialMask );
			}
			ENDCG
		}
	}
}
