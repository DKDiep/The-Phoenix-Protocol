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
	private bool enableGraphics, swap;
    
	private SESSAO sessao;
	private SENaturalBloomAndDirtyLens bloom;
	private AmplifyColorEffect color;
	private Smaa.SMAA smaa;
	private LightShafts volumetricLighting;
	
	void Start () 
	{
		enableGraphics = true;
        swap           = false;
		sessao         = GetComponent<SESSAO>();
		bloom          = GetComponent<SENaturalBloomAndDirtyLens>();
		color          = GetComponent<AmplifyColorEffect>();
		smaa           = GetComponent<Smaa.SMAA>();
		
		UpdateGraphics ();
	}
	
	void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;

		sessao.enabled = enableGraphics;
		bloom.enabled  = enableGraphics;
		color.enabled  = enableGraphics;
		smaa.enabled   = enableGraphics;
        if(volumetricLighting != null)
			volumetricLighting.enabled = enableGraphics;
        else
            swap = true;       

		Debug.Log ("Graphics are now " + enableGraphics);
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

        if(GameObject.Find("StarLight") != null)
        {
            volumetricLighting = GameObject.Find("StarLight").GetComponent<LightShafts>();
        } 
	}
}
