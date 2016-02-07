///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Utilities.
  /// </summary>
  public static class VideoGlitchesHelper
  {
    /// <summary>
    /// Load a 2D texture from "Resources/Textures".
    /// </summary>
    public static Texture2D LoadTextureFromResources(string texturePathFromResources)
    {
      Texture2D texture = Resources.Load<Texture2D>(texturePathFromResources);
      if (texture == null)
        Debug.LogWarning(string.Format("Texture '{0}' not found in 'Resources/Textures' folder.", texturePathFromResources));

      return texture;
    }
  }
}