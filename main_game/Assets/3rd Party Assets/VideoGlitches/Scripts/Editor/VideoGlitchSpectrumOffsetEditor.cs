///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Spectrum Offset Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchSpectrumOffset))]
  public class VideoGlitchSpectrumOffsetEditor : ImageEffectBaseEditor
  {
    private VideoGlitchSpectrumOffset thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchSpectrumOffset)target;

      this.Help = @"Spectrum color offset.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.strength = VideoGlitchEditorHelper.SliderWithReset("Strength", @"Effect strength.", thisTarget.strength, 0.0f, 1.0f, 0.1f);

      thisTarget.steps = VideoGlitchEditorHelper.IntSliderWithReset("Steps", @"Effect steps.", (int)thisTarget.steps, 3, 10, 5);
    }
  }
}