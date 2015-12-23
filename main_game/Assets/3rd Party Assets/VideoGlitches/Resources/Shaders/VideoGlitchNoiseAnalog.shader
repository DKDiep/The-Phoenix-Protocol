///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Video Glitch Noise Analog.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchNoiseAnalog"
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

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;
	float3 final = float3(0.0, 0.0, 0.0);

    float2 uv = i.uv;

    float t = fmod(_Time.y, 360.0);
    float t2 = floor(t * 0.6);

    float x, y, yt, xt;

    yt = abs(cos(t)) * Rand(float2(t, t)) * 100.0;
    xt = sin(360.0 * Rand(float2(t, t))) * 0.25;
    
	if (xt < 0.0)
      xt = 0.125;

    x = uv.x - xt * exp(-pow(uv.y * 100.0 - yt, 2.0) / 24.0);
    y = uv.y;
        
    uv.x = x;
    uv.y = y;

    yt = 0.5 * cos((yt / 100.0) / 100.0 * 360.0);
    float yr = 0.1 * cos((yt / 100.0) / 100.0 * 360.0);
    
	if (i.uv.y > yt && i.uv.y < yt + Rand(float2(t2, t)) * 0.25)
	{
      float md = fmod(x * 100.0, 10.0);

      if (md * sin(t) > sin(yr * 360.0) || Rand(float2(md, md)) > 0.4)
	  {
        float4 org_c = float4(tex2D(_MainTex, uv).rgb, 1.0);
        float colx = Rand(float2(t2, t2)) * 0.75;
        float coly = Rand(float2(uv.x + t, t));
        float colz = Rand(float2(t2, t2));
        
		final = float3(org_c.x + colx, org_c.y + colx, org_c.z + colx);
      }
    }
    else if (y < cos(t) && fmod(x * 40.0, 2.0) > Rand(float2(y * t, t * t)) * 1.0 || fmod(y * 12.0, 2.0) < Rand(float2(x, t)) * 1.0)
	{
      if (Rand(float2(x + t, y + t)) > 0.8)
	    final = float3(Rand(float2(x * t, y * t)), Rand(float2(x * t, y * t)), Rand(float2(x * t, y * t)));
      else 
	    final = tex2D(_MainTex, uv).rgb;
    }
	else
	{
      uv.x = uv.x + Rand(float2(t, uv.y)) * 0.0087 * sin(y * 2.0);
      
      final = tex2D(_MainTex, uv).rgb;
	}

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final = PixelDemo(pixel, final, i.uv, 0);
#endif

    return float4(final, 1.0f);
  }

  float4 frag_linear(v2f_img i) : COLOR
  {
    float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);
	float3 final = float3(0.0, 0.0, 0.0);

    float2 uv = i.uv;

    float t = fmod(_Time.y, 360.0);
    float t2 = floor(t * 0.6);

    float x, y, yt, xt;

    yt = abs(cos(t)) * Rand(float2(t, t)) * 100.0;
    xt = sin(360.0 * Rand(float2(t, t))) * 0.25;
    
	if (xt < 0.0)
      xt = 0.125;

    x = uv.x - xt * exp(-pow(uv.y * 100.0 - yt, 2.0) / 24.0);
    y = uv.y;
        
    uv.x = x;
    uv.y = y;

    yt = 0.5 * cos((yt / 100.0) / 100.0 * 360.0);
    float yr = 0.1 * cos((yt / 100.0) / 100.0 * 360.0);
    
	if (i.uv.y > yt && i.uv.y < yt + Rand(float2(t2, t)) * 0.25)
	{
      float md = fmod(x * 100.0, 10.0);

      if (md * sin(t) > sin(yr * 360.0) || Rand(float2(md, md)) > 0.4)
	  {
        float4 org_c = float4(sRGB(tex2D(_MainTex, uv).rgb), 1.0);
        float colx = Rand(float2(t2, t2)) * 0.75;
        float coly = Rand(float2(uv.x + t, t));
        float colz = Rand(float2(t2, t2));
        
		final = float3(org_c.x + colx, org_c.y + colx, org_c.z + colx);
      }
    }
    else if (y < cos(t) && fmod(x * 40.0, 2.0) > Rand(float2(y * t, t * t)) * 1.0 || fmod(y * 12.0, 2.0) < Rand(float2(x, t)) * 1.0)
	{
      if (Rand(float2(x + t, y + t)) > 0.8)
	    final = float3(Rand(float2(x * t, y * t)), Rand(float2(x * t, y * t)), Rand(float2(x * t, y * t)));
      else 
	    final = sRGB(tex2D(_MainTex, uv).rgb);
    }
	else
	{
      uv.x = uv.x + Rand(float2(t, uv.y)) * 0.0087 * sin(y * 2.0);
      
      final = sRGB(tex2D(_MainTex, uv).rgb);
	}

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final = PixelDemo(pixel, final, i.uv, 0);
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