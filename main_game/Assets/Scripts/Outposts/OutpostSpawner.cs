﻿using UnityEngine;
using System.Collections;

public class OutpostSpawner : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private GameObject outpost1;
	private GameObject gameManager;
	private float collectionDistance; // The distance from the outpost the ship has to be in order to collect resources
	private int hardOutposts, mediumOutposts, easyOutposts, totalOutposts;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject resources;     // The resources prefab
	#pragma warning restore 0649

	private GameState gameState;
	private EnemySpawner enemySpawner;
	private const float OUTPOST_MIN_DISTANCE = 1000;

	private GameObject player, outpost, logic, spawnLocation, outpostManager;

	private int numOutposts = 0;

	void Start ()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		gameState    = gameManager.GetComponent<GameState>();
		enemySpawner = gameState.GetComponentInChildren<EnemySpawner>();

		logic = Instantiate(Resources.Load("Prefabs/OutpostLogic", typeof(GameObject))) as GameObject;
        outpostManager = Instantiate(Resources.Load("Prefabs/OutpostManager", typeof(GameObject))) as GameObject;
        OutpostManager outpostManagerScript = outpostManager.GetComponent<OutpostManager>();
        outpostManagerScript.giveGameStateReference(gameState);

        spawnLocation = new GameObject();
		spawnLocation.name = "OutpostSpawnLocation";
	}

	private void LoadSettings()
	{
		gameManager 	   = settings.GameManager;
		outpost1 		   = settings.OutpostModel1Prefab;
		collectionDistance = settings.OutpostResourceCollectionDistance;
		easyOutposts 	   = settings.EasyOutposts;
        mediumOutposts       = settings.MediumOutposts;
        hardOutposts       = settings.HardOutposts;
        totalOutposts = easyOutposts + mediumOutposts + hardOutposts;
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
					spawnLocation.transform.Translate(transform.forward * Random.Range(1000,2000));
				} while(!CheckOutpostProximity(spawnLocation.transform.position));

				SpawnOutpost ();
				numOutposts++;
			}
		}

	}

	/// <summary>
	/// Checks if the new outpost is closer than OUTPOST_MIN_DISTANCE to any existing outposts
	/// </summary>
	/// <returns><c>true</c>, if outpost is further than OUTPOST_MIN_DISTANCE away from any outposts, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	private bool CheckOutpostProximity(Vector3 position) 
	{
		foreach(GameObject outpost in gameState.GetOutpostList()) {
			if(Vector3.Distance(position, outpost.transform.position) < OUTPOST_MIN_DISTANCE) {
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

		Rigidbody rigid = outpostObject.AddComponent<Rigidbody>();
		rigid.isKinematic = true;
		gameState.AddToOutpostList(outpostObject);
		ServerManager.NetworkSpawn(outpostObject);

		// Request the enemy spawner to spawn protecting ships around this outpost
		int numGuards = Random.Range(5, 10); // TODO: might want to set this manually based on difficulty
		enemySpawner.RequestSpawnForOutpost(numGuards, spawnLocation.transform.position);
	}
}