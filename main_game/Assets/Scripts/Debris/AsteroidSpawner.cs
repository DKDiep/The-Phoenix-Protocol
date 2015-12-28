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

	[SerializeField] GameObject asteroid1;
	[SerializeField] GameObject asteroid2;
	[SerializeField] GameObject asteroid3;
	[SerializeField] int maxAsteroids;
	[SerializeField] float maxVariation; // Max variation in size (0-10)
	public static int numAsteroids = 0;

    [SerializeField] GameObject gameManager;
    private GameState state;
	
	void Start ()
    {
        // Set game state reference
        if (gameManager != null)
        {
            state = gameManager.GetComponent<GameState>();
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
                int rnd = Random.Range(0,3);
                GameObject asteroid;
                if(rnd == 0) asteroid = asteroid1;
                else if(rnd == 1) asteroid = asteroid2;
                else asteroid = asteroid3;
                GameObject asteroidObject = Instantiate(asteroid, rand_position, Quaternion.identity) as GameObject;
				GameObject asteroidLogic = Instantiate(Resources.Load("Prefabs/AsteroidLogic", typeof(GameObject))) as GameObject;
				asteroidLogic.transform.parent = asteroidObject.transform;
        asteroidLogic.transform.localPosition = Vector3.zero;
                asteroidLogic.GetComponent<AsteroidLogic>().SetPlayer(state.GetPlayerShip(), maxVariation, rnd);
                asteroidLogic.GetComponent<AsteroidLogic>().SetStateReference(state);
                asteroidObject.AddComponent<AsteroidCollision>();
				SphereCollider sphere = asteroidObject.AddComponent<SphereCollider>();
				sphere.isTrigger = true;
				Rigidbody rigid = asteroidObject.AddComponent<Rigidbody>();
				rigid.isKinematic = true;
                state.AddAsteroidList(asteroidObject);
				ServerManager.NetworkSpawn(asteroidObject);
                numAsteroids += 1;
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
                if(asteroidObject == null)
                {
                  state.RemoveAsteroidAt(i);
                }
            }
        }
        StartCoroutine("Cleanup");
    }
}
