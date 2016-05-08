/*
    The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    public enum GameStatus { Setup, Started, Won, Died };

    private GameSettings settings;

	private AsteroidSpawner asteroidSpawner = null;
	private List<GameObject> asteroidList;
    private List<GameObject> newAsteroids;
    private List<uint> removedAsteroids;
    private List<Notification> activeNotifications;
    private List<Notification> newNotifications;
    private List<Notification> removedNotifications;
    private Dictionary<uint, Officer> currentOfficers;

    private List<GameObject> enemyList;
    private List<int> removedEnemies;
    private List<GameObject> engineerList;

	private List<GameObject> outpostList;

	public GameObject Portal { get; set; }

	public GameObject PlayerShip { get; set; }

    public SyncListInt playerScore = new SyncListInt();
    private int totalKills = 0;

    public ShieldEffects myShield = null;

    private string teamName;

	// Ship variables used for modifying the ships behaviour
	private bool boostOn = false;
	private float shipSpeed;

	private float shipMaxShields;
    private float shipShieldRechargeRate;
 
	// The total ship resources that has been collected over the whole game.
	// This is used for the final score.
	private int totalShipResources;

    // Controls enemy spawning behaviour, set in EnemySpawner.cs
    private int difficulty;

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

	[SyncVar] private float droneSpeed;
	[SyncVar] private float droneWorkTime;

	// Upgradable components
	public UpgradableComponent[] upgradableComponents { get; set; }

	private bool godMode = false;
	private bool nosMode = false;
	private const int NOS_SPEED = 400;

	void Start()
	{
		asteroidSpawner = GetComponentInChildren<AsteroidSpawner>(true); // For some reason, the spawner is disabled, so need to pass true here

		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        difficulty = 0;
        LoadSettings();
		
        Status = GameStatus.Setup;
		InitialiseUpgradableComponents();

		StartCoroutine(ResourceInterest());
        StartCoroutine(UpdateComponents());
	}

	// Uncomment to help debug null enemies
	/*IEnumerator FindNull()
	{
		foreach (GameObject enemy in enemyList)
			if (enemy == null)
				Debug.LogWarning("Found a null enemy.");

		yield return new WaitForSeconds(0.1f);
		StartCoroutine(FindNull());
	}*/

    public void Reset()
    {
        StopAllCoroutines();
        totalKills = 0;
        teamName = "";
        LoadSettings();
        InitialiseUpgradableComponents();
        StartCoroutine(ResourceInterest());
        StartCoroutine(UpdateComponents());

        // This is really stupid but it works. I think the problem is
        // that Unity does not recognize the currentShipResources value being
        // updated when it is assigned to in LoadSettings and hence the dirty bit is not
        // set, and therefore it is not synced with clients. There may be a better way
        // around this issue - Artur
        currentShipResources += 1;
        currentShipResources -= 1;
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
	/// Update ship parameters based on components health and upgrade levels.
	/// </summary>
	IEnumerator UpdateComponents()
	{
		UpgradableShieldGenerator shieldGen = (UpgradableShieldGenerator)upgradableComponents[(int)UpgradableComponentIndex.ShieldGen];
		shipMaxShields 						= shieldGen.GetCurrentMaxShield();
		shipShieldRechargeRate 				= shieldGen.GetCurrentRechargeRate();

		// When the boost (NOS) is on, keep the boost (NOS) speed
		if (!boostOn && !nosMode)
		{
			UpgradableEngine engine = (UpgradableEngine)upgradableComponents[(int)UpgradableComponentIndex.Engines];
			shipSpeed               = engine.GetCurrentSpeed();
		}

		UpgradableDrone drone = (UpgradableDrone)upgradableComponents[(int)UpgradableComponentIndex.Drone];
		droneSpeed 			  = drone.MovementSpeed;
		droneWorkTime   	  = drone.ImprovementTime;

		yield return new WaitForSeconds(1f);
		StartCoroutine(UpdateComponents());
	}

	/// <summary>
	/// Gives resource interest every 10 seconds based on the resource storage upgrade level.
	/// </summary>
	IEnumerator ResourceInterest()
	{
		float rate = ((UpgradableResourceStorage)upgradableComponents[(int)UpgradableComponentIndex.ResourceStorage]).InterestRate;
		currentShipResources = Convert.ToInt32(currentShipResources * (1 + rate/100));

		yield return new WaitForSeconds(10f);
		StartCoroutine(ResourceInterest());
	}

    /// <summary>
    /// Increase the current difficulty of the game.
    /// </summary>
    /// <param name="amount">The amount to increase by.</param>
    public void IncreaseDifficulty(int amount)
    {
        difficulty += amount;
    }

    /// <summary>
    /// Increase the current difficulty of the game.
    /// </summary>
    /// <param name="amount">The amount to decrease by.</param>
    public void DecreaseDifficulty(int amount)
    {
        difficulty -= amount;
    }

    public void SetDifficulty(int newDifficulty)
    {
        difficulty = newDifficulty;
    }

    /// <summary>
    /// Returns the current difficulty
    /// </summary>
    public int GetDifficulty()
    {
        return difficulty;
    }

	/// <summary>
	/// Initialises the upgradable components of the ship.
	/// </summary>
	private void InitialiseUpgradableComponents()
	{
		int numComponents    = Enum.GetNames(typeof(UpgradableComponentIndex)).Length;
		upgradableComponents = new UpgradableComponent[numComponents];

		upgradableComponents[(int)UpgradableComponentIndex.Engines] 		=
			new UpgradableEngine(settings.PlayerShipStartingSpeed, settings.PlayerShipStartingMaxTurnSpeed,
                settings.upgradeProperties[(int)ComponentType.Engine].engineMaxSpeedUpgradeRate,
                settings.upgradeProperties[(int)ComponentType.Engine].engineMaxTurningSpeedUpgradeRate);
        
		upgradableComponents[(int)UpgradableComponentIndex.Hull] 			= 
            new UpgradableHull(settings.upgradeProperties[(int)ComponentType.Hull].hullMaxHealthUpgradeRate);

		upgradableComponents[(int)UpgradableComponentIndex.Turrets] 		=
			new UpgradableTurret(settings.PlayerShipStartingFiringDelay, settings.PlayerShipStartingBulletDamage,
                settings.upgradeProperties[(int)ComponentType.Turret].turretsMaxDamageUpgradeRate,
                settings.upgradeProperties[(int)ComponentType.Turret].turretsMinFireDelayUpgradeRate);
        
		upgradableComponents[(int)UpgradableComponentIndex.ShieldGen]	    =
			new UpgradableShieldGenerator(settings.PlayerShipStartingShields, settings.PlayerShipStartingRechargeRate,
                settings.upgradeProperties[(int)ComponentType.ShieldGenerator].shieldsMaxShieldUpgradeRate,
                settings.upgradeProperties[(int)ComponentType.ShieldGenerator].shieldsMaxRechargeRateUpgradeRate);
        
		upgradableComponents[(int)UpgradableComponentIndex.Drone]		    =
			new UpgradableDrone(settings.EngineerWalkSpeed, settings.EngineerStartingWorkTime,
                settings.upgradeProperties[(int)ComponentType.Drone].droneMovementSpeedUpgradeRate,
                settings.upgradeProperties[(int)ComponentType.Drone].droneImprovementTimeUpgradeRate);
        
		upgradableComponents[(int)UpgradableComponentIndex.ResourceStorage] =
			new UpgradableResourceStorage(settings.PlayerShipInitialResourceBonus,settings.PlayerShipInitialResourceInterest,
                settings.upgradeProperties[(int)ComponentType.ResourceStorage].storageInterestRateUpgradeBonus,
                settings.upgradeProperties[(int)ComponentType.ResourceStorage].storageCollectionBonusUpgradeRate);
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
		case ComponentType.Hull:
			index = UpgradableComponentIndex.Hull;
			break;
		case ComponentType.ShieldGenerator:
			index = UpgradableComponentIndex.ShieldGen;
			break;
		case ComponentType.Turret:
			index = UpgradableComponentIndex.Turrets;
			break;
		case ComponentType.Drone:
			index = UpgradableComponentIndex.Drone;
			break;
		case ComponentType.ResourceStorage:
			index = UpgradableComponentIndex.ResourceStorage;
			break;
		}

		return upgradableComponents[(int)index];
	}

    /*
	 *  Getters and setters for Asteroid list
	 */
    public List<GameObject> GetAsteroidList()
    {
        return asteroidList;
    }

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
		asteroidSpawner.DecrementNumAsteroids();
    }

    public void RemoveAsteroidAt(int i)
    {
        bool wasDeleted = newAsteroids.Remove(asteroidList[i]);
        if (!wasDeleted)
			removedAsteroids.Add((uint)asteroidList[i].GetInstanceID());
        asteroidList.RemoveAt(i);
		asteroidSpawner.DecrementNumAsteroids();
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

	/// <summary>
	/// Removes the specified enemy from the enemy list.
	/// </summary>
	/// <param name="i">The index of the enemy in the list.</param>
	/// <param name="isGuard">If set to <c>true</c>, the enemy was a guard.</param>
	public void RemoveEnemyAt(int i, bool isGuard)
    {
        removedEnemies.Add(enemyList[i].GetInstanceID());
        enemyList.RemoveAt(i);

		if (!isGuard)
        	EnemySpawner.DecrementNumEnemies();
    }

	/// <summary>
	/// Removes the given enemy from the enemy list.
	/// </summary>
	/// <param name="enemy">The enemy object.</param>
	/// <param name="isGuard">If set to <c>true</c>, the enemy was a guard.</param>
	public void RemoveEnemy(GameObject enemy, bool isGuard)
	{
        removedEnemies.Add(enemy.GetInstanceID());
		enemyList.Remove(enemy);

		if (!isGuard)
			EnemySpawner.DecrementNumEnemies();
	}

	/// <summary>
	/// Notifies that there is a null enemy in the enemy list.
	/// </summary>
	public void NotifyNullEnemy()
	{
		for (int i = enemyList.Count - 1; i >= 0; i--)
			if (enemyList[i] == null)
				enemyList.RemoveAt(i);
	}
		
    public void AddToEngineerList(GameObject engineerObject)
    {
        engineerList.Add(engineerObject);
    }

    public List<GameObject> GetEngineerList()
    {
        return engineerList;
    }

	/*
	 *  Getters and setters for outpost list
	 */
	public List<GameObject> GetOutpostList()
	{
		return outpostList;
	}

    public GameObject GetOutpostById(int outpostId)
    {
        if(outpostList.Count < 1)
            return null;
        return outpostList[outpostId];
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

    /*
     * Getters and setters for the notification list
     */
    public void AddNotification(bool isUpgrade, ComponentType component)
    {
        Notification newNotification = Notification.create(isUpgrade, component);
        AIVoice.SendCommand(18);
        activeNotifications.Add(newNotification);
        newNotifications.Add(newNotification);
    }

    public void RemoveNotification(bool isUpgrade, ComponentType component)
    {
        Notification toRemove = Notification.create(isUpgrade, component);
        bool wasDeleted = newNotifications.Remove(toRemove);
        if (!wasDeleted) removedNotifications.Add(toRemove);
        if(!isUpgrade)
            AIVoice.SendCommand(12);
        else
            AIVoice.SendCommand(13);
        activeNotifications.Remove(toRemove);
    }

    public List<Notification> GetActiveNotifications()
    {
        return activeNotifications;
    }
    
    public List<Notification> GetNewNotifications()
    {
        return newNotifications;
    }
    
    public List<Notification> GetRemovedNotifications()
    {
        return removedNotifications;
    }
    
    public void ClearNewNotifications()
    {
        newNotifications.Clear();
    }
    
    public void ClearRemovedNotifications()
    {
        removedNotifications.Clear();
    }

    private void InitializeVariables()
    {
        asteroidList         = new List<GameObject>();        
        newAsteroids         = new List<GameObject>();
        removedAsteroids     = new List<uint>();
        enemyList            = new List<GameObject>();
        removedEnemies       = new List<int>();
        engineerList         = new List<GameObject>();
		outpostList          = new List<GameObject>();
        activeNotifications  = new List<Notification>();
        newNotifications     = new List<Notification>();
        removedNotifications = new List<Notification>();
        currentOfficers      = new Dictionary<uint, Officer>();

		// StartCoroutine(FindNull()); // Uncomment to help debug null enemies
    }

    public void Setup()
    {
        InitializeVariables();
        // Create scores for each player.
        for(int i = 0; i < 4; i++) 
            playerScore.Add(0);
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
        totalKills += 1;
	}
      
    /// <summary>
    /// Gets the player score.
    /// </summary>
    /// <returns>The player score.</returns>
    /// <param name="id">Identifier.</param>
    public int GetPlayerScore(int id)
    {
        return playerScore[id];
    }

    public int GetTotalKills()
    {
        return totalKills;
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
		float bonus = ((UpgradableResourceStorage)upgradableComponents[(int)UpgradableComponentIndex.ResourceStorage]).CollectionBonus;
		resources   = Convert.ToInt32(resources * (1 + bonus));

		currentShipResources += resources;
		totalShipResources   += resources;
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
	/// Increases the ship health.
	/// </summary>
	/// <param name="shield">The value by which to increase the health.</param>
	public void AddShipHealth(float health)
	{
		shipHealth += health;
	}

	/// <summary>
	/// Absorbs as much damage as possible using the current shield.
	/// </summary>
	/// <returns>The amount of damage left that could not be absorbed because the shields have depleated.</returns>
	/// <param name="damage">The damage value.</param>
	private float ShieldAbsorb(float damage)
	{
		if (shipShield > damage)
		{
			SetShipShield(shipShield - damage);
			myShield.Impact(GetShipShield());

			return 0f;
		}
		else
		{
			if (shipShield > 0)
			{
				damage -= shipShield;
				SetShipShield(0);
				myShield.ShieldDown();
			}

			return damage;
		}
	}

	/// <summary>
	/// Damage the ship's main hull.
	/// </summary>
	/// <param name="shielded">If set to <c>true</c>, damage is substracted from the shields before going to the hull.</param>
	/// <param name="damage">The ammount of damage to inflict.</param>
	public void DamageShip(float damage, bool shielded = true)
	{
		if (shielded)
			damage = ShieldAbsorb(damage);
		
		ReduceShipHealth(damage);
	}

	/// <summary>
	/// Reduces the ship health by a specific value
	/// </summary>
	/// <param name="value">Value.</param>
	private void ReduceShipHealth(float value) 
	{
		shipHealth -= value;
        if(shipHealth < 25)
            AIVoice.SendCommand(16);
        else if(shipHealth < 45)
            AIVoice.SendCommand(UnityEngine.Random.Range(14,16));

		if (shipHealth <= 0)
        {
            PlayerShip.transform.Find("PlayerShipLogic(Clone)").gameObject.GetComponent<ShipMovement>().Death();
			Status = GameStatus.Died;
        }
	}
		
	/// <summary>
	/// Reduces the health of a component. Hitting the bridge reduces the ship's (hull) health.
	/// </summary>
	/// <param name="component">The component.</param>
	/// <param name="value">The value by which to decrease health.</param>
	/// <param name="shielded">If set to <c>true</c>, damage is substracted from the shields before going to the component.</param>
	public void DamageComponent(ComponentType component, float value, bool shielded = true)
	{
		if (shielded)
			value = ShieldAbsorb(value);

		// Debug.Log("Damage " + value + " to " + component.ToString());
			
		UpgradableComponent upgradableComponent = null;
		switch(component)
		{
		case ComponentType.Hull:
			ReduceShipHealth(value);
			upgradableComponent = upgradableComponents[(int)UpgradableComponentIndex.Hull];
			// Debug.Log("Hull health " + shipHealth);
			break;
		case ComponentType.Engine:
			engineHealth        -= value;
			upgradableComponent  = upgradableComponents[(int)UpgradableComponentIndex.Engines];
			// Debug.Log("Engine health " + engineHealth);
			break;
		case ComponentType.Turret:
			turretHealth        -= value;
			upgradableComponent  = upgradableComponents[(int)UpgradableComponentIndex.Turrets];
			// Debug.Log("Turret health " + turretHealth);
			break;
		case ComponentType.ShieldGenerator:
			shieldGeneratorHealth -= value;
			upgradableComponent    = upgradableComponents[(int)UpgradableComponentIndex.ShieldGen];
			// Debug.Log("Generator health " + shieldGeneratorHealth);
			break;
		}

		if (upgradableComponent != null)
		{
			upgradableComponent.Damage(value);
			// Debug.Log("Component health: " + upgradableComponent.Health);
		}
		else
			Debug.LogWarning("Tried to damage non-existent component of type: " + component);
	}

	/// <summary>
	/// Repairs a ship part.
	/// </summary>
	/// <param name="part">The part to repair.</param>
	public void RepairPart(ComponentType part)
	{
		float maxHealth = GetUpgradableComponent(part).MaxHealth;

		switch(part)
		{
		case ComponentType.Hull:
			shipHealth = maxHealth;
			break;
		case ComponentType.Engine:
			engineHealth = maxHealth;
			break;
		case ComponentType.Turret:
			turretHealth = maxHealth;
			break;
		case ComponentType.ShieldGenerator:
			shieldGeneratorHealth = maxHealth;
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
	/// Gets the ship's speed without boost or NOS, even if they're currently on.
	/// </summary>
	/// <returns>The ship base speed.</returns>
	public float GetShipBaseSpeed()
	{
		UpgradableEngine engine = (UpgradableEngine)upgradableComponents[(int)UpgradableComponentIndex.Engines];
		return engine.GetCurrentSpeed();
	}

	/// <summary>
	/// Activates the ship's boost ability.
	/// </summary>
	/// <param name="boostSpeed">The boost speed.</param>
	public void ActivateBoost(float boostSpeed)
	{
		boostOn   = true;
		shipSpeed = boostSpeed;
	}

	/// <summary>
	/// Deactivates the ship's boost ability.
	/// </summary>
	public void DeactivateBoost()
	{
		boostOn     = false;
		float speed = ((UpgradableEngine)upgradableComponents[(int)UpgradableComponentIndex.Engines]).GetCurrentSpeed();
		shipSpeed   = speed;
	}

	/// <summary>
	/// Gets the max ship turn speed.
	/// </summary>
	/// <returns>The max turn speed.</returns>
	public float GetShipMaxTurnSpeed()
	{
		return ((UpgradableEngine)upgradableComponents[(int)UpgradableComponentIndex.Engines]).GetCurrentTurningSpeed();
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
	/// Increases the ship shield.
	/// </summary>
	/// <param name="shield">The value by which to increase the shield.</param>
	public void AddShipShield(float shield)
	{
		shipShield += shield;
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
	/// Recharge the ship's shield..
	/// </summary>
	public void RechargeShield()
	{
		float value = shipShieldRechargeRate / 10f; // The 10f is copied over from old code. It translates rate into value per tick

		// Don't recharge the shields over the maximum value
		float newShieldvalue = shipShield + value;
		if (newShieldvalue > shipMaxShields)
			value = shipMaxShields - shipShield;
			
		SetShipShield(shipShield + value);
	}

	/// <summary>
	/// Gets the turret firing delay.
	/// </summary>
	/// <returns>The firing delay.</returns>
	public float GetFiringDelay()
	{
		return ((UpgradableTurret)upgradableComponents[(int)UpgradableComponentIndex.Turrets]).GetCurrentFireDelay();
	}

	/// <summary>
	/// Gets the damage per bullet fired.
	/// </summary>
	/// <returns>The bullet damage.</returns>
	public float GetBulletDamage()
	{
		return ((UpgradableTurret)upgradableComponents[(int)UpgradableComponentIndex.Turrets]).GetCurrentDamage();
	}

	/// <summary>
	/// Gets the drone stats.
	/// </summary>
	/// <param name="movementSpeed">The movement speed.</param>
	/// <param name="workTime">The improvement work time.</param>
	public void GetDroneStats(out float movementSpeed, out float workTime)
	{
		movementSpeed = droneSpeed;
		workTime      = droneWorkTime;
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
		case ComponentType.Hull:
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
    /// Updates the officer list based on the contents of
    /// officerData
    /// </summary>
    /// <param name="officerData"></param>
    public void UpdateOfficerList(string officerData)
    {
        ResetOfficerList();
        string[] semicolon = { ";" };
        string[] officerObjects = officerData.Split(semicolon, StringSplitOptions.RemoveEmptyEntries);

        foreach (string officer in officerObjects)
        {
            Officer newOfficer = Officer.DeserializeFromString(officer);
            currentOfficers.Add(newOfficer.RemoteId, newOfficer);
        }
    }

    public void ResetOfficerList()
    {
        if (currentOfficers == null)
            currentOfficers = new Dictionary<uint, Officer>();

        // Clear the list so that we avoid having
        // officers from the previous game in there
        currentOfficers.Clear();
    }

    public Dictionary<uint, Officer> GetOfficerMap()
    {
        return currentOfficers;
    }

    public int GetOfficerCount()
    {
        if(currentOfficers == null)
            return 1;
        return currentOfficers.Count;
    }

    public void SetTeamName(string name)
    {
        teamName = name;
    }

    public string GetTeamName()
    {
        return teamName;
    }
}
