///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Digital Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchNoiseDigital))]
  public class VideoGlitchDigitalEditor : ImageEffectBaseEditor
  {
    private VideoGlitchNoiseDigital thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchNoiseDigital)target;

      this.Help = @"Digital noise.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.threshold = VideoGlitchEditorHelper.IntSliderWithReset("Threshold", @"Strength of the effect.", Mathf.CeilToInt(thisTarget.threshold * 100.0f), 0, 100, 10) * 0.01f;

      thisTarget.maxOffset = VideoGlitchEditorHelper.IntSliderWithReset("Max offset", @"Max displacement.", Mathf.CeilToInt(thisTarget.maxOffset * 100.0f), 0, 100, 10) * 0.01f;

      thisTarget.thresholdYUV = VideoGlitchEditorHelper.IntSliderWithReset("Threshold YUV", @"Color change.", Mathf.CeilToInt(thisTarget.thresholdYUV * 100.0f), 0, 100, 50) * 0.01f;
    }
  }
}