/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Display object icon on minimap
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_IconItem : MonoBehaviour 
{
    public Image TargetGrapihc;
    private CanvasGroup m_CanvasGroup;
    private float delay = 0.1f;

    void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_CanvasGroup.ignoreParentGroups = true;
        m_CanvasGroup.alpha = 0;
    }

    public void DestroyIcon()
    {
        Destroy(gameObject);
    }

    IEnumerator FadeIcon()
    {
        yield return new WaitForSeconds(delay);
        while(m_CanvasGroup.alpha < 1)
        {
            m_CanvasGroup.alpha += Time.deltaTime * 2;
            yield return null;
        }
    }

    public void DelayStart(float v) { delay = v; StartCoroutine(FadeIcon()); }
}