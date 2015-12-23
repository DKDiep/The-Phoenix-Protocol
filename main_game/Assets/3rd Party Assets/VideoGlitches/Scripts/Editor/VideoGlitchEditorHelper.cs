///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Utilities for the Editor.
  /// </summary>
  public static class VideoGlitchEditorHelper
  {
    /// <summary>
    /// Errors.
    /// </summary>
    public static readonly string ErrorTextureMissing = @"Some texture channels are missing. Please check that nothing is missing in 'VideoGlitches/Resources/Textures'.";

    /// <summary>
    /// Misc.
    /// </summary>
    public static readonly string DocumentationURL = @"http://www.ibuprogames.com/2015/07/02/video-glitches/";

    /// <summary>
    /// A slider with a reset button.
    /// </summary>
    public static float SliderWithReset(string label, string tooltip, float value, float minValue, float maxValue, float defaultValue)
    {
      EditorGUILayout.BeginHorizontal();
      {
        value = EditorGUILayout.Slider(new GUIContent(label, tooltip), value, minValue, maxValue);

        if (GUILayout.Button("R", GUILayout.Width(18.0f), GUILayout.Height(17.0f)) == true)
          value = defaultValue;
      }
      EditorGUILayout.EndHorizontal();

      return value;
    }

    /// <summary>
    /// A slider with a reset button.
    /// </summary>
    public static int IntSliderWithReset(string label, string tooltip, int value, int minValue, int maxValue, int defaultValue)
    {
      EditorGUILayout.BeginHorizontal();
      {
        value = EditorGUILayout.IntSlider(new GUIContent(label, tooltip), value, minValue, maxValue);

        if (GUILayout.Button(new GUIContent("R", "Reset to '" + defaultValue + "'."), GUILayout.Width(18.0f), GUILayout.Height(17.0f)) == true)
          value = defaultValue;
      }
      EditorGUILayout.EndHorizontal();

      return value;
    }

    /// <summary>
    /// Range with a reset button.
    /// </summary>
    public static void MinMaxSliderWithReset(string label, string tooltip, ref float minValue, ref float maxValue, float minLimit, float maxLimit, float defaultMinLimit, float defaultMaxLimit)
    {
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.MinMaxSlider(new GUIContent(label, tooltip), ref minValue, ref maxValue, minLimit, maxLimit);

        if (GUILayout.Button("R", GUILayout.Width(18.0f), GUILayout.Height(17.0f)) == true)
        {
          minValue = defaultMinLimit;
          maxValue = defaultMaxLimit;
        }
      }
      EditorGUILayout.EndHorizontal();
    }
  }
}