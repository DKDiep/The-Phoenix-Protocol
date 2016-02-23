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
    float lightIntensity;
    [SerializeField] float lifetime;
    [SerializeField] string managerName;

    private ObjectPoolManager particleManager;

    private void Awake()
    {
        if(hasLight)
        { 
            myLight = GetComponent<Light>();
            lightIntensity = myLight.intensity;
        }

        if(snd.Length > 0)
        {
            mySrc = GetComponent<AudioSource>();
        }

        if(GameObject.Find(managerName) != null) particleManager = GameObject.Find(managerName).GetComponent<ObjectPoolManager>();
        else Destroy(this);
    }

    private void OnEnable()
    {
        if(hasLight) myLight.intensity = lightIntensity;

        if(snd.Length > 0)
        {
            int rnd = Random.Range(0, snd.Length);
            mySrc.clip = snd[rnd];
            if(randomPitch) mySrc.pitch = Random.Range(0.7f, 1.3f);
            mySrc.Play();
        }

        StartCoroutine("Disable");
    }

    private void Update()
    {
        if(hasLight && myLight.intensity > 0) myLight.intensity -= 10f * Time.deltaTime;
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(lifetime + 0.5f);
        //Debug.Log(gameObject.name);
        particleManager.RemoveObject(gameObject.name);
    }
        
}