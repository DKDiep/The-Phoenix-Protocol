using UnityEngine;
using System.Collections;

public class MothershipLogic : MonoBehaviour {

    private float health;
    private float maxHealth;
    private int spawnedEnemies = 0;
    private EnemySpawner spawner;
    private GameSettings settings;
    private GameState gameState;
    private bool destroyEffects = false;
    [SerializeField] GameObject[] particleSpawnLocations;
    private int numExplosions = 0;
    private int maxExplosions = 50;
    private ObjectPoolManager effectsManager;

	// Use this for initialization
	void Start () {

        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        effectsManager = GameObject.Find("MothershipDamageManager").GetComponent<ObjectPoolManager>();
        LoadSettings();

        GameObject server = settings.GameManager;
        gameState         = server.GetComponent<GameState>();

        StartCoroutine(SpawnExplosions());
	}

    IEnumerator SpawnExplosions()
    {
        if(destroyEffects && numExplosions < maxExplosions)
        {
            GameObject obj = effectsManager.RequestObject();
            obj.transform.position = particleSpawnLocations[Random.Range(0,particleSpawnLocations.Length)].transform.position;
            numExplosions++;
        }
        yield return new WaitForSeconds(Random.Range(0.1f,1f));
        StartCoroutine(SpawnExplosions());
    }

    public void SetSpawner(EnemySpawner temp)
    {
        spawner = temp;
        spawner.mothershipEnemySpawner = transform.parent.Find("MothershipEnemySpawner").gameObject;
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
		// Do not spawn enemies after the player has died
		if (gameState == null)
			yield return new WaitForSeconds(3f);
		else if (gameState.Status != GameState.GameStatus.Started)
			yield break;
		
        if(spawnedEnemies < 30)
        {
            spawner.SpawnEnemyFromMothership();
            spawnedEnemies++;
			yield return new WaitForSeconds(3f);
            StartCoroutine(SpawnEnemies());
        }
        else
        {
            // Take a little break
            yield return new WaitForSeconds(15f);
            spawnedEnemies = 0;
            StartCoroutine(SpawnEnemies());
        }
    }
	
    private void LoadSettings()
    {
        health = settings.GlomMothershipHealth;
        maxHealth = health;
    }

    // Detect collisions with other game objects
    public void collision(float damage, int playerId)
    {
            if(health > damage)
            {
                health -= damage;
            }
            else if (transform.parent != null) // The null check prevents trying to destroy an object again while it's already being destroyed
            {
                Debug.Log("Glom mothership destroyed");
                GameObject explosion = Instantiate(Resources.Load("Prefabs/OutpostExplode", typeof(GameObject))) as GameObject;
                explosion.transform.position = transform.position;
                explosion.SetActive(true);
                ServerManager.NetworkSpawn(explosion);
                Destroy(transform.parent.gameObject);
            }

            if(health < maxHealth * 0.4f && !destroyEffects)
            {
                Debug.Log("Damage effects enabled");
                destroyEffects = true;
            }
    }
}
