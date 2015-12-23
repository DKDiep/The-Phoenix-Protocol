///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Noise Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchNoiseAnalog))]
  public class VideoGlitchNoiseEditor : ImageEffectBaseEditor
  {
    private VideoGlitchNoiseAnalog thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchNoiseAnalog)target;

      this.Help = @"Analogue noise.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.threshold = VideoGlitchEditorHelper.IntSliderWithReset("Threshold", @"Strength of the effect.", Mathf.CeilToInt(thisTarget.threshold * 100.0f), 0, 100, 100) * 0.01f;
    }
  }
}