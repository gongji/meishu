Shader "DigitalSalmon.Fade/Fade" {
	Properties 	{
		[HideInInspector] _MainTex ("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5	
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
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

			fixed4 effect(ds_v2f i) : SV_Target
			{							
				fixed4 col = tex2D(_MainTex, i.uv);
				return lerp(col, _BaseColor, _Delta);
			}
			ENDCG
		}
	}
}
