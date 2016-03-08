/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Server-side logic for enemy spawner

	Relevant Documentation:
	  * Enemy AI:    https://bitbucket.org/pyrolite/game/wiki/Enemy%20AI
      * Enemy Types: https://bitbucket.org/pyrolite/game/wiki/Enemy%20Types
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private GameObject gameManager;
	private float minDistance;
	private float maxDistance;
	private int maxEnemies;         // Maximum number of enemies at a time
	private int aiWaypointsPerEnemy;
	private int aiWaypointGenerationFactor;
	private int aiWaypointRadius;
	private float aiWaypointWidthScale;
	private float aiWaypointHeightScale;
	private Vector3 aiWaypointShift;
	private int outpostSpawnRadius; // The radius of the sphere around an outpost in which to spawn protecting enemies

	private static int numEnemies = 0; // Number of currently active enemies

    // These control the likelihood of each enemy type to be spawned. Set in IncreaseDifficulty. See InstantiateEnemy for usage.
    private int gnatLimit, fireflyLimit, termiteLimit, lightningBugLimit, 
        hornetLimit, blackWidowLimit, glomCruiserLimit;

	private GameState state;
	private GameObject player, spawnLocation;

	private List<GameObject> aiWaypoints;

	private List<GameObject> playerShipTargets = null;

	private static List<EnemyProperties> enemyTypeList = null;

    private ObjectPoolManager logicManager;
    private ObjectPoolManager gnatManager;
    private ObjectPoolManager fireflyManager;
	private Queue<OutpostSpawnRequest> outpostSpawnRequests = null;

    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        if (gameManager != null)
            state = gameManager.GetComponent<GameState>();

        player = null;
        gnatManager = GameObject.Find("GnatManager").GetComponent<ObjectPoolManager>();
        fireflyManager = GameObject.Find("FireflyManager").GetComponent<ObjectPoolManager>();
        logicManager = GameObject.Find("EnemyLogicManager").GetComponent<ObjectPoolManager>();
        spawnLocation = new GameObject(); // Create temporary object to spawn enemies at
        spawnLocation.name = "EnemySpawnLocation";

        StartCoroutine("Cleanup");

		if (enemyTypeList == null)
			InitialiseEnemyTypes ();

		outpostSpawnRequests = new Queue<OutpostSpawnRequest>();

        StartCoroutine("TimedDifficulty");
    }

    // Increase difficulty by 1 every 30 seconds
    IEnumerator TimedDifficulty()
    {
        IncreaseDifficulty();
        yield return new WaitForSeconds(45f);
        StartCoroutine("TimedDifficulty");

    }

    private void IncreaseDifficulty()
    {
        state.IncreaseDifficulty(1);
        int difficulty = state.GetDifficulty();

        Debug.Log("Difficulty is " + difficulty);

        // Random number is picked between 0-100, so 101 means the enemy type will never spawn at this difficulty level
        switch(difficulty) 
        {
            case 1 :
                maxEnemies = 20;
                gnatLimit = 80;
                fireflyLimit = 100;
                termiteLimit = 101;
                lightningBugLimit = 101;
                hornetLimit = 101;
                blackWidowLimit = 101;
                glomCruiserLimit = 101;
                break;
        case 2 :
                maxEnemies = 30;
                gnatLimit = 60;
                fireflyLimit = 100;
                termiteLimit = 101;
                lightningBugLimit = 101;
                hornetLimit = 101;
                blackWidowLimit = 101;
                glomCruiserLimit = 101;
                break;
        case 3 :
                maxEnemies = 35;
                gnatLimit = 50;
                fireflyLimit = 100;
                termiteLimit = 101;
                lightningBugLimit = 101;
                hornetLimit = 101;
                blackWidowLimit = 101;
                glomCruiserLimit = 101;
                break;
            // The default case will run when the difficulty exceeds the number set by us. In this case, the number of enemies will increase until 120
            default :
                if(maxEnemies < 100)
                    maxEnemies += 10;
                break;
        }

    }


	private void LoadSettings()
	{
		gameManager = settings.GameManager;

		minDistance = settings.EnemyMinSpawnDistance;
		maxDistance = settings.EnemyMaxSpawnDistance;

		aiWaypointsPerEnemy		   = settings.AIWaypointsPerEnemy;
		aiWaypointGenerationFactor = settings.AIWaypointGenerationFactor;
		aiWaypointRadius 		   = settings.AIWaypointRadius;
		aiWaypointWidthScale 	   = settings.AIWaypointWidthScale;
		aiWaypointHeightScale 	   = settings.AIWaypointHeightScale;
		aiWaypointShift 		   = settings.AIWaypointShift;

		outpostSpawnRadius = settings.EnemyOutpostSpawnRadius;
	}

	// Create an EnemyProperties object for each type of enemy that will be used
	private static void InitialiseEnemyTypes()
	{
		enemyTypeList = new List<EnemyProperties>();
		enemyTypeList.Add(new EnemyProperties(EnemyType.Gnat, 50, 0, 20, 15, false, 3f, 4f));
        enemyTypeList.Add(new EnemyProperties(EnemyType.Firefly, 125, 0, 35, 20, false, 3f, 7f ));
        //enemyTypeList.Add(new EnemyProperties(EnemyType.Termite, 30, 0, 10, 25, true, 0f, 0f));
        //enemyTypeList.Add(new EnemyProperties(EnemyType.LightningBug, 30, 0, 5, 25, true, 0f, 0f));
        //enemyTypeList.Add(new EnemyProperties(EnemyType.Hornet, 200, 0, 60, 12, false, 3f, 4f));
        //enemyTypeList.Add(new EnemyProperties(EnemyType.BlackWidow, 350, 0, 75, 18, false, 4f, 6f));
        //enemyTypeList.Add(new EnemyProperties(EnemyType.GlomCruiser, 1000, 0, 1000, 5, false, 5f, 5f));
	}

    // Spawn a new enemy in a random position if less than specified by maxEnemies
    void Update ()
	{
        if (state.Status == GameState.GameStatus.Started)
        {
            if(player == null)
            {
                player = state.PlayerShip;

				Transform playerSpaceshipModel = player.transform.Find ("Model").Find ("Spaceship");
				CreateAIWaypoints(playerSpaceshipModel);
				GetPlayerShipTargets(playerSpaceshipModel);
            }

            // First, spawn regular enemies. Then, spawn enemies around outposts, if needed
			if (numEnemies < maxEnemies)
            {
				SpawnEnemy();
            }
			else if (outpostSpawnRequests.Count > 0)
			{
				OutpostSpawnRequest req = outpostSpawnRequests.Dequeue();
				for (int i = 0; i < req.NumEnemies; i++)
					SpawnWaitingEnemy(req.Location);
			}
        }
	}
		
	private void InstantiateEnemy(out GameObject enemyObject, out EnemyLogic enemyLogic)
	{
		// Spawn enemy and server logic
        GameObject enemyLogicObject = logicManager.RequestObject();

        int random = Random.Range(1,101);
        int type = -1;

        if(random < gnatLimit)
            type = 0;
        else if(random < fireflyLimit)
            type = 1;

        // Default enemy type is the Gnat
        if(type == -1)
            type = 0;

        //int type = Random.Range(0, enemyTypeList.Count);
        if(type == 0) 
            enemyObject = gnatManager.RequestObject();
        else 
            enemyObject = fireflyManager.RequestObject();

        enemyObject.transform.position = spawnLocation.transform.position;
        enemyLogicObject.transform.parent = enemyObject.transform;
        enemyLogicObject.transform.localPosition = Vector3.zero;


		// Set up enemy with components, spawn on network      
		enemyLogic = enemyLogicObject.GetComponent<EnemyLogic> ();

		ApplyEnemyType (enemyLogic, type); // random enemy type
		enemyLogic.SetControlObject(enemyObject);
		enemyLogic.SetPlayer(state.PlayerShip);
		enemyLogic.SetPlayerShipTargets(playerShipTargets);
		enemyLogic.SetAIWaypoints(GetAIWaypointsForEnemy ());

		enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
		if(type == 0)
            gnatManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
        else
            fireflyManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
		numEnemies += 1;
		state.AddToEnemyList(enemyObject);
	}

	// Spawn an enemy with the default settings
	private void SpawnEnemy()
	{
		// Set spawn position based on input attributes
		spawnLocation.transform.position = player.transform.position;
		spawnLocation.transform.rotation = Random.rotation;
		spawnLocation.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

		GameObject enemy; 
		EnemyLogic logic;
		InstantiateEnemy(out enemy, out logic);
	}

	// Spawn an enemy waiting at a location
	private void SpawnWaitingEnemy(Vector3 location)
	{
		spawnLocation.transform.position = location + Random.insideUnitSphere * outpostSpawnRadius;
		spawnLocation.transform.rotation = Random.rotation;

		GameObject enemy; 
		EnemyLogic logic;
		InstantiateEnemy(out enemy, out logic);

		logic.SetGuarding(location);
	}

	// Generate a list of waypoints around the player to guide the enemy ships
	// Each enemy spawned will get some waypoints from this list
	private void CreateAIWaypoints(Transform spaceshipModel)
	{
		int n_waypoints = maxEnemies * aiWaypointGenerationFactor;
		aiWaypoints = new List<GameObject> (n_waypoints);

		// Get the bounds of all (important) ship parts
		List<Bounds> bounds = new List<Bounds> ();
		foreach (Transform child in spaceshipModel)
		{
			GameObject gameObject = child.gameObject;
			if (gameObject.name.Contains ("Engine") || gameObject.name.Contains ("Hull") || gameObject.name.Equals ("CaptainBridge"))
				bounds.Add (gameObject.GetComponent<Renderer> ().bounds);
		}

		GameObject waypoint;
		bool intersects;

		for (int i = 0; i < n_waypoints; i++)
		{
			// Randomly generate a waypoint around the player, but discard it if it's inside the ship
			do
			{
				// Uncomment this to see waypoints as spheres
				/*if (Debug.isDebugBuild)
			    {
					waypoint = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					//waypoint.transform.localScale = waypoint.transform.localScale / 2;
					waypoint.GetComponent<Renderer>().material.color = Color.red;
			    }
				else*/
				waypoint = new GameObject ("AIWaypoint");
				Vector3 pos = Random.insideUnitSphere * aiWaypointRadius;
				pos.x *= aiWaypointWidthScale; // Widen the sphere on the horizontal axis
				pos.y *= aiWaypointHeightScale; // Squash the sphere on the vertical axis

				waypoint.transform.position = pos;
				waypoint.transform.Translate (aiWaypointShift); // Shift the waypoints a upwards and forwards a little, to keep enemies on sight more
				waypoint.transform.parent = player.transform;

				// Check if the waypoint intersects any of the the player ship parts
				intersects = false;
				for (int j = 0; j < bounds.Count && !intersects; j++)
					if (bounds[j].Contains(waypoint.transform.position))
						intersects = true;
			} while(intersects);

			aiWaypoints.Add (waypoint);
		}
	}

	// Get a subset of waypoints to hand out to a particular enemy
	private List<GameObject> GetAIWaypointsForEnemy()
	{
		List<GameObject> waypoints = new List<GameObject> (aiWaypointsPerEnemy);
		int n_waypoints = aiWaypoints.Count, r;

		for (int i = 0; i < aiWaypointsPerEnemy; i++)
		{
			// Make sure we don't hand out the same waypoint more than once
			do
			{
				r = Random.Range (0, n_waypoints);
			} while(waypoints.Contains(aiWaypoints[r]));

			waypoints.Add (aiWaypoints [r]);
		}

		return waypoints;
	}

	// Build a list of targets on the player's ship
	// The enemies will use these to shoot at the player
	private void GetPlayerShipTargets(Transform spaceshipModel)
	{
		playerShipTargets = new List<GameObject>();

		foreach (Transform child in spaceshipModel)
		{
			GameObject gameObject = child.gameObject;
			if (gameObject.name.Contains ("Engine") || gameObject.name.Contains ("Hull") || gameObject.name.Equals ("CaptainBridge"))
				playerShipTargets.Add(gameObject);
		}
	}

	/// <summary>
	/// Decrements the enemy count. Call this when an enemy is destroyed.
	/// </summary>
	public static void DecrementNumEnemies()
	{
		numEnemies--;
	}

	// Remove destroyed enemies from Game State
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);

        if (state.Status == GameState.GameStatus.Started)
			state.CleanupEnemies();

        StartCoroutine("Cleanup");
    }

	// This class holds the various atributes of an enemy. Each enemy type will be be represented by a separate instance
	// TODO: the enemies should look differently based on their type
	private class EnemyProperties
	{
		public int maxHealth, maxShield, collisionDamage, speed;
        public bool isSuicidal;
        public float shootPeriod, shotsPerSec;
		public EnemyType Type { get; private set; }

		public EnemyProperties(EnemyType type, int maxHealth, int maxShield, int collisionDamage,
         int speed, bool isSuicidal, float shootPeriod, float shotsPerSec)
		{
			this.Type            = type;
			this.maxHealth       = maxHealth;
			this.maxShield       = maxShield;
			this.collisionDamage = collisionDamage;
			this.speed           = speed;
            this.isSuicidal = isSuicidal;
            this.shootPeriod = shootPeriod;
            this.shotsPerSec = shotsPerSec;
		}
	}

	// Apply properties to an enemy object, i.e. make it be of certain type
	private static void ApplyEnemyType (EnemyLogic enemy, EnemyType type)
	{
		EnemyProperties props = GetPropertiesOfType (type);
		ApplyEnemyType (enemy, props);
	}

	private static void ApplyEnemyType (EnemyLogic enemy, int index)
	{
		ApplyEnemyType (enemy, enemyTypeList [index]);
	}

	private static void ApplyEnemyType (EnemyLogic enemy, EnemyProperties props)
	{
		enemy.maxHealth       = props.maxHealth;
		enemy.health          = props.maxHealth;
		enemy.maxShield       = props.maxShield;
		enemy.speed           = props.speed;
		enemy.collisionDamage = props.collisionDamage;
        enemy.isSuicidal = props.isSuicidal;
        enemy.shootPeriod = props.shootPeriod;
        enemy.shotsPerSec = props.shotsPerSec;
		enemy.type            = props.Type;
	}

	private static EnemyProperties GetPropertiesOfType(EnemyType type)
	{
		foreach (EnemyProperties props in enemyTypeList)
		{
			if (props.Type == type)
				return props;
		}

		// If our code is correct, this should never happen
		Debug.LogError("Tried to spawn invalid enemy type: " + type.ToString());
		return null;
	}

	public class OutpostSpawnRequest
	{
		public int NumEnemies { get; private set; }
		public Vector3 Location { get; private set; }

		public OutpostSpawnRequest(int numEnemies, Vector3 location)
		{
			this.NumEnemies = numEnemies;
			this.Location   = location;
		}
	}

	// Request spawning of count enemies around outpostLocation
	public void RequestSpawnForOutpost (int count, Vector3 outpostLocation)
	{
		// Only register the request here. It will be spawned on the next frame after all regular enemies are spawned.
		outpostSpawnRequests.Enqueue(new OutpostSpawnRequest(count, outpostLocation));
	}
}

// The types of enemies available. Each type should have its properties initialised before it's used
public enum EnemyType
{
	Gnat,
    Firefly,
    Termite,
    LightningBug,
    Hornet,
    BlackWidow,
    GlomCruiser
}
