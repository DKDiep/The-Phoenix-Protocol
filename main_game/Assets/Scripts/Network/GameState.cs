/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru
    Description: The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    public enum GameStatus { Setup, Started, Won, Died };

    private GameSettings settings;

    private List<GameObject> asteroidList;
    private List<GameObject> newAsteroids;
    private List<uint> removedAsteroids;

    private List<GameObject> enemyList;
    private List<int> removedEnemies;
    private List<GameObject> engineerList;

	private List<GameObject> outpostList;

	public GameObject Portal { get; set; }

	public GameObject PlayerShip { get; set; }
	private int[] playerScore;

	// Ship variables used for modifying the ships behaviour
	private float shipSpeed;
	private float shipMaxShields;
    private float shipShieldRechargeRate;
 
	// The total ship resources that has been collected over the whole game.
	// This is used for the final score.
	private int totalShipResources;

    [SyncVar] public GameStatus Status;
	// We set this to the max shields as we assume we start off with max shields.
	[SyncVar] private float shipShield;
	// The ships resources value that is shown to the commander, this is used to purchase upgrades. 
	[SyncVar] private int currentShipResources;
	// Number of civilians currently saved on the ship
	[SyncVar] private int civilians;
	// The health of the ship. 
	[SyncVar] private float shipHealth;
	[SyncVar] private float engineHealth;
	[SyncVar] private float turretHealth;
	[SyncVar] private float shieldGeneratorHealth;

	// Upgradable components
	// TODO: once all components are implemented like this, they will replace the current variables
	private UpgradableComponent[] upgradableComponents;

	private bool godMode = false;
	private bool nosMode = false;
	private const int NOS_SPEED = 80;

	void Start()
	{
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        LoadSettings();
		
        Status = GameStatus.Setup;
		InitialiseUpgradableComponents();
	}
        
    private void LoadSettings()
    {
        shipHealth              = settings.PlayerShipStartingHealth;
        shipShield              = settings.PlayerShipStartingShields;
        shipMaxShields          = settings.PlayerShipStartingShields;
        shipShieldRechargeRate  = settings.PlayerShipStartingRechargeRate;
        shipSpeed               = settings.PlayerShipStartingSpeed;
        totalShipResources      = settings.PlayerShipStartingResources;
        currentShipResources    = settings.PlayerShipStartingResources;
        civilians               = settings.PlayerShipStartingCivilians;
        engineHealth            = settings.PlayerShipComponentHealth;
        turretHealth            = settings.PlayerShipComponentHealth;
        shieldGeneratorHealth   = settings.PlayerShipComponentHealth;
    }
        
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
                shipHealth = settings.PlayerShipStartingHealth;
                engineHealth = turretHealth = shieldGeneratorHealth = settings.PlayerShipComponentHealth;
            }
            Debug.Log("God mode " + godMode);
        }
        if(Input.GetKeyDown(KeyCode.N))
        {
            nosMode = !nosMode;
            if (nosMode)
                shipSpeed = NOS_SPEED;
            else
                shipSpeed = settings.PlayerShipStartingSpeed;
            Debug.Log("NOS mode " + nosMode);
        }

        // If in god mode, reset the ship to max possible health every frame
        if (godMode)
            shipHealth = engineHealth = turretHealth = shieldGeneratorHealth = float.MaxValue;
    }

	/// <summary>
	/// Initialises the upgradable components of the ship.
	/// </summary>
	private void InitialiseUpgradableComponents()
	{
		int numComponents    = Enum.GetNames(typeof(UpgradableComponentIndex)).Length;
		upgradableComponents = new UpgradableComponent[numComponents];

		// TODO: fill in with the other components
		upgradableComponents[(int)UpgradableComponentIndex.Engines] = new UpgradableEngine();
	}

	/// <summary>
	/// Gets the upgradable component specified by <c>type</c>.
	/// </summary>
	/// <returns>The upgradable component.</returns>
	/// <param name="type">The type of the component.</param>
	public UpgradableComponent GetUpgradableComponent(ComponentType type)
	{
		UpgradableComponentIndex index = UpgradableComponentIndex.Hull;

		switch(type)
		{
		case ComponentType.Engine:
			index = UpgradableComponentIndex.Engines;
			break;
		case ComponentType.Bridge:
			index = UpgradableComponentIndex.Hull;
			break;
		case ComponentType.ShieldGenerator:
			index = UpgradableComponentIndex.ShieldGen;
			break;
		case ComponentType.Turret:
			index = UpgradableComponentIndex.Turrets;
			break;
		// TODO: add drone and resource storage
		}

		return upgradableComponents[(int)index];
	}
        
	/*
	 *  Getters and setters for Asteroid list
	 */
    public void AddToAsteroidList(GameObject asteroidObject)
    {
        asteroidList.Add(asteroidObject);
        newAsteroids.Add(asteroidObject);
    }

    public void RemoveAsteroid(GameObject removeObject)
    {        
        bool wasDeleted = newAsteroids.Remove(removeObject);
        if (!wasDeleted) removedAsteroids.Add((uint)removeObject.GetInstanceID());
        asteroidList.Remove(removeObject);
		AsteroidSpawner.DecrementNumAsteroids();
    }

    private void RemoveAsteroidAt(int i)
    {
        bool wasDeleted = newAsteroids.Remove(asteroidList[i]);
        if (!wasDeleted)
			removedAsteroids.Add((uint)asteroidList[i].GetInstanceID());
        asteroidList.RemoveAt(i);
		AsteroidSpawner.DecrementNumAsteroids();
    }

	/// <summary>
	/// Removes any null asteroids from the asteroids list.
	/// </summary>
	public void CleanUpAsteroids()
	{
		for (int i = asteroidList.Count - 1; i >= 0; i--)
		{
			if(asteroidList[i] == null)
				RemoveAsteroidAt(i);
		}
	}

	/*
	 *  Getters and setters for enemy list
	 */
    public List<GameObject> GetEnemyList()
    {
        return enemyList;
    }

    public void AddToEnemyList(GameObject enemyObject)
    {
        enemyList.Add(enemyObject);
    }

    private void RemoveEnemyAt(int i)
    {
        removedEnemies.Add(enemyList[i].GetInstanceID());
        enemyList.RemoveAt(i);
    }

	public void RemoveEnemy(GameObject enemy)
	{
        removedEnemies.Add(enemy.GetInstanceID());
		enemyList.Remove(enemy);
		EnemySpawner.DecrementNumEnemies();
	}

	/// <summary>
	/// Removes any null enemies from the emeny list.
	/// </summary>
	public void CleanupEnemies()
	{
		for (int i = enemyList.Count - 1; i >= 0; i--)
		{
			if(enemyList[i] == null)
				RemoveEnemyAt(i);
		}
	}

    public void AddToEngineerList(GameObject engineerObject)
    {
        engineerList.Add(engineerObject);
    }

	/*
	 *  Getters and setters for outpost list
	 */
	public List<GameObject> GetOutpostList()
	{
		return outpostList;
	}

	public void AddToOutpostList(GameObject outpostObject)
	{
		outpostList.Add(outpostObject);
	}
		
	public void RemoveOutpost(GameObject outpost)
	{
		outpostList.Remove(outpost);
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
		removedAsteroids.Clear();
    }
    
    public List<int> GetRemovedEnemies()
    {
        return removedEnemies;
    }
    
    public void ClearRemovedEnemies()
    {
		removedEnemies.Clear();
    }

    private void InitializeVariables()
    {
        asteroidList     = new List<GameObject>();        
        newAsteroids     = new List<GameObject>();
        removedAsteroids = new List<uint>();
        enemyList        = new List<GameObject>();
        removedEnemies   = new List<int>();
        engineerList     = new List<GameObject>();
		outpostList      = new List<GameObject>();
		playerScore      = new int[4];

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
	public void AddToPlayerScore(int playerId, int score) 
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
	/// Gets the number of civilians on the ship at the moment. 
	/// </summary>
	/// <returns>The number of civilians.</returns>
	public int GetCivilians() 
	{
		return civilians;
	}


	/// <summary>
	/// Adds the civilians to the total number currently on the ship.
	/// </summary>
	/// <param name="newCivilians">New civilians.</param>
	public void AddCivilians(int newCivilians)
	{
		civilians += newCivilians;
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
	private void SetShipHealth(float value) 
	{
		shipHealth = value;
	}

	/// <summary>
	/// Damage the ship's main hull.
	/// </summary>
	/// <param name="shielded">If set to <c>true</c>, damage is substracted from the shields before going to the hull.</param>
	/// <param name="damage">The ammount of damage to inflict.</param>
	/// <returns><c>true</c> if shields are still available</returns>
	public bool DamageShip(bool shielded, float damage)
	{
		if (shielded && shipShield > damage)
		{
			SetShipShield(shipShield - damage);
			return true;
		}
		else
		{
			if (shielded)
			{
				damage -= shipShield;
				SetShipShield(0);
			}
			ReduceShipHealth(damage);
			return false;
		}
	}

	/// <summary>
	/// Reduces the ship health by a specific value
	/// </summary>
	/// <param name="value">Value.</param>
	private void ReduceShipHealth(float value) 
	{
		shipHealth -= value;
		if (shipHealth <= 0)
			Status = GameStatus.Died;
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
	private void SetShipShield(float shield)
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
	/// Recharge the ship's shield by a given value.
	/// </summary>
	/// <param name="value">The ammount by which to recharge the shields.</param>
	public void RechargeShield(float value)
	{
		// Don't recharge the shields over the maximum value
		float newShieldvalue = shipShield + value;
		if (newShieldvalue > shipMaxShields)
			value = shipMaxShields - shipShield;
			
		SetShipShield(shipShield + value);
	}

	/// <summary>
	/// Get the health of a specified component type.
	/// </summary>
	/// <returns>The component health, or -1 if ComponentType.None is passed.</returns>
	/// <param name="type">The component type.</param>
	public float GetComponentHealth(ComponentType type)
	{
		// TODO: update this to work with UpgradableComponent

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

	/// <summary>
	/// Enum used to index the <c>UpgradableComponent</c>s array.
	/// </summary>
	private enum UpgradableComponentIndex
	{
		// WARNING: changing this will likely cause array indexing problems
		ShieldGen, Turrets, Engines, Hull, Drone, ResourceStorage
	}
}
