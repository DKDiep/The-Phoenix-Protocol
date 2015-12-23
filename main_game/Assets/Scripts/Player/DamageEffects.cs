using UnityEngine;
using System.Collections;

public class DamageEffects : MonoBehaviour 
{

	VideoGlitches.VideoGlitchSpectrumOffset lowHealth;
	ShipMovement myMove;
	float health;

	// Use this for initialization
	void Start () 
	{
		lowHealth = GetComponent<VideoGlitches.VideoGlitchSpectrumOffset>();
		myMove = GameObject.Find("PlayerShipLogic(Clone)").GetComponent<ShipMovement>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		health = myMove.GetHealth();
		lowHealth.amount = Mathf.Clamp(0.25f - ((float)health/100f),0f,0.25f) * 2f;
	}
}
