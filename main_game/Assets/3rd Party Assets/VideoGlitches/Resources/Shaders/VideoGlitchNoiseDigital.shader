///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Video Glitch Noise Digital.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchNoiseDigital"
{
  // http://unity3d.com/support/documentation/Components/SL-Properties.html
  Properties
  {
    _MainTex("Base (RGB)", 2D) = "white" {}

	// Amount of the effect (0 none, 1 full).
    _Amount("Amount", Range(0.0, 1.0)) = 1.0
  }

  CGINCLUDE
  #include "UnityCG.cginc"
  #include "VideoGlitchCG.cginc"

  /////////////////////////////////////////////////////////////
  // BEGIN CONFIGURATION REGION
  /////////////////////////////////////////////////////////////

  // Define this to change the strength of the effect.
  #define USE_AMOUNT

  // Define this to change brightness / contrast / gamma.
  #define USE_BRIGHTNESSCONTRASTGAMMA

  /////////////////////////////////////////////////////////////
  // END CONFIGURATION REGION
  /////////////////////////////////////////////////////////////

  sampler2D _MainTex;

  float _Amount;
  float _Brightness;
  float _Contrast;
  float _Gamma;

  float _Threshold;
  float _MaxOffset;
  float _ThresholdYUV;

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float modTime = fmod(_Time.y, 32.0);
    
	float thresholdGlitch = 1.0 - _Threshold;
	const float timeFreq = 16.0;

    const float minChangeFreq = 4.0;
    float ct = Trunc(modTime, minChangeFreq);
    float randChange = Rand(Trunc(i.uv.yy, float2(16.0, 16.0)) + 150.0 * ct);

	float tf = timeFreq * randChange;

	float t = 5.0 * Trunc(modTime, tf);
	float randVT = 0.5 * Rand(Trunc(i.uv.yy + t, float2(11.0, 11.0)));
	randVT += 0.5 * Rand(Trunc(i.uv.yy + t, float2(7.0, 7.0)));
	randVT = randVT * 2.0 - 1.0;
	randVT = sign(randVT) * clamp((abs(randVT) - thresholdGlitch) / (1.0 - thresholdGlitch), 0.0, 1.0);

	float2 uvNM = i.uv;
	uvNM = clamp(uvNM + float2(_MaxOffset * randVT, 0.0), 0.0, 1.0);

	float rnd = Rand(float2(Trunc(modTime, 8.0), Trunc(modTime, 8.0)));
	uvNM.y = (rnd > lerp(1.0, 0.975, clamp(_Threshold, 0.0, 1.0))) ? 1.0 - uvNM.y : uvNM.y;

    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float3 final = tex2D(_MainTex, uvNM).rgb;

	final = RGB2YUV(final);

	final.g /= 1.0 - 3.0 * abs(randVT) * clamp(_ThresholdYUV - randVT, 0.0, 1.0);
	final.b += 0.125 * randVT * clamp(randVT - _ThresholdYUV, 0.0, 1.0);

    final = YUV2RGB(final);

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final = PixelDemo(pixel, final, i.uv, 1);
#endif

    return float4(final, 1.0f);
  }

  float4 frag_linear(v2f_img i) : COLOR
  {
    float modTime = fmod(_Time.y, 32.0);
    
	float thresholdGlitch = 1.0 - _Threshold;
	const float timeFreq = 16.0;

    const float minChangeFreq = 4.0;
    float ct = Trunc(modTime, minChangeFreq);
    float randChange = Rand(Trunc(i.uv.yy, float2(16.0, 16.0)) + 150.0 * ct);

	float tf = timeFreq * randChange;

	float t = 5.0 * Trunc(modTime, tf);
	float randVT = 0.5 * Rand(Trunc(i.uv.yy + t, float2(11.0, 11.0)));
	randVT += 0.5 * Rand(Trunc(i.uv.yy + t, float2(7.0, 7.0)));
	randVT = randVT * 2.0 - 1.0;
	randVT = sign(randVT) * clamp((abs(randVT) - thresholdGlitch) / (1.0 - thresholdGlitch), 0.0, 1.0);

	float2 uvNM = i.uv;
	uvNM = clamp(uvNM + float2(_MaxOffset * randVT, 0.0), 0.0, 1.0);

	float rnd = Rand(float2(Trunc(modTime, 8.0), Trunc(modTime, 8.0)));
	uvNM.y = (rnd > lerp(1.0, 0.975, clamp(_Threshold, 0.0, 1.0))) ? 1.0 - uvNM.y : uvNM.y;

    float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);

    float3 final = sRGB(tex2D(_MainTex, uvNM).rgb);

	final = RGB2YUV(final);

	final.g /= 1.0 - 3.0 * abs(randVT) * clamp(_ThresholdYUV - randVT, 0.0, 1.0);
	final.b += 0.125 * randVT * clamp(randVT - _ThresholdYUV, 0.0, 1.0);

    final = YUV2RGB(final);

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final = PixelDemo(pixel, final, i.uv, 1);
#endif

    return float4(Linear(final), 1.0f);
  }
  ENDCG

  // Techniques (http://unity3d.com/support/documentation/Components/SL-SubShader.html).
  SubShader
  {
    // Tags (http://docs.unity3d.com/Manual/SL-CullAndDepth.html).
    ZTest Always
    Cull Off
    ZWrite Off
    Fog { Mode off }

    // Pass 0: Color Space Gamma.
    Pass
    {
      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #pragma vertex vert_img
      #pragma fragment frag_gamma
      ENDCG
    }

    // Pass 1: Color Space Linear.
    Pass
    {
      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #pragma vertex vert_img
      #pragma fragment frag_linear
      ENDCG
    }
  }

  Fallback off
}