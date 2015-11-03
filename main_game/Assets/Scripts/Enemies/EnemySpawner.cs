using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour 
{
	
	[SerializeField] GameObject enemy;
	public static int numEnemies = 0;
	[SerializeField] int maxEnemies;

    public GameObject gameManager;
	
	// Spawn a new enemy in a random position if less than specified by maxEnemies
	void Update () 
	{
		if(numEnemies < maxEnemies)
		{
			Vector3 rand_position = new Vector3(transform.position.x + Random.Range (-400, 400), transform.position.y + Random.Range (-400, 400), transform.position.z + 200 + Random.Range (50, 1000));
			GameObject temp = Instantiate (enemy, rand_position, transform.rotation) as GameObject;
			temp.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
			temp.GetComponent<EnemyLogic>().SetPlayer(this.gameObject);
			numEnemies += 1;
		}
	}
}