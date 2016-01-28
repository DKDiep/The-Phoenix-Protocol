using UnityEngine;
using System.Collections;

public class SoundSource : MonoBehaviour 
{

    public void PlaySound(AudioClip snd)
    {
        AudioSource mySrc = GetComponent<AudioSource>();
        mySrc.clip = snd;
        mySrc.Play();
        Destroy(this.gameObject, 5);
    }
}
