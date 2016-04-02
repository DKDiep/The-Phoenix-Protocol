using UnityEngine;
using System.Collections;

public class MothershipLogic : MonoBehaviour {

    private float health;
    private EnemySpawner spawner;
    GameSettings settings;
    GameState gameState;


	// Use this for initialization
	void Start () {

        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        LoadSettings();

        GameObject server = settings.GameManager;
        gameState         = server.GetComponent<GameState>();
	
	}

    public void SetSpawner(EnemySpawner temp)
    {
        spawner = temp;
        spawner.mothershipObject = transform.parent.Find("MothershipEnemySpawner").gameObject;
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(settings.GlomMothershipSpawnRate);
        spawner.SpawnEnemyFromMothership();
        StartCoroutine(SpawnEnemies());
    }
	
    private void LoadSettings()
    {
        health = settings.GlomMothershipHealth;
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
    }
}
