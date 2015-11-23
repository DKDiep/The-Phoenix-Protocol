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

    public GameObject gameManager;
    private GameState state;

    void Start()
    {
        if (gameManager != null)
            state = gameManager.GetComponent<GameState>();
        StartCoroutine("Cleanup");
    }

    // Spawn a new enemy in a random position if less than specified by maxEnemies
    void Update () 
	{
		if(numEnemies < maxEnemies)
		{
			Vector3 rand_position = new Vector3(transform.position.x + Random.Range (-400, 400), transform.position.y + Random.Range (-400, 400), transform.position.z + 200 + Random.Range (50, 1000));
			GameObject enemyObject = Instantiate (enemy, rand_position, transform.rotation) as GameObject;
			enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
			enemyObject.GetComponent<EnemyLogic>().SetPlayer(state.GetPlayerShip());
			numEnemies += 1;
            state.AddEnemyList(enemyObject);
            //NOTIFY CLIENT
		}
	}

    // Automatically destroy if 100 units behind player
    IEnumerator Cleanup()
    {
        
        yield return new WaitForSeconds(1f);
        
        for (int i = state.GetEnemyListCount() - 1; i >= 0; i--)
        {
            GameObject enemyObject = state.GetEnemyAt(i);
            EnemyLogic enemyLogic = enemyObject.GetComponent<EnemyLogic>();
            if (enemyObject.transform.position.z < enemyLogic.player.transform.position.z - 100f)
            {
                numEnemies -= 1;
                state.RemoveEnemyAt(i);
                Destroy(enemyObject.gameObject);
                //NOTIFY CLIENT
            }
        }
        //Debug.Log(numEnemies);
        StartCoroutine("Cleanup");
    }
}