using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour 
{

	[SerializeField] GameObject asteroid;
	[SerializeField] int maxAsteroids;
	public static int numAsteroids = 0;
	
	// Spawn a new asteroid in a random position if there are less than specified by maxAsteroids
	void Update () 
	{
		if(numAsteroids < maxAsteroids)
		{
			Vector3 rand_position = new Vector3(transform.position.x + Random.Range (-800, 800), transform.position.y + Random.Range (-800, 800), transform.position.z + 150 + Random.Range (50, 1000));
			Instantiate (asteroid, rand_position, Quaternion.identity);
			numAsteroids += 1;
		}
	}
}
