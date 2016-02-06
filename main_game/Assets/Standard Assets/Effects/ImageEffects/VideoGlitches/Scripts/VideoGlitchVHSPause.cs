///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch VHS Pause.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch VHS Pause")]
  public sealed class VideoGlitchVHSPause : ImageEffectBase
  {
    /// <summary>
    /// Effect strength [0..1].
    /// </summary>
    public float strength = 1.0f;

    /// <summary>
    /// Color noise [0..1].
    /// </summary>
    public float colorNoise = 0.1f;

    /// <summary>
    /// Noise color.
    /// </summary>
    public Color noiseColor = Color.white;

    private const string variableStrength = @"_Strength";
    private const string variableColorNoise = @"_ColorNoise";
    private const string variableNoiseColor = @"_NoiseColor";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchVHSPause"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      strength = 1.0f;
      colorNoise = 0.1f;
      noiseColor = Color.white;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableStrength, strength);
      this.Material.SetFloat(variableColorNoise, colorNoise);
      this.Material.SetColor(variableNoiseColor, noiseColor);

      base.SendValuesToShader();
    }
  }
}