///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Old tape Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchOldTape))]
  public class VideoGlitchOldTapeEditor : ImageEffectBaseEditor
  {
    private VideoGlitchOldTape thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchOldTape)target;

      this.Help = @"Old VCR tape.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.noiseSpeed = VideoGlitchEditorHelper.SliderWithReset("Speed", @"Noise speed.", thisTarget.noiseSpeed, 0.0f, 100.0f, 100.0f);

      thisTarget.noiseAmplitude = VideoGlitchEditorHelper.SliderWithReset("Amplitude", @"Noise amplitude.", thisTarget.noiseAmplitude, 1.0f, 100.0f, 1.0f);
    }
  }
}