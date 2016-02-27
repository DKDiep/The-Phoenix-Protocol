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
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private AudioClip[] snd;
	[SerializeField] private bool randomPitch;
	[SerializeField] private bool hasLight;
	[SerializeField] private float lifetime;
	[SerializeField] private string managerName;
	[SerializeField] private bool destroyObject = false;
	[SerializeField] private float lightIntensity;
	#pragma warning restore 0649

	private AudioSource mySrc;
	private Light myLight;

    private ObjectPoolManager particleManager;

    private void OnEnable()
    {
        if(!destroyObject)
        {
            if(GameObject.Find("MusicManager(Clone)") != null)
				particleManager = GameObject.Find(managerName).GetComponent<ObjectPoolManager>();
            else
                Destroy(this);
			
            StartCoroutine("Disable");
        }
        else
        {
            Destroy(this.gameObject,lifetime);
        }

        if(hasLight)
        {
            myLight = GetComponent<Light>();
            myLight.intensity = lightIntensity;
        } 

        if(snd.Length > 0)
        {
            mySrc = GetComponent<AudioSource>();
            int rnd = Random.Range(0, snd.Length);
            mySrc.clip = snd[rnd];
            if(randomPitch) mySrc.pitch = Random.Range(0.7f, 1.3f);
            mySrc.Play();
        }


    }

    private void Update()
    {
        if(hasLight && myLight.intensity > 0) myLight.intensity -= 10f * Time.deltaTime;
    }

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(lifetime);
        particleManager.DisableClientObject(gameObject.name);
        particleManager.RemoveObject(gameObject.name);
    }
        
}