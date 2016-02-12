/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Server-side logic for asteroid spawner

	Relevant Documentation:
	  * Asteroid Fields:    https://bitbucket.org/pyrolite/game/wiki/Asteroid%20Fields
*/

using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour 
{
    [SerializeField] GameObject asteroid1; // 3 asteroid objects for the 3 different models
    [SerializeField] GameObject asteroid2;
    [SerializeField] GameObject asteroid3;
    [SerializeField] GameObject gameManager;
    [SerializeField] int maxAsteroids; // Maximum number of asteroids that can exist simultaneously
    [SerializeField] float maxVariation; // Max variation in size (0-10)
    [SerializeField] float minDistance; // Minimum distance to the player that an asteroid can spawn
    [SerializeField] float maxDistance; // Maximum distance to the player that an asteroid can spawn

    GameObject player, spawnLocation, asteroid, logic;
    public static int numAsteroids = 0;
    private GameState state;

	private int SPAWN_MAX_PER_FRAME = 30;

	private const float AVG_SIZE             = 43.6f; // The average asteroid size. Please update this manually if you change the sizes to avoid useless computation
	private const float FIELD_SPACING_FACTOR = 2f;    // Higher values make asteroid fields more sparse. TODO: This value looks good, but is quite expensive

	private bool fieldSpawned = false;

    void Start ()
    {
        // Set game state reference
        if (gameManager != null) state = gameManager.GetComponent<GameState>();
        player = null;
        if(SPAWN_MAX_PER_FRAME > numAsteroids) SPAWN_MAX_PER_FRAME = numAsteroids;
        spawnLocation = new GameObject(); // A temporary game object to spawn asteroids on
        logic = Instantiate(Resources.Load("Prefabs/AsteroidLogic", typeof(GameObject))) as GameObject;
		spawnLocation.name = "AsteroidSpawnLocation";
        StartCoroutine("Cleanup");
    }

    void Update () 
    {
        if (state.GetStatus() == GameState.Status.Started)
        {
            if(player == null) player = state.GetPlayerShip();

			// Spawn up to SPAWN_MAX_PER_FRAME asteroids in a random position if there are less than specified by maxAsteroids
			for (int i = 0; i < SPAWN_MAX_PER_FRAME && numAsteroids < maxAsteroids; i++)
			{
				// The spawn location is positioned randomly within the bounds set by minDistance and maxDistance
				spawnLocation.transform.position = player.transform.position;
				spawnLocation.transform.rotation = Random.rotation;
				spawnLocation.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

				SpawnAsteroid ();
			}

			// Create a demo asteroid field
			// TODO: this is for demonstration purposes only and should be removed in the final game
			if (numAsteroids >= maxAsteroids && !fieldSpawned)
			{
				Vector3 count = new Vector3(10, 3, 10);
				CreateAsteroidField(player.transform.position - Vector3.right * 1000, count);
				fieldSpawned = true;
			}
        }
    }

	// Spawn a single asteroid at the spawn location
	// aka "How to Kill Paralellism in a Nutshell"
	private void SpawnAsteroid()
	{
		int rnd = Random.Range(0,3); // Choose which asteroid prefab to spawn

		if(rnd == 0) asteroid = asteroid1;
		else if(rnd == 1) asteroid = asteroid2;
		else asteroid = asteroid3;

		// Spawn object and logic
		GameObject asteroidObject = Instantiate(asteroid, spawnLocation.transform.position, Quaternion.identity) as GameObject;
		GameObject asteroidLogic = Instantiate(logic, spawnLocation.transform.position, Quaternion.identity) as GameObject;

		// Initialise logic
		asteroidLogic.transform.parent = asteroidObject.transform;
		asteroidLogic.transform.localPosition = Vector3.zero;
		asteroidObject.AddComponent<AsteroidCollision>();
		asteroidLogic.GetComponent<AsteroidLogic>().SetPlayer(state.GetPlayerShip(), maxVariation, rnd);
		asteroidLogic.GetComponent<AsteroidLogic>().SetStateReference(state);

		// Add collider and rigidbody
		SphereCollider sphere = asteroidObject.AddComponent<SphereCollider>();
		sphere.isTrigger = true;
		Rigidbody rigid = asteroidObject.AddComponent<Rigidbody>();
		rigid.isKinematic = true;

		// Spawn on the network and add to GameState
		state.AddAsteroidList(asteroidObject);
		ServerManager.NetworkSpawn(asteroidObject);
		numAsteroids += 1;
	}

	// Create an asteroid field of the default density around a specified poistion and with a set number of asteroids
	private void CreateAsteroidField(Vector3 position, Vector3 count)
	{
		Vector3 size = new Vector3(count.x * AVG_SIZE, count.y * AVG_SIZE, count.z * AVG_SIZE) * FIELD_SPACING_FACTOR;
		int numAsteroids = System.Convert.ToInt32(count.x * count.y * count.z);
		Vector3 spawnPosition = new Vector3();

		for (int i = 0; i < numAsteroids; i++)
		{
			// Get a random position inside the field
			spawnPosition.x = position.x + Random.Range(-size.x / 2, size.x / 2);
			spawnPosition.y = position.y + Random.Range(-size.y / 2, size.y / 2);
			spawnPosition.z = position.z + Random.Range(-size.z / 2, size.z / 2);

			spawnLocation.transform.position = spawnPosition;

			SpawnAsteroid();
		}
	}

	// Create an asteroid field centred around position with specified dimensions and density
	private void CreateAsteroidField(Vector3 position, Vector3 size, float density)
	{
		Vector3 count = new Vector3(size.x / AVG_SIZE, size.z / AVG_SIZE, size.z / AVG_SIZE);
		count *= density / FIELD_SPACING_FACTOR;

		CreateAsteroidField(position, count);
	}

    // Remove asteroid from GameState if destroyed
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
