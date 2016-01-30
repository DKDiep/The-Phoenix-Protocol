/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Server-side logic for enemy spawner
*/

using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour 
{
	
	[SerializeField] GameObject enemy;
	public static int numEnemies = 0; // Number of currently active enemies
	public int maxEnemies; // Maximum number of enemies at a time
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] GameObject gameManager;
    private GameState state;
    GameObject player, temp;

    void Start()
    {
        if (gameManager != null)
        {
            state = gameManager.GetComponent<GameState>();
        }

        player = null;
        temp = new GameObject(); // Create temporary object to spawn enemies at
        temp.name = "EnemySpawnLocation";
        StartCoroutine("Cleanup");
    }

    // Spawn a new enemy in a random position if less than specified by maxEnemies
    void Update () 
	{
        if (state.GetStatus() == GameState.Status.Started)
        {
            if(player == null) player = state.GetPlayerShip();
            if (numEnemies < maxEnemies)
            {
                // Set spawn position based on input attributes
                temp.transform.position = player.transform.position;
                temp.transform.rotation = Random.rotation;
                temp.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

                // Spawn enemy and server logic
                GameObject enemyObject = Instantiate(enemy, temp.transform.position, transform.rotation) as GameObject;
                GameObject enemyObjectLogic = Instantiate(Resources.Load("Prefabs/EnemyShipLogic", typeof(GameObject))) as GameObject;

                // Set up enemy with components, spawn on network
                enemyObject.AddComponent<EnemyCollision>();
				enemyObjectLogic.transform.parent = enemyObject.transform;
				enemyObjectLogic.transform.localPosition = Vector3.zero;
                enemyObjectLogic.GetComponent<EnemyLogic>().SetControlObject(enemyObject);
                enemyObjectLogic.GetComponent<EnemyLogic>().SetPlayer(state.GetPlayerShip());
                ServerManager.NetworkSpawn(enemyObject);

                enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
                numEnemies += 1;
                state.AddEnemyList(enemyObject);
            }
        }
	}

    // Remove destroyed enemies from Game State
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        if (state.GetStatus() == GameState.Status.Started)
        {
            for (int i = state.GetEnemyListCount() - 1; i >= 0; i--)
            {
                GameObject enemyObject = state.GetEnemyAt(i);
                if(enemyObject == null)
                {
                  state.RemoveEnemyAt(i);
                }
            }
        }

        StartCoroutine("Cleanup");
    }
}