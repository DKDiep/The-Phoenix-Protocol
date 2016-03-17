using UnityEngine;
using System.Collections;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class EyeAdaptationLogic
{
    [SerializeField]
    public float m_MinimumExposure = 0.4f;

    [SerializeField]
    public float m_MaximumExposure = 8.0f;

    [SerializeField]
    public float m_Range = 4.0f;

    [SerializeField]
    public float m_AdaptationSpeedUp = 1.0f;

    [SerializeField]
    public float m_AdaptationSpeedDown = 1.0f;

    [SerializeField]
    public float m_BrightnessMultiplier = 1.0f;

    [SerializeField]
    public float m_ExposureOffset = 0.0f;

    [SerializeField]
    public float m_AverageThresholdMin = 0.8f;

    [SerializeField]
    public float m_AverageThresholdMax = 0.98f;

    private const float m_HistMin = 0.005f;
    [SerializeField]
    float m_HistLogMin = Mathf.Log(m_HistMin,2);

    [SerializeField]
    float m_HistLogMax = Mathf.Log(5.0f,2);

    [SerializeField]
    public Vector4 m_HistCoefs = new Vector4();


    // NOTE: Must always be a multiple of 2
    private const int m_BinCount = 64;
    private const float m_BinTexStep = 2.0f / m_BinCount;
    private const float m_BinTexStart = -m_BinTexStep * m_BinCount * 0.5f + m_BinTexStep * 0.5f;

    private Mesh[] m_Meshes;

    public int m_CurrentWidth;
    public int m_CurrentHeight;

    const int HIST_COUNT = 1;
    int m_CurrentHistogram = 0;
    public RenderTexture[] m_HistogramList = new RenderTexture[HIST_COUNT];
    public RenderTexture m_BrightnessRT;
    RenderTexture m_PreviousBrightnessRT;
    Texture2D m_LocalHistogram;
    Texture2D m_LocalBrightness;

    public bool m_SetTargetExposure = false;


    private const int m_Height = 1;

    void RenderHistogram(Material histogramMaterial)
    {
        ComputeHistogramCoefs();

        RenderTexture currentHistogram = m_HistogramList[m_CurrentHistogram];
        RenderTexture.active = currentHistogram;
        GL.Clear(true, true, Color.black);

        histogramMaterial.SetFloat("_ValueRange", m_Range);
        histogramMaterial.SetFloat("_StepSize", m_BinTexStep);
        histogramMaterial.SetFloat("_BinCount", m_BinCount);
        histogramMaterial.SetVector("_HistogramCoefs", m_HistCoefs);

        if (histogramMaterial.SetPass(0))
        {
            for (int i = 0; i < m_Meshes.Length; ++i)
                Graphics.DrawMeshNow(m_Meshes[i], Matrix4x4.identity);
        }
        else
        {
            //Debug.LogError("Can't render histogram mesh");
        }

        RenderTexture readHistogram = m_HistogramList[0];

        float nbPixeldd = m_CurrentWidth * m_CurrentHeight;
        histogramMaterial.SetFloat("_90PixelCount", nbPixeldd * m_AverageThresholdMin);
        histogramMaterial.SetFloat("_98PixelCount", nbPixeldd * m_AverageThresholdMax);
        histogramMaterial.SetVector("_Coefs", m_HistCoefs);
        histogramMaterial.SetTexture("_HistogramTex", readHistogram);
        histogramMaterial.SetTexture("_PreviousBrightness", m_PreviousBrightnessRT);
        histogramMaterial.SetFloat("_ExposureOffset", -1.0f * m_ExposureOffset);

        float dt = Time.deltaTime;
        histogramMaterial.SetVector("_MinMaxSpeedDt", new Vector4(1.0f / m_MaximumExposure, 1.0f / m_MinimumExposure, m_AdaptationSpeedDown * Time.deltaTime, m_AdaptationSpeedUp * Time.deltaTime));
        readHistogram.filterMode = FilterMode.Point;

        RenderTexture.active = m_BrightnessRT;
        GL.Clear(true, true, Color.black);

        Graphics.Blit(readHistogram, m_BrightnessRT, histogramMaterial, 1);


        RenderTexture tmp = m_BrightnessRT;
        m_BrightnessRT = m_PreviousBrightnessRT;
        m_PreviousBrightnessRT = tmp;


    }
    
    void ComputeHistogramCoefs()
    {
        m_HistLogMax = Mathf.Log(m_Range, 2.0f);
        float delta = m_HistLogMax - m_HistLogMin;
        float coef0 = 1.0f / delta;
        float coef1 = -(coef0*m_HistLogMin);
        m_HistCoefs = new Vector4(coef0, coef1, m_HistMin, m_Range);
    }

    public void ComputeExposure(int screenWidth, int screenHeight, Material histogramMaterial)
    {


        for (int i = 0; i < HIST_COUNT; ++i)
        {
            if (m_HistogramList[i] == null)
            {
                m_HistogramList[i] = new RenderTexture(m_BinCount, m_Height, 0, RenderTextureFormat.ARGBFloat);
                m_HistogramList[i].hideFlags = HideFlags.HideAndDontSave;
            }
        }

        if (m_BrightnessRT == null)
        {
            m_BrightnessRT = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat);
            m_BrightnessRT.hideFlags = HideFlags.HideAndDontSave;

            RenderTexture.active = m_BrightnessRT;
            GL.Clear(false, true, Color.white);
        }

        if (m_PreviousBrightnessRT == null)
        {
            m_PreviousBrightnessRT = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat);
            m_PreviousBrightnessRT.hideFlags = HideFlags.HideAndDontSave;
            RenderTexture.active = m_PreviousBrightnessRT;
            GL.Clear(false, true, Color.white);
        }

        /*
        if (m_LocalHistogram == null)
        {
#if UNITY_EDITOR
            EditorUtility.UnloadUnusedAssetsImmediate();
#endif
            //Debug.Log("Try to create T2D");
            m_LocalHistogram = new Texture2D(m_BinCount, m_Height, TextureFormat.RGBAFloat, false, true);
            m_LocalHistogram.hideFlags = HideFlags.HideAndDontSave;
        }

        if (m_LocalBrightness == null)
        {
#if UNITY_EDITOR
            EditorUtility.UnloadUnusedAssetsImmediate();
#endif
            //Debug.Log("Try to create T2D");
            m_LocalBrightness = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);
            m_LocalBrightness.hideFlags = HideFlags.HideAndDontSave;
        }

         */ 
         
        RebuildMeshIfNeeded(screenWidth, screenHeight);
        RenderHistogram(histogramMaterial);
    }

    public void RebuildMeshIfNeeded(int width, int height)
    {
        if (m_CurrentWidth == width && m_CurrentHeight == height && m_Meshes != null)
            return;

        if (m_Meshes != null)
            foreach (Mesh m in m_Meshes)
            {
                GameObject.DestroyImmediate(m, true);
            }
        m_Meshes = null;

        BuildMeshes(width, height);
    }

    public void BuildMeshes(int width, int height)
    {
        //Debug.Log("Rebuilding mesh");

        int maxTriangles = 65000 / 3;
        int totalTriangles = width * height;
        int meshCount = Mathf.CeilToInt((1.0f * totalTriangles) / (1.0f * maxTriangles));
        m_Meshes = new Mesh[meshCount];
        int currentQuads = totalTriangles;

        m_CurrentWidth = width;
        m_CurrentHeight = height;
        int currentPixel = 0;

        float spriteWidth = 2.0f / m_BinCount;
        float spriteHeigth = 2.0f;

        Vector2 halfPixelSize = new Vector2(1.0f / m_CurrentWidth * 0.5f, 1.0f / m_CurrentHeight * 0.5f);

        for (int m = 0; m < meshCount; ++m)
        {
            Mesh currentMesh = new Mesh();
            currentMesh.hideFlags = HideFlags.HideAndDontSave;
            //int pixelCount = width * height;

            int nbQuads = currentQuads;
            if (currentQuads > maxTriangles)
                nbQuads = maxTriangles;
            currentQuads -= nbQuads;

            Vector3[] vertices = new Vector3[nbQuads * 3];
            int[] triangles = new int[nbQuads * 3];
            Vector2[] uv0 = new Vector2[nbQuads * 3];
            Vector2[] uv1 = new Vector2[nbQuads * 3];
            Vector3[] normals = new Vector3[nbQuads * 3];
            Color[] colors = new Color[nbQuads * 3];

            for (int i = 0; i < nbQuads; ++i)
            {
                int x = currentPixel % width;
                int y = (currentPixel - x) / width;
                SetupSprite(i, x, y, vertices, triangles, uv0, uv1, normals, colors, new Vector2((float)x / (float)width + halfPixelSize.x, 1.0f - (((float)y / (float)height) + halfPixelSize.y)), spriteWidth * 0.5f, spriteHeigth * 0.5f);
                currentPixel++;
            }

            currentMesh.vertices = vertices;
            currentMesh.triangles = triangles;
            currentMesh.colors = null;
            currentMesh.uv = uv0;
            currentMesh.uv2 = null;
            currentMesh.normals = normals;
            currentMesh.RecalculateBounds();
            currentMesh.UploadMeshData(true);
            m_Meshes[m] = currentMesh;
        }

        //Debug.Log("Meshcount=" + m_Meshes.Length);
    }

    public void SetupSprite(int idx, int x, int y, Vector3[] vertices, int[] triangles, Vector2[] uv0, Vector2[] uv1, Vector3[] normals, Color[] colors, Vector2 targetPixelUV, float halfWidth, float halfHeight)
    {
        int vIdx = idx * 3;
        int tIdx = idx * 3;

        triangles[tIdx + 0] = vIdx + 0;
        triangles[tIdx + 1] = vIdx + 2;
        triangles[tIdx + 2] = vIdx + 1;

        float offset = m_BinTexStep * 0;

        vertices[vIdx + 0] = new Vector3(-1.0f + offset, -1.0f, 0.0f);
        vertices[vIdx + 2] = new Vector3(-1.0f + offset, 1.0f, 0.0f);
        vertices[vIdx + 1] = new Vector3(-1.0f + m_BinTexStep * 1.5f + offset, 1.0f, 0.0f);


        normals[vIdx + 0] = -Vector3.forward;
        normals[vIdx + 1] = -Vector3.forward;
        normals[vIdx + 2] = -Vector3.forward;

        uv0[vIdx + 0] = targetPixelUV;
        uv0[vIdx + 1] = targetPixelUV;
        uv0[vIdx + 2] = targetPixelUV;
    }

    public void ClearMeshes()
    {
        if (m_Meshes != null)
            foreach (Mesh m in m_Meshes)
            {
                GameObject.DestroyImmediate(m, true);
            }
        m_Meshes = null;


        for (int i = 0; i < HIST_COUNT; ++i)
        {
            if (m_HistogramList[i] == null)
            {
                GameObject.DestroyImmediate(m_HistogramList[i]);
                m_HistogramList[i] = null;
            }
        }

        if (m_BrightnessRT != null)
        {
            GameObject.DestroyImmediate(m_BrightnessRT);
            m_BrightnessRT = null;
        }

        if (m_PreviousBrightnessRT != null)
        {
            GameObject.DestroyImmediate(m_PreviousBrightnessRT);
            m_PreviousBrightnessRT = null;
        }

        if (m_LocalHistogram != null)
        {
            GameObject.DestroyImmediate(m_LocalHistogram);
            m_LocalHistogram = null;
        }

        if (m_LocalBrightness != null)
        {
            GameObject.DestroyImmediate(m_LocalBrightness);
            m_LocalBrightness = null;
        }

    }
}
