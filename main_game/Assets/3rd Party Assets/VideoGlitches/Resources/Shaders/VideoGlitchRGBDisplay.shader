///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Video Glitch RGB Display.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchRGBDisplay"
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

  int _CellSize = 2;

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv);

    float3 final = float3(0.0, 0.0, 0.0);

	float cellSize = float(_CellSize);
    int redCell = int(cellSize / 3.0);
    int greenCell = _CellSize - redCell;

    float2 uv = i.uv * _ScreenParams.xy;

    float2 p = floor(uv / cellSize) * cellSize;

    float3 downScale = tex2D(_MainTex, p / _ScreenParams.xy).rgb;

	int offsetX = int(fmod(uv.x, cellSize));
    int offsetY = int(fmod(uv.y, cellSize));

    if (offsetY < _CellSize - 1)
	{
      if (offsetX < redCell)
	    final.r = downScale.r;
      else if (offsetX < greenCell)
	    final.g = downScale.g;
      else
	    final.b = downScale.b;
    }

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

    float3 final = float3(0.0, 0.0, 0.0);

	float cellSize = float(_CellSize);
    int redCell = int(cellSize / 3.0);
    int greenCell = _CellSize - redCell;

    float2 uv = i.uv * _ScreenParams.xy;

    float2 p = floor(uv / cellSize) * cellSize;

    float3 downScale = sRGB(tex2D(_MainTex, p / _ScreenParams.xy).rgb);

	int offsetX = int(fmod(uv.x, cellSize));
    int offsetY = int(fmod(uv.y, cellSize));

    if (offsetY < _CellSize - 1)
	{
      if (offsetX < redCell)
	    final.r = downScale.r;
      else if (offsetX < greenCell)
	    final.g = downScale.g;
      else
	    final.b = downScale.b;
    }

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