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
	private const float INITIAL_SHIP_HEALTH      = 100;
	private const float INITIAL_COMPONENT_HEALTH = 100;

	private const float INITIAL_SHIP_SPEED               = 10;
	private const float INITIAL_SHIP_MAXSHIELDS          = 100;
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
	[SyncVar] private float shipShield = INITIAL_SHIP_MAXSHIELDS;
	private float shipShieldRechargeRate = INITIAL_SHIP_SHIELD_RECHARGERATE;

	// The total ship resources that has been collected over the whole game.
	// This is used for the final score.
	private int totalShipResources = BASE_SHIP_RESOURCES;

	// The ships resources value that is shown to the commander, this is used to purchase upgrades. 
	[SyncVar] private int currentShipResources = BASE_SHIP_RESOURCES;

	// The health of the ship. 
	[SyncVar] private float shipHealth            = INITIAL_SHIP_HEALTH;
	[SyncVar] private float engineHealth          = INITIAL_COMPONENT_HEALTH;
	[SyncVar] private float turretHealth          = INITIAL_COMPONENT_HEALTH;
	[SyncVar] private float shieldGeneratorHealth = INITIAL_COMPONENT_HEALTH;

	private bool godMode = false;
	private bool nosMode = false;
	private const int NOS_SPEED = 80;
    
    void Update()
    {
    	if(Input.GetKeyDown (KeyCode.Escape))
    	{
    		Application.Quit ();
    	}
		if (Input.GetKeyDown(KeyCode.G))
		{
			godMode = !godMode;
			if(!godMode)
			{
				shipHealth = INITIAL_SHIP_HEALTH;
				engineHealth = turretHealth = shieldGeneratorHealth = INITIAL_COMPONENT_HEALTH;
			}
			Debug.Log("God mode " + godMode);
		}
		if(Input.GetKeyDown(KeyCode.N))
		{
			nosMode = !nosMode;
			if (nosMode)
				shipSpeed = NOS_SPEED;
			else
				shipSpeed = INITIAL_SHIP_SPEED;
			Debug.Log("NOS mode " + nosMode);
		}

		// If in god mode, reset the ship to max possible health every frame
		if (godMode)
			shipHealth = engineHealth = turretHealth = shieldGeneratorHealth = float.MaxValue;
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

	/// <summary>
	/// Resets the player scores to zero.
	/// </summary>
	public void ResetPlayerScores() 
	{
		for(int id = 0; id < 4; id++) 
		{
			playerScore[id] = 0;
		}
	}

	/// <summary>
	/// Adds a value to a specific players score. 
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="score">Score.</param>
	public void AddPlayerScore(int playerId, int score) 
	{
		playerScore[playerId] += score;
	}

	/// <summary>
	/// Gets the player scores.
	/// </summary>
	/// <returns>The player scores.</returns>
    public int[] GetPlayerScores()
    {
        return playerScore;
    }

    /// <summary>
	/// Gets the current ship resources
    /// </summary>
    /// <returns>The ship resources.</returns>
    public int GetShipResources() 
	{
		return currentShipResources;
	}

	/// <summary>
	/// Gets the total ship resources for the whole game
	/// Used for scoring at end of game, Use GetShipResources() for current ships resources
	/// </summary>
	/// <returns>The total ship resources.</returns>
	public int GetTotalShipResources() 
	{
		return totalShipResources;
	}

	/// <summary>
	/// Adds a value to the total ship resources
	/// </summary>
	/// <param name="resources">Resources.</param>
	public void AddShipResources(int resources) 
	{
		currentShipResources += resources;
		totalShipResources += resources;
	}

	/// <summary>
	/// Uses the ship resources.
	/// Subtracts an amount of the current ship resources.
	/// e.g. When buying things the ships resources will go down
	/// </summary>
	/// <param name="resources">Resources.</param>
	public void UseShipResources(int resources) 
	{
		currentShipResources -= resources;
	}

	/// <summary>
	/// Gets the ship health.
	/// </summary>
	/// <returns>The ship health.</returns>
	public float GetShipHealth() 
	{
		return shipHealth;
	}
		
	/// <summary>
	/// Sets the ship health to a specific value
	/// </summary>
	/// <param name="value">Value.</param>
	public void SetShipHealth(float value) 
	{
		shipHealth = value;
	}

	/// <summary>
	/// Reduces the ship health by a specific value
	/// </summary>
	/// <param name="value">Value.</param>
	public void ReduceShipHealth(float value) 
	{
		shipHealth -= value;
		if (shipHealth <= 0)
			SetStatus(Status.Died);
	}
		
	/// <summary>
	/// Reduces the health of a component. Hitting the bridge reduces the ship's (hull) health.
	/// </summary>
	/// <param name="component">The component.</param>
	/// <param name="value">The value by which to decrease health.</param>
	public void ReduceComponentHealth(ComponentType component, float value)
	{
		switch(component)
		{
		case ComponentType.Bridge:
			ReduceShipHealth(value);
			break;
		case ComponentType.Engine:
			engineHealth -= value;
			break;
		case ComponentType.Turret:
			turretHealth -= value;
			break;
		case ComponentType.ShieldGenerator:
			shieldGeneratorHealth -= value;
			break;
		}
	}

	/// <summary>
	/// Gets the ship speed.
	/// </summary>
	/// <returns>The ship speed.</returns>
	public float GetShipSpeed()
	{
		return shipSpeed;
	}

	/// <summary>
	/// Sets the ship speed.
	/// </summary>
	/// <param name="speed">Speed.</param>
	public void SetShipSpeed(float speed)
	{
		shipSpeed = speed;
	}

	/// <summary>
	/// Gets the ship max shields.
	/// </summary>
	/// <returns>The ship max shields.</returns>
	public float GetShipMaxShields()
	{
		return shipMaxShields;
	}

	/// <summary>
	/// Sets the ship max shields.
	/// </summary>
	/// <param name="shield">Shield.</param>
	public void SetShipMaxShields(float shield)
	{
		shipMaxShields = shield;
	}

	/// <summary>
	/// Gets the ship shield value.
	/// </summary>
	/// <returns>The ship shield.</returns>
	public float GetShipShield()
	{
		return shipShield;
	}

	/// <summary>
	/// Sets the ship shield.
	/// </summary>
	/// <param name="shield">Shield.</param>
	public void SetShipShield(float shield)
	{
		shipShield = shield;
	}

	/// <summary>
	/// Gets the ship shield recharge rate.
	/// </summary>
	/// <returns>The ship shield recharge rate.</returns>
	public float GetShipShieldRechargeRate()
	{
		return shipShieldRechargeRate;
	}

	/// <summary>
	/// Sets the ships shield recharge rate.
	/// </summary>
	/// <param name="shieldRechargeRate">Shield recharge rate.</param>
	public void SetShipShieldRechargeRate(float shieldRechargeRate)
	{
		shipShieldRechargeRate = shieldRechargeRate;
	}

	/// <summary>
	/// Get the health of a specified component type.
	/// </summary>
	/// <returns>The component health, or -1 if ComponentType.None is passed.</returns>
	/// <param name="type">The component type.</param>
	public float GetComponentHealth(ComponentType type)
	{
		switch(type)
		{
		case ComponentType.Bridge:
			return shipHealth;
		case ComponentType.Engine:
			return engineHealth;
		case ComponentType.Turret:
			return turretHealth;
		case ComponentType.ShieldGenerator:
			return shieldGeneratorHealth;
		default:
			return -1;
		}
	}

}
