/*
    Full screen color fade in and out
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FadeTexture : NetworkBehaviour 
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] Texture2D texture;
	[SerializeField] float enterWait; // How long to wait before fading in the texture
	[SerializeField] float exitWait; // How long to wait before fading out
	[SerializeField] float fadeSpeed = 1.0f;
	[SerializeField] bool fullScreen;
	[SerializeField] bool centered;
	[SerializeField] float xPos; // x Offset
	[SerializeField] float yPos; // y Offset
	[SerializeField] float alpha = 1.0f; // Starting alpha value
	#pragma warning restore 0649

	Color fading;
	bool canFade = false;
    private bool gameStarted = false;

	// Use this for initialization
	public void Play () 
	{
		fading = new Color(1.0f, 1.0f, 1.0f, 1.0f); // Initialise colour
        gameStarted = true;
        RpcFadeClient();
		StartCoroutine ("Fading");
	}

    public void Reset()
    {
        RpcReset();
        /*StopAllCoroutines();
        alpha = 1.0f;
        canFade = false;
        gameStarted = false;*/
    }

    [ClientRpc]
    void RpcReset()
    {
        //StopAllCoroutines();
        alpha = 1.0f;
        canFade = false;
        gameStarted = false;
    }

    [ClientRpc]
    void RpcFadeClient()
    {
        fading = new Color(1.0f, 1.0f, 1.0f, 1.0f); // Initialise colour
        gameStarted = true;
        canFade = true;
    }

    [ClientRpc]
    void RpcFadeOutClient()
    {
        canFade = false;
    }
	
	void Update()
	{
        if(gameStarted)
        {
		    if(canFade && alpha < 1.0f) 
                alpha += fadeSpeed * Time.deltaTime;
		    else if(!canFade && alpha > 0.0f) 
                alpha -= fadeSpeed * Time.deltaTime;
        }
	}
	
	void OnGUI()
	{
        if(gameStarted)
        {
		    fading.a = alpha; // Set a color's alpha value
		    GUI.color = fading; // Set the texture color

            if(fullScreen) 
                GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), texture, ScaleMode.StretchToFill);
		    else if(centered) 
                GUI.DrawTexture (new Rect ((Screen.width / 2) - (texture.width / 2) + xPos, (Screen.height / 2) - (texture.height / 2) + yPos, texture.width, texture.height), texture, ScaleMode.ScaleToFit);
		    else 
                GUI.DrawTexture (new Rect (xPos, yPos, texture.width, texture.height), texture, ScaleMode.ScaleToFit);
        }
	}

    // Controls when fading is triggered
	IEnumerator Fading()
	{
		//yield return new WaitForSeconds(enterWait);
		canFade = true;
		yield return new WaitForSeconds(exitWait);
        RpcFadeOutClient();
		canFade = false;
		yield return new WaitForSeconds(5f);
		//Destroy (this);
	}
}
