/*
    Stores array of music files with a public function to play them
*/

using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] AudioClip[] music;
	#pragma warning restore 0649

    public void Play()
    {
        GetComponent<AudioSource>().clip = music[0];
        StartCoroutine("DelayMusic");
    }

	void Update()
	{
		if (Input.GetKeyDown ("m") )
		{
			// Mute all sounds in game
			AudioListener.volume = 1 - AudioListener.volume;
			// Mute just the music
			//GetComponent<AudioSource>().mute = !GetComponent<AudioSource>().mute;
		}
	}

    IEnumerator DelayMusic()
    {
        yield return new WaitForSeconds(3f);
        GetComponent<AudioSource>().Play ();
    }
        
	// Plays a music track with ID as input	
	public void PlayMusic(int id)
	{
		GetComponent<AudioSource>().clip = music[id];
		GetComponent<AudioSource>().Play ();
	}
}
