///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Image effect base.
  /// </summary>
  public abstract class ImageEffectBase : MonoBehaviour
  {
    /// <summary>
    /// Amount of the effect [0 - 1].
    /// </summary>
    public float amount = 1.0f;

    /// <summary>
    /// Brightness [-1 - 1].
    /// </summary>
    public float brightness = 0.0f;

    /// <summary>
    /// Contrast [-1 - 1].
    /// </summary>
    public float contrast = 0.0f;

    /// <summary>
    /// Gamma [0.1 - 10].
    /// </summary>
    public float gamma = 1.0f;

    private Shader shader;

    private Material material;

    private const string variableAmount = @"_Amount";
    private const string variableBrightness = @"_Brightness";
    private const string variableContrast = @"_Contrast";
    private const string variableGamma = @"_Gamma";

    /// <summary>
    /// Get/set the shader.
    /// </summary>
    public Shader Shader
    {
      get { return shader; }
      set
      {
        if (shader != value)
        {
          shader = value;

          CreateMaterial();
        }
      }
    }

    /// <summary>
    /// Get the material.
    /// </summary>
    public Material Material
    {
      get
      {
        if (material == null && shader != null)
          CreateMaterial();

        return material;
      }
    }

    /// <summary>
    /// Shader path.
    /// </summary>
    protected abstract string ShaderPath { get; }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
      if (CheckHardwareRequirements() == true)
      {
        shader = Resources.Load<Shader>(ShaderPath);
        if (shader != null)
        {
          if (shader.isSupported == true)
          {
            CreateMaterial();

            if (material == null)
            {
              Debug.LogWarning(string.Format("'{0}' material null.", this.name));

              this.enabled = false;
            }
          }
          else
          {
            Debug.LogWarning(string.Format("'{0}' shader not supported.", this.GetType().ToString()));

            this.enabled = false;
          }
        }
        else
        {
          Debug.LogError(string.Format("'{0}' shader not found.", ShaderPath));

          this.enabled = false;
        }
      }
      else
        this.enabled = false;
    }

    /// <summary>
    /// Destroy resources.
    /// </summary>
    protected virtual void OnDestroy()
    {
      if (material != null)
        DestroyImmediate(material);
    }

    /// <summary>
    /// Check hardware requirements.
    /// </summary>
    protected virtual bool CheckHardwareRequirements()
    {
      if (SystemInfo.supportsImageEffects == false)
      {
        Debug.LogWarning(string.Format("Hardware not support Image Effects. '{0}' disabled.", this.GetType().ToString()));

        return false;
      }

      return true;
    }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public virtual void ResetDefaultValues()
    {
      amount = 1.0f;
      brightness = 0.0f;
      contrast = 0.0f;
      gamma = 1.0f;
    }

    /// <summary>
    /// Creates the material and textures.
    /// </summary>
    protected virtual void CreateMaterial()
    {
      if (material != null)
        DestroyImmediate(material);

      material = new Material(shader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
      if (material != null)
      {
        SendValuesToShader();

        Graphics.Blit(source, destination, material, QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
      }
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected virtual void SendValuesToShader()
    {
      material.SetFloat(variableAmount, amount);
      material.SetFloat(variableBrightness, brightness);
      material.SetFloat(variableContrast, contrast + 1.0f);
      material.SetFloat(variableGamma, 1.0f / gamma);
    }
  }
}
