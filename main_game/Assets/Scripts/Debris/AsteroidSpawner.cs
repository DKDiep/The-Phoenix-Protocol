/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
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
    private ServerManager serverManager;
	
	void Start ()
    {
        // Set game state reference
        if (gameManager != null)
        {
            state = gameManager.GetComponent<GameState>();
            serverManager = gameManager.GetComponent<ServerManager>();
        }
        StartCoroutine("Cleanup");
    }

	void Update () 
	{
        if (state.GetStatus() == GameState.Status.Started)
        {
            // Spawn a new asteroid in a random position if there are less than specified by maxAsteroids
            if (numAsteroids < maxAsteroids)
            {
                Vector3 rand_position = new Vector3(transform.position.x + Random.Range(-800, 800), transform.position.y + Random.Range(-800, 800), transform.position.z + 150 + Random.Range(50, 1000));
                GameObject asteroidObject = Instantiate(asteroid, rand_position, Quaternion.identity) as GameObject;
                ServerManager.NetworkSpawn(asteroidObject);
                asteroidObject.GetComponent<AsteroidLogic>().SetPlayer(state.GetPlayerShip(), maxVariation);
                state.AddAsteroidList(asteroidObject);
                numAsteroids += 1;
                //NOTIFY CLIENT
                //if (serverManager.clientIdCount() > 1)
                // serverManager.RpcSpawn("asteroid");
            }
            if (Input.GetMouseButtonDown(0) && serverManager != null)
            {
                //serverManager.RpcSpawn("asteroid");
                //ClientController.networkIdentity.RPC();
            }
        }
    }

    // Automatically destroy if 100 units behind the player
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        if (state.GetStatus() == GameState.Status.Started)
        {
            for (int i = state.GetAsteroidListCount() - 1; i >= 0; i--)
            {
                GameObject asteroidObject = state.GetAsteroidAt(i);
                AsteroidLogic asteroidLogic = asteroidObject.GetComponent<AsteroidLogic>();
                if (asteroidObject.transform.position.z < asteroidLogic.player.transform.position.z - 100f)
                {
                    numAsteroids -= 1;
                    state.RemoveAsteroidAt(i);
                    Destroy(asteroidObject.gameObject);
                    //NOTIFY CLIENT

                }
            }
        }
        StartCoroutine("Cleanup");
    }
}
