/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Server-side logic for enemy spawner
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour 
{
	
	[SerializeField] GameObject enemy;
	public static int numEnemies = 0;
	public int maxEnemies;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] GameObject gameManager;
    private GameState state;
    GameObject player, temp;

	// TODO: the AI constants probably need to be tweaked
	private const int AI_WAYPOINTS_PER_ENEMY      = 5;
	private const int AI_GEN_WAYPOINTS_FACTOR     = 3;
	private const int AI_WAYPOINT_RADIUS          = 50;
	private readonly Vector3 AI_WAYPOINT_SHIFT_UP = new Vector3 (0, 8, 0);
	private List<GameObject> aiWaypoints;

    void Start()
    {
        if (gameManager != null)
        {
            state = gameManager.GetComponent<GameState>();
        }
        player = null;
        temp = new GameObject();
        temp.name = "EnemySpawnLocation";
        StartCoroutine("Cleanup");
    }

    // Spawn a new enemy in a random position if less than specified by maxEnemies
    void Update () 
	{
        if (state.GetStatus() == GameState.Status.Started)
        {
			if (player == null)
			{
				player = state.GetPlayerShip ();
				CreateAIWaypoints ();
			}

            if (numEnemies < maxEnemies)
            {
                temp.transform.position = player.transform.position;
                temp.transform.rotation = Random.rotation;
                temp.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

                //Spawn enemy and server logic
                GameObject enemyObject = Instantiate(enemy, temp.transform.position, transform.rotation) as GameObject;
                GameObject enemyObjectLogic = Instantiate(Resources.Load("Prefabs/EnemyShipLogic", typeof(GameObject))) as GameObject;
                enemyObject.AddComponent<EnemyCollision>();
				enemyObjectLogic.transform.parent = enemyObject.transform;
				enemyObjectLogic.transform.localPosition = Vector3.zero;
                
				EnemyLogic enemyObjectLogicComponent = enemyObjectLogic.GetComponent<EnemyLogic> ();
				enemyObjectLogicComponent.SetControlObject(enemyObject);
				enemyObjectLogicComponent.SetPlayer(state.GetPlayerShip());
				enemyObjectLogicComponent.SetAIWaypoints (GetAIWaypointsForEnemy ());

                ServerManager.NetworkSpawn(enemyObject);

                enemyObject.transform.eulerAngles = new Vector3(-90, 0, 0); // Set to correct rotation
                numEnemies += 1;
                state.AddEnemyList(enemyObject);
            }
        }
	}

	// Generate a list of waypoints around the player to guide the enemy ships
	// Each enemy spawned will get some waypoints from this list
	private void CreateAIWaypoints()
	{
		int n_waypoints = maxEnemies * AI_GEN_WAYPOINTS_FACTOR;
		aiWaypoints = new List<GameObject> (n_waypoints);

		GameObject waypoint;

		for (int i = 0; i < n_waypoints; i++)
		{
			if (Debug.isDebugBuild)
				waypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			else
				waypoint = new GameObject ();
			waypoint.transform.localScale = waypoint.transform.localScale / 25;
			waypoint.transform.position = Random.insideUnitSphere * AI_WAYPOINT_RADIUS;
			waypoint.transform.Translate(AI_WAYPOINT_SHIFT_UP); // Shift the waypoints a upwards a little, otherwise too many end up under the ship
			waypoint.transform.parent = player.transform;

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
        //Debug.Log(numEnemies);
        StartCoroutine("Cleanup");
    }
}