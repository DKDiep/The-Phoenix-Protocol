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
	Smaa.SMAA smaa;
    LightShafts volumetricLighting;
	
	void Start () 
	{
		enableGraphics = true;
        swap = false;
		sessao = GetComponent<SESSAO>();
		bloom = GetComponent<SENaturalBloomAndDirtyLens>();
		color = GetComponent<AmplifyColorEffect>();
		smaa = GetComponent<Smaa.SMAA>();
		
		UpdateGraphics ();
		
	}
	
	void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;
		Debug.Log ("Graphics are now " + enableGraphics);
		sessao.enabled = enableGraphics;
		bloom.enabled = enableGraphics;
		color.enabled = enableGraphics;
		smaa.enabled = enableGraphics;
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

        if(GameObject.Find("StarLight") != null)
        {
            volumetricLighting = GameObject.Find("StarLight").GetComponent<LightShafts>();
        } 
	}
}
