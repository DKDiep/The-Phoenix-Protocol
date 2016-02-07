using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(DeluxeEyeAdaptation))]
class DeluxeEyeAdaptationEditor : Editor
{
    Texture2D m_Logo;

    void OnEnable()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string path = AssetDatabase.GetAssetPath(script);
        string logoPath = Path.GetDirectoryName(path) + "/logo.png";
        m_Logo = (Texture2D)AssetDatabase.LoadAssetAtPath(logoPath, typeof(Texture2D));
        if (m_Logo != null)
            m_Logo.hideFlags = HideFlags.HideAndDontSave;
    }

    public override void OnInspectorGUI () 
    {
        DeluxeEyeAdaptation de = (DeluxeEyeAdaptation)target;
        Undo.RecordObject(de, "Deluxe Eye Adaptation");

        if (m_Logo != null)
        {
            Rect rect = GUILayoutUtility.GetRect(m_Logo.width, m_Logo.height);
            //GUI.DrawTexture(rect, m_Background);
            GUI.DrawTexture(rect, m_Logo, ScaleMode.ScaleToFit);
        }

        if (de.m_Logic != null)
        {
            EditorGUILayout.LabelField("Exposure", EditorStyles.boldLabel);
            de.m_Logic.m_MinimumExposure = DoSlider("Minimum Exposure", de.m_Logic.m_MinimumExposure, 0.1f, 2.0f);
            de.m_Logic.m_MaximumExposure = DoSlider("Maximum Exposure", de.m_Logic.m_MaximumExposure, 0.1f, 100.0f);
            de.m_Logic.m_ExposureOffset = DoSlider("Exposure Offset", de.m_Logic.m_ExposureOffset, -0.5f, 0.5f);
            de.m_Logic.m_BrightnessMultiplier = DoSlider("Exposure Multiplier", de.m_Logic.m_BrightnessMultiplier, 0.1f, 2.0f);
            
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Adaptation Speed", EditorStyles.boldLabel);
            de.m_Logic.m_AdaptationSpeedUp = DoSlider("Speed Up", de.m_Logic.m_AdaptationSpeedUp, 0.1f, 10.0f);
            de.m_Logic.m_AdaptationSpeedDown = DoSlider("Speed Down", de.m_Logic.m_AdaptationSpeedDown, 0.1f, 10.0f);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Histogram Analysis Range", EditorStyles.boldLabel);
            de.m_Logic.m_AverageThresholdMin = DoSlider("Histogram Min (%)", de.m_Logic.m_AverageThresholdMin, 0.25f, 1.0f);
            de.m_Logic.m_AverageThresholdMax = DoSlider("Histogram Max (%)", de.m_Logic.m_AverageThresholdMax, 0.25f, 1.0f);
            if (de.GetComponent<DeluxeTonemapper>() == null)
            {
                de.m_Logic.m_Range = DoSlider("Histogram Range", de.m_Logic.m_Range, 1.0f, 8.0f);
            }
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Optimization", EditorStyles.boldLabel);
            de.m_LowResolution = EditorGUILayout.Toggle("Low Resolution Buffer", de.m_LowResolution);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            de.m_ShowHistogram = EditorGUILayout.Toggle("Visualize Histogram", de.m_ShowHistogram);
            de.m_HistogramSize = DoSlider("Histogram Size", de.m_HistogramSize, 0.1f, 0.99f);
            
            if (de.m_Logic.m_AverageThresholdMax < de.m_Logic.m_AverageThresholdMin)
                de.m_Logic.m_AverageThresholdMax = de.m_Logic.m_AverageThresholdMin + 0.01f;

            if (de.m_Logic.m_MaximumExposure < de.m_Logic.m_MinimumExposure)
                de.m_Logic.m_MaximumExposure = de.m_Logic.m_MinimumExposure + 0.01f;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        if (Event.current.commandName == "UndoRedoPerformed")
        {
            de.OnDisable();
            de.OnEnable();
        }

    }

    [MenuItem("Component/Filmic Tonemapping/Add Eye Adaptation")]
    public static void AddPreset()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
            return;
        if (obj.GetComponent<Camera>() == null)
            return;

        obj.AddComponent<DeluxeEyeAdaptation>();
    }

    float DoSlider(string label, float value, float min, float max)
    {
        float v = value;
        EditorGUILayout.BeginHorizontal();
        v = Mathf.Clamp(EditorGUILayout.FloatField(label, v), min, max);
        v = GUILayout.HorizontalSlider(v, min, max);
        EditorGUILayout.EndHorizontal();

        return v;
    }
    


}
