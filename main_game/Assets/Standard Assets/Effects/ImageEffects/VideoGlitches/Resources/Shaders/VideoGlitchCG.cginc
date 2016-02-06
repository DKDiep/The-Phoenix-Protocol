///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// \brief   Utils and global config.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////
// BEGIN CONFIGURATION REGION
/////////////////////////////////////////////////////////////

// Define this to change the strength of ALL effects.
#define ENABLE_ALL_AMOUNT

// Do not activate. Only to promotion videos.
//#define ENABLE_ALL_DEMO

/////////////////////////////////////////////////////////////
// END CONFIGURATION REGION
/////////////////////////////////////////////////////////////


// Gamma <-> Linear.

inline float3 sRGB(float3 pixel)
{
  return (pixel <= float3(0.0031308, 0.0031308, 0.0031308)) ? pixel * 12.9232102 : 1.055f * pow(pixel, 0.41666) - 0.055;
}

inline float4 sRGB(float4 pixel)
{
  return (pixel <= float4(0.0031308, 0.0031308, 0.0031308, pixel.a)) ? pixel * 12.9232102 : 1.055 * pow(pixel, 0.41666) - 0.055;
}

inline float3 Linear(float3 pixel)
{
  return (pixel <= float3(0.0404482, 0.0404482, 0.0404482)) ? pixel / 12.9232102 : pow((pixel + 0.055) * 0.9478672, 2.4);
}

inline float4 Linear(float4 pixel)
{
  return (pixel <= float4(0.0404482, 0.0404482, 0.0404482, pixel.a)) ? pixel / 12.9232102 : pow((pixel + 0.055) * 0.9478672, 2.4);
}

// Corrects the brightness, contrast and gamma.
inline float3 PixelBrightnessContrastGamma(float3 pixel, float brightness, float contrast, float gamma)
{
  pixel = (pixel - 0.5f) * contrast + 0.5f + brightness;

  pixel = clamp(pixel, 0.0f, 1.0f);

  pixel = pow(pixel, gamma);
  
  return pixel;
}

// Rand [0, 1].
inline float Rand(float2 n)
{
  return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * 43758.5453);
}

// SRand -1; 1].
inline float SRand(float2 n)
{
  return Rand(n) * 2.0 - 1.0;
}

inline float Trunc(float x, float num_levels)
{
  return floor(x * num_levels) / num_levels;
}

inline float2 Trunc(float2 x, float2 num_levels)
{
  return floor(x * num_levels) / num_levels;
}

// RGB -> YUV.
inline float3 RGB2YUV(float3 rgb)
{
  float3 yuv;
  yuv.x = dot(rgb, float3(0.299, 0.587, 0.114));
  yuv.y = dot(rgb, float3(-0.14713, -0.28886, 0.436));
  yuv.z = dot(rgb, float3(0.615, -0.51499, -0.10001));

  return yuv;
}

// YUV -> RGB.
inline float3 YUV2RGB(float3 yuv)
{
  float3 rgb;
  rgb.r = yuv.x + yuv.z * 1.13983;
  rgb.g = yuv.x + dot(float2(-0.39465, -0.58060), yuv.yz);
  rgb.b = yuv.x + yuv.y * 2.03211;

  return rgb;
}

// Strength of the effect.
inline float3 PixelAmount(float3 pixel, float3 final, float amount)
{
#ifdef ENABLE_ALL_AMOUNT
  return lerp(pixel, final, amount);
#else
  return final;
#endif
}

#ifdef ENABLE_ALL_DEMO
inline float3 PixelDemo(float3 pixel, float3 final, float2 uv, int zone)
{
  if (zone == 0)
  {
    if (uv.x > 0.333)
	  final = pixel;
  }
  else if (zone == 1)
  {
    if (uv.x < 0.333 || uv.x > 0.666)
	  final = pixel;
  }
  else if (zone == 2)
  {
    if (uv.x < 0.666)
	  final = pixel;
  }

  const float separator = 0.0025f;

  if ((uv.x > (0.333 - separator) && uv.x < (0.333 + separator)) ||
      (uv.x > (0.666 - separator) && uv.x < (0.666 + separator)))
    final = float4(1.0f, 1.0f, 1.0f, 1.0f);

  return final;
}
#endif