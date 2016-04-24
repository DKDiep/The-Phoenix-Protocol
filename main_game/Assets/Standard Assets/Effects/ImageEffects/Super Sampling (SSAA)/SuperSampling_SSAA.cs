using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SSAA;
using System.IO;

#pragma warning disable 219

public class SuperSampling_SSAA : MonoBehaviour
{
    public float Scale = 0f;

    public bool unlocked = false;
    
    public SSAAFilter Filter = SSAAFilter.NearestNeighbor;

    public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBHalf;

    public bool showScreenshot = false;

    public int screenshotWidth = 1920;

    public int screenshotHeight = 1080;

    public float screenshotScale = 2f;

    public int scalingSelector = 0;

    public SSAA.SSAAFilter screenshotFilter = SSAA.SSAAFilter.NearestNeighbor;

    public string relativeScreenshotPath = "/Assets/MyImage";


    void OnEnable()
    {
        Camera cam = GetComponent<Camera>();
        if(cam == null)
        {
            Debug.LogWarning("No Camera attached!");
            return;
        }

        
        if (cam.targetTexture == null)
        {
            SSAA.internal_SSAA aa = gameObject.AddComponent<SSAA.internal_SSAA>();
            //aa.hideFlags = HideFlags.HideAndDontSave;
            aa.hideFlags = (HideFlags)((int)HideFlags.HideAndDontSave + (int)HideFlags.HideInInspector);
            SSAA.internal_SSAA.Filter = Filter;
            SSAA.internal_SSAA.ChangeScale(Scale);
            SSAA.internal_SSAA.Format = renderTextureFormat;
        }
        else
        {
            SSAA.SSAARenderTarget aa = gameObject.AddComponent<SSAA.SSAARenderTarget>();
            //aa.hideFlags = HideFlags.HideAndDontSave;
            aa.hideFlags = (HideFlags)((int)HideFlags.HideAndDontSave + (int)HideFlags.HideInInspector);
            aa.Scale = Scale;
            aa.TargetTexture = cam.targetTexture;
            aa.Filter = Filter;
            aa.Format = renderTextureFormat;
        }
    }

    void OnDisable()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
            return;

        if (cam.targetTexture == null)
        {
            SSAA.internal_SSAA aa = gameObject.GetComponent<SSAA.internal_SSAA>();
            if (aa != null)
            {
                Destroy(aa);
            }
        }
        else
        {
            SSAA.SSAARenderTarget aaRenderTgt = gameObject.GetComponent<SSAA.SSAARenderTarget>();
            if (aaRenderTgt != null)
            {
                if(Application.isPlaying)
                {
                    Destroy(aaRenderTgt);
                }
                else
                {
                    DestroyImmediate(aaRenderTgt);
                }
            }
        }
    }

    public void TakeHighScaledShot(int width, int height, float scale, SSAAFilter filter, string path)
    {
        Texture2D shot = GetHighScaledScreenshot(width, height, scale, filter);
        byte[] bytes = shot.EncodeToPNG();
        Object.DestroyImmediate(shot);
#if !UNITY_WEBPLAYER    //write not supported in webplayer and maybe other plattforms
        string fileName = Application.dataPath + path + ".png";
        if (File.Exists(fileName))
        {
            int fileNum = 1;
            while (File.Exists(Application.dataPath + path + " (" + fileNum.ToString() + ").png"))
            {
                ++fileNum;
                if (fileNum == int.MaxValue)        // :)
                    break;
            }
            fileName = Application.dataPath + path + " (" + fileNum.ToString() + ").png";
        }
        File.WriteAllBytes(fileName, bytes);
#endif
    }

    public Texture2D GetHighScaledScreenshot(int width = 1920, int height = 1080, float scale = 2f, SSAA.SSAAFilter filter = SSAA.SSAAFilter.BilinearSharper)
    {
        if(!Application.isPlaying)
        {
            Debug.LogWarning("Screenshots only supported in PlayMode");
            return null;
        }
        RenderTexture screenShotTarget = new RenderTexture(width, height, 24);
        screenShotTarget.name = "HighScaleShot";

        List<Camera> cams = new List<Camera>((Camera[])MonoBehaviour.FindObjectsOfType(typeof(Camera)));    //get all cameras to add/find ssaa instances

        //----
        //exclude any cameras you don't want to super sample here
        //----


        //exclude disabled cams, active ssaa render cameras
        for (int i = 0; i < cams.Count; ++i)
        {
            if (cams[i].enabled == false || cams[i].gameObject.name == "SSAARenderTargetCamera")
            {
                cams.RemoveAt(i);
                --i;
                continue;
            }
        }

        //get the high scaled screenshot
        SSAA.SSAARenderTarget.SampleSSAAForTexture(screenShotTarget, scale, filter, cams);
        Texture2D highScaledTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = screenShotTarget;
        highScaledTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        highScaledTexture.Apply();
        RenderTexture.active = null;

        //force remove targetTexture    -- mono sync bug
        foreach (Camera c in cams)
            if (c.targetTexture != null && c.targetTexture.name == "HighScaleShot")
                c.targetTexture = null;

        screenShotTarget.Release();
        return highScaledTexture;
    }
}