/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru
    Description: The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    public enum Status { Setup, Started, Won, Died };

	// The amount of resources the player starts off with.
	// https://bitbucket.org/pyrolite/game/wiki/Collecting%20Resources
	private const int BASE_SHIP_RESOURCES = 100;

	// The starting health value of the ship
	private const float INITIAL_SHIP_HEALTH = 100;

	private const float INITIAL_SHIP_SPEED = 10;
	private const float INITIAL_SHIP_MAXSHIELDS = 100;
	private const float INITIAL_SHIP_SHIELD_RECHARGERATE = 10;

    private Status status = Status.Setup;

    private List<GameObject> asteroidList;
    private List<GameObject> newAsteroids;
    private List<uint> removedAsteroids;

    private List<GameObject> enemyList;
    private List<uint> removedEnemies;
    private List<GameObject> engineerList;

	private List<GameObject> outpostList;

	private GameObject portal;

    private GameObject playerShip;
	private int[] playerScore;

	// Ship variables used for modifying the ships behaviour
	private float shipSpeed = INITIAL_SHIP_SPEED;

	private float shipMaxShields = INITIAL_SHIP_MAXSHIELDS;
	// We set this to the max shields as we assume we start off with max shields.
	private float shipShield = INITIAL_SHIP_MAXSHIELDS;
	private float shipShieldRechargeRate = INITIAL_SHIP_SHIELD_RECHARGERATE;

	// The total ship resources that has been collected over the whole game.
	// This is used for the final score.
	private int totalShipResources = BASE_SHIP_RESOURCES;

	// The ships resources value that is shown to the commander, this is used to purchase upgrades. 
	[SyncVar]
	private int currentShipResources = BASE_SHIP_RESOURCES;

	// The health of the ship. 
	[SyncVar]
	private float shipHealth = INITIAL_SHIP_HEALTH;
	private bool godMode = false;
    
    void Update()
    {
    	if(Input.GetKeyDown (KeyCode.Escape))
    	{
    		Application.Quit ();
    	}
		if (Input.GetKeyDown(KeyCode.G))
		{
			godMode = !godMode;
			if (!godMode)
				shipHealth = INITIAL_SHIP_HEALTH;
			Debug.Log("God mode " + godMode);
		}

		if (godMode)
			shipHealth = float.MaxValue;
    }

    public Status GetStatus()
    {
        return status;
    }

    public void SetStatus(Status newStatus)
    {
        status = newStatus;
    }


	/*
	 *  Getters and setters for Asteroid list
	 */
    public List<GameObject> GetAsteroidList()
    {
        return asteroidList;
    }

    public int GetAsteroidListCount()
    {
        return asteroidList.Count;
    }

    public void AddAsteroidList(GameObject asteroidObject)
    {
        asteroidList.Add(asteroidObject);
        newAsteroids.Add(asteroidObject);
    }

    public void RemoveAsteroid(GameObject removeObject)
    {        
        bool wasDeleted = newAsteroids.Remove(removeObject);
        if (!wasDeleted) removedAsteroids.Add((uint)removeObject.GetInstanceID());
        asteroidList.Remove(removeObject);
        AsteroidSpawner.numAsteroids--;
    }

    public void RemoveAsteroidAt(int i)
    {
        bool wasDeleted = newAsteroids.Remove(asteroidList[i]);
        if (!wasDeleted) removedAsteroids.Add((uint)asteroidList[i].GetInstanceID());
        asteroidList.RemoveAt(i);
        AsteroidSpawner.numAsteroids--;
    }

    public GameObject GetAsteroidAt(int i)
    {
        return asteroidList[i];
    }
		

	/*
	 *  Getters and setters for enemy list
	 */
    public List<GameObject> GetEnemyList()
    {
        return enemyList;
    }

    public int GetEnemyListCount()
    {
        return enemyList.Count;
    }

    public void AddEnemyList(GameObject enemyObject)
    {
        enemyList.Add(enemyObject);
    }

    public void RemoveEnemyAt(int i)
    {
        removedEnemies.Add((uint)enemyList[i].GetInstanceID());
        enemyList.RemoveAt(i);
    }

	public void RemoveEnemy(GameObject enemy)
	{
        removedEnemies.Add((uint)enemy.GetInstanceID());
		enemyList.Remove(enemy);
		EnemySpawner.numEnemies--;
	}

    public GameObject GetEnemyAt(int i)
    {
        return enemyList[i];
    }
		

	/*
	 *  Getters and setters for Engineer list
	 */
    public List<GameObject> GetEngineerList()
    {
        return engineerList;
    }

    public int GetEngineerCount()
    {
        return engineerList.Count;
    }

    public void AddEngineerList(GameObject engineerObject)
    {
        engineerList.Add(engineerObject);
    }

    public void RemoveEngineerAt(int i)
    {
        engineerList.RemoveAt(i);
    }

    public GameObject GetEngineerAt(int i)
    {
        return engineerList[i];
    }


	/*
	 *  Getters and setters for outpost list
	 */
	public List<GameObject> GetOutpostList()
	{
		return outpostList;
	}

	public int GetOutpostListCount()
	{
		return outpostList.Count;
	}

	public void AddOutpostList(GameObject outpostObject)
	{
		outpostList.Add(outpostObject);
	}

	public void RemoveOutpostAt(int i)
	{
		outpostList.RemoveAt(i);
	}

	public GameObject GetOutpostAt(int i)
	{
		return outpostList[i];
	}

	public GameObject GetPortal()
	{
		return portal;
	}

	public void SetPortal(GameObject portalObject)
	{
		portal = portalObject;
	}
		
    public GameObject GetPlayerShip()
    {
        return playerShip;
    }

    public void SetPlayerShip(GameObject newPlayerShip)
    {
        playerShip = newPlayerShip;
    }

    public List<GameObject> GetNewAsteroids()
    {
        return newAsteroids;
    }

    public void ClearNewAsteroids()
    {
        newAsteroids = new List<GameObject>();
    }

    public List<uint> GetRemovedAsteroids()
    {
        return removedAsteroids;
    }

    public void ClearRemovedAsteroids()
    {
        removedAsteroids = new List<uint>();
    }
    
    public List<uint> GetRemovedEnemies()
    {
        return removedEnemies;
    }
    
    public void ClearRemovedEnemies()
    {
        removedEnemies = new List<uint>();
    }

    private void InitializeVariables()
    {
        asteroidList = new List<GameObject>();        
        newAsteroids = new List<GameObject>();
        removedAsteroids = new List<uint>();
        enemyList = new List<GameObject>();
        removedEnemies = new List<uint>();
        engineerList = new List<GameObject>();
		outpostList = new List<GameObject>();
		playerScore = new int[4];
		ResetPlayerScores();
    }

    public void Setup()
    {
        InitializeVariables();
    }

	public void ResetPlayerScores() 
	{
		for(int id = 0; id < 4; id++) 
		{
			playerScore[id] = 0;
		}
	}

	public void AddPlayerScore(int id, int score) 
	{
		playerScore[id] += score;
		Debug.Log("Score for player " + id + " is now " + playerScore[id]);
	}

    public int[] GetPlayerScores()
    {
        return playerScore;
    }

    /*
	 * Gets the current ship resources
	*/
    public int GetShipResources() 
	{
		return currentShipResources;
	}

	/*
	 * Gets the total ship resources for the whole game
	*/
	public int GetTotalShipResources() 
	{
		return totalShipResources;
	}

	/*
	 * Adds a value to the total ship resources
	*/
	public void AddShipResources(int resources) 
	{
		currentShipResources += resources;
		totalShipResources += resources;
	}

	/*
	 * Subtracts a value to the total ship resources
	*/
	public void UseShipResources(int resources) 
	{
		currentShipResources -= resources;
	}

	/*
	 * Gets the ships health
	*/
	public float GetShipHealth() 
	{
		return shipHealth;
	}
		
	/*
	 * Sets the ships health to a specific value
	*/
	public void SetShipHealth(float value) 
	{
		shipHealth = value;
	}

	/*
	 * Reduces the ships health by a specific value
	*/
	public void ReduceShipHealth(float value) 
	{
		shipHealth -= value;	}
		

	public float GetShipSpeed()
	{
		return shipSpeed;
	}

	public void SetShipSpeed(float speed)
	{
		shipSpeed = speed;
	}

	public float GetShipMaxShields()
	{
		return shipMaxShields;
	}

	public void SetShipMaxShields(float shield)
	{
		shipMaxShields = shield;
	}

	public float GetShipShield()
	{
		return shipShield;
	}

	public void SetShipShield(float shield)
	{
		shipShield = shield;
	}

	public float GetShipShieldRechargeRate()
	{
		return shipShieldRechargeRate;
	}

	public void SetShipShieldRechargeRate(float shieldRechargeRate)
	{
		shipShieldRechargeRate = shieldRechargeRate;
	}

}
