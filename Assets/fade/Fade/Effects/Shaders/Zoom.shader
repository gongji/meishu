Shader "DigitalSalmon.Fade/Zoom" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5	
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Anchor("[Zoom] Anchor", Vector) = (0.5,0.5,0,0)	
		_MaximumStrength("[Zoom] Maximum Strength", Float) = 1
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

			uniform float4 _Anchor;
			uniform float _MaximumStrength;

			fixed4 effect(ds_v2f i) : SV_Target
			{		
				float2 domain = i.uv;

				domain -= _Anchor.xy;
				domain *= max(0.0000001,1-(_Delta*_MaximumStrength));
				domain += _Anchor.xy;

				fixed4 col = tex2D(_MainTex, domain);

				return lerp(col, _BaseColor, _Delta);
			}
			ENDCG
		}
	}
}
