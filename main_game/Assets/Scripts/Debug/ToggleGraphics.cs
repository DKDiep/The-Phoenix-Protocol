/*
    Disables post processing effects by default to increase FPS during development
*/

using UnityEngine;
using System.Collections;

public class ToggleGraphics : MonoBehaviour 
{
	private bool enableGraphics, swap;
    
	private SESSAO sessao;
    private UltimateBloom bloom;
	private AmplifyColorEffect color;
	private LightShafts volumetricLighting;
    private bool foundLight = false;
    private EyeAdaptation eyeAdaptation;
	
	void Start () 
	{
		enableGraphics = false;
        swap           = false;
		sessao         = GetComponent<SESSAO>();
		bloom          = GetComponent<UltimateBloom>();
		color          = GetComponent<AmplifyColorEffect>();
        eyeAdaptation = GetComponent<EyeAdaptation>();
		
		UpdateGraphics ();
	}
	
	public void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;

		sessao.enabled = enableGraphics;
		bloom.enabled  = enableGraphics;
		color.enabled  = enableGraphics;
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
