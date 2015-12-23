///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Video Glitch Shift.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchShift"
{
  // http://unity3d.com/support/documentation/Components/SL-Properties.html
  Properties
  {
    _MainTex("Base (RGB)", 2D) = "white" {}

    // Default 'Resources/Textures/Noise256.png'.
    _NoiseTex("Noise (RGB)", 2D) = "" {}

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
  sampler2D _NoiseTex;

  float _Amount;
  float _Brightness;
  float _Contrast;
  float _Gamma;

  float _Amplitude;
  float _Speed;

  inline float4 Pow4(float4 v, float p)
  {
    return float4(pow(v.x, p), pow(v.y, p), pow(v.z, p), v.w);
  }

  inline float4 Noise(float2 p)
  {
    return tex2D(_NoiseTex, p);
  }

  inline float3 ShiftRGBGamma(float2 p, float4 shift)
  {
    shift *= 2.0 * shift.w - 1.0;

    float r = tex2D(_MainTex, p + float2(shift.x, -shift.y)).r;
    float g = tex2D(_MainTex, p + float2(shift.y, -shift.z)).g;
    float b = tex2D(_MainTex, p + float2(shift.z, -shift.x)).b;

    return float3(r, g, b);
  }

  inline float3 ShiftRGBLinear(float2 p, float4 shift)
  {
    shift *= 2.0 * shift.w - 1.0;

    float r = sRGB(tex2D(_MainTex, p + float2(shift.x, -shift.y)).rgb).r;
    float g = sRGB(tex2D(_MainTex, p + float2(shift.y, -shift.z)).rgb).g;
    float b = sRGB(tex2D(_MainTex, p + float2(shift.z, -shift.x)).rgb).b;

    return float3(r, g, b);
  }
  
  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float4 shift = Pow4(Noise(float2(_Speed * _Time.y, 2.0 * _Speed * _Time.y / 25.0)), 8.0) * float4(_Amplitude, _Amplitude, _Amplitude, 1.0);
    
    float3 final = ShiftRGBGamma(i.uv, shift);

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final = PixelDemo(pixel, final, i.uv, 2);
#endif

    return float4(final, 1.0);
  }

  float4 frag_linear(v2f_img i) : COLOR
  {
    float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);

    float4 shift = Pow4(Noise(float2(_Speed * _Time.y, 2.0 * _Speed * _Time.y / 25.0)), 8.0) * float4(_Amplitude, _Amplitude, _Amplitude, 1.0);
    
    float3 final = ShiftRGBLinear(i.uv, shift);

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final = PixelDemo(pixel, final, i.uv, 2);
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