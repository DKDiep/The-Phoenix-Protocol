///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Black and White Distortion.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Black and White Distortion")]
  public sealed class VideoGlitchBlackWhiteDistortion : ImageEffectBase
  {
    /// <summary>
    /// Distortion [1..10].
    /// </summary>
    public float distortionSteps = 2.0f;

    /// <summary>
    /// Distortion min limit [0..360].
    /// </summary>
    public float distortionAmountMinLimit = 340.0f;

    /// <summary>
    /// Distortion min limit [0..360].
    /// </summary>
    public float distortionAmountMaxLimit = 360.0f;

    /// <summary>
    /// Distortion speed [0..10].
    /// </summary>
    public float distortionSpeed = 1.0f;

    private float distortionAmount = 340.0f;

    private const string variableDistortionSteps = @"_DistortionSteps";
    private const string variableDistortionAmount = @"_DistortionAmount";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchBlackWhiteDistortion"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      distortionSteps = 2.0f;
      distortionAmountMinLimit = 340.0f;
      distortionAmountMaxLimit = 360.0f;
      distortionSpeed = 1.0f;
      
      distortionAmount = distortionAmountMinLimit;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableDistortionSteps, distortionSteps);

      if (distortionAmount > distortionAmountMaxLimit)
        distortionAmount = distortionAmountMinLimit;

      if (distortionAmount < distortionAmountMinLimit)
        distortionAmount = distortionAmountMaxLimit;

      if (distortionSpeed > 0.0f)
        distortionAmount += Time.deltaTime * distortionSpeed;

      this.Material.SetFloat(variableDistortionAmount, distortionAmount);

      base.SendValuesToShader();
    }
  }
}