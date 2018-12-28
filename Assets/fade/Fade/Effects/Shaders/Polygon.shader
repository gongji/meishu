Shader "DigitalSalmon.Fade/Polygon" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		[MaterialToggle]_Invert("[Base] Invert]", Float) = 1
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Anchor("[Polygon] Anchor", Vector) = (0.5,0.5,0,0)
		_Softness("[Polygon] Softness", Float) = 1
		_PolygonSides("[Polygon] Sides", Float) = 1
		_Angle("[Polygon] Angle", Float) = 0
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
			uniform int _PolygonSides;
			uniform float _Angle;

			fixed4 effect(ds_v2f i) : SV_Target
			{
				REMAP_SOFTNESS

				_PolygonSides = max(3, _PolygonSides);

				float2 domain = i.uv; 

					
				domain -= _Anchor.xy;
				domain.x *= _ScreenParams.x/ _ScreenParams.y;	
				domain = rotateAround(domain, 0, _Angle);
				
				
				
				if (_Invert) _Delta = 1 - _Delta;

				float delta = remapDelta(_Delta/1.5, _Softness);
				
				float boost = length(_Anchor.xy-0.5) * 2;

				float field = sdNgon(domain, _PolygonSides, delta*(1.1 + boost));

				float radialMask = smoothstep(delta + (_Softness), delta - (_Softness), field);

				fixed4 col = tex2D(_MainTex, i.uv);

				if (_Invert) radialMask = 1 - radialMask;

				return lerp(col, _BaseColor, radialMask );
			}
			ENDCG
		}
	}
}
