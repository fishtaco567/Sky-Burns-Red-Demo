// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Screen"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_LightTex("Light Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Blend("Blend", Range(0, 1)) = 0
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Shift("Shift", Vector) = (0, 0, 0, 0)
	}

	SubShader
	{
		Tags
		{ 
			"IgnoreProjector"="True" 
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			float _Blend;
			float4 _Shift;
			static const float PI = 3.14159265f;

			float3 rgb2hsv(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 hsv2rgb(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

			v2f vert(appdata_t IN, out float4 outpos : SV_POSITION)
			{
				v2f OUT;
				IN.vertex.y += 0.0625;
				IN.vertex.x = floor(IN.vertex.x * 8) / 8;
				IN.vertex.y = floor(IN.vertex.y * 8) / 8;
				outpos = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _LightTex;
			float _AlphaSplitEnabled;

			fixed4 frag(v2f IN, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float2 offset = float2(0, 0);

				//Screen Shake
				offset += float2(_Shift.x / 32, _Shift.y / 18);
				fixed4 c = lerp(tex2D(_MainTex, IN.texcoord + offset), IN.color, _Blend);

				fixed4 light = tex2D(_LightTex, IN.texcoord + offset);
				c.rgb *= float3(1, 1, 1) + light.rgb;

				return c;
			}
		ENDCG
		}
	}
}