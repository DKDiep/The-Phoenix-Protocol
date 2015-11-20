/*
    2015-2016 Team Pyrolite, University of Bristol.
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Server-side logic for asteroid spawner
*/

using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour 
{

	[SerializeField] GameObject asteroid;
	[SerializeField] int maxAsteroids;
	[SerializeField] float maxVariation; // Max variation in size (0-10)
	public static int numAsteroids = 0;

    [SerializeField] GameObject gameManager;
    private GameState state;
	
	void Start ()
    {
        // Set game state reference
        if (gameManager != null)
            state = gameManager.GetComponent<GameState>();
        StartCoroutine("Cleanup");
    }

	void Update () 
	{
        // Spawn a new asteroid in a random position if there are less than specified by maxAsteroids
        if (numAsteroids < maxAsteroids)
		{
			Vector3 rand_position = new Vector3(transform.position.x + Random.Range (-800, 800), transform.position.y + Random.Range (-800, 800), transform.position.z + 150 + Random.Range (50, 1000));
            GameObject asteroidObject = Instantiate (asteroid, rand_position, Quaternion.identity) as GameObject;
            asteroidObject.GetComponent<AsteroidLogic>().SetPlayer (state.getPlayerShip(), maxVariation);
            state.addAsteroidList(asteroidObject);
            numAsteroids += 1;
            //NOTIFY CLIENT
		}
	}

    // Automatically destroy if 100 units behind the player
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        for (int i = state.getAsteroidListCount() - 1; i >= 0; i--)
        {
            GameObject asteroidObject = state.getAsteroidAt(i);
            AsteroidLogic asteroidLogic = asteroidObject.GetComponent<AsteroidLogic>();
            if (asteroid.transform.position.z < asteroidLogic.player.transform.position.z - 100f)
            {
                numAsteroids -= 1;
                Destroy(asteroid.gameObject);
                state.removeAsteroidAt(i);
                //NOTIFY CLIENT
            }
        }
        StartCoroutine("Cleanup");
    }
}
