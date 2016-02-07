Shader "Hidden/Deluxe/Tonemapper"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_Toe ("Toe", Vector) = (0.30, 0.59, 0.11, 1.0)
		_Shoulder ("Shoulder", Vector) = (0.30, 0.59, 0.11, 1.0)
		_K ("K", Float) = 0.2
		_Crossover ("Crossover", Float) = 0.2
		_Tint ("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 
				#include "UnityCG.cginc"

				sampler2D _MainTex;

				float4 _Toe;
				float4 _Shoulder;
				float _K;
				float _Crossover;
				float4 _Tint;

				float Tone(float x)
				{
					x *= 1;

					float4 data;
					float endAdd;

					if (x > _Crossover)
					{
						data = _Shoulder;
						endAdd = _K;
					}
					else
					{
						data = _Toe;
						endAdd = 0;
					}


					float2 numDenum = data.xy * x + data.zw;
					return numDenum.x / numDenum.y + endAdd;
				}

				fixed4 frag(v2f_img i):COLOR
				{
					float4 color = tex2D(_MainTex, i.uv);
					color.x = Tone(color.x);
					color.y = Tone(color.y);
					color.z = Tone(color.z);
					return color * _Tint;
				}

			ENDCG
		}
	}

	FallBack off
}
