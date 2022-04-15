Shader "Custom/Bloom"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{

		Cull Off
		ZWrite Off
		ZTest Always

		CGINCLUDE
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			sampler2D _SourceTex;

			float _DeltaDownsample;
			float _DeltaUpsample;
			float _Intensity;

			half4 _Filter;

			struct VertexData {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varying {
				float4 pos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			Varying Vert(VertexData v) {
				Varying i;
				i.pos = UnityObjectToClipPos(v.vertex);
				i.texcoord = v.uv;
				return i;
			}

			half3 Sample(float2 uv) {
				return tex2D(_MainTex, uv).rgb;
			}

			half3 SampleBox(float2 uv, float delta) {
				float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
				half3 s = Sample(uv + o.xy) + Sample(uv + o.zy) + Sample(uv + o.xw) + Sample(uv + o.zw);
				return s * 0.25;
			}

			half3 Prefilter(half3 c) {
				half brightness = max(c.r, max(c.g, c.b));
				half soft = brightness - _Filter.r;
				soft = clamp(soft, 0, _Filter.z);
				soft = soft * soft * _Filter.w;
				half con = max(soft, brightness - _Filter.x);
				con /= max(brightness, 0.000001);
				return c * con;
			}
		ENDCG

		Pass{ //0
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				half4 Frag(Varying i) : SV_Target {
					return half4(Prefilter(SampleBox(i.texcoord, _DeltaDownsample)), 1);
				}
			ENDCG
		}
		Pass{ //1
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				half4 Frag(Varying i) : SV_Target {
					return half4(SampleBox(i.texcoord, _DeltaDownsample), 1);
				}
			ENDCG
		}
		Pass{ //2
			Blend One One
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				half4 Frag(Varying i) : SV_Target {
					return half4(SampleBox(i.texcoord, _DeltaUpsample), 1);
				}
			ENDCG
		}
		Pass{ //3
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				half4 Frag(Varying i) : SV_Target {
					half4 c = tex2D(_SourceTex, i.texcoord);
					c.rgb += SampleBox(i.texcoord, _DeltaUpsample) * _Intensity;
					return c;
				}
			ENDCG
		}
			
	}
}