using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeluxeTonemapper : MonoBehaviour 
{
    [SerializeField]
    public FilmicCurve m_MainCurve = new FilmicCurve();

    [SerializeField]
    public Color m_Tint = new Color(1,1,1,1);

    [SerializeField]
    public Mode m_Mode;


    private Mode m_LastMode;
    Material m_Material;
    Shader m_Shader;


    public enum Mode
    {
        Color,
        Luminance,
        ExtendedLuminance
    }


    public Shader TonemappingShader
    {
        get { return m_Shader; }
    }

    private void DestroyMaterial(Material mat)
    {
        if (mat)
        {
            DestroyImmediate(mat);
            mat = null;
        }
    }

    private void CreateMaterials()
    {
        if (m_Shader == null)
        {
            if (m_Mode == Mode.Color)
                m_Shader = Shader.Find("Hidden/Deluxe/TonemapperColor");
            if (m_Mode == Mode.Luminance)
                m_Shader = Shader.Find("Hidden/Deluxe/TonemapperLuminosity");
            if (m_Mode == Mode.ExtendedLuminance)
                m_Shader = Shader.Find("Hidden/Deluxe/TonemapperLuminosityExtended");
        }

        if (m_Material == null && m_Shader != null && m_Shader.isSupported)
        {
            m_Material = CreateMaterial(m_Shader);
        }
    }

    private Material CreateMaterial(Shader shader)
    {
        if (!shader)
            return null;
        Material m = new Material(shader);
        m.hideFlags = HideFlags.HideAndDontSave;
        return m;
    }

    void OnDisable()
    {
        DestroyMaterial(m_Material); m_Material = null; m_Shader = null;
    }

    public void StoreK()
    {
        m_MainCurve.StoreK();
    }

    public void UpdateCoefficients()
    {
        StoreK();
        m_MainCurve.UpdateCoefficients();
        if (m_Material == null)
            return;

        m_Material.SetFloat("_K", m_MainCurve.m_k);
        m_Material.SetFloat("_Crossover", m_MainCurve.m_CrossOverPoint);
        m_Material.SetVector("_Toe", m_MainCurve.m_ToeCoef);
        m_Material.SetVector("_Shoulder", m_MainCurve.m_ShoulderCoef);
        m_Material.SetVector("_Tint", m_Tint);
        m_Material.SetFloat("_LuminosityWhite", m_MainCurve.m_LuminositySaturationPoint * m_MainCurve.m_LuminositySaturationPoint);
    }

    public void ReloadShaders()
    {
        OnDisable();
    }

    void OnEnable()
    {
        if (m_MainCurve == null)
            m_MainCurve = new FilmicCurve();
        CreateMaterials();
        UpdateCoefficients();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_Mode != m_LastMode)
            ReloadShaders();
        m_LastMode = m_Mode;

        CreateMaterials();
        UpdateCoefficients();

        Graphics.Blit(source, destination, m_Material);

        
    }
}
