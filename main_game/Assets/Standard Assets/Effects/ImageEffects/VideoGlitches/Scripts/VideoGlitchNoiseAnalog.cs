///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Noise.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Noise Analog")]
  public sealed class VideoGlitchNoiseAnalog : ImageEffectBase
  {
    /// <summary>
    /// Threshold [0..1].
    /// </summary>
    public float threshold = 0.1f;

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchNoiseAnalog"; } }

    private const string variableThreshold = @"_Threshold";

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      threshold = 0.1f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableThreshold, threshold);

      base.SendValuesToShader();
    }
  }
}