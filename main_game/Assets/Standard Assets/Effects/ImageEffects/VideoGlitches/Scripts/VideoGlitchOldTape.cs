///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Old Tape.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Old Tape")]
  public sealed class VideoGlitchOldTape : ImageEffectBase
  {
    /// <summary>
    /// Noise speed [1 - 100].
    /// </summary>
    public float noiseSpeed = 100.0f;

    /// <summary>
    /// Noise amplitude [1 - 100].
    /// </summary>
    public float noiseAmplitude = 1.0f;

    private const string variableNoiseSpeed = @"_NoiseSpeed";
    private const string variableNoiseAmplitude = @"_NoiseAmplitude";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchOldTape"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      noiseSpeed = 100.0f;
      noiseAmplitude = 1.0f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableNoiseSpeed, noiseSpeed / 100000.0f);
      this.Material.SetFloat(variableNoiseAmplitude, (101.0f - noiseAmplitude) * 1000.0f);

      base.SendValuesToShader();
    }
  }
}