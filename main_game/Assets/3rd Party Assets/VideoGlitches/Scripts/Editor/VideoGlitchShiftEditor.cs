///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Shift Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchShift))]
  public class VideoGlitchShiftEditor : ImageEffectBaseEditor
  {
    private VideoGlitchShift thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchShift)target;

      this.Help = @"Displacement of the color channels.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.amplitude = VideoGlitchEditorHelper.IntSliderWithReset("Amplitude", @"Offset amount.", Mathf.CeilToInt(thisTarget.amplitude * 100.0f), 0, 100, 100) * 0.01f;

      thisTarget.speed = (float)VideoGlitchEditorHelper.IntSliderWithReset(@"Speed", @"Speed of change.", Mathf.FloorToInt(thisTarget.speed * 500.0f), 0, 100, 10) * 0.002f;
    }
  }
}