/*
    Disables post processing effects by default to increase FPS during development
*/

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CinematicEffects;

public class ToggleGraphics : MonoBehaviour 
{
	private bool enableGraphics, swap;
    
	private SESSAO sessao;
    private UltimateBloom bloom;
	private AmplifyColorEffect color;
	private LightShafts volumetricLighting;
    private bool foundLight = false;
    private EyeAdaptation eyeAdaptation;
    private AmplifyMotionEffect motionBlur;
    private ScreenSpaceReflection reflections;
	
	void Start () 
	{
		enableGraphics = false;
        swap           = false;
		sessao         = GetComponent<SESSAO>();
		bloom          = GetComponent<UltimateBloom>();
		color          = GetComponent<AmplifyColorEffect>();
        eyeAdaptation = GetComponent<EyeAdaptation>();
        motionBlur = GetComponent<AmplifyMotionEffect>();
        reflections = GetComponent<ScreenSpaceReflection>();
		
		UpdateGraphics ();
	}
	
	public void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;

		sessao.enabled = enableGraphics;
		bloom.enabled  = enableGraphics;
		color.enabled  = enableGraphics;
        motionBlur.enabled = enableGraphics;
        reflections.enabled = enableGraphics;
        if(volumetricLighting != null)
			volumetricLighting.enabled = enableGraphics;
        else
            swap = true;       

        //Debug.Log ("Graphics are now " + enableGraphics);
	}

    public void SetCommandGraphics()
    {
        sessao.enabled = true;
        bloom.enabled = true;
        eyeAdaptation.enabled = false;
        motionBlur.enabled = false;
        reflections.enabled = false;


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

        if(!foundLight && GameObject.Find("StarLight") != null)
        {
            volumetricLighting = GameObject.Find("StarLight").GetComponent<LightShafts>();
            foundLight = true;
        } 
	}
}
