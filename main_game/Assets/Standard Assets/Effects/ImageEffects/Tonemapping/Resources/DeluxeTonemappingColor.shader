Shader "Hidden/Deluxe/TonemapperColor"
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

				struct v2f 
				{
					half4 pos : SV_POSITION;
					half2 uv : TEXCOORD0;
				};

				v2f vert( appdata_img v ) 
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv =  v.texcoord.xy;
					return o;
				} 

				sampler2D _MainTex;

				float4 _Toe;
				float4 _Shoulder;
				float _K;
				float _Crossover;
				float4 _Tint;
				float _LuminosityWhite;

				float Tone(float x)
				{
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


				fixed4 frag(v2f i):COLOR
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
