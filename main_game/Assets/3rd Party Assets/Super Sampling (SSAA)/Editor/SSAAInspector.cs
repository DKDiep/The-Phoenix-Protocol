using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CustomEditor(typeof(SuperSampling_SSAA))]



public class SSAAInspector : Editor
{
    public const float defaultValue = 2.0f;

    public const float MinScale = 1.1f;

    public const float MaxScale = 2f;

    public const float MinScaleHigh = 0.5f;

    public const float MaxScaleHigh = 4f;

    public string ScreenshotPathName = "SSAAScreenshot";

    private string screenshotExt = ".png";


    public override void OnInspectorGUI()
    {
        SuperSampling_SSAA ssaaTarget = (SuperSampling_SSAA)target;

        if (ssaaTarget.Scale < 0.5f)
            ssaaTarget.Scale = defaultValue;

        if (SSAA.internal_SSAA.scale < 1f)
            SSAA.internal_SSAA.ChangeScale(ssaaTarget.Scale);

        GUI.changed = false;

		
		EditorGUILayout.LabelField("");

        ssaaTarget.unlocked = EditorGUILayout.Toggle("HighScale (Caution!)", ssaaTarget.unlocked);


        float newScale = SSAA.internal_SSAA.scale;
        if (!ssaaTarget.unlocked)
            newScale = (float)Math.Round(EditorGUILayout.Slider("Resolution multiplier", SSAA.internal_SSAA.scale, MinScale, MaxScale), 1);
        else
            newScale = (float)Math.Round(EditorGUILayout.Slider("Resolution multiplier", SSAA.internal_SSAA.scale, MinScaleHigh, MaxScaleHigh), 1);

        if (Mathf.Abs(newScale - SSAA.internal_SSAA.scale) > 0.005f)
        {
            SSAA.internal_SSAA.ChangeScale(newScale);
            ssaaTarget.Scale = newScale;
        }

        SSAA.internal_SSAA.Filter = (SSAA.SSAAFilter)EditorGUILayout.EnumPopup("Filter", SSAA.internal_SSAA.Filter);
        ssaaTarget.Filter = SSAA.internal_SSAA.Filter;

        ssaaTarget.UseDynamicOutputResolution = EditorGUILayout.Toggle("Dynamic Output Resolution", ssaaTarget.UseDynamicOutputResolution);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
#if UNITY_5_0 || UNITY_5_1
            EditorApplication.MarkSceneDirty();
#endif
        }



        EditorGUILayout.LabelField("");



		EditorGUILayout.LabelField("• Screenshots are saved to your top Project folder.");
        if (GUILayout.Button("Save .PNG Screenshot"))
        {
            SSAA.internal_SSAA.SaveSuperSampledToPNG(ScreenshotPathName + screenshotExt);
        }

        EditorGUILayout.LabelField("");
    }
}
