/*
    Server-side logic for asteroid spawner

	Relevant Documentation:
	  * Asteroid Fields:    https://bitbucket.org/pyrolite/game/wiki/Asteroid%20Fields
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidSpawner : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private GameObject gameManager;
	private int maxAsteroids;    // Maximum number of asteroids that can exist simultaneously
	private float maxVariation;  // Max variation in size (0-10)
	private float minDistance;   // Minimum distance to the player that an asteroid can spawn
	private float maxDistance;   // Maximum distance to the player that an asteroid can spawn
	private int maxSpawnedPerFrame;
	private float avgSize;               // The average asteroid size. Please update this manually if you change the sizes to avoid useless computation
	private float fieldSpacingFactor;    // Higher values make asteroid fields more sparse. TODO: 2f looks good, but is quite expensive
	private float visibilityEdgeSpawnMaxAngle; // The maximum rotation angle on the x and y axes when spawning on the visibility edge

    private GameObject player, spawnLocation;
	private GameState state;

	private int numAsteroids;
	private int numAsteroidsInFields;

	private bool initialSpawnCompleted = false;
	private bool fieldSpawned = false;

    private ObjectPoolManager explosionManager;
    private ObjectPoolManager logicManager;
    private ObjectPoolManager asteroidManager;

    void Start ()
    {
		numAsteroids = numAsteroidsInFields = 0;

		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        // Set game state reference
        if (gameManager != null)
			state = gameManager.GetComponent<GameState>();
        
		Instantiate(Resources.Load("Prefabs/AsteroidLogic", typeof(GameObject)));

		player = null;
        spawnLocation      = new GameObject(); // A temporary game object to spawn asteroids on
		spawnLocation.name = "AsteroidSpawnLocation";
        explosionManager   = GameObject.Find("AsteroidExplosionManager").GetComponent<ObjectPoolManager>();
        logicManager       = GameObject.Find("AsteroidLogicManager").GetComponent<ObjectPoolManager>();
        asteroidManager    = GameObject.Find("AsteroidManager").GetComponent<ObjectPoolManager>();
        
		StartCoroutine("Cleanup");
    }

    public void Reset()
    {
        state.CleanUpAsteroids();
        List<GameObject> asteroidList = state.GetAsteroidList();
        // Must loop backwards as removing
        for (int i = asteroidList.Count - 1; i >= 0; i--)
        {
            // Remove using logic
            AsteroidLogic logic = asteroidList[i].GetComponentInChildren<AsteroidLogic>();
            if (logic != null)
            {
                logic.Despawn();
            }
        }

        numAsteroids = numAsteroidsInFields = 0;
        LoadSettings();
    }

    private void LoadSettings()
	{
		gameManager 	   = settings.GameManager;
		maxAsteroids 	   = settings.MaxAsteroids;
		maxVariation 	   = settings.AsteroidMaxVariation;
		minDistance 	   = settings.AsteroidMinDistance;
		maxDistance 	   = settings.AsteroidMaxDistance;
		maxSpawnedPerFrame = settings.MaxAsteroidsSpawnedPerFrame;
		avgSize 		   = settings.AsteroidAvgSize;

		fieldSpacingFactor = settings.AsteroidFieldSpacingFactor;

		visibilityEdgeSpawnMaxAngle = settings.AsteroidVisibilityEdgeSpawnMaxAngle;
	}

    void Update () 
    {
        if (state.Status == GameState.GameStatus.Started)
        {
            if(player == null)
				player = state.PlayerShip;

			// Spawn up to maxSpawnedPerFrame asteroids in a random position if there are less than specified by maxAsteroids
			for (int i = 0; !initialSpawnCompleted && i < maxSpawnedPerFrame && numAsteroids < maxAsteroids; i++)
			{
				// The spawn location is positioned randomly within the bounds set by minDistance and maxDistance
				spawnLocation.transform.position = player.transform.position;
				spawnLocation.transform.rotation = Random.rotation;
				spawnLocation.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

				SpawnAsteroid ();
			}

			if (numAsteroids == maxAsteroids)
				initialSpawnCompleted = true;

			// Create a demo asteroid field
			// TODO: this is for demonstration purposes only and should be removed in the final game
			/*if (numAsteroids >= maxAsteroids && !fieldSpawned)
			{
				Vector3 count = new Vector3(10, 3, 10);
				CreateAsteroidField(player.transform.position - Vector3.right * 1000, count);
				fieldSpawned = true;
			}*/
        }
    }

	// Spawn a single asteroid at the spawn location
	// aka "How to Kill Paralellism in a Nutshell"
	private void SpawnAsteroid()
	{
		int rnd = Random.Range(0,3); // Choose which asteroid prefab to spawn

		// Spawn object and logic
        GameObject asteroidObject = asteroidManager.RequestObject();
        asteroidObject.transform.position = spawnLocation.transform.position;
        asteroidObject.GetComponent<AsteroidRotation>().SetSpeed(Random.Range(6f,30f));

        GameObject asteroidLogic = logicManager.RequestObject();
        asteroidLogic.transform.position = spawnLocation.transform.position;

		// Initialise logic
		asteroidLogic.transform.parent = asteroidObject.transform;
		asteroidLogic.GetComponent<AsteroidLogic>().SetPlayer(state.PlayerShip, maxVariation, rnd, explosionManager, logicManager, asteroidManager);
		asteroidLogic.GetComponent<AsteroidLogic>().SetStateReference(state);

        asteroidManager.EnableClientObject(asteroidObject.name, asteroidObject.transform.position, asteroidObject.transform.rotation, asteroidObject.transform.localScale);

		// Spawn on the network and add to GameState
		state.AddToAsteroidList(asteroidObject);
		numAsteroids += 1;
	}

	// Create an asteroid field of the default density around a specified poistion and with a set number of asteroids
	private void CreateAsteroidField(Vector3 position, Vector3 count)
	{
		Vector3 size = new Vector3(count.x * avgSize, count.y * avgSize, count.z * avgSize) * fieldSpacingFactor;
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

		numAsteroidsInFields += numAsteroids;
	}

	/// <summary>
	/// Decrements the asteroid count. Call this when an asteroid is destroyed.
	/// </summary>
	public void DecrementNumAsteroids()
	{
		numAsteroids--;
	}

	// Create an asteroid field centred around position with specified dimensions and density
	private void CreateAsteroidField(Vector3 position, Vector3 size, float density)
	{
		Vector3 count = new Vector3(size.x / avgSize, size.z / avgSize, size.z / avgSize);
		count *= density / fieldSpacingFactor;

		CreateAsteroidField(position, count);
	}

	/// <summary>
	/// Registers that an Asteroid has gone out of view.
	/// 
	/// This currently spawns a new aasteroid in front of the player to keep the number constant.
	/// Asteroid fields will probably fuck up the count.
	/// </summary>
	public void OnAsteroidOutOfView()
	{
		if ((numAsteroids - numAsteroidsInFields) < maxAsteroids)
			SpawnAsteroidAtVisibilityEdge();
	}

	/// <summary>
	/// Spawns a new asteroid on the player's visibility edge, i.e. as far as they can see.
	/// 
	/// This should be called when a new asteroid is needed affter one has moved out of view.
	/// </summary>
	private void SpawnAsteroidAtVisibilityEdge()
	{
		spawnLocation.transform.position = player.transform.position;

		float halfAngle = visibilityEdgeSpawnMaxAngle / 2.0f;
		spawnLocation.transform.rotation = player.transform.rotation;
		spawnLocation.transform.Rotate(Random.Range(-halfAngle, halfAngle), Random.Range(-90f, 90), 0);
		spawnLocation.transform.Translate(Vector3.forward * maxDistance);

		SpawnAsteroid();
	}

    // Remove asteroid from GameState if destroyed
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        if (state.Status == GameState.GameStatus.Started)
			state.CleanUpAsteroids();

        StartCoroutine("Cleanup");
    }
}
