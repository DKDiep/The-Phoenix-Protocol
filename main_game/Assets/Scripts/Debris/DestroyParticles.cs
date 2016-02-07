/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Plays a sound then destroys particle effects upon completion
*/

using UnityEngine;
using System.Collections;
 
public class DestroyParticles : MonoBehaviour
{
    [SerializeField] AudioClip[] snd;
    [SerializeField] bool randomPitch;
    [SerializeField] bool hasLight;
    AudioSource mySrc;
    Light myLight;

    private void Start()
    {
        if(snd.Length > 0)
        {
            mySrc = GetComponent<AudioSource>();
            int rnd = Random.Range(0, snd.Length);
            mySrc.clip = snd[rnd];
            if(randomPitch) mySrc.pitch = Random.Range(0.7f, 1.3f);
            mySrc.Play();
        }

        if(hasLight)
        { 
            myLight = GetComponent<Light>();
        }

        Destroy(gameObject, 6f); // Destroys the object after 6 seconds
    }

    private void Update()
    {
        if(hasLight && myLight.intensity > 0) myLight.intensity -= 10f * Time.deltaTime;
    }
        
}