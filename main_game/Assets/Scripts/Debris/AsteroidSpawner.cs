/*
    Server-side logic for asteroid spawner

	Relevant Documentation:
	  * Asteroid Fields:    https://bitbucket.org/pyrolite/game/wiki/Asteroid%20Fields
*/

using UnityEngine;
using System.Collections;

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
	private static float avgSize;              // The average asteroid size. Please update this manually if you change the sizes to avoid useless computation
	private static float fieldSpacingFactor;   // Higher values make asteroid fields more sparse. TODO: 2f looks good, but is quite expensive
	private float visibilityEdgeSpawnMaxAngle; // The maximum rotation angle on the x and y axes when spawning on the visibility edge

    private GameObject player, spawnLocation;
	private GameState state;

	private int numAsteroids;
	private int numAsteroidsInFields;

	private bool initialSpawnCompleted = false;
	private AsteroidField testField;

    private ObjectPoolManager explosionManager;
    private ObjectPoolManager logicManager;
    private ObjectPoolManager asteroidManager;

	private const string TAG_DEBRIS = "Debris";

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
			if (numAsteroids >= maxAsteroids && testField == null)
			{
				testField = AsteroidField.Create(player.transform.position - Vector3.right * 1000, new Vector3(10, 3, 10));
				SpawnAsteroidField(testField);
			}
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

	/// <summary>
	/// Spawns an asteroid field if not already spawned.
	/// </summary>
	/// <param name="field">The field description.</param>
	private void SpawnAsteroidField(AsteroidField field)
	{
		// Don't spawn a field if it's already spawned
		if (field.Spawned)
			return;

		Vector3 spawnPosition = new Vector3();
		spawnLocation.transform.rotation = Quaternion.identity;

		for (int i = 0; i < field.TotalNumAsteroids; i++)
		{
			// Get a random position inside the field
			spawnPosition.x = field.Position.x + Random.Range(-field.Size.x / 2, field.Size.x / 2);
			spawnPosition.y = field.Position.y + Random.Range(-field.Size.y / 2, field.Size.y / 2);
			spawnPosition.z = field.Position.z + Random.Range(-field.Size.z / 2, field.Size.z / 2);

			spawnLocation.transform.position = spawnPosition;

			SpawnAsteroid();
		}

		field.Spawned = true;

		numAsteroidsInFields += field.TotalNumAsteroids;
		Debug.Log("Spawned field: " + field.TotalNumAsteroids);
	}

	/// <summary>
	/// Despawns an asteroid field if it is spawned.
	/// </summary>
	/// <param name="field">The field description.</param>
	private void DespawnAsteroidField(AsteroidField field)
	{
		// Don't despawn a field that is not spawned
		if (!field.Spawned)
			return;

		// Find the asteroids in the field's area and despawn them
		// Of course, this might hit asteroids that are not part of the field but are in the same general area,
		// but that's fine.
		Collider[] asteroids = Physics.OverlapBox(field.Position, field.Size / 2);
		int despawned = 0;
		foreach (Collider col in asteroids)
		{
			if (col.CompareTag(TAG_DEBRIS))
			{
				AsteroidLogic logic = col.gameObject.GetComponentInChildren<AsteroidLogic>();
				/*if (logic != null)
				{*/
				logic.Despawn();
				despawned++;
				//}
			}
		}

		field.Spawned 		  = false;
		numAsteroidsInFields -= field.TotalNumAsteroids;
		Debug.Log("Despawned: " + despawned);
	}
		
	/// <summary>
	/// Decrements the asteroid count. Call this when an asteroid is destroyed.
	/// </summary>
	public void DecrementNumAsteroids()
	{
		numAsteroids--;
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

	/// <summary>
	/// Class describing an asteroid field.
	/// </summary>
	private class AsteroidField
	{
		/// <summary>
		/// Indicates whether this <see cref="AsteroidSpawner+AsteroidField"/> is spawned.
		/// </summary>
		/// <value><c>true</c> if spawned; otherwise, <c>false</c>.</value>
		public bool Spawned { get; set; }

		/// <summary>
		/// Gets the position of the field.
		/// </summary>
		/// <value>The centre of the field.</value>
		public Vector3 Position { get; private set; }

		/// <summary>
		/// Gets the asteroid count in each direction.
		/// </summary>
		/// <value>The asteroid count.</value>
		public Vector3 AsteroidCount { get; private set; }

		/// <summary>
		/// Gets the total number of asteroids in this field.
		/// </summary>
		/// <value>The total number of asteroids.</value>
		public int TotalNumAsteroids
		{
			get { return System.Convert.ToInt32(AsteroidCount.x * AsteroidCount.y * AsteroidCount.z); }
		}

		/// <summary>
		/// Gets the spacial dimensions of the field.
		/// </summary>
		/// <value>The field size.</value>
		public Vector3 Size { get; private set; }

		/// <summary>
		/// Initializes a new <see cref="AsteroidSpawner+AsteroidField"/>.
		/// </summary>
		/// <param name="position">The centre position.</param>
		/// <param name="count">The asteroid count.</param>
		private AsteroidField(Vector3 position, Vector3 count)
		{
			this.Position 	   = position;
			this.AsteroidCount = count;
			this.Size 		   = new Vector3(count.x * avgSize, count.y * avgSize,
				count.z * avgSize) * fieldSpacingFactor;
		}
			
		/// <summary>
		/// Creates an asteroid field of the default density around a specified poistion and with a set number of asteroids
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="count">The asteroid count.</param>
		public static AsteroidField Create(Vector3 position, Vector3 count)
		{
			return new AsteroidField(position, count);
		}

		/// <summary>
		/// Creates an asteroid field centred around <c>position</c> with specified dimensions and density.
		/// </summary>
		/// <param name="position">The centre position.</param>
		/// <param name="size">The size.</param>
		/// <param name="density">The density.</param>
		public static AsteroidField Create(Vector3 position, Vector3 size, float density)
		{
			Vector3 count  = new Vector3(size.x / avgSize, size.z / avgSize, size.z / avgSize);
			count 	      *= density / fieldSpacingFactor;

			return new AsteroidField(position, count);
		}
	}
}
