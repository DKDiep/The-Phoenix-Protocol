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
	bool enableGraphics;

	SuperSampling_SSAA ssaa;
	SESSAO sessao;
	AmplifyMotionEffect motion;
	SENaturalBloomAndDirtyLens bloom;
	AmplifyColorEffect color;
	Aubergine.PP_Vignette vignette;
	Smaa.SMAA smaa;
    UnityStandardAssets.ImageEffects.SunShafts shafts;
	
	void Start () 
	{
		enableGraphics = false;
		ssaa = GetComponent<SuperSampling_SSAA>();
		sessao = GetComponent<SESSAO>();
		motion = GetComponent<AmplifyMotionEffect>();
		bloom = GetComponent<SENaturalBloomAndDirtyLens>();
		color = GetComponent<AmplifyColorEffect>();
		vignette = GetComponent<Aubergine.PP_Vignette>();
		smaa = GetComponent<Smaa.SMAA>();
        shafts = GetComponent<UnityStandardAssets.ImageEffects.SunShafts>();
        SSAA.internal_SSAA.filter = SSAA.SSAAFilter.BilinearDefault;
		
		UpdateGraphics ();
		
	}
	
	void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;
		Debug.Log ("Graphics are now " + enableGraphics);
		ssaa.enabled = false;
		sessao.enabled = enableGraphics;
		motion.enabled = enableGraphics;
		bloom.enabled = enableGraphics;
		color.enabled = enableGraphics;
		vignette.enabled = enableGraphics;
		smaa.enabled = enableGraphics;
        shafts.enabled = enableGraphics;
       
	}

	void Update () 
	{
		if(Input.GetKeyDown (KeyCode.Alpha0))
		{
			UpdateGraphics();
		}
        if(shafts.sunTransform == null && GameObject.Find("StarLight") != null) shafts.sunTransform = GameObject.Find("StarLight").transform;
	}
}
