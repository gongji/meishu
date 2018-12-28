Shader "DigitalSalmon.Fade/Pinch" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5	
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Anchor("[Pinch] Anchor", Vector) = (0.5,0.5,0,0)		
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

			fixed4 effect(ds_v2f i) : SV_Target
			{		
				float2 domain = i.uv;

				domain -= 0.5;
				domain -= _Anchor.xy;
				domain /= 1-_Delta;
				domain += _Anchor.xy;
				domain += 0.5;

				float mask = (domain.x > 0) && (domain.y > 0) && (domain.x < 1) && (domain.y < 1);

				fixed4 col = tex2D(_MainTex, domain);

				return lerp(_BaseColor, col, mask);
			}
			ENDCG
		}
	}
}
