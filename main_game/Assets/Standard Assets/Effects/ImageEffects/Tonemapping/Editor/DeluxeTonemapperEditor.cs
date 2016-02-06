using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(DeluxeTonemapper))]
class DeluxeTonemapperEditor : Editor
{
    Texture2D m_Logo;

    enum Presets
    {
        ChoosePreset,
        LDR_Neutral,
        LDR_Soft,
        LDR_Contrasted1,
        LDR_Contrasted2,
        HDR_Neutral,
        HDR_Soft,
        HDR_Contrasted1,
        HDR_Contrasted2
    }

    void OnEnable()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string path = AssetDatabase.GetAssetPath(script);
        string logoPath = Path.GetDirectoryName(path) + "/logo.png";
        //m_Logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Paroxe/FilmicTonemappingDeluxe/Editor/logo.png", typeof(Texture2D));
        m_Logo = (Texture2D)AssetDatabase.LoadAssetAtPath(logoPath, typeof(Texture2D));

        if (m_Logo != null)
            m_Logo.hideFlags = HideFlags.HideAndDontSave;
    }

    public override void OnInspectorGUI () 
    {

        DeluxeTonemapper dt = (DeluxeTonemapper)target;
        Undo.RecordObject(dt, "Deluxe Tonemapper");

        if (m_Logo != null)
        {
            Rect rect = GUILayoutUtility.GetRect(m_Logo.width, m_Logo.height);
            //GUI.DrawTexture(rect, m_Background);
            GUI.DrawTexture(rect, m_Logo, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.LabelField("Filmic Curve", EditorStyles.boldLabel);
        dt.m_MainCurve.OnGUI(dt.m_Mode);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Curve Settings", EditorStyles.boldLabel);

        Presets preset = (Presets)EditorGUILayout.EnumPopup("Presets", Presets.ChoosePreset);
        if (preset != Presets.ChoosePreset)
            SetPreset(preset, dt);

        dt.m_Mode = (DeluxeTonemapper.Mode)EditorGUILayout.EnumPopup("Mode", dt.m_Mode);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Color Correction", EditorStyles.boldLabel);
        dt.m_Tint = EditorGUILayout.ColorField("Tint", dt.m_Tint);

        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

    }

    void SetPreset(Presets preset, DeluxeTonemapper tonemapper)
    {
        if (preset == Presets.LDR_Neutral)
            Set(tonemapper, 0.0f, 0.5f, 0.0f, 0.0f, 1.0f);
        else if (preset == Presets.LDR_Contrasted1)
            Set(tonemapper, 0.518f, 0.1f, 0.0f, 0.54f, 1.0f);
        else if (preset == Presets.LDR_Contrasted2)
            Set(tonemapper, 0.5f, 0.1f, 0.0f, 0.5f, 1.0f);
        else if (preset == Presets.LDR_Soft)
            Set(tonemapper, -0.2f, 0.2f, 0.0f, 0.4f, 1.0f);
        else if (preset == Presets.HDR_Neutral)
            Set(tonemapper, 0.0f, 0.5f, 0.0f, 0.0f, 4.0f);
        else if (preset == Presets.HDR_Soft)
            Set(tonemapper, 0.127f, 0.3f, 0.0f, 0.833f, 4.0f);
        else if (preset == Presets.HDR_Contrasted1)
            Set(tonemapper, 0.402f, 0.04f, 0.0f, 0.9f, 4.0f);
        else if (preset == Presets.HDR_Contrasted2)
            Set(tonemapper, 0.385f, 0.12f, 0.0f, 0.883f, 4.0f);
    }

    void Set(DeluxeTonemapper dt, float t, float c, float b, float s, float w)
    {
        dt.m_MainCurve.m_ToeStrength = t;
        dt.m_MainCurve.m_CrossOverPoint = c;
        dt.m_MainCurve.m_BlackPoint = b;
        dt.m_MainCurve.m_ShoulderStrength = s;
        dt.m_MainCurve.m_WhitePoint = w;
        dt.UpdateCoefficients();
    }


    [MenuItem("Component/Filmic Tonemapping/Add Tonemapper")]
    public static void AddPreset()
    {
        AddTonamappingComponent();
    }


    static void AddTonamappingComponent()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
            return;
        if (obj.GetComponent<Camera>() == null)
            return;

        obj.AddComponent<DeluxeTonemapper>();
    }

    


}
