/*
    Handles cutscene events including music tracks
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CutsceneManager : MonoBehaviour 
{
	public bool playCutscene = true; // Used to disable the cutscene when testing

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] MusicManager music;
	#pragma warning restore 0649
    
	void Play () 
	{
		if(playCutscene) StartCoroutine(Cutscene());
	}
	
	IEnumerator Cutscene()
	{
		yield return new WaitForSeconds(9f);
		music.PlayMusic (1);
	}
}
