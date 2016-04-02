using UnityEngine;
using System.Collections;

public class EarthFX : MonoBehaviour {

    private float radius = 4035f; // The Earth's radius
    private int maxExplosions = 40; // Maximum number of explosions allowed
    private int currentExplosions = 0;
    ObjectPoolManager waveManager;

    void Start()
    {
        waveManager = GameObject.Find("ImpactWaveManager").GetComponent<ObjectPoolManager>();
    }
      
    void Update()
    {
        if(currentExplosions < maxExplosions)
        {
            StartCoroutine(SpawnExplosion());
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
        float size = Random.Range(0.15f, 2f);
        explosion.transform.localScale = new Vector3(size, size, size);
        waveManager.EnableClientObject(explosion.name, explosion.transform.position, explosion.transform.rotation, explosion.transform.localScale);
        yield return new WaitForSeconds(3f);
        waveManager.DisableClientObject(explosion.name);
        waveManager.RemoveObject(explosion.name);
        currentExplosions--;
    }

}
