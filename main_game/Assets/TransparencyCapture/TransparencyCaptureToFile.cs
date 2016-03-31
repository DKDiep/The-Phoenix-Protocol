using System.Collections;
using UnityEngine;

public class TransparencyCaptureToFile:MonoBehaviour
{
    private int currentFrame = 0;
    private bool increase = false;
    [SerializeField] private bool animated;

    public IEnumerator capture()
    {
        
        yield return new WaitForEndOfFrame();
        //After Unity4,you have to do this function after WaitForEndOfFrame in Coroutine
        //Or you will get the error:"ReadPixels was called to read pixels from system frame buffer, while not inside drawing frame"
        zzTransparencyCapture.captureScreenshot("capture" + currentFrame + ".png");
        increase = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(capture());
            currentFrame = 0;
        }
        else if (currentFrame < 14 && increase && animated)
        {
            StartCoroutine(capture());
            currentFrame++;
            increase = false;
        }

    }
}