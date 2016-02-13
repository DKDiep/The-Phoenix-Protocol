using UnityEngine;
using System.Collections;

public class OutpostSpawner : MonoBehaviour 
{
	[SerializeField] GameObject outpost1;

	// The distance from the outpost the ship has to be in order to collect resources
	[SerializeField] float collectionDistance;

	[SerializeField] GameObject gameManager;
	private GameState gameState;

	private GameObject player, outpost, logic, spawnLocation;

	[SerializeField] int maxOutposts;
	private int numOutposts = 0;

	void Start ()
	{
		gameState = gameManager.GetComponent<GameState>();

		logic = Instantiate(Resources.Load("Prefabs/OutpostLogic", typeof(GameObject))) as GameObject;
	
		spawnLocation = new GameObject();
		spawnLocation.name = "OutpostSpawnLocation";
	}



	void Update() {
		if (gameState.GetStatus() == GameState.Status.Started)
		{
			if(numOutposts < maxOutposts)
			{
				if(player == null)
					player = gameState.GetPlayerShip();
			
				spawnLocation.transform.position = player.transform.position;
				spawnLocation.transform.eulerAngles = new Vector3(Random.Range(-10,10), Random.Range(0,360), Random.Range(0,360));

				spawnLocation.transform.Translate(transform.forward * Random.Range(1000,2000));

				SpawnOutpost ();
				numOutposts++;
			}
		}

	}
	private void SpawnOutpost() 
	{
		// Set up like this as we may have different outposts models like we do with asteroids. 
		outpost = outpost1;

		// Spawn object and logic
		GameObject outpostObject = Instantiate(outpost, spawnLocation.transform.position, Quaternion.identity) as GameObject;
		GameObject outpostLogic = Instantiate(logic, spawnLocation.transform.position, Quaternion.identity) as GameObject;

		// Initialise logic
		outpostLogic.transform.parent = outpostObject.transform;
		outpostLogic.transform.localPosition = Vector3.zero;
		outpostObject.AddComponent<OutpostCollision>();

		//outpostLogic.GetComponent<OutpostLogic>().SetPlayer(state.GetPlayerShip(), maxVariation, rnd);
		outpostLogic.GetComponent<OutpostLogic>().SetStateReference(gameState);

		// Add collider and rigidbody
		SphereCollider sphere = outpostObject.AddComponent<SphereCollider>();

		// Change radius that the player picks up resources
		sphere.radius = collectionDistance;
		sphere.isTrigger = true;

		Rigidbody rigid = outpostObject.AddComponent<Rigidbody>();
		rigid.isKinematic = true;
		gameState.AddOutpostList(outpostObject);
		ServerManager.NetworkSpawn(outpostObject);
	}

}