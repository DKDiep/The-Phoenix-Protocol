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

	[SerializeField] GameObject enemy;
	public static int numEnemies = 0; // Number of currently active enemies
	public int maxEnemies; // Maximum number of enemies at a time
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] GameObject gameManager;
    private GameState state;
	GameObject player, spawnLocation, logic;

	// TODO: the AI constants probably need to be tweaked
	private const int AI_WAYPOINTS_PER_ENEMY      = 10;
	private const int AI_GEN_WAYPOINTS_FACTOR     = 8;
	private const int AI_WAYPOINT_RADIUS          = 125;
	private readonly Vector3 AI_WAYPOINT_SHIFT_UP = new Vector3 (0, 8, 0);
	private List<GameObject> aiWaypoints;

	private static List<EnemyProperties> enemyTypeList = null;

	private Queue<OutpostSpawnRequest> outpostSpawnRequests = null;
	private const int OUTPOST_SPAWN_RADIUS                  = 150; // The radius of the sphere around an outpost in which to spawn protecting enemies

    void Start()
    {
        if (gameManager != null)
        {
            state = gameManager.GetComponent<GameState>();
        }

        player = null;
        spawnLocation = new GameObject(); // Create temporary object to spawn enemies at
        spawnLocation.name = "EnemySpawnLocation";
        StartCoroutine("Cleanup");

		if (enemyTypeList == null)
			InitialiseEnemyTypes ();

		outpostSpawnRequests = new Queue<OutpostSpawnRequest>();
    }

	// Create an EnemyProperties object for each type of enemy that will be used
	private static void InitialiseEnemyTypes()
	{
		enemyTypeList = new List<EnemyProperties>();
		enemyTypeList.Add(new EnemyProperties(EnemyType.Fighter, 100, 0, 15, 15)); // This is the "default" enemy we had before introducing types
		enemyTypeList.Add(new EnemyProperties(EnemyType.Tank, 200, 50, 50, 10));
		enemyTypeList.Add(new EnemyProperties(EnemyType.Assassin, 50, 20, 5, 30));
	}

    // Spawn a new enemy in a random position if less than specified by maxEnemies
    void Update ()
	{
        if (state.GetStatus() == GameState.Status.Started)
        {
            if(player == null)
            {
                player = state.GetPlayerShip();
                logic = Resources.Load("Prefabs/EnemyShipLogic", typeof(GameObject)) as GameObject;
                //logic.GetComponent<EnemyLogic>().SetPlayer(state.GetPlayerShip());

				CreateAIWaypoints ();
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
		enemyObject = Instantiate(enemy, spawnLocation.transform.position, transform.rotation) as GameObject;
		GameObject enemyLogicObject  = Instantiate(logic, spawnLocation.transform.position, transform.rotation) as GameObject;

		// Set up enemy with components, spawn on network
		enemyObject.AddComponent<EnemyCollision>();
		enemyLogicObject.transform.parent = enemyObject.transform;
		enemyLogicObject.transform.localPosition = Vector3.zero;

		enemyLogic = enemyLogicObject.GetComponent<EnemyLogic> ();
		ApplyEnemyType (enemyLogic, Random.Range(0, enemyTypeList.Count)); // random enemy type
		enemyLogic.SetControlObject(enemyObject);
		enemyLogic.SetPlayer(state.GetPlayerShip());
		enemyLogic.SetAIWaypoints (GetAIWaypointsForEnemy ());

		ServerManager.NetworkSpawn(enemyObject);

		enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
		numEnemies += 1;
		state.AddEnemyList(enemyObject);
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
		spawnLocation.transform.position = location + Random.insideUnitSphere * OUTPOST_SPAWN_RADIUS;
		spawnLocation.transform.rotation = Random.rotation;

		GameObject enemy; 
		EnemyLogic logic;
		InstantiateEnemy(out enemy, out logic);

		logic.state = EnemyAIState.Wait;
	}

	// Generate a list of waypoints around the player to guide the enemy ships
	// Each enemy spawned will get some waypoints from this list
	private void CreateAIWaypoints()
	{
		int n_waypoints = maxEnemies * AI_GEN_WAYPOINTS_FACTOR;
		aiWaypoints = new List<GameObject> (n_waypoints);

		// Get the bounds of all (important) ship parts
		List<Bounds> bounds = new List<Bounds> ();
		Transform spaceshipModel = player.transform.Find ("Model").Find ("Spaceship");
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
				/* Uncomment this to see waypoints as spheres
				 * if (Debug.isDebugBuild)
				   {
					waypoint = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					waypoint.transform.localScale = waypoint.transform.localScale / 25;
				   }
				else*/
				waypoint = new GameObject ("AIWaypoint");
				waypoint.transform.position = Random.insideUnitSphere * AI_WAYPOINT_RADIUS;
				waypoint.transform.Translate (AI_WAYPOINT_SHIFT_UP); // Shift the waypoints a upwards a little, otherwise too many end up under the ship
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
		List<GameObject> waypoints = new List<GameObject> (AI_WAYPOINTS_PER_ENEMY);
		int n_waypoints = aiWaypoints.Count, r;

		for (int i = 0; i < AI_WAYPOINTS_PER_ENEMY; i++)
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

	// Remove destroyed enemies from Game State
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        if (state.GetStatus() == GameState.Status.Started)
        {
            for (int i = state.GetEnemyListCount() - 1; i >= 0; i--)
            {
                GameObject enemyObject = state.GetEnemyAt(i);
                if(enemyObject == null)
                {
                  state.RemoveEnemyAt(i);
                }
            }
        }

        StartCoroutine("Cleanup");
    }

	// This class holds the various atributes of an enemy. Each enemy type will be be represented by a separate instance
	// TODO: the enemies should look differently based on their type
	private class EnemyProperties
	{
		public int maxHealth, maxShield, collisionDamage, speed;
		public EnemyType Type { get; private set; }

		public EnemyProperties(EnemyType type, int maxHelath, int maxShield, int collisionDamage, int speed)
		{
			this.Type            = type;
			this.maxHealth       = maxHealth;
			this.maxShield       = maxShield;
			this.collisionDamage = collisionDamage;
			this.speed           = speed;
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
	Fighter,
	Tank,
	Assassin
}
