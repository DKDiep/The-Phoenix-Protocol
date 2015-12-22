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
	

	// Use this for initialization
	void Start () 
	{
		enableGraphics = true;
		ssaa = GetComponent<SuperSampling_SSAA>();
		sessao = GetComponent<SESSAO>();
		motion = GetComponent<AmplifyMotionEffect>();
		bloom = GetComponent<SENaturalBloomAndDirtyLens>();
		color = GetComponent<AmplifyColorEffect>();
		vignette = GetComponent<Aubergine.PP_Vignette>();
		smaa = GetComponent<Smaa.SMAA>();
		
		UpdateGraphics ();
		
	}
	
	void UpdateGraphics()
	{
		enableGraphics = !enableGraphics;
		Debug.Log ("Graphics are now " + enableGraphics);
		ssaa.enabled = enableGraphics;
		sessao.enabled = enableGraphics;
		motion.enabled = enableGraphics;
		bloom.enabled = enableGraphics;
		color.enabled = enableGraphics;
		vignette.enabled = enableGraphics;
		smaa.enabled = enableGraphics;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown (KeyCode.Alpha1))
		{
			UpdateGraphics();
		}
	}
}
