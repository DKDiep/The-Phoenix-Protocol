using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EyeAdaptation : MonoBehaviour 
{
    [SerializeField]
    public DeluxeEyeAdaptationLogic m_Logic;


    [SerializeField]
    public bool m_LowResolution = false;

    [SerializeField]
    public bool m_ShowHistogram = false;

    [SerializeField]
    public float m_HistogramSize = 0.3f; 

    Material m_HistogramMaterial;
    Shader m_HistogramShader;

    Material m_BrightnessMaterial;
    Shader m_BrightnessShader;

    private Material CreateMaterial(Shader shader)
    {
        if (!shader)
            return null;
        Material m = new Material(shader);
        m.hideFlags = HideFlags.HideAndDontSave;
        return m;
    }

    private void DestroyMaterial(Material mat)
    {
        if (mat)
        {
            DestroyImmediate(mat);
            mat = null;
        }
    }

    public void OnDisable()
    {
        if (m_Logic != null)
        {
            m_Logic.ClearMeshes();
        }
        DestroyMaterial(m_HistogramMaterial); m_HistogramMaterial = null; m_HistogramShader = null;
        DestroyMaterial(m_BrightnessMaterial); m_BrightnessMaterial = null; m_BrightnessShader = null;
    }

    private void CreateMaterials()
    {
        if (m_HistogramShader == null)
            m_HistogramShader = Shader.Find("Hidden/Deluxe/EyeAdaptation");
        if (m_HistogramMaterial == null && m_HistogramShader != null && m_HistogramShader.isSupported)
            m_HistogramMaterial = CreateMaterial(m_HistogramShader);

        if (m_BrightnessShader == null)
            m_BrightnessShader = Shader.Find("Hidden/Deluxe/EyeAdaptationBright");
        if (m_BrightnessMaterial == null && m_BrightnessShader != null && m_BrightnessShader.isSupported)
            m_BrightnessMaterial = CreateMaterial(m_BrightnessShader);
        else
        {
            if (m_BrightnessShader == null)
                Debug.LogError("Cant find brightness shader");
            else if (!m_BrightnessShader.isSupported)
                Debug.LogError("Brightness shader unsupported");
        }
    }

    public void OnEnable()
    {
        CreateMaterials();
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        CreateMaterials();

        if (m_Logic == null)
            m_Logic = new DeluxeEyeAdaptationLogic();

        DeluxeTonemapper tonemapper = GetComponent<DeluxeTonemapper>();
        if (tonemapper != null)
            m_Logic.m_Range = tonemapper.m_MainCurve.m_WhitePoint;


        RenderTexture ds0 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);
        RenderTexture ds1 = RenderTexture.GetTemporary(ds0.width / 2, ds0.height / 2, 0, source.format);

        if (m_BrightnessMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        m_BrightnessMaterial.SetTexture("_UpTex", source);
        source.filterMode = FilterMode.Bilinear;
        m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1.0f / source.width * 0.5f, 1.0f / source.height * 0.5f));
        Graphics.Blit(source, ds0, m_BrightnessMaterial, 1);

        m_BrightnessMaterial.SetTexture("_UpTex", ds0);
        ds0.filterMode = FilterMode.Bilinear;
        m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1.0f / ds0.width * 0.5f, 1.0f / ds0.height * 0.5f));
        Graphics.Blit(ds0, ds1, m_BrightnessMaterial, 1);

        source.filterMode = FilterMode.Point;
        if (m_LowResolution)
        {
            RenderTexture ds2 = RenderTexture.GetTemporary(ds1.width / 2, ds1.height / 2, 0, source.format);
            m_BrightnessMaterial.SetTexture("_UpTex", ds1);
            ds1.filterMode = FilterMode.Bilinear;
            m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1.0f / ds1.width * 0.5f, 1.0f / ds1.height * 0.5f));
            Graphics.Blit(ds1, ds2, m_BrightnessMaterial, 1);

            m_HistogramMaterial.SetTexture("_FrameTex", ds2);
            m_Logic.ComputeExposure(ds2.width, ds2.height, m_HistogramMaterial);


            RenderTexture.ReleaseTemporary(ds2);
        }
        else
        {
            m_HistogramMaterial.SetTexture("_FrameTex", ds1);
            m_Logic.ComputeExposure(ds1.width, ds1.height, m_HistogramMaterial);
        }

        RenderTexture.ReleaseTemporary(ds0);
        RenderTexture.ReleaseTemporary(ds1);

        m_BrightnessMaterial.SetFloat("_BrightnessMultiplier", m_Logic.m_BrightnessMultiplier);
        m_BrightnessMaterial.SetTexture("_BrightnessTex", m_Logic.m_BrightnessRT);
        m_BrightnessMaterial.SetTexture("_ColorTex", source);

        if (m_ShowHistogram)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            Graphics.Blit(source, tmp, m_BrightnessMaterial, 0);

            m_BrightnessMaterial.SetTexture("_Histogram", m_Logic.m_HistogramList[0]);
            m_BrightnessMaterial.SetTexture("_ColorTex", tmp);
            m_BrightnessMaterial.SetFloat("_LuminanceRange", m_Logic.m_Range);
            m_BrightnessMaterial.SetVector("_HistogramCoefs", m_Logic.m_HistCoefs);
            m_BrightnessMaterial.SetVector("_MinMax", new Vector4(0.01f, m_HistogramSize, 0.01f, m_HistogramSize));
            m_BrightnessMaterial.SetFloat("_TotalPixelNumber", m_Logic.m_CurrentWidth * m_Logic.m_CurrentHeight);
            Graphics.Blit(tmp, destination, m_BrightnessMaterial, 2);
 
            RenderTexture.ReleaseTemporary(tmp);
        }
        else
            Graphics.Blit(source, destination, m_BrightnessMaterial, 0);



    }
}
