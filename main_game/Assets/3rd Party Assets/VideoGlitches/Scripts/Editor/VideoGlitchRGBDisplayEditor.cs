///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch RGB Display Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchRGBDisplay))]
  public class VideoGlitchRGBDisplayEditor : ImageEffectBaseEditor
  {
    private VideoGlitchRGBDisplay thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchRGBDisplay)target;

      this.Help = @"RGB display.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.cellSize = VideoGlitchEditorHelper.IntSliderWithReset("Cell size", @"Cell size.", thisTarget.cellSize, 1, 10, 2);

      //VideoGlitchEditorHelper.MinMaxSliderWithReset("Distortion range", @"Distortion range.", ref thisTarget.distortionAmountMinLimit, ref thisTarget.distortionAmountMaxLimit, 0.0f, 360.0f, 340.0f, 360.0f);

      //thisTarget.distortionSpeed = VideoGlitchEditorHelper.SliderWithReset("Distortion speed", @"Distortion speed.", thisTarget.distortionSpeed, 0.0f, 10.0f, 1.0f);
    }
  }
}