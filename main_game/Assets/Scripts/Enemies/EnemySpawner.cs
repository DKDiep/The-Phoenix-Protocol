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
	public static int numEnemies = 0;
	public int maxEnemies;

    [SerializeField]
    GameObject gameManager;
    private GameState state;

    void Start()
    {
        if (gameManager != null)
        {
            state = gameManager.GetComponent<GameState>();
        }
        StartCoroutine("Cleanup");
    }

    // Spawn a new enemy in a random position if less than specified by maxEnemies
    void Update () 
	{
        if (state.GetStatus() == GameState.Status.Started)
        {
            if (numEnemies < maxEnemies)
            {
                Vector3 rand_position = new Vector3(transform.position.x + Random.Range(-400, 400), transform.position.y + Random.Range(-400, 400), transform.position.z + 200 + Random.Range(50, 1000));
                //Spawn enemy and server logic
                GameObject enemyObject = Instantiate(enemy, rand_position, transform.rotation) as GameObject;
                GameObject enemyObjectLogic = Instantiate(Resources.Load("Prefabs/EnemyShipLogic", typeof(GameObject))) as GameObject;
                enemyObject.AddComponent<EnemyCollision>();
				enemyObjectLogic.transform.parent = enemyObject.transform;
				enemyObjectLogic.transform.localPosition = Vector3.zero;
                enemyObjectLogic.GetComponent<EnemyLogic>().SetControlObject(enemyObject);
                enemyObjectLogic.GetComponent<EnemyLogic>().SetPlayer(state.GetPlayerShip());
                ServerManager.NetworkSpawn(enemyObject);
                
                //parent enemyObject to find
                enemyObjectLogic.name = "enemyObjectLogic";

                enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
                numEnemies += 1;
                state.AddEnemyList(enemyObject);
            }
        }
	}

    // Automatically destroy if 100 units behind player
    IEnumerator Cleanup()
    {
        
        yield return new WaitForSeconds(1f);
        if (state.GetStatus() == GameState.Status.Started)
        {
            for (int i = state.GetEnemyListCount() - 1; i >= 0; i--)
            {
                GameObject enemyObject = state.GetEnemyAt(i);
                //EnemyLogic enemyLogic = enemyObject.transform.Find("enemyObjectLogic").GetComponent<EnemyLogic>();
                /*if (enemyObject.transform.position.z < enemyLogic.player.transform.position.z - 100f)
                {
                    numEnemies -= 1;
                    state.RemoveEnemyAt(i);
                    Destroy(enemyObject.gameObject);
                    //NOTIFY CLIENT
                }*/
            }
        }
        //Debug.Log(numEnemies);
        StartCoroutine("Cleanup");
    }
}