/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Disables post processing effects by default to increase FPS during development
*/

using UnityEngine;
using System.Collections;

public class ToggleGraphics : MonoBehaviour 
{
	bool enableGraphics, swap;
    
	SESSAO sessao;
	SENaturalBloomAndDirtyLens bloom;
	AmplifyColorEffect color;
	Aubergine.PP_Vignette vignette;
	Smaa.SMAA smaa;
    UnityStandardAssets.ImageEffects.SunShafts shafts;
    LightShafts volumetricLighting;
	
	void Start () 
	{
		enableGraphics = true;
        swap = false;
		sessao = GetComponent<SESSAO>();
		bloom = GetComponent<SENaturalBloomAndDirtyLens>();
		color = GetComponent<AmplifyColorEffect>();
		vignette = GetComponent<Aubergine.PP_Vignette>();
		smaa = GetComponent<Smaa.SMAA>();
        shafts = GetComponent<UnityStandardAssets.ImageEffects.SunShafts>();
		
		UpdateGraphics ();
		
	}
	
	void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;
		Debug.Log ("Graphics are now " + enableGraphics);
		sessao.enabled = enableGraphics;
		bloom.enabled = enableGraphics;
		color.enabled = enableGraphics;
		vignette.enabled = false;
		smaa.enabled = enableGraphics;
        shafts.enabled = enableGraphics;
        if(volumetricLighting != null) volumetricLighting.enabled = enableGraphics;
        else
        {
            swap = true;
        }
       
	}

	void Update () 
	{
		if(Input.GetKeyDown (KeyCode.Alpha0))
		{
			UpdateGraphics();
		}

        if(swap && volumetricLighting != null)
        {
            volumetricLighting.enabled = enableGraphics;
            swap = false; 
        }

        if(shafts.sunTransform == null && GameObject.Find("StarLight") != null)
        {
            shafts.sunTransform = GameObject.Find("StarLight").transform;
            volumetricLighting = GameObject.Find("StarLight").GetComponent<LightShafts>();
        } 
	}
}
