using UnityEngine;
using System.Collections;

public class LoadingText : MonoBehaviour {

    [SerializeField] Texture2D text;
    bool fadeSound = false;

	// Use this for initialization
	void Start () 
    {
        AudioListener.volume = 0;
        StartCoroutine("Loaded");
	}

    IEnumerator Loaded()
    {
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
        if(!fadeSound) GUI.DrawTexture (new Rect (Screen.width - 200, Screen.height - 80, 195, 66), text, ScaleMode.ScaleToFit);
    }
}
