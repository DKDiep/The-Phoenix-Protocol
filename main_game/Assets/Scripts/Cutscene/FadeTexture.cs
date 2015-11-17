using UnityEngine;
using System.Collections;

// Flexible script that allows the fading of 2D textures
public class FadeTexture : MonoBehaviour 
{
	[SerializeField] Texture2D texture;
	[SerializeField] float enterWait; // How long to wait before fading in the texture
	[SerializeField] float exitWait; // How long to wait before fading out
	[SerializeField] float fadeSpeed = 1.0f;
	[SerializeField] bool fullScreen;
	[SerializeField] bool centered;
	[SerializeField] float xPos; // x Offset
	[SerializeField] float yPos; // y Offset
	[SerializeField] float alpha = 1.0f; // Starting alpha value
	Color fading;
	bool canFade = false;

	// Use this for initialization
	void Start () 
	{
		fading = new Color(1.0f, 1.0f, 1.0f, 1.0f); // Initialise colour
		StartCoroutine ("Fading");
	}
	
	void Update()
	{
		if(canFade && alpha < 1.0f) alpha += fadeSpeed * Time.deltaTime;
		else if(!canFade && alpha > 0.0f) alpha -= fadeSpeed * Time.deltaTime;
	}
	
	void OnGUI()
	{
		fading.a = alpha;
		GUI.color = fading;
		if(fullScreen) GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), texture, ScaleMode.StretchToFill);
		else if(centered) GUI.DrawTexture (new Rect ((Screen.width / 2) - (texture.width / 2) + xPos, (Screen.height / 2) - (texture.height / 2) + yPos, texture.width, texture.height), texture, ScaleMode.ScaleToFit);
		else GUI.DrawTexture (new Rect (xPos, yPos, texture.width, texture.height), texture, ScaleMode.ScaleToFit);
	}
	
	IEnumerator Fading()
	{
		yield return new WaitForSeconds(enterWait);
		canFade = true;
		yield return new WaitForSeconds(exitWait);
		canFade = false;
		yield return new WaitForSeconds(5f);
		Destroy (this);
	}
}
