﻿/*
    Server-side logic for enemy spawner

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
    // Maximum number of enemies at a time
	private int maxEnemies;
	private int aiWaypointsPerEnemy;
	private int aiWaypointGenerationFactor;
	private int aiWaypointRadius;
	private float aiWaypointWidthScale;
	private float aiWaypointHeightScale;
	private Vector3 aiWaypointShift;
    // The radius of the sphere around an outpost in which to spawn protecting enemies
	private int outpostSpawnRadius;

    private MusicManager music;
    
	private bool mothershipSpawned = false;
    public GameObject mothershipEnemySpawner;
	public GameObject mothership;

    // Number of currently active enemies
	private static int numEnemies = 0;

    // These control the likelihood of each enemy type to be spawned. Set in IncreaseDifficulty. See InstantiateEnemy for usage.
    private int gnatLimit, fireflyLimit, termiteLimit, lightningBugLimit, 
        hornetLimit, blackWidowLimit, glomCruiserLimit;

	private GameState state;
	private GameObject player, spawnLocation;

	private List<GameObject> aiWaypoints;

	private List<GameObject> playerShipTargets = null;

	private static EnemyProperties[] enemyTypeList;

    private ObjectPoolManager logicManager;
    private ObjectPoolManager gnatManager;
    private ObjectPoolManager fireflyManager;
    private ObjectPoolManager termiteManager;
    private ObjectPoolManager lightningBugManager;
    private ObjectPoolManager hornetManager;
    private ObjectPoolManager blackWidowManager;

	private Queue<OutpostSpawnRequest> outpostSpawnRequests;
	private Queue<SingleSpawnRequest> singleSpawnRequests;

    private GameObject portal;

    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        if (gameManager != null)
            state = gameManager.GetComponent<GameState>();

        player = null;
        gnatManager = GameObject.Find("GnatManager").GetComponent<ObjectPoolManager>();
        fireflyManager = GameObject.Find("FireflyManager").GetComponent<ObjectPoolManager>();
        termiteManager = GameObject.Find("TermiteManager").GetComponent<ObjectPoolManager>();
        lightningBugManager = GameObject.Find("LightningBugManager").GetComponent<ObjectPoolManager>();
        hornetManager = GameObject.Find("HornetManager").GetComponent<ObjectPoolManager>();
        blackWidowManager = GameObject.Find("BlackWidowManager").GetComponent<ObjectPoolManager>();
        logicManager = GameObject.Find("EnemyLogicManager").GetComponent<ObjectPoolManager>();
        spawnLocation = new GameObject(); // Create temporary object to spawn enemies at
        spawnLocation.name = "EnemySpawnLocation";

		outpostSpawnRequests = new Queue<OutpostSpawnRequest>();
		singleSpawnRequests  = new Queue<SingleSpawnRequest>();

        StartCoroutine(CheckPortalDistance());
    }

    IEnumerator CheckPortalDistance()
    {
        if(portal == null)
            portal = GameObject.Find("Portal(Clone)");
		else if (state.Status == GameState.GameStatus.Started)
        {
            float distance = Vector3.Distance(portal.transform.position, state.PlayerShip.transform.position);
            if(distance < settings.GlomMothershipSpawnDistance)
            {
                SpawnGlomMothership();
                mothershipSpawned = true;
                state.motherShipSpawned = true;
            }

        }

        yield return new WaitForSeconds(1f);
        if(!mothershipSpawned)
            StartCoroutine(CheckPortalDistance());

    }

    void SpawnGlomMothership()
    {
        // Debug.Log("Glom mothership spawned");
        if(music == null)
            music = GameObject.Find("MusicManager(Clone)").GetComponent<MusicManager>();

        music.PlayMusic(2);
        GameObject spawnEffect = Instantiate(Resources.Load("Prefabs/MothershipSpawnEffect", typeof(GameObject))) as GameObject;
        //spawnEffect.transform.position = settings.GlomMothershipSpawnPosition;
        Vector3 direction = player.transform.position - settings.PortalPosition;
        direction.Normalize();
        spawnEffect.transform.position = settings.PortalPosition + (direction * (settings.GlomMothershipSpawnDistance / 3));
        ServerManager.NetworkSpawn(spawnEffect);
        StartCoroutine(SetupMothership(spawnEffect));

    }

    IEnumerator SetupMothership(GameObject spawnEffect)
    {
        yield return new WaitForSeconds(0.5f);
        mothership = Instantiate(Resources.Load("Prefabs/GlomMothership", typeof(GameObject))) as GameObject;
        GameObject logic = Instantiate(Resources.Load("Prefabs/GlomMothershipLogic", typeof(GameObject))) as GameObject;
        logic.transform.parent = mothership.transform;
        logic.transform.localPosition = Vector3.zero;
        mothership.transform.position = spawnEffect.transform.position;
        ServerManager.NetworkSpawn(mothership);

        mothership.GetComponent<Collider>().enabled = true;
        logic.GetComponent<MothershipLogic>().SetSpawner(this);
        yield return new WaitForSeconds(1f);
        spawnEffect.GetComponent<DisableSpawnEffect>().DisableParticles();
        yield return new WaitForSeconds(1f);
        CommanderVoice.SendCommand(5);
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

            // First, spawn regular enemies.
			// Then, spawn enemies around outposts, and single requested enemies, if needed
            if (numEnemies < maxEnemies)
            {
                numEnemies += 1;
                SpawnEnemy();
            }
            else if (outpostSpawnRequests.Count > 0)
            {
                OutpostSpawnRequest req = outpostSpawnRequests.Dequeue();
                for (int i = 0; i < req.NumEnemies; i++)
                    SpawnWaitingEnemy(req.Location, req.TriggerDistance);
            }
			else if (singleSpawnRequests.Count > 0)
			{
				SingleSpawnRequest req = singleSpawnRequests.Dequeue();
				SpawnEnemy(req.Location, req.Type);
			}
        }
    }

    private void LoadSettings()
    {
        gameManager = settings.GameManager;

        minDistance = settings.EnemyMinSpawnDistance;
        maxDistance = settings.EnemyMaxSpawnDistance;

        aiWaypointsPerEnemy = settings.AIWaypointsPerEnemy;
        aiWaypointGenerationFactor = settings.AIWaypointGenerationFactor;
        aiWaypointRadius = settings.AIWaypointRadius;
        aiWaypointWidthScale = settings.AIWaypointWidthScale;
        aiWaypointHeightScale = settings.AIWaypointHeightScale;
        aiWaypointShift = settings.AIWaypointShift;

        outpostSpawnRadius = settings.EnemyOutpostSpawnRadius;

        enemyTypeList = settings.enemyProperties;
    }

    public void Reset()
    {
        StopAllCoroutines();

        // Despawn enemies
		List<GameObject> enemyList = state.GetEnemyList();
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            // Remove using logic if able
            EnemyLogic logic = enemyList[i].GetComponentInChildren<EnemyLogic>(true);
			if (logic != null)
				logic.Despawn();
			else
			{
				// Some enemies have a null logic (we don't know why), so remove just the enemy as a workaround
				ObjectPoolManager manager = FindManager(enemyList[i]);
				if (manager != null)
				{
					string name = enemyList[i].name;
					state.RemoveEnemy(enemyList[i], false);
					manager.DisableClientObject(name);
					manager.RemoveObject(name);
				}
				else
					Debug.LogWarning("Could not find manager for " + enemyList[i].name + " so it was not removed.");
			}
        }

		if (mothership != null)
			ServerManager.NetworkDestroy(mothership);
		mothershipSpawned = false;
		StartCoroutine(CheckPortalDistance());

        state.SetDifficulty(0);
		outpostSpawnRequests.Clear();
		singleSpawnRequests.Clear();
		numEnemies = 0;
    }

	/// <summary>
	/// Trues to find the pool manager that owns the specified enemy.
	/// </summary>
	/// <param name="obj">The object.</param>
	/// <returns>The manager, or <c>null</c> if no enemy manager owns the enemy.</returns>
	private ObjectPoolManager FindManager(GameObject obj)
	{
		ObjectPoolManager[] managers = new ObjectPoolManager[] { logicManager,  gnatManager, fireflyManager,
			termiteManager, lightningBugManager, hornetManager, blackWidowManager };

		foreach (ObjectPoolManager manager in managers)
		{
			if (manager.Owns(obj))
				return manager;
		}

		return null;
	}

	/// <summary>
	/// Starts the difficulty timer.
	/// 
	/// This should be called from <c>ReadyScreen</c> instead of starting the coroutine directly.
	/// </summary>
	public void StartDifficultyTimer()
	{
		StartCoroutine(TimedDifficulty());
	}

    // Increase difficulty by 1 every 60 seconds
    IEnumerator TimedDifficulty()
    {
        IncreaseDifficulty();
        yield return new WaitForSeconds(30f);
        StartCoroutine(TimedDifficulty());
    }

    private void IncreaseDifficulty()
    {
        state.IncreaseDifficulty(1);
        int difficulty = state.GetDifficulty();

        // Debug.Log("Difficulty is " + difficulty);

        // Random number is picked between 0-100, so 101 means the enemy type will never spawn at this difficulty level
        switch(difficulty) 
        {
            case 1 :
                maxEnemies = 15;
                gnatLimit = 90;
                fireflyLimit = 100;
                termiteLimit = 101;
                lightningBugLimit = 101;
                hornetLimit = 101;
                blackWidowLimit = 101;
                break;

        case 2 :
                maxEnemies = 30;
                gnatLimit = 60;
                fireflyLimit = 100;
                termiteLimit = 101;
                lightningBugLimit = 101;
                hornetLimit = 101;
                blackWidowLimit = 101;
                break;
        case 3 :
                maxEnemies = 35;
                gnatLimit = 50;
                fireflyLimit = 80;
                termiteLimit = 100;
                lightningBugLimit = 101;
                hornetLimit = 101;
                blackWidowLimit = 101;
                break;
        case 4 :
                maxEnemies = 40;
                gnatLimit = 30;
                fireflyLimit = 60;
                termiteLimit = 80;
                lightningBugLimit = 100;
                hornetLimit = 101;
                blackWidowLimit = 101;
                break;
        case 5 :
                maxEnemies = 40;
                gnatLimit = 20;
                fireflyLimit = 60;
                termiteLimit = 70;
                lightningBugLimit = 80;
                hornetLimit = 100;
                blackWidowLimit = 101;
                break;
        case 6 :
                maxEnemies = 45;
                gnatLimit = 20;
                fireflyLimit = 40;
                termiteLimit = 60;
                lightningBugLimit = 70;
                hornetLimit = 100;
                blackWidowLimit = 101;
                break;
        case 7 :
                maxEnemies = 50;
                gnatLimit = 20;
                fireflyLimit = 40;
                termiteLimit = 50;
                lightningBugLimit = 60;
                hornetLimit = 90;
                blackWidowLimit = 100;
                break;
        case 8 :
                maxEnemies = 50;
                gnatLimit = 10;
                fireflyLimit = 25;
                termiteLimit = 40;
                lightningBugLimit = 50;
                hornetLimit = 85;
                blackWidowLimit = 100;
                break;
        case 9 :
                maxEnemies = 50;
                gnatLimit = 10;
                fireflyLimit = 20;
                termiteLimit = 35;
                lightningBugLimit = 45;
                hornetLimit = 70;
                blackWidowLimit = 100;
                break;
            // The default case will run when the difficulty exceeds the number set by us. In this case, the number of enemies will increase until 120
            default :
                if(maxEnemies < 100)
                    maxEnemies += 10;
                break;
        }

    }

	private void InstantiateEnemy(out GameObject enemyObject, out EnemyLogic enemyLogic)
	{
		EnemyType type = GetRandomEnemyTypeForDifficulty();
		InstantiateEnemy(type, out enemyObject, out enemyLogic);
	}
		
	private void InstantiateEnemy(EnemyType type, out GameObject enemyObject, out EnemyLogic enemyLogic)
	{
		// Spawn enemy and server logic
        GameObject enemyLogicObject = logicManager.RequestObject();
        enemyObject = null;

		if(type == EnemyType.Gnat)
			enemyObject = gnatManager.RequestObject();
		else if(type == EnemyType.Firefly)
            enemyObject = fireflyManager.RequestObject();
		else if(type == EnemyType.Termite)
            enemyObject = termiteManager.RequestObject();
		else if(type == EnemyType.LightningBug)
            enemyObject = lightningBugManager.RequestObject();
        else if(type == EnemyType.Hornet)
            enemyObject = hornetManager.RequestObject();
        else if(type == EnemyType.BlackWidow)
            enemyObject = blackWidowManager.RequestObject();

        enemyObject.transform.position = spawnLocation.transform.position;

		// Set up enemy with components, spawn on network      
		enemyLogic = enemyLogicObject.GetComponent<EnemyLogic> ();

		ApplyEnemyType (enemyLogic, type, state.GetShipBaseSpeed()); 
        enemyLogicObject.transform.parent = enemyObject.transform;
        enemyLogicObject.transform.localPosition = Vector3.zero;
		enemyLogic.SetControlObject(enemyObject);
		enemyLogic.SetPlayer(state.PlayerShip);
		enemyLogic.SetPlayerShipTargets(playerShipTargets);
		enemyLogic.SetAIWaypoints(GetAIWaypointsForEnemy ());

		enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation

		if(type == EnemyType.Gnat)
            gnatManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
		else if(type == EnemyType.Firefly)
            fireflyManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
		else if(type == EnemyType.Termite)
            termiteManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
		else if(type == EnemyType.LightningBug)
            lightningBugManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
        else if(type == EnemyType.Hornet)
            hornetManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);
        else if(type == EnemyType.BlackWidow)
            blackWidowManager.EnableClientObject(enemyObject.name, enemyObject.transform.position, enemyObject.transform.rotation, enemyObject.transform.localScale);

		state.AddToEnemyList(enemyObject);
        enemyLogicObject.transform.parent = enemyObject.transform;
        enemyLogicObject.transform.localPosition = Vector3.zero;
	}
		
	/// <summary>
	/// Spawns an enemy with the default settings.
	/// </summary>
	private void SpawnEnemy()
	{
		// Get a random position around the player
		spawnLocation.transform.position = player.transform.position;
		spawnLocation.transform.rotation = Random.rotation;
		spawnLocation.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

		GameObject enemy; 
		EnemyLogic logic;
		InstantiateEnemy(out enemy, out logic);
	}

    public void SpawnEnemyFromMothership()
    {
        // Get a random position around the player
        spawnLocation.transform.position = mothershipEnemySpawner.transform.position;

        GameObject enemy; 
        EnemyLogic logic;
        InstantiateEnemy(out enemy, out logic);
    }

	/// <summary>
	/// Spawns an enemy of a specified type at a specified location.
	/// </summary>
	/// <param name="location">The spawn location.</param>
	/// <param name="type">The enemy type.</param>
	private void SpawnEnemy(Vector3 location, EnemyType type)
	{
		spawnLocation.transform.position = location;

		GameObject enemy; 
		EnemyLogic logic;
		InstantiateEnemy(type, out enemy, out logic);
	}

	/// <summary>
	/// Spawns an enemy guarding a location.
	/// </summary>
	/// <param name="location">The location.</param>
	/// <param name="triggerDistance">The guard's trigger distance.</param>
	private void SpawnWaitingEnemy(Vector3 location, int triggerDistance)
	{
		spawnLocation.transform.position = location + Random.insideUnitSphere * outpostSpawnRadius;
		spawnLocation.transform.rotation = Random.rotation;

		GameObject enemy; 
		EnemyLogic logic;
		InstantiateEnemy(out enemy, out logic);

		logic.SetGuarding(triggerDistance);
	}

	/// <summary>
	/// Gets a random enemy type while maintaining balance for the current difficulty.
	/// </summary>
	/// <returns>The chosen enemy type.</returns>
	private EnemyType GetRandomEnemyTypeForDifficulty()
	{
		int random = Random.Range(1,101);
		EnemyType type = EnemyType.Gnat;

		if(random < gnatLimit)
			type = EnemyType.Gnat;
		else if(random < fireflyLimit)
			type = EnemyType.Firefly;
		else if(random < termiteLimit)
			type = EnemyType.Termite;
		else if(random < lightningBugLimit)
			type = EnemyType.LightningBug;
		else if(random < hornetLimit)
			type = EnemyType.Hornet;
		else if(random < blackWidowLimit)
			type = EnemyType.BlackWidow;

		return type;
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

				waypoint.transform.position = player.transform.position + pos;
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

	// This class holds the various atributes of an enemy. Each enemy type will be be represented by a separate instance
    [System.Serializable] 
	public class EnemyProperties
	{
        public EnemyType type;
		public int maxHealth, maxShield, collisionDamage;
        public bool isSuicidal;
        public float shootPeriod, shotsPerSec, engageDistance;
		public float accuracy, bulletDamage, bulletSpeed;
		public float extraSpeed; // The enemy's speed will be equal to the player's speed + extraSpeed
	}

	// Apply properties to an enemy object, i.e. make it be of certain type
	private static void ApplyEnemyType (EnemyLogic enemy, EnemyType type, float playerSpeed)
	{
		EnemyProperties props = GetPropertiesOfType (type);
		ApplyEnemyType (enemy, props, playerSpeed);
	}

	private static void ApplyEnemyType (EnemyLogic enemy, int index, float playerSpeed)
	{
		ApplyEnemyType (enemy, enemyTypeList[index], playerSpeed);
	}

	private static void ApplyEnemyType (EnemyLogic enemy, EnemyProperties props, float playerSpeed)
	{
		enemy.maxHealth       = props.maxHealth;
		enemy.health          = props.maxHealth;
		enemy.maxShield       = props.maxShield;
		enemy.speed           = playerSpeed + props.extraSpeed;
		enemy.extraSpeed 	  = props.extraSpeed;
		enemy.collisionDamage = props.collisionDamage;
        enemy.isSuicidal      = props.isSuicidal;
        enemy.shootPeriod     = props.shootPeriod;
        enemy.shotsPerSec     = props.shotsPerSec;
        enemy.engageDistance  = props.engageDistance;
		enemy.accuracy 		  = props.accuracy;
		enemy.bulletDamage    = props.bulletDamage;
		enemy.bulletSpeed     = props.bulletSpeed;
		enemy.type            = props.type;
	}

	private static EnemyProperties GetPropertiesOfType(EnemyType type)
	{
		foreach (EnemyProperties props in enemyTypeList)
		{
			if (props.type == type)
				return props;
		}

		// If our code is correct, this should never happen
		Debug.LogError("Tried to spawn invalid enemy type: " + type.ToString());
		return null;
	}

	/// <summary>
	/// A request to spawn outpost guards.
	/// </summary>
	private class OutpostSpawnRequest
	{
		public int NumEnemies { get; private set; }
		public Vector3 Location { get; private set; }
		public int TriggerDistance { get; private set; }

		public OutpostSpawnRequest(int numEnemies, Vector3 location, int triggerDistance)
		{
			this.NumEnemies 	 = numEnemies;
			this.Location   	 = location;
			this.TriggerDistance = triggerDistance;
		}
	}

	// Request spawning of count enemies around outpostLocation
	public void RequestSpawnForOutpost (int count, Vector3 outpostLocation, int triggerDistance)
	{
		// Only register the request here. It will be spawned on the next frame after all regular enemies are spawned.
		outpostSpawnRequests.Enqueue(new OutpostSpawnRequest(count, outpostLocation, triggerDistance));
	}

	/// <summary>
	/// A request to spawn a single enemy.
	/// </summary>
	private class SingleSpawnRequest
	{
		public EnemyType Type { get; private set; }
		public Vector3 Location { get; private set; }

		public SingleSpawnRequest(EnemyType type, Vector3 location)
		{
			this.Type 	  = type;
			this.Location = location;
		}
	}

	/// <summary>
	/// Requests spawning of a single enemy of the chosen type.
	/// </summary>
	/// <param name="location">The enemy location.</param>
	/// <param name="type">The enemy type.</param>
	public void RequestSingleEnemy(Vector3 location, EnemyType type)
	{
		// Only register the request here. It will be spawned on the next frame after all regular enemies and guards are spawned.
		singleSpawnRequests.Enqueue(new SingleSpawnRequest(type, location));
	}

	/// <summary>
	/// Requests spawning of a single enemy of a random type.
	/// </summary>
	/// <param name="location">The enemy location.</param>
	public void RequestSingleEnemy(Vector3 location)
	{
		EnemyType type = GetRandomEnemyTypeForDifficulty();
		RequestSingleEnemy(location, type);
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
    BlackWidow
}
