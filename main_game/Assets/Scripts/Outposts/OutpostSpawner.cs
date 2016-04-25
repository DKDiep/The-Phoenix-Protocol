using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OutpostSpawner : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private GameObject outpost1;
	private GameObject gameManager;
    private OutpostManager outpostManagerScript;
	private float collectionDistance; // The distance from the outpost the ship has to be in order to collect resources
	private int hardOutposts, mediumOutposts, easyOutposts, totalOutposts;
	private float minDistance; // The minimum distance between outposts
	private int guardTriggerDistance;
	private float minAsteroidFieldSize, maxAsteroidFieldSize;
	private float minAsteroidFieldDensity, maxAsteroidFieldDensity;
    private List<Vector3> spawnLocations;
    private List<DifficultyEnum> difficulties;
    private float spawnLocationsVariance;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject resources;     // The resources prefab
	#pragma warning restore 0649

	private GameState gameState;
	private EnemySpawner enemySpawner;
	private AsteroidSpawner asteroidSpawner;

	private GameObject player, outpost, logic, spawnLocation, outpostManager;

	private int numOutposts = 0;

	void Start ()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		easyOutposts = mediumOutposts = hardOutposts = 0;

		gameState    	= gameManager.GetComponent<GameState>();
		enemySpawner 	= gameManager.GetComponentInChildren<EnemySpawner>();
		asteroidSpawner = gameManager.GetComponentInChildren<AsteroidSpawner>();

		logic = Instantiate(Resources.Load("Prefabs/OutpostLogic", typeof(GameObject))) as GameObject;
        outpostManager = Instantiate(Resources.Load("Prefabs/OutpostManager", typeof(GameObject))) as GameObject;
        outpostManagerScript = outpostManager.GetComponent<OutpostManager>();
        outpostManagerScript.giveGameStateReference(gameState);

        spawnLocation = new GameObject();
		spawnLocation.name = "OutpostSpawnLocation";
	}

    public void Reset()
    {
        LoadSettings();
        numOutposts = easyOutposts = mediumOutposts = hardOutposts = 0;
        // Remove outposts
        for (int i = gameState.GetOutpostList().Count-1; i >= 0; i--)
        {
            Destroy(gameState.GetOutpostList()[i]);
            gameState.RemoveOutpost(gameState.GetOutpostList()[i]);
        }
    }

	private void LoadSettings()
	{
		gameManager 	     = settings.GameManager;
		outpost1 		     = settings.OutpostModel1Prefab;
		collectionDistance   = settings.OutpostResourceCollectionDistance;
		minDistance          = settings.OutpostMinDistance;
		guardTriggerDistance = settings.OutpostGuardTriggerDistance;
        totalOutposts	     = settings.EasyOutposts + settings.MediumOutposts + settings.HardOutposts;
        spawnLocations       = settings.OutpostSpawnLocations;
        difficulties         = settings.OutpostDifficulties;
        spawnLocationsVariance  = settings.OutpostSpawnLocationsVariance;
		minAsteroidFieldSize    = settings.OutpostMinAsteroidFieldSize;
		maxAsteroidFieldSize    = settings.OutpostMaxAsteroidFieldSize;
		minAsteroidFieldDensity = settings.OutpostMinAsteroidFieldDensity;
		maxAsteroidFieldDensity = settings.OutpostMaxAsteroidFieldDensity;
	}
		
	void Update() {
		if (gameState.Status == GameState.GameStatus.Started)
		{
			if(numOutposts < totalOutposts)
			{
				if(player == null) 
					player = gameState.PlayerShip;
                if (numOutposts < spawnLocations.Count)
                {
                    Vector3 specPosition = spawnLocations[numOutposts];
                    Vector3 variance = new Vector3(Random.Range(-spawnLocationsVariance, spawnLocationsVariance), 
                        Random.Range(-spawnLocationsVariance, spawnLocationsVariance), 
                        Random.Range(-spawnLocationsVariance, spawnLocationsVariance));
                    spawnLocation.transform.position = specPosition + variance;
                    spawnLocation.transform.eulerAngles = new Vector3(Random.Range(-10, 10), Random.Range(90, -90), Random.Range(90, -90));
                    SpawnOutpost(numOutposts, difficulties[numOutposts]);
                }
                else
                {
                    spawnLocation.transform.position = player.transform.position;

                    // The range (90,-90) is in in front of the ship. 
                    spawnLocation.transform.eulerAngles = new Vector3(Random.Range(-10, 10), Random.Range(90, -90), Random.Range(90, -90));

                    // Loop until we find a position that is not close to another outpost
                    do
                    {
                        spawnLocation.transform.Translate(transform.forward * Random.Range(1000, 3500));
                    } while (!CheckOutpostProximity(spawnLocation.transform.position));
                    SpawnOutpost(numOutposts, DifficultyEnum.Pool);
                }
                numOutposts++;
            } else {
                outpostManagerScript.outpostSpawned = true;
            }
		}

	}

	/// <summary>
	/// Checks if the new outpost is closer than minDistance to any existing outposts
	/// </summary>
	/// <returns><c>true</c>, if outpost is further than minDistance away from any outposts, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	private bool CheckOutpostProximity(Vector3 position) 
	{
		foreach(GameObject outpost in gameState.GetOutpostList()) {
			if(Vector3.Distance(position, outpost.transform.position) < minDistance) {
				return false;
			}
		}
		return true;
	}

	private void SpawnOutpost(int id, DifficultyEnum difficulty) 
	{
		// Set up like this as we may have different outposts models like we do with asteroids. 
		outpost = outpost1;

		// Spawn object and logic
		GameObject outpostObject    = Instantiate(outpost, spawnLocation.transform.position, Quaternion.identity) as GameObject;
		GameObject outpostLogic     = Instantiate(logic, spawnLocation.transform.position, Quaternion.identity) as GameObject;
		GameObject outpostResources = Instantiate(resources, spawnLocation.transform.position, Quaternion.identity) as GameObject;

		// Initialise logic
		outpostLogic.transform.parent = outpostObject.transform;
		outpostLogic.transform.localPosition = Vector3.zero;
        outpostObject.transform.eulerAngles = new Vector3(Random.Range(-30, 10), Random.Range(0, 359), Random.Range(-20, 20));

		//outpostLogic.GetComponent<OutpostLogic>().SetPlayer(state.GetPlayerShip(), maxVariation, rnd);
		outpostLogic.GetComponent<OutpostLogic>().SetStateReference(gameState);
        outpostLogic.GetComponent<OutpostLogic>().id = id;
        // Set the resources collider on a child object to avoid shooting and outpost collision issues
        outpostResources.transform.parent = outpostObject.transform;
		outpostResources.GetComponent<ResourcesCollision>().SetOutpost(outpostObject);
		outpostResources.GetComponent<SphereCollider>().radius = collectionDistance;



		Rigidbody rigid = outpostObject.AddComponent<Rigidbody>();
		rigid.isKinematic = true;
		gameState.AddToOutpostList(outpostObject);
		ServerManager.NetworkSpawn(outpostObject);

        // Hide the target by default.
        outpostObject.GetComponent<OutpostTarget>().EndMission();
        if(difficulty == DifficultyEnum.Pool)
        {
            if (hardOutposts < settings.HardOutposts) difficulty = DifficultyEnum.Hard;
            else if (mediumOutposts < settings.MediumOutposts) difficulty = DifficultyEnum.Medium;
            else difficulty = DifficultyEnum.Easy;
        }
        if (difficulty == DifficultyEnum.Hard)
        {
            int numGuards = Random.Range(settings.HardMinEnemies, settings.HardMaxEnemies);
			enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position, guardTriggerDistance);
			SpawnAsteroidFieldAroundOutpost(outpostObject.transform.position);
            outpostLogic.GetComponent<OutpostLogic>().SetDifficulty(0, settings.HardMultiplier);
            hardOutposts++;
        }
        else if(difficulty == DifficultyEnum.Medium)
        {
            int numGuards = Random.Range(settings.MediumMinEnemies, settings.MediumMaxEnemies);
			enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position, guardTriggerDistance);
            outpostLogic.GetComponent<OutpostLogic>().SetDifficulty(1, settings.MediumMultiplier);
            mediumOutposts++;
        }
        else
        {
            int numGuards = Random.Range(settings.EasyMinEnemies, settings.EasyMaxEnemies);
			enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position, guardTriggerDistance);
            outpostLogic.GetComponent<OutpostLogic>().SetDifficulty(2, settings.EasyMultiplier);
            easyOutposts++;
        }
	}

	/// <summary>
	/// Spaws an asteroid field around an outpost.
	/// </summary>
	/// <param name="outpostLoc">The outpost location.</param>
	private void SpawnAsteroidFieldAroundOutpost(Vector3 outpostLoc)
	{
		Vector3 size = new Vector3(Random.Range(minAsteroidFieldSize, maxAsteroidFieldSize),
	       Random.Range(minAsteroidFieldSize / 2 , maxAsteroidFieldSize / 2),
	       Random.Range(minAsteroidFieldSize, maxAsteroidFieldSize));
		Vector3 offset = new Vector3(Random.Range(-size.x / 4, size.x / 4),
			Random.Range(-size.y / 4, size.y / 4),
			Random.Range(-size.z / 4, size.z / 4));
		float density = Random.Range(minAsteroidFieldDensity, maxAsteroidFieldDensity);

		asteroidSpawner.RequestAsteroidField(outpostLoc + offset, size, density);
	}
}