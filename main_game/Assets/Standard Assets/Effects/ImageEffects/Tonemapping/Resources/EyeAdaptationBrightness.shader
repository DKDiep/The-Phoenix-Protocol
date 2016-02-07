Shader "Hidden/Deluxe/EyeAdaptationBright"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Pass // 0
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _BrightnessTex;
				sampler2D _ColorTex;
				
				float _BrightnessMultiplier;

							struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert( appdata_img v ) 
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.texcoord.xy;
					return o;
				} 

				float4 frag(v2f i):COLOR
				{
					float4 color = tex2D(_ColorTex, i.uv);
					return color * 1/tex2D(_BrightnessTex, float2(0.5,0.5)).r * _BrightnessMultiplier;
				}

			ENDCG
		}

		Pass // 1
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"
				sampler2D _UpTex;

				float4 _PixelSize;

				fixed4 frag (v2f_img i) : SV_Target
				{

					float2 UV[4];

					UV[0] = i.uv + float2(-1.0 * _PixelSize.x, -1.0 * _PixelSize.y);
					UV[1] = i.uv + float2( 1.0 * _PixelSize.x, -1.0 * _PixelSize.y);
					UV[2] = i.uv + float2(-1.0 * _PixelSize.x,  1.0 * _PixelSize.y);
					UV[3] = i.uv + float2( 1.0 * _PixelSize.x,  1.0 * _PixelSize.y);

				
					fixed4 Sample[4];

					for(int j = 0; j < 4; ++j)
					{
						Sample[j] = tex2D(_UpTex, UV[j]);
					}

					return (Sample[0] + Sample[1] + Sample[2] + Sample[3]) * 1.0/4;
				}

			ENDCG
		}

	Pass // 2
	{  
	CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma target 3.0


		#include "UnityCG.cginc"

		sampler2D _Histogram;
		sampler2D _ColorTex;

		float _LuminanceRange;
		float4 _HistogramCoefs;
		float4 _MinMax;
		float _TotalPixelNumber;

		inline float HistogramNormalizedIndex(float luminance)
		{
			return log2(luminance) * _HistogramCoefs.x + _HistogramCoefs.y;
		}

		fixed4 frag (v2f_img i) : SV_Target
		{
			// Find number of pixels from current bin
			float len = _MinMax.y - _MinMax.x;
			float x = clamp(i.uv.x - _MinMax.x, 0, len);
			float normalizedPos = x / len;
			float histogramPosition = normalizedPos;//HistogramNormalizedIndex(normalizedPos * _LuminanceRange);
			float nbPixels = tex2D(_Histogram, float2(histogramPosition, 0.5)).r;

			// determine normalized height from number of pixels
			float normalizedHeight = nbPixels / _TotalPixelNumber;

			// Find current normalized height
			float lenY = _MinMax.w - _MinMax.z;
			float y = clamp( i.uv.y - _MinMax.z, 0, lenY);
			float normalizedPosY = y / lenY;

			//return float4(normalizedHeight,0.0,0.0,1.0);

			normalizedHeight = pow(normalizedHeight, 1.0/2.0);

			float4 sceneColor = float4(tex2D(_ColorTex, i.uv.xy).xyz, 1.0);
			if (x <= 0.0001 || normalizedPos > 0.9999)
				return sceneColor;

			if (y <= 0.0001 || normalizedPosY > 0.9999)
				return sceneColor;

			if (normalizedPosY > normalizedHeight)
				return float4(0.5,0.5,0.5,1.0) * 0.5 + sceneColor*0.5;


			return float4(0.9,0.9,0.9,1.0);

		}

	ENDCG
	}
	}

	FallBack off
}

