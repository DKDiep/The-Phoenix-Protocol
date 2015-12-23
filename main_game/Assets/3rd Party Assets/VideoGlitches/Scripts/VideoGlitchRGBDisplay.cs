///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch RGB Display.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch RGB Display")]
  public sealed class VideoGlitchRGBDisplay : ImageEffectBase
  {
    /// <summary>
    /// Distortion [1..10].
    /// </summary>
    public int cellSize = 2;

    private const string variableCellSize = @"_CellSize";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchRGBDisplay"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      cellSize = 2;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetInt(variableCellSize, cellSize * 3);

      base.SendValuesToShader();
    }
  }
}