Shader "DigitalSalmon.Fade/BarnDoor" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Tiling("[BarnDoor] Tiling", Float) = 1
		_SliceAngle("[BarnDoor] Slice Angle (Degrees)", Float) = 0	
		_DomainStretch("[BarnDoor] Domain Stretch", Float) = 0
		_OffsetDirection("[Offset] Direction", Vector) = (0,0,0,0)
		[MaterialToggle] _Normalize("[Offset] Normalize Direction", Float) = 0				
				
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
			uniform float _SliceAngle;
			uniform float _DomainStretch;
			uniform float4 _OffsetDirection;
			uniform fixed _Normalize;			

			fixed4 effect(ds_v2f i) : SV_Target
			{					
				float2 domain = i.uv; 
				domain -= 0.5;
				domain = rotateAround(domain, 0, _SliceAngle);
				domain += 0.5;

				float doorMask = domain.x > 0.5;	

				float2 offsetDirection = _OffsetDirection;
				if (_Normalize) offsetDirection = normalize(offsetDirection);

				float delta = _Delta/2;
				delta *= 1+_DomainStretch;

				float2 offsetDomain = lerp(i.uv + (offsetDirection*delta), i.uv - (offsetDirection*delta), doorMask);

				float doorCentre = domain >= 0.5+ delta || domain <= 0.5- delta;
				return lerp (_BaseColor, tex2D(_MainTex, offsetDomain), doorCentre);

			}
			ENDCG
		}
	}
}
