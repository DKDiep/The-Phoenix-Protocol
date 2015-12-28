using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DamageEffects : MonoBehaviour 
{

	VideoGlitches.VideoGlitchSpectrumOffset lowHealth;
	ShipMovement myMove;
  GameObject player;
	float health, alpha;
	int direction;
	[SerializeField] Texture2D left;
	[SerializeField] Texture2D right;
	[SerializeField] Texture2D up;
	[SerializeField] Texture2D down;
	[SerializeField] Texture2D topLeft;
	[SerializeField] Texture2D topRight;
	[SerializeField] Texture2D bottomLeft;
	[SerializeField] Texture2D bottomRight;

	// Use this for initialization
	void Start () 
	{
		lowHealth = GetComponent<VideoGlitches.VideoGlitchSpectrumOffset>();
    player = GameObject.Find("PlayerShipLogic(Clone)");
    if(player != null)
    {
      myMove = player.GetComponent<ShipMovement>();
    }

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(alpha > 0f)
		{
			alpha -= 4f * Time.deltaTime;
		}
	}

	void OnGUI()
	{
		GUI.color = new Color(1f,1f,1f,alpha);
		if(direction == 0) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), left, ScaleMode.StretchToFill);
		if(direction == 1) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), up, ScaleMode.StretchToFill);
		if(direction == 2) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), right, ScaleMode.StretchToFill);
		if(direction == 3) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), down, ScaleMode.StretchToFill);
		if(direction == 4) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), topLeft, ScaleMode.StretchToFill);
		if(direction == 5) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), topRight, ScaleMode.StretchToFill);
		if(direction == 6) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), bottomLeft, ScaleMode.StretchToFill);
		if(direction == 7) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), bottomRight, ScaleMode.StretchToFill);
	}

	public void Damage(int dir, float damage, float hp)
	{
    health = hp;
    lowHealth.amount = Mathf.Clamp(0.25f - ((float)health/100f),0f,0.25f) * 2f;
		direction = dir;
		alpha = Mathf.Clamp(0.5f + (damage/20f),0f,1f);
	}
}


