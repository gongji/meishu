Shader "Enviro/Blit" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudsTex ("Clouds (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		//Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
			
		Pass
		{
			Cull Off 
			ZWrite Off
			Ztest LEqual 

			//Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_ST;
			uniform half4 _MainTex_TexelSize;
			sampler2D _CloudsTex;
			half4 _CloudsTex_ST;
			sampler2D _CameraDepthTexture;
			half4 _CameraDepthTexture_ST;

			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			   float2 uv1 : TEXCOORD1;
			};
			
			v2f vert(appdata_img v)
			{
			   	v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
			#if UNITY_UV_STARTS_AT_TOP
				o.uv1 = v.texcoord.xy;
				if (_MainTex_TexelSize.y < 0)
				o.uv1.y = 1-o.uv1.y;
			#endif	

			   	return o;
			}
			
			float4 frag (v2f i) : COLOR
			{


			#if UNITY_UV_STARTS_AT_TOP
			float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _CameraDepthTexture_ST));
			#else
			float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _CameraDepthTexture_ST));
			#endif

			depthSample = Linear01Depth (depthSample);

			float4 main = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv,_MainTex_ST));

			#if UNITY_UV_STARTS_AT_TOP
			float4 clouds = tex2D(_CloudsTex, UnityStereoScreenSpaceUVAdjust(i.uv1,_MainTex_ST));
	    	#else
			float4 clouds = tex2D(_CloudsTex, UnityStereoScreenSpaceUVAdjust(i.uv,_MainTex_ST));
			#endif

			float4 final = main;

			if(depthSample > 0.9999)
			{
			//float blending = clamp(clouds.a*1.1,0.0,1.0);
			final = lerp(main,clouds, clouds.a);
			}

			return final;
			}
			
			ENDCG
		}
	} 
}
