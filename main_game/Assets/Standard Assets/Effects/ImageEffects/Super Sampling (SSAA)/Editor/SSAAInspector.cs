using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
#if (!UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2)
using UnityEditor.SceneManagement;
#endif

[CustomEditor(typeof(SuperSampling_SSAA))]

public class SSAAInspector : Editor
{
    public const float defaultValue = 2.0f;

    public const float MinScale = 1.1f;

    public const float MaxScale = 2f;

    public const float MinScaleHigh = 0.5f;

    public const float MaxScaleHigh = 4f;

    private const string screenshotExt = ".png";

    

    public override void OnInspectorGUI()
    {
        SuperSampling_SSAA ssaaTarget = (SuperSampling_SSAA)target;
        Camera c = ssaaTarget.GetComponent<Camera>();
        if (c == null)
        {
            if (GUILayout.Button("Add Camera"))
            {
                ssaaTarget.gameObject.AddComponent<Camera>();
            }
            else
            {
                return;
            }
        }

        if (c.targetTexture == null)
        {

            if (ssaaTarget.Scale < 0.5f)
                ssaaTarget.Scale = defaultValue;

            if (SSAA.internal_SSAA.scale < 1f)
                SSAA.internal_SSAA.ChangeScale(ssaaTarget.Scale);

            GUI.changed = false;

            GUIContent cnt = new GUIContent("HighScale (Caution!)");
            cnt.tooltip = "Unlocks 2+ multipliers Range. We recommend 1.25-2 for normal game usage. 2 is already very high.";
            ssaaTarget.unlocked = EditorGUILayout.Toggle(cnt, ssaaTarget.unlocked);
            
            float newScale = SSAA.internal_SSAA.scale;
            cnt = new GUIContent("Resolution multiplier");
            cnt.tooltip = "We recommend 1.25 - 2 for normal game usage";

            if (!ssaaTarget.unlocked)
                newScale = (float)Math.Round(EditorGUILayout.Slider(cnt, SSAA.internal_SSAA.scale, MinScale, MaxScale), 1);
            else
                newScale = (float)Math.Round(EditorGUILayout.Slider(cnt, SSAA.internal_SSAA.scale, MinScaleHigh, MaxScaleHigh), 1);

            if (Mathf.Abs(newScale - SSAA.internal_SSAA.scale) > 0.005f)
            {
                SSAA.internal_SSAA.ChangeScale(newScale);
                ssaaTarget.Scale = newScale;
            }

            int filterSelector = 0;
            switch (SSAA.internal_SSAA.Filter)
            {
                case SSAA.SSAAFilter.NearestNeighbor: filterSelector = 0; break;
                case SSAA.SSAAFilter.BilinearSharper: filterSelector = 1; break;
                case SSAA.SSAAFilter.BilinearDefault: filterSelector = 2; break;
                case SSAA.SSAAFilter.BilinearHigh: filterSelector = 3; break;
                case SSAA.SSAAFilter.LanczosHigh: filterSelector = 4; break;
            }

            string[] options = { "Nearest Neighbor      (0.5 - 2)",
                                 "Bilinear Sharper          (1.25 - 2)",
                                 "Bilinear Default          (2 - 3)",
                                 "Bilinear High              (3 - 4)",
                                 "Lanczos High             (3 - 4)" };

            filterSelector = EditorGUILayout.Popup("Filter Algorithm", filterSelector, options);

            switch(filterSelector)
            {
                case 0: SSAA.internal_SSAA.Filter = SSAA.SSAAFilter.NearestNeighbor; break;
                case 1: SSAA.internal_SSAA.Filter = SSAA.SSAAFilter.BilinearSharper; break;
                case 2: SSAA.internal_SSAA.Filter = SSAA.SSAAFilter.BilinearDefault; break;
                case 3: SSAA.internal_SSAA.Filter = SSAA.SSAAFilter.BilinearHigh; break;
                case 4: SSAA.internal_SSAA.Filter = SSAA.SSAAFilter.LanczosHigh; break;
            }
            ssaaTarget.Filter = SSAA.internal_SSAA.Filter;

            bool usingHigh = false;
            if (ssaaTarget.renderTextureFormat == RenderTextureFormat.ARGBHalf)
                usingHigh = true;

            cnt = new GUIContent("16Bit Texture Format");
            cnt.tooltip = "Use 16 Bit Color Depth per Channel instead of 8 Bit";
            usingHigh = EditorGUILayout.Toggle(cnt, usingHigh);

            if (ssaaTarget.renderTextureFormat == RenderTextureFormat.ARGBHalf && !usingHigh)
                ssaaTarget.renderTextureFormat = RenderTextureFormat.ARGB32;
            else if (ssaaTarget.renderTextureFormat == RenderTextureFormat.ARGB32 && usingHigh)
                ssaaTarget.renderTextureFormat = RenderTextureFormat.ARGBHalf;

            if (GUI.changed)
            {
                ssaaTarget.enabled = false;
                ssaaTarget.enabled = true;
            }
        }
        else
        {
            if (ssaaTarget.Scale < 0.5f)
                ssaaTarget.Scale = defaultValue;
            

            GUI.changed = false;


            EditorGUILayout.LabelField("");

            GUIContent cnt = new GUIContent("HighScale (Caution!)");
            cnt.tooltip = "Unlocks 2+ multipliers Range. We recommend 1.25-2 for normal game usage. 2 is already very high.";
            ssaaTarget.unlocked = EditorGUILayout.Toggle(cnt, ssaaTarget.unlocked);

            cnt = new GUIContent("Resolution multiplier");
            cnt.tooltip = "We recommend 1.25 - 2 for normal game usage";

            float newScale = ssaaTarget.Scale;
            if (!ssaaTarget.unlocked)
                newScale = (float)Math.Round(EditorGUILayout.Slider(cnt, ssaaTarget.Scale, MinScale, MaxScale), 1);
            else
                newScale = (float)Math.Round(EditorGUILayout.Slider(cnt, ssaaTarget.Scale, MinScaleHigh, MaxScaleHigh), 1);

            if (Mathf.Abs(newScale - ssaaTarget.Scale) > 0.005f)
            {
                ssaaTarget.Scale = newScale;
            }

            int filterSelector = 0;
            switch (ssaaTarget.Filter)
            {
                case SSAA.SSAAFilter.NearestNeighbor: filterSelector = 0; break;
                case SSAA.SSAAFilter.BilinearSharper: filterSelector = 1; break;
                case SSAA.SSAAFilter.BilinearDefault: filterSelector = 2; break;
                case SSAA.SSAAFilter.BilinearHigh: filterSelector = 3; break;
                case SSAA.SSAAFilter.LanczosHigh: filterSelector = 4; break;
            }

            string[] options = { "Nearest Neighbor      (0.5 - 2)",
                                 "Bilinear Sharper          (1.25 - 2)",
                                 "Bilinear Default          (2 - 3)",
                                 "Bilinear High              (3 - 4)",
                                 "Lanczos High             (3 - 4)" };

            filterSelector = EditorGUILayout.Popup("Filter Algorithm", filterSelector, options);

            switch (filterSelector)
            {
                case 0: ssaaTarget.Filter = SSAA.SSAAFilter.NearestNeighbor; break;
                case 1: ssaaTarget.Filter = SSAA.SSAAFilter.BilinearSharper; break;
                case 2: ssaaTarget.Filter = SSAA.SSAAFilter.BilinearDefault; break;
                case 3: ssaaTarget.Filter = SSAA.SSAAFilter.BilinearHigh; break;
                case 4: ssaaTarget.Filter = SSAA.SSAAFilter.LanczosHigh; break;
            }

            bool usingHigh = false;
            if (ssaaTarget.renderTextureFormat == RenderTextureFormat.ARGBHalf)
                usingHigh = true;

            cnt = new GUIContent("16Bit Texture Format");
            cnt.tooltip = "Use 16 Bit Color Depth per Channel instead of 8 Bit";
            usingHigh = EditorGUILayout.Toggle(cnt, usingHigh);

            if (ssaaTarget.renderTextureFormat == RenderTextureFormat.ARGBHalf && !usingHigh)
                ssaaTarget.renderTextureFormat = RenderTextureFormat.ARGB32;
            else if (ssaaTarget.renderTextureFormat == RenderTextureFormat.ARGB32 && usingHigh)
                ssaaTarget.renderTextureFormat = RenderTextureFormat.ARGBHalf;

            if (GUI.changed)
            {
                ssaaTarget.enabled = false;
                ssaaTarget.enabled = true;
            }
        }
        
        EditorGUILayout.LabelField("");
        
        ssaaTarget.showScreenshot = EditorGUILayout.Foldout(ssaaTarget.showScreenshot, "Screenshot Menu");

        if (ssaaTarget.showScreenshot)
        {
            Vector2 v2 = new Vector2(ssaaTarget.screenshotWidth, ssaaTarget.screenshotHeight);
            v2 = EditorGUILayout.Vector2Field("Resolution", v2);
            ssaaTarget.screenshotWidth = Mathf.RoundToInt(v2.x);
            ssaaTarget.screenshotHeight = Mathf.RoundToInt(v2.y);

            string[] options = { "x4 Lanczos", "x3 Bilinear", "x2 Nearest Neighbor", "Native" };
            ssaaTarget.scalingSelector = EditorGUILayout.Popup("Scaling", ssaaTarget.scalingSelector, options);
            
            ssaaTarget.screenshotScale = (float)(options.Length - ssaaTarget.scalingSelector);
            
            switch (options.Length - ssaaTarget.scalingSelector)
            {
                case 4: ssaaTarget.screenshotFilter = SSAA.SSAAFilter.LanczosHigh; break;
                case 3: ssaaTarget.screenshotFilter = SSAA.SSAAFilter.BilinearDefault; break;
                case 2: ssaaTarget.screenshotFilter = SSAA.SSAAFilter.NearestNeighbor; break;
                case 1: ssaaTarget.screenshotFilter = SSAA.SSAAFilter.NearestNeighbor; break;
            }
            GUIContent cnt = new GUIContent("Path/Name");
            cnt.tooltip = "You can add folders as well: /Assets/Screenshots/MyImage";
            ssaaTarget.relativeScreenshotPath = EditorGUILayout.TextField(cnt, ssaaTarget.relativeScreenshotPath);

            //if the user added the extension himself, we want to make sure its ".png"
            if (ssaaTarget.relativeScreenshotPath.Contains(".png") || ssaaTarget.relativeScreenshotPath.Contains(".jpg") || ssaaTarget.relativeScreenshotPath.Contains(".tga"))
            {
                Debug.LogWarning("Don't add the extension manually! .png will be added automatically");
            }
            

            if (Application.isPlaying && GUILayout.Button("Save Screenshot"))
            {
                ssaaTarget.TakeHighScaledShot(ssaaTarget.screenshotWidth, ssaaTarget.screenshotHeight, ssaaTarget.screenshotScale, ssaaTarget.screenshotFilter, "/../" + ssaaTarget.relativeScreenshotPath);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            else if(!Application.isPlaying)
            {
                GUILayout.Label("Screenshots work in Play and Pause Mode", GUI.skin.button);
            }
        }
        EditorGUILayout.LabelField("");
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);

#if (UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            if(!EditorApplication.isPlaying)
                EditorApplication.MarkSceneDirty();
#elif (!UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6)
            if (!EditorApplication.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();
#endif
        }
    }  
}
