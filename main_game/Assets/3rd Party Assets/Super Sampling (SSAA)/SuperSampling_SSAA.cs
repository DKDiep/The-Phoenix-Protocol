using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SSAA;

public class SuperSampling_SSAA : MonoBehaviour
{
    public float Scale = 0f;

    public bool unlocked = false;

    public SSAAFilter Filter = SSAAFilter.BilinearDefault;

    public bool UseDynamicOutputResolution = false;


    void OnEnable()
    {
        SSAA.internal_SSAA aa = gameObject.AddComponent<SSAA.internal_SSAA>();
        aa.hideFlags = HideFlags.HideAndDontSave;
        SSAA.internal_SSAA.UseDynamicOutputResolution = UseDynamicOutputResolution;
        SSAA.internal_SSAA.Filter = Filter;
        SSAA.internal_SSAA.ChangeScale(Scale);
    }

    void OnDisable()
    {
        Destroy(gameObject.GetComponent<SSAA.internal_SSAA>());
    }
}