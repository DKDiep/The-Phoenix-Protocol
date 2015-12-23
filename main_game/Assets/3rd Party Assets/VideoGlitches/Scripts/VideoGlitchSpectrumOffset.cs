///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Spectrum Offset.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Spectrum Offset")]
  public sealed class VideoGlitchSpectrumOffset : ImageEffectBase
  {
    /// <summary>
    /// Effect strength [0..1].
    /// </summary>
    public float strength = 0.1f;

    /// <summary>
    /// Effect steps [3..10].
    /// </summary>
    public int steps = 5;

    private const string variableStrength = @"_Strength";
    private const string variableSteps = @"_Steps";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchSpectrumOffset"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      strength = 0.1f;
      steps = 5;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableStrength, strength);
      this.Material.SetInt(variableSteps, steps);

      base.SendValuesToShader();
    }
  }
}