using UnityEngine;
using System.Collections;

public class LoadingText : MonoBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Texture2D text;
	#pragma warning restore 0649

	private bool fadeSound = false;
    private bool gameStarted = false;

	// Use this for initialization
	public void Play () 
    {
        StartCoroutine("Loaded");
	}

    IEnumerator Loaded()
    {
        gameStarted = true;
        yield return new WaitForSeconds(3f);
        fadeSound = true;
        Destroy(this, 3f);
    }

    void Update()
    {
        if(AudioListener.volume < 1f && fadeSound)
        {
            AudioListener.volume += 10f * Time.deltaTime;
        }
    }
	
    void OnGUI()
    {
        if(gameStarted && !fadeSound) 
			GUI.DrawTexture (new Rect (Screen.width - 200, Screen.height - 80, 195, 66), text, ScaleMode.ScaleToFit);
    }
}
