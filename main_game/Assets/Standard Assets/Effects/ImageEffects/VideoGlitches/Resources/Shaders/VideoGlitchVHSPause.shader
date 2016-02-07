///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Video Glitch VHS Pause.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchVHSPause"
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
  float _ColorNoise;
  float4 _NoiseColor;

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float2 uv = i.uv;
	uv.x += ((Rand(float2(_Time.y, i.uv.y)) - 0.5) / 64.0) * _Strength;
	uv.y += ((Rand(_Time.y) - 0.5) / 32.0) * _Strength;

	float3 final = (-0.5 + float3(Rand(float2(i.uv.y, _Time.y)), Rand(float2(i.uv.y, _Time.y + 1.0)), Rand(float2(i.uv.y, _Time.y + 2.0)))) * _ColorNoise;

	float noise = Rand(float2(floor(uv.y * 80.0), floor(uv.x * 50.0)) + float2(_Time.y, 0.0));

	if (noise > 11.5 - 30.0 * uv.y || noise < 1.5 - 5.0 * uv.y)
	  final += tex2D(_MainTex, uv).rgb;
	else
	  final = _NoiseColor.rgb;

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final.rgb = PixelBrightnessContrastGamma(final.rgb, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final.rgb = PixelAmount(pixel, final.rgb, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final.rgb = PixelDemo(pixel, final.rgb, i.uv, 2);
#endif

    return float4(final, 1.0);
  }

  float4 frag_linear(v2f_img i) : COLOR
  {
    float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);

    float2 uv = i.uv;
	uv.x += ((Rand(float2(_Time.y, i.uv.y)) - 0.5) / 64.0) * _Strength;
	uv.y += ((Rand(_Time.y) - 0.5) / 32.0) * _Strength;

	float3 final = (-0.5 + float3(Rand(float2(i.uv.y, _Time.y)), Rand(float2(i.uv.y, _Time.y + 1.0)), Rand(float2(i.uv.y, _Time.y + 2.0)))) * _ColorNoise;

	float noise = Rand(float2(floor(uv.y * 80.0), floor(uv.x * 50.0)) + float2(_Time.y, 0.0));

	if (noise > 11.5 - 30.0 * uv.y || noise < 1.5 - 5.0 * uv.y)
	  final += sRGB(tex2D(_MainTex, uv).rgb);
	else
	  final = _NoiseColor.rgb;

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
	final.rgb = PixelBrightnessContrastGamma(final.rgb, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_AMOUNT
    final.rgb = PixelAmount(pixel, final.rgb, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
    final.rgb = PixelDemo(pixel, final.rgb, i.uv, 2);
#endif

    return Linear(float4(final, 1.0));
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