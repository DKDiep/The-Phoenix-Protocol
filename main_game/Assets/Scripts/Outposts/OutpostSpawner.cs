using UnityEngine;
using System.Collections;

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

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject resources;     // The resources prefab
	#pragma warning restore 0649

	private GameState gameState;
	private EnemySpawner enemySpawner;

	private GameObject player, outpost, logic, spawnLocation, outpostManager;

	private int numOutposts = 0;

	void Start ()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		easyOutposts = mediumOutposts = hardOutposts = 0;

		gameState    = gameManager.GetComponent<GameState>();
		enemySpawner = gameState.GetComponentInChildren<EnemySpawner>();

		logic = Instantiate(Resources.Load("Prefabs/OutpostLogic", typeof(GameObject))) as GameObject;
        outpostManager = Instantiate(Resources.Load("Prefabs/OutpostManager", typeof(GameObject))) as GameObject;
        outpostManagerScript = outpostManager.GetComponent<OutpostManager>();
        outpostManagerScript.giveGameStateReference(gameState);

        spawnLocation = new GameObject();
		spawnLocation.name = "OutpostSpawnLocation";
	}

	private void LoadSettings()
	{
		gameManager 	     = settings.GameManager;
		outpost1 		     = settings.OutpostModel1Prefab;
		collectionDistance   = settings.OutpostResourceCollectionDistance;
		minDistance          = settings.OutpostMinDistance;
		guardTriggerDistance = settings.OutpostGuardTriggerDistance;
        totalOutposts	     = settings.EasyOutposts + settings.MediumOutposts + settings.HardOutposts;
	}
		
	void Update() {
		if (gameState.Status == GameState.GameStatus.Started)
		{
			if(numOutposts < totalOutposts)
			{
				if(player == null) 
					player = gameState.PlayerShip;
			
				spawnLocation.transform.position = player.transform.position;

				// The range (90,-90) is in in front of the ship. 
				spawnLocation.transform.eulerAngles = new Vector3(Random.Range(-10,10), Random.Range(90,-90), Random.Range(90,-90));

				// Loop until we find a position that is not close to another outpost
				do {
					spawnLocation.transform.Translate(transform.forward * Random.Range(1000,3500));
				} while(!CheckOutpostProximity(spawnLocation.transform.position));

				SpawnOutpost ();
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

	private void SpawnOutpost() 
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

		// Set the resources collider on a child object to avoid shooting and outpost collision issues
		outpostResources.transform.parent = outpostObject.transform;
		outpostResources.GetComponent<ResourcesCollision>().SetOutpost(outpostObject);
		outpostResources.GetComponent<SphereCollider>().radius = collectionDistance;

        // Hide the target by default.
        outpostObject.GetComponentsInChildren<OutpostTarget>()[0].HideTarget();

		Rigidbody rigid = outpostObject.AddComponent<Rigidbody>();
		rigid.isKinematic = true;
		gameState.AddToOutpostList(outpostObject);
		ServerManager.NetworkSpawn(outpostObject);

        if(hardOutposts < settings.HardOutposts)
        {
            int numGuards = Random.Range(settings.HardMinEnemies, settings.HardMaxEnemies);
			enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position, guardTriggerDistance);
            outpostLogic.GetComponent<OutpostLogic>().SetDifficulty(1, settings.HardMultiplier);
            hardOutposts++;
        }
        else if(mediumOutposts < settings.MediumOutposts)
        {
            int numGuards = Random.Range(settings.MediumMinEnemies, settings.MediumMaxEnemies);
			enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position, guardTriggerDistance);
            outpostLogic.GetComponent<OutpostLogic>().SetDifficulty(2, settings.MediumMultiplier);
            mediumOutposts++;
        }
        else
        {
            int numGuards = Random.Range(settings.EasyMinEnemies, settings.EasyMaxEnemies);
			enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position, guardTriggerDistance);
            outpostLogic.GetComponent<OutpostLogic>().SetDifficulty(3, settings.EasyMultiplier);
            easyOutposts++;
        }
	}
}