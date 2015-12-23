///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch VHS Pause Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchVHSPause))]
  public class VideoGlitchVHSPauseEditor : ImageEffectBaseEditor
  {
    private VideoGlitchVHSPause thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchVHSPause)target;

      this.Help = @"VHS pause.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.strength = VideoGlitchEditorHelper.SliderWithReset(@"Strength", @"Effect strength [0 - 1].", thisTarget.strength, 0.0f, 1.0f, 0.5f);

      thisTarget.colorNoise = VideoGlitchEditorHelper.SliderWithReset(@"Color noise", @"Color noise [0 - 1].", thisTarget.colorNoise, 0.0f, 1.0f, 0.1f);

      thisTarget.noiseColor = EditorGUILayout.ColorField(@"Noise color", thisTarget.noiseColor);
    }
  }
}