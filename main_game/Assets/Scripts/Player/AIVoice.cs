using UnityEngine;
using System.Collections;

public class AIVoice : MonoBehaviour {

    /*
       Command List
       1. 
    */

    public AudioClip[] aiClips;
    private AudioSource mySource;
    public static AIVoice aiObject;

	public static void SendCommand(int id)
    {
        if(aiObject == null)
            aiObject = Camera.main.gameObject.GetComponent<AIVoice>();
        aiObject.PlaySound(id);
    }

    public void PlaySound(int id)
    {
        if(mySource == null)
            mySource = GetComponent<AudioSource>(); 
        if(!mySource.isPlaying)
        {
            mySource.clip = aiClips[id];
            mySource.Play();
        }

    }
}
