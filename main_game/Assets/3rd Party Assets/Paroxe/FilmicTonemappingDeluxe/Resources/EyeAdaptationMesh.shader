// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Hidden/Deluxe/EyeAdaptation" {
Properties {
	_FrameTex ("Base (RGB) Trans (A)", 2D) = "black" {}
}

SubShader {

	Cull Off 
    ZWrite Off 
	Blend One One
	
	Pass // 0 histogram
	{  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl
			
			#include "UnityCG.cginc"

			float4x4 _FlareProj; 
			half _Intensity;


			struct v2f {
				float4 vertex : SV_POSITION;
			};

			sampler2D _FrameTex;
			sampler2D _BrightTexture;
			float _ValueRange;
			float _StepSize;
			float _BinCount;
			float4 _HistogramCoefs;

			inline float HistogramNormalizedIndex(float luminance)
			{
				 return log2(luminance) * _HistogramCoefs.x + _HistogramCoefs.y;
			}

			v2f vert (appdata_full v)
			{
				v2f o;

				half3 color = tex2Dlod(_FrameTex, float4(v.texcoord.xy,0,0) ).xyz;
				half luminance = max(color.r, max(color.g, color.b));

				luminance = clamp(luminance, _HistogramCoefs.z, _HistogramCoefs.w);
				float nIdx = HistogramNormalizedIndex(luminance);
				half stepCount = clamp(floor(nIdx * _BinCount),0, _BinCount-1);

#if SHADER_API_D3D9 
				// Dx9 half pixel offset
				o.vertex = float4(v.vertex.x + stepCount * _StepSize - _StepSize*0.5, v.vertex.y, v.vertex.z,1);
#else
				o.vertex = float4(v.vertex.x + stepCount * _StepSize, v.vertex.y, v.vertex.z,1);
#endif
				return o;

			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(1,0,0,0);
			}

		ENDCG
	}

	Blend SrcAlpha OneMinusSrcAlpha

	Pass // 1 Histogram analysis
	{  
	CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl

		#include "UnityCG.cginc"
		sampler2D _MainTex;
		sampler2D _HistogramTex;
		sampler2D _PreviousBrightness;

		float _90PixelCount;
		float _98PixelCount;

		float _ExposureOffset;

		float4 _Coefs;
		float4 _MinMaxSpeedDt;

		#define BIN_COUNT_INT 64
		#define BIN_COUNT_FLOAT 64.0

		inline float LuminanceFromBin(float normHistogramPosition)
		{
			return exp2((normHistogramPosition - _Coefs.y) / _Coefs.x);
		}

		fixed4 frag (v2f_img i) : SV_Target
		{
			float average = 0.0;
			float textureStep = 1.0 / BIN_COUNT_FLOAT;
			float idx = textureStep*0.1;
			float sum = 0;
			float countOver90 = 0;

			
			/*  // Branchless version
			for (int i = 0; i < BIN_COUNT_INT; i++)
			{
				float count =  tex2D(_HistogramTex, float2(idx,0.5) ).r;
				float lastSum = sum;
				sum += count;

				float binLuminance = LuminanceFromBin(i / BIN_COUNT_FLOAT);

				// case 0 : sum > _90PixelCount && lastSum < _90PixelCount
				float case0Validation = saturate(sum - _90PixelCount) * saturate(_90PixelCount - lastSum);
				float diff0 = sum - _90PixelCount;
				average += diff0 * binLuminance * case0Validation;
				countOver90 += diff0 * case0Validation;

				// case 1 : sum > _98PixelCount && lastSum < _98PixelCount
				float case1Validation = saturate(sum - _98PixelCount) * saturate(_98PixelCount - lastSum) * saturate(1-case0Validation);
				float diff1 = _98PixelCount - lastSum;
				average += diff1 * binLuminance * case1Validation;
				countOver90 += diff1 * case1Validation;

				// case 2 : sum > _90PixelCount && sum < _98PixelCount
				float case2Validation = saturate(sum - _90PixelCount)  * saturate(_98PixelCount - sum) *  saturate(1-case1Validation);
				average += count * binLuminance * case2Validation;
				countOver90 += count * case2Validation;


				idx += textureStep;
			}
			*/
			
			for (int i = 0; i < BIN_COUNT_INT; i++)
			{
				float count =  tex2D(_HistogramTex, float2(idx,0.5) ).r;
				float lastSum = sum;
				sum += count;

				if (sum > _90PixelCount && lastSum < _90PixelCount)
				{
					float diff = sum - _90PixelCount;
					average += diff * LuminanceFromBin(i / BIN_COUNT_FLOAT);
					countOver90 += diff;
				}
				else if (sum > _98PixelCount && lastSum < _98PixelCount)
				{
					float diff = _98PixelCount - lastSum;
					average += diff * LuminanceFromBin(i / BIN_COUNT_FLOAT);
					countOver90 += diff;
				}
				else if (sum > _90PixelCount && sum < _98PixelCount)
				{
					average += count * LuminanceFromBin(i / BIN_COUNT_FLOAT);
					countOver90 += count;
				}

				idx += textureStep;
			}
			
			float previousBrightness = tex2D(_PreviousBrightness, float2(0.5,0.5) ).r;
			float targetBrightness = clamp((average/countOver90) + _ExposureOffset,0,65000);

			float delta = targetBrightness - previousBrightness;
			float speed = (delta > 0) ? _MinMaxSpeedDt.z : _MinMaxSpeedDt.w;
			float mul = 1.0f - exp2(-speed);
			float currentBrightness =  mul * delta + previousBrightness;

			return float4( clamp(currentBrightness, _MinMaxSpeedDt.x, _MinMaxSpeedDt.y),0,0,1);
		}

	ENDCG
	}


	Pass // 2 Downscale
	{  
	CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma target 3.0
		#pragma glsl

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


}

}
