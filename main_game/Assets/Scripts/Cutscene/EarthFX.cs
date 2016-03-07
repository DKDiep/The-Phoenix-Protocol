using UnityEngine;
using System.Collections;

public class EarthFX : MonoBehaviour {

    private float radius = 4035f;
    [SerializeField] GameObject impact;
    [SerializeField] int maxExplosions;
    int currentExplosions = 0;
    ObjectPoolManager waveManager;

    void Start()
    {
        waveManager = GameObject.Find("ImpactWaveManager").GetComponent<ObjectPoolManager>();
    }
      

    void Update()
    {
        if(currentExplosions < maxExplosions)
        {
            StartCoroutine("SpawnExplosion");
            currentExplosions++;
        }
    }
       
    IEnumerator SpawnExplosion()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 4f));
        GameObject explosion = waveManager.RequestObject();
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Random.rotation;
        explosion.transform.Translate(transform.forward * radius);
        yield return new WaitForSeconds(3f);
        waveManager.RemoveObject(explosion.name);
        currentExplosions--;
    }

}
