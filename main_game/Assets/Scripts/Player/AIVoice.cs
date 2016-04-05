using UnityEngine;
using System.Collections;

public class AIVoice : MonoBehaviour {

    [SerializeField] AudioClip[] aiClips;
    private AudioSource mySource;

    private void Start()
    {
        mySource = GetComponent<AudioSource>();
    }

	public void PlaySound(int id)
    {
        mySource.PlayOneShot(aiClips[id]);
    }
}
