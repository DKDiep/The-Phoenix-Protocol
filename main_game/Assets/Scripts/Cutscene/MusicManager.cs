/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Stores array of music files with a public function to play them
*/

using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour 
{

	[SerializeField] AudioClip[] music;

    void Start()
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
