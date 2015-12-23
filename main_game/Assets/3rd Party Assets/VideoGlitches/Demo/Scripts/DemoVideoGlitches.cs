///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
// Copyright (c) Ibuprogames. All rights reserved.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;

using UnityEngine;

namespace VideoGlitches.Demo
{
  /// <summary>
  /// UI for the demo.
  /// </summary>
  public class DemoVideoGlitches : MonoBehaviour
  {
    public bool guiShow = true;

    public bool showEffectName = false;

    public float slideEffectTime = 0.0f;

    public AudioClip musicClip = null;

    private float effectTime = 0.0f;

    private List<ImageEffectBase> videoGlitches = new List<ImageEffectBase>();

#if USE_SLIDESHOW
    private SlideShow slideShow;
#endif
    private int guiSelection = 0;

    private bool menuOpen = false;

    private bool consoleShow = false;

    private const float guiMargen = 10.0f;
    private const float guiWidth = 200.0f;
    private const string guiTab = "   ";

    private Vector2 scrollPosition = Vector2.zero;
    private Vector2 scrollLog = Vector2.zero;

    private float updateInterval = 0.5f;
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private float fps = 0.0f;

    private GUIStyle effectNameStyle;
    private GUIStyle menuStyle;

    private List<string> logs = new List<string>();

    private void OnEnable()
    {
      Application.logMessageReceived += LogMessageReceived;
#if SHOW_SYSTEM_INFO
    Debug.Log(string.Format("{0}x{1}x{2}", Screen.width, Screen.height, Screen.currentResolution.refreshRate));
    Debug.Log(SystemInfo.operatingSystem);
    Debug.Log(string.Format("{0} x {1}", SystemInfo.processorCount, SystemInfo.processorType));
    Debug.Log(string.Format("Mem: {0:f1}gb", SystemInfo.systemMemorySize / 1000.0f));
    Debug.Log(SystemInfo.graphicsDeviceName);
    Debug.Log(SystemInfo.graphicsDeviceVersion);
    Debug.Log(string.Format("VMem: {0}mb", SystemInfo.graphicsMemorySize));
    Debug.Log(string.Format("Shader Level: {0:f1}", SystemInfo.graphicsShaderLevel / 10.0f));
    Debug.Log(string.Format("deviceName: {0}", SystemInfo.deviceName));
    Debug.Log(string.Format("deviceModel: {0}", SystemInfo.deviceModel));
#endif
      timeleft = updateInterval;

      Camera selectedCamera = null;
      Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

      for (int i = 0; i < cameras.Length; ++i)
      {
        if (cameras[i].enabled == true)
        {
          selectedCamera = cameras[i];

          break;
        }
      }

      if (selectedCamera != null)
      {
        if (selectedCamera.GetComponents<ImageEffectBase>().Length == 0)
        {
          // Video Glitches.
          selectedCamera.gameObject.AddComponent<VideoGlitchShift>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchNoiseDigital>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchNoiseAnalog>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchBlackWhiteDistortion>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchRGBDisplay>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchSpectrumOffset>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchOldTape>().enabled = false;
          selectedCamera.gameObject.AddComponent<VideoGlitchVHSPause>().enabled = false;

          videoGlitches.AddRange(selectedCamera.GetComponents<ImageEffectBase>());
        }
        else
        {
          ImageEffectBase[] imageEffects = selectedCamera.GetComponents<ImageEffectBase>();
          for (int i = 0; i < imageEffects.Length; ++i)
            videoGlitches.Add(imageEffects[i]);
        }

        Debug.Log(string.Format("{0} Video Glitches.", videoGlitches.Count));

        for (int i = 0; i < videoGlitches.Count; ++i)
          videoGlitches[i].enabled = (i == guiSelection);

        if (musicClip != null)
        {
          AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
          audioSource.clip = musicClip;
          audioSource.loop = (slideEffectTime > 0.0f);
          audioSource.PlayDelayed(2.0f);

          if (slideEffectTime == 0.0f)
            slideEffectTime = (musicClip.length + 2.0f) / videoGlitches.Count;
        }
      }
      else
        Debug.LogWarning("No camera found.");

#if USE_SLIDESHOW
      slideShow = GameObject.FindObjectOfType<SlideShow>();
#endif
    }

    private void OnDisable()
    {
      Application.logMessageReceived -= LogMessageReceived;

      videoGlitches.Clear();
    }

    private void Update()
    {
      timeleft -= Time.deltaTime;
      accum += Time.timeScale / Time.deltaTime;
      frames++;

      if (timeleft <= 0.0f)
      {
        fps = accum / frames;
        timeleft = updateInterval;
        accum = 0.0f;
        frames = 0;
      }

      if (slideEffectTime > 0.0f && videoGlitches.Count > 0)
      {
        effectTime += Time.deltaTime;
        if (effectTime >= slideEffectTime)
        {
          videoGlitches[guiSelection].enabled = false;

          guiSelection = (guiSelection < (videoGlitches.Count - 1) ? guiSelection + 1 : 0);

          videoGlitches[guiSelection].enabled = true;
          
          effectTime = 0.0f;
        }
      }

      if (Input.GetKeyUp(KeyCode.F1) == true)
        guiShow = !guiShow;

      if (Input.GetKeyUp(KeyCode.KeypadPlus) == true ||
          Input.GetKeyUp(KeyCode.KeypadMinus) == true ||
          Input.GetKeyUp(KeyCode.PageUp) == true ||
          Input.GetKeyUp(KeyCode.PageDown) == true)
      {
        int effectSelected = 0;

        slideEffectTime = 0.0f;

        for (int i = 0; i < videoGlitches.Count; ++i)
        {
          if (videoGlitches[i].enabled == true)
          {
            videoGlitches[i].enabled = false;

            effectSelected = i;

            break;
          }
        }

        if (Input.GetKeyUp(KeyCode.KeypadPlus) == true || Input.GetKeyUp(KeyCode.PageUp) == true)
        {
          guiSelection = (effectSelected < videoGlitches.Count - 1 ? effectSelected + 1 : 0);

          videoGlitches[guiSelection].enabled = true;
        }

        if (Input.GetKeyUp(KeyCode.KeypadMinus) == true || Input.GetKeyUp(KeyCode.PageDown) == true)
        {
          guiSelection = (effectSelected > 0 ? effectSelected - 1 : videoGlitches.Count - 1);

          videoGlitches[guiSelection].enabled = true;
        }
      }

#if !UNITY_WEBPLAYER
    if (Input.GetKeyDown(KeyCode.Escape))
      Application.Quit();
#endif
    }

    private void OnGUI()
    {
/*
      GUILayout.BeginArea(new Rect(20.0f, 20.0f, 120.0f, 28.0f), "ANALOG", effectNameStyle);
      GUILayout.EndArea();

      GUILayout.BeginArea(new Rect((Screen.width / 2) - 60.0f, 20.0f, 120.0f, 28.0f), "DIGITAL", effectNameStyle);
      GUILayout.EndArea();

      GUILayout.BeginArea(new Rect(Screen.width - 140.0f, 20.0f, 120.0f, 28.0f), "SHIFT", effectNameStyle);
      GUILayout.EndArea();
 */

#if UNITY_ANDROID || UNITY_IPHONE
      GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 1280.0f, (float)Screen.height / 720.0f, 1.0f));
#endif
      if (videoGlitches.Count == 0)
        return;

      if (effectNameStyle == null)
      {
        effectNameStyle = new GUIStyle(GUI.skin.textArea);
        effectNameStyle.alignment = TextAnchor.MiddleCenter;
        effectNameStyle.fontSize = 22;
      }

      if (menuStyle == null)
      {
        menuStyle = new GUIStyle(GUI.skin.textArea);
        menuStyle.alignment = TextAnchor.MiddleCenter;
        menuStyle.fontSize = 14;
      }

      if (showEffectName == true && guiShow == false)
      {
        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 150.0f, 20.0f, 300.0f, 30.0f),
          videoGlitches[guiSelection].GetType().ToString().Replace(@"VideoGlitches.VideoGlitch", string.Empty).ToUpper(),
          effectNameStyle);
        GUILayout.EndArea();
      }

      if (guiShow == false)
        return;

      GUILayout.BeginHorizontal("box", GUILayout.Width(Screen.width));
      {
        GUILayout.Space(guiMargen);

        if (GUILayout.Button("MENU", menuStyle, GUILayout.Width(80.0f)) == true)
          menuOpen = !menuOpen;

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("<<<", menuStyle) == true)
        {
          slideEffectTime = 0.0f;

          if (guiSelection > 0)
            guiSelection--;
          else
            guiSelection = videoGlitches.Count - 1;

          Event.current.Use();
        }

        GUI.contentColor = Color.white;

        GUILayout.Label(videoGlitches[guiSelection].GetType().ToString().Replace(@"VideoGlitches.VideoGlitch", string.Empty).ToUpper(),
          menuStyle,
          GUILayout.Width(200.0f));

        if (GUILayout.Button(">>>", menuStyle) == true)
        {
          slideEffectTime = 0.0f;

          if (guiSelection < videoGlitches.Count - 1)
            guiSelection++;
          else
            guiSelection = 0;
        }

        GUILayout.FlexibleSpace();

        if (fps < 30.0f)
          GUI.contentColor = Color.yellow;
        else if (fps < 15.0f)
          GUI.contentColor = Color.red;
        else
          GUI.contentColor = Color.green;

        GUILayout.Label(fps.ToString("000"), menuStyle, GUILayout.Width(40.0f));

        GUI.contentColor = Color.white;

        GUILayout.Space(guiMargen);
      }
      GUILayout.EndHorizontal();

      // Update
      for (int i = 0; i < videoGlitches.Count; ++i)
      {
        ImageEffectBase imageEffect = videoGlitches[i];

        if (guiSelection == i && imageEffect.enabled == false)
          imageEffect.enabled = true;

        if (imageEffect.enabled == true && guiSelection != i)
          imageEffect.enabled = false;
      }

      if (menuOpen == true)
      {
        GUILayout.BeginVertical("box", GUILayout.Width(guiWidth));
        {
          GUILayout.Space(guiMargen);

          // Cideo Glitches.
          if (videoGlitches.Count > 0)
          {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
            {
              int effectChanged = -1;

              // Draw
              for (int i = 0; i < videoGlitches.Count; ++i)
              {
                ImageEffectBase imageEffect = videoGlitches[i];

                GUILayout.BeginHorizontal();
                {
                  if (imageEffect.enabled == true)
                    GUILayout.BeginVertical("box");

                  bool enableChanged = GUILayout.Toggle(imageEffect.enabled, guiTab + imageEffect.GetType().ToString().Replace(@"VideoGlitches.VideoGlitch", string.Empty));
                  if (enableChanged != imageEffect.enabled)
                    effectChanged = i;

                  if (imageEffect.enabled == true)
                  {
                    DrawCommonControls(imageEffect);

                    DrawCustomControls(imageEffect);

                    GUILayout.EndVertical();
                  }
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(guiMargen * 0.5f);
              }

              // Update
              for (int i = 0; i < videoGlitches.Count; ++i)
              {
                ImageEffectBase imageEffect = videoGlitches[i];

                if (effectChanged == i)
                {
                  imageEffect.enabled = !imageEffect.enabled;

                  if (imageEffect.enabled == true)
                    guiSelection = i;
                }

                if (imageEffect.enabled == true && guiSelection != i)
                  imageEffect.enabled = false;
              }
            }
            GUILayout.EndScrollView();
          }
          else
            GUILayout.Label("No 'Video Glitches' found.");

          GUILayout.FlexibleSpace();

          GUILayout.BeginVertical("box");
          {
#if USE_SLIDESHOW
            if (slideShow != null)
            {
              slideShow.changeTime = GUILayout.Toggle(slideShow.changeTime > 0.0f, "Auto slideshow") ? 5.0f : 0.0f;

              GUILayout.BeginHorizontal();
              {
                if (GUILayout.Button("Prev") == true)
                  slideShow.PrevPicture();

                if (GUILayout.Button("Next") == true)
                  slideShow.NextPicture();
              }
              GUILayout.EndHorizontal();
            }
#endif
            GUILayout.Label("F1 - Hide/Show gui.");
            GUILayout.Label("PageUp\nPageDown - Change effects.");
          }
          GUILayout.EndVertical();

          GUILayout.Space(guiMargen);

          if (Debug.isDebugBuild == true && GUILayout.Button(consoleShow ? "Close console" : "Open console") == true)
          {
            consoleShow = !consoleShow;

            scrollLog.y = Mathf.Infinity;
          }

          if (GUILayout.Button(@"Open Web") == true)
            Application.OpenURL(@"http://www.ibuprogames.com/2015/07/02/video-glitches/ ‎");

#if !UNITY_WEBPLAYER
        if (GUILayout.Button(@"Quit") == true)
          Application.Quit();
#endif
        }
        GUILayout.EndVertical();

        // Log console
        if (consoleShow == true)
        {
          GUILayout.BeginVertical("box", GUILayout.Width(Screen.width), GUILayout.Height(Screen.height / 4));
          {
            scrollLog = GUILayout.BeginScrollView(scrollLog, false, true);
            {
              for (int i = 0; i < logs.Count; ++i)
                GUILayout.Label(logs[i]);
            }
            GUILayout.EndScrollView();
          }
          GUILayout.EndVertical();
        }
      }
    }

    private void DrawCommonControls(ImageEffectBase imageEffect)
    {
      // Amount.
      GUILayout.BeginHorizontal();
      {
        GUILayout.Label("Amount", GUILayout.Width(70));
        imageEffect.amount = GUILayout.HorizontalSlider(imageEffect.amount, 0.0f, 1.0f);
      }
      GUILayout.EndHorizontal();
    }

    private void DrawCustomControls(ImageEffectBase imageEffect)
    {
      System.Type type = imageEffect.GetType();

      if (type == typeof(VideoGlitchShift))
      {
        VideoGlitchShift effect = imageEffect as VideoGlitchShift;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Amplitude", GUILayout.Width(60));
            effect.amplitude = GUILayout.HorizontalSlider(effect.amplitude, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Speed", GUILayout.Width(60));
            effect.speed = GUILayout.HorizontalSlider(effect.speed, 0.0f, 0.2f);
          }
          GUILayout.EndHorizontal();
        }
      }
      else if (type == typeof(VideoGlitchNoiseDigital))
      {
        VideoGlitchNoiseDigital effect = imageEffect as VideoGlitchNoiseDigital;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Threshold", GUILayout.Width(60));
            effect.threshold = GUILayout.HorizontalSlider(effect.threshold, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Max offset", GUILayout.Width(60));
            effect.maxOffset = GUILayout.HorizontalSlider(effect.maxOffset, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Threshold YUV", GUILayout.Width(60));
            effect.thresholdYUV = GUILayout.HorizontalSlider(effect.thresholdYUV, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();
        }
      }
      else if (type == typeof(VideoGlitchBlackWhiteDistortion))
      {
        VideoGlitchBlackWhiteDistortion effect = imageEffect as VideoGlitchBlackWhiteDistortion;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Steps", GUILayout.Width(60));
            effect.distortionSteps = GUILayout.HorizontalSlider(effect.distortionSteps, 1.0f, 10.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Speed", GUILayout.Width(60));
            effect.distortionSpeed = GUILayout.HorizontalSlider(effect.distortionSpeed, 0.0f, 10.0f);
          }
          GUILayout.EndHorizontal();
        }
      }
      else if (type == typeof(VideoGlitchRGBDisplay))
      {
        VideoGlitchRGBDisplay effect = imageEffect as VideoGlitchRGBDisplay;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Cell size", GUILayout.Width(60));
            effect.cellSize = (int)GUILayout.HorizontalSlider(effect.cellSize, 1.0f, 10.0f);
          }
          GUILayout.EndHorizontal();
        }
      }
      else if (type == typeof(VideoGlitchSpectrumOffset))
      {
        VideoGlitchSpectrumOffset effect = imageEffect as VideoGlitchSpectrumOffset;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Strength", GUILayout.Width(60));
            effect.strength = GUILayout.HorizontalSlider(effect.strength, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Steps", GUILayout.Width(60));
            effect.steps = (int)GUILayout.HorizontalSlider(effect.steps, 3.0f, 10.0f);
          }
          GUILayout.EndHorizontal();
        }
      }
      else if (type == typeof(VideoGlitchOldTape))
      {
        VideoGlitchOldTape effect = imageEffect as VideoGlitchOldTape;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Speed", GUILayout.Width(60));
            effect.noiseSpeed = GUILayout.HorizontalSlider(effect.noiseSpeed, 1.0f, 100.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Amplitude", GUILayout.Width(60));
            effect.noiseAmplitude = (int)GUILayout.HorizontalSlider(effect.noiseAmplitude, 1.0f, 100.0f);
          }
          GUILayout.EndHorizontal();
        }
      }
      else if (type == typeof(VideoGlitchVHSPause))
      {
        VideoGlitchVHSPause effect = imageEffect as VideoGlitchVHSPause;
        if (effect != null)
        {
          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Strength", GUILayout.Width(60));
            effect.strength = GUILayout.HorizontalSlider(effect.strength, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Color noise", GUILayout.Width(80));
            effect.colorNoise = GUILayout.HorizontalSlider(effect.colorNoise, 0.0f, 1.0f);
          }
          GUILayout.EndHorizontal();
        }
      }
    }

    private void LogMessageReceived(string logString, string stackTrace, LogType type)
    {
      if (type == LogType.Error)
      {
        logs.Add("[<color=red>ERROR</color>] " + logString);
        logs.Add("[STACKTRACE] " + stackTrace);
      }
      else if (type == LogType.Warning)
        logs.Add("[<color=yellow>WARNING</color>] " + logString);
      else
        logs.Add("[LOG] " + logString);
    }
  }
}