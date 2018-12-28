Shader "DigitalSalmon.Fade/Blur" {
	Properties{
		[HideInInspector] _MainTex("Screen Texture", 2D) = "white" {}
		[HideInInspector] _Alpha("[Fade] Alpha", Float) = 0.5
		[HideInInspector] _Delta("[Fade] Delta", Float) = 0.5
		[MaterialToggle]_Invert("[Base] Invert]", Float) = 1
		_BaseColor("[Base] Color", Color) = (0,0,0,1)
		_Samples("[Blur] Samples", Float) = 3
		_MaximumBlurStrength("[Blur] Maximum Strength", Float) = 10
		_QualityLevel("[Blur] Quality Level", Float) = 0
	}
		SubShader{
			Cull Off ZWrite Off ZTest Always
			Pass
			{
			CGPROGRAM

			#pragma vertex ds_vert_img
			#pragma fragment effect

			#include "UnityCG.cginc"
			#include "DigitalSalmon.Fade.cginc"	

			uniform int _Samples;
			uniform float _MaximumBlurStrength;
			uniform int _QualityLevel;			

			fixed4 effect(ds_v2f i) : SV_Target 
			{			
				fixed4 col = fixed4(0,0,0,0);
				if (_QualityLevel <= 0) col = gaussian3(_MainTex, i.uv, _Delta * _MaximumBlurStrength * 0.01);
				if (_QualityLevel == 1) col = gaussian5(_MainTex, i.uv, _Delta * _MaximumBlurStrength * 0.01);
				if (_QualityLevel == 2) col = gaussian7(_MainTex, i.uv, _Delta * _MaximumBlurStrength * 0.01);
				if (_QualityLevel >= 3) col = gaussian9(_MainTex, i.uv, _Delta * _MaximumBlurStrength * 0.01);

				return lerp(col, _BaseColor, _Delta);
			}
			ENDCG
		}
		}
}