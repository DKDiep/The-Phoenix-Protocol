using UnityEngine;
using System.Collections;

public class AIVoice : MonoBehaviour {

    /*
       Command List
       1. 
    */

    [SerializeField] AudioClip[] aiClips;
    private AudioSource mySource;

    private void Start()
    {
        mySource = GetComponent<AudioSource>();
    }

	public void SendCommand(int id)
    {
        mySource.PlayOneShot(aiClips[id]);
    }
}
