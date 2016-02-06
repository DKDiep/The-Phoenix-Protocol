///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Video Glitch Spectrum Offset.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchSpectrumOffset"
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

  float _Strength;
  int _Steps;

  float Clamp01(float t)
  {
    return clamp(t, 0.0, 1.0);
  }

  float2 Clamp01(float2 t)
  {
    return clamp(t, 0.0, 1.0);
  }

  // Remaps [a; b] to [0; 1].
  float Remap(float t, float a, float b)
  {
    return Clamp01((t - a) / (b - a));
  }

  // t = [0; 0.5; 1], y = [0; 1; 0]
  float Linterp(float t)
  {
    return Clamp01(1.0 - abs(2.0 * t - 1.0));
  }

  float3 SpectrumOffset(float t)
  {
	float3 ret;
    
	float lo = step(t, 0.5);
    float hi = 1.0 - lo;
    float w = Linterp(Remap(t, 0.1666666666, 0.8333333333)); // 1 / 6, 5 / 6.
    
	float neg_w = 1.0 - w;
    ret = float3(lo, 1.0, hi) * float3(neg_w, w, neg_w);
	
	return pow(ret, 0.4545454545); // 1.0 / 2.2
  }

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float2 uv = i.uv;

    float time = fmod(_Time.y, 32.0);
    	
    float gnm = Clamp01(_Strength);
    float rnd0 = Rand(Trunc(float2(time, time), 6.0));
    float r0 = Clamp01((1.0 - gnm) * 0.7 + rnd0);

	// Horizontal.
    float rnd1 = Rand(float2(Trunc(uv.x, 10.0 * r0), time));
    float r1 = 0.5 - 0.5 * gnm + rnd1;

    // Vertical.
    float rnd2 = Rand(float2(Trunc(uv.y, 40.0 * r1), time));
    float r2 = Clamp01(rnd2);
    
    float rnd3 = Rand(float2(Trunc(uv.y, 10.0 * r0), time));
    float r3 = (1.0 - Clamp01(rnd3 + 0.8)) - 0.1;
    
    float pxrnd = Rand(uv + time);
    
    float ofs = 0.05 * r2 * _Strength * (rnd0 > 0.5 ? 1.0 : -1.0);
    ofs += 0.5 * pxrnd * ofs;
    
    uv.y += 0.1 * r3 * _Strength;
    
    const float fSamples = 1.0 / float(_Steps);
        
    float4 final = 0.0;
    float3 wsum = 0.0;
    for (int i = 0; i < _Steps; ++i)
    {
      float t = float(i) * fSamples;
      uv.x = Clamp01(uv.x + ofs * t);
      float4 samplecol = tex2D(_MainTex, uv);
      float3 s = SpectrumOffset(t);
      samplecol.rgb = samplecol.rgb * s;
      final += samplecol;
      wsum += s;
    }

    final.rgb /= wsum;
    final.a *= fSamples;

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final.rgb = PixelBrightnessContrastGamma(final.rgb, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final.rgb = PixelAmount(pixel, final.rgb, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final.rgb = PixelDemo(pixel, final.rgb, i.uv, 2);
#endif

    return final;
  }

  float4 frag_linear(v2f_img i) : COLOR
  {
    float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);

    float2 uv = i.uv;

    float time = fmod(_Time.y, 32.0);
    	
    float gnm = Clamp01(_Strength);
    float rnd0 = Rand(Trunc(float2(time, time), 6.0));
    float r0 = Clamp01((1.0 - gnm) * 0.7 + rnd0);

	// Horizontal.
    float rnd1 = Rand(float2(Trunc(uv.x, 10.0 * r0), time));
    float r1 = 0.5 - 0.5 * gnm + rnd1;

    // Vertical.
    float rnd2 = Rand(float2(Trunc(uv.y, 40.0 * r1), time));
    float r2 = Clamp01(rnd2);
    
    float rnd3 = Rand(float2(Trunc(uv.y, 10.0 * r0), time));
    float r3 = (1.0 - Clamp01(rnd3 + 0.8)) - 0.1;
    
    float pxrnd = Rand(uv + time);
    
    float ofs = 0.05 * r2 * _Strength * (rnd0 > 0.5 ? 1.0 : -1.0);
    ofs += 0.5 * pxrnd * ofs;
    
    uv.y += 0.1 * r3 * _Strength;
    
    const float fSamples = 1.0 / float(_Steps);
        
    float4 final = 0.0;
    float3 wsum = 0.0;
    for (int i = 0; i < _Steps; ++i)
    {
      float t = float(i) * fSamples;
      uv.x = Clamp01(uv.x + ofs * t);
      float4 samplecol = sRGB(tex2D(_MainTex, uv));
      float3 s = SpectrumOffset(t);
      samplecol.rgb = samplecol.rgb * s;
      final += samplecol;
      wsum += s;
    }

    final.rgb /= wsum;
    final.a *= fSamples;

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final.rgb = PixelBrightnessContrastGamma(final.rgb, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final.rgb = PixelAmount(pixel, final.rgb, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final.rgb = PixelDemo(pixel, final.rgb, i.uv, 2);
#endif

    return Linear(final);
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