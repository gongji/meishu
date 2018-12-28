Shader "Enviro/CloudsShadows" {
    Properties {
		_MainTex("Cloud Map", 2D) = "white" {}
        _ShadowStrength ("Clouds Shadow Strength", Float ) = 0.5
      	[HideInInspector]_Offset ("Offset", Float ) = 1

    }
    SubShader {
        Tags { "RenderType"="Transparent" "PerformanceChecks"="False" }
		LOD 300
		Pass {
            Name "ShadowCaster"
			Tags 
			{ 
				"LightMode" = "ShadowCaster" 
			}

			Cull Front ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster


           	#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityInstancing.cginc"
			#include "UnityStandardConfig.cginc"
			#include "UnityStandardUtils.cginc"
            
          	#pragma multi_compile_shadowcaster
            #pragma target 3.0

            sampler2D _MainTex;
			float4 _MainTex_ST;
            sampler3D _DitherMaskLOD;
            half _ShadowStrength;

            struct VertexInput {
                float4 vertex	: POSITION;
				float3 normal	: NORMAL;
				float2 uv0		: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput {
                V2F_SHADOW_CASTER_NOPOS
                float4 posWorld : TEXCOORD1;
				float2 uv : TEXCOORD2;
            }; 

            VertexOutput vertShadowCaster (VertexInput v,out float4 opos : SV_POSITION) {
            
				VertexOutput o;
         	    UNITY_SETUP_INSTANCE_ID(v);
                TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.uv = v.uv0;
                return o;
            }
            float4 fragShadowCaster(VertexOutput i, UNITY_VPOS_TYPE vpos : VPOS) : SV_TARGET {
                
				float2 worldUV = i.posWorld.xz * 0.00001 + 0.5;
            	float4 cloudTexture = tex2D(_MainTex, worldUV);

            	float cloudMorph = cloudTexture.r * 10;
            	//cloudMorph = clamp(cloudMorph,0,0.2);
            	cloudMorph = lerp(0,1,cloudMorph);

            	float alpha = smoothstep(0.01,0.6,cloudMorph);
            	alpha = saturate(alpha * _ShadowStrength);

            	float alphaRef = cloudMorph * tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,alpha*0.9375)).a;
				clip (alphaRef - 0.01);

				SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }


    }
}
