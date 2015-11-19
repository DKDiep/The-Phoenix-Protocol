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
			GameObject temp = Instantiate (enemy, rand_position, transform.rotation) as GameObject;
			temp.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
			temp.GetComponent<EnemyLogic>().SetPlayer(state.playerShip);
			numEnemies += 1;
            state.enemyShipList.Add(temp);
		}
	}

    // Automatically destroy if 100 units behind player
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);

        for (int i = state.enemyShipList.Count - 1; i >= 0; i--)
        {
            GameObject enemyShip = state.enemyShipList[i];
            EnemyLogic enemyLogic = enemyShip.GetComponent<EnemyLogic>();
            if (enemyShip.transform.position.z < enemyLogic.player.transform.position.z - 100f)
            {
                numEnemies -= 1;
                state.enemyShipList.RemoveAt(i);
                Destroy(enemyShip.gameObject);
            }
        }
        //Debug.Log(numEnemies);
        StartCoroutine("Cleanup");
    }
}