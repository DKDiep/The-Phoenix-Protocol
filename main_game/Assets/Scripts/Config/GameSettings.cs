/* Relevant documentation: https://bitbucket.org/pyrolite/game/wiki/Game%20Settings */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class that contains all the game settings. These are all set through the Unity IDE and should only be retrieved from here.
/// </summary>
public class GameSettings : MonoBehaviour
{
	public GameObject GameManager;

	[Header("Asteroid Logic")]
	public int AsteroidMaxDroppedResources; // The maximum number of resources that can be dropped by an asteroid. 

	[Header("Asteroid Spawner")]
	public int MaxAsteroids;            		// Maximum number of asteroids that can exist simultaneously
	public float AsteroidMaxVariation;  		// Max variation in size (0-10)
	public float AsteroidMinDistance;   		// Minimum distance to the player that an asteroid can spawn
	public float AsteroidMaxDistance;   		// Maximum distance to the player that an asteroid can spawn
	public int MaxAsteroidsSpawnedPerFrame;
	public float AsteroidAvgSize;               // The average asteroid size. Please update this manually if you change the sizes to avoid useless computation
	public float AsteroidFieldSpacingFactor;    // Higher values make asteroid fields more sparse. TODO: 2f looks good, but is quite expensive
	public float AsteroidVisibilityEdgeSpawnMaxAngle; // The maximum rotation angle on the x and y axes when spawning on the visibility edge

    [Header("Commander Abilities")]
    public float shootCooldown;
    public int projectileCount;
    public float maxTargetRadius;
    public float boostDuration;
    public float boostCooldown;
    public float boostSpeedMultiplier;
    public float shieldOverdriveDuration;
    public float shieldOverdriveCooldown;
    public float empCooldown;
    public float empRadius;
    public float empDuration;
    public float smartBombCooldown;
    public float smartBombRadius;

	[Header("Enemy Spawner")]
	public float EnemyMinSpawnDistance;
	public float EnemyMaxSpawnDistance;
	//public int MaxEnemies;
	public int AIWaypointsPerEnemy;
	public int AIWaypointGenerationFactor;
	public int AIWaypointRadius;
	public float AIWaypointWidthScale;
	public float AIWaypointHeightScale;
	public Vector3 AIWaypointShift;
	public int EnemyOutpostSpawnRadius; // The radius of the sphere around an outpost in which to spawn protecting enemies

    [Header("Enemy Type Settings")]
    public EnemySpawner.EnemyProperties[] enemyProperties;
	
	[Header("Enemy Logic")]
	public float EnemyShotsPerSec;
	public float EnemyShootPeriod;                  // How long in seconds the enemy should shoot for when it fires
	public int EnemyShootPeriodPercentageVariation; // Percentage variation +/- in the length of the shooting period
	public float EnemyShieldDelay;                  // Delay in seconds to wait before recharging shield
	public float EnemyShieldRechargeRate;           // Units of shield to increase per second
	public AudioClip EnemyFireSoundPrefab;
	public bool EnemyFireSoundRandomPitch;
	public float EnemyTurningSpeed;
	public float EnemySuicidalSpeedUpdateInterval; // The interval (seconds) at which suicidal enemies update their speed to match the player
	public float EnemySuicidalExtraSpeed; // The amount by which a suicidal enemy always goes faster than the player ship
	public float HackedEnemyBulletDamage;
	public float HackedEnemyMaxY; // The max relative Y at which a hacked enemy will follow the player

	[Header("Engineer Controller")]
	public float EngineerWalkSpeed;
	public float EngineerMouseLookXSensitivity;
	public float EngineerMouseLookYSensitivity;
    public float EngineerControllerLookXSensitivity;
    public float EngineerControllerLookYSensitivity;
	public bool EngineerMouseLookClampVerticalRotation;
	public float EngineerMouseLookMinimumX;
	public float EngineerMouseLookMaximumX;
	public bool EngineerMouseLookSmooth;
	public float EngineerMouseLookSmoothTime;
    public float EngineerMaxDistance;
    public float EngineerMaxInactiveTime;
    public float EngineerYawSpeed;
    public float EngineerCollisionDistance;
    public float EngineerMinMovement;
    public Texture EmptyProgressBar;
    public Texture FilledProgressBar;
	public float EngineerStartingWorkTime;
    public Material EngineerDefaultMat;
    public Material EngineerRepairMat;
    public Material EngineerUpgradeMat;

    [Header("Portal")]
    public Vector3 PortalPosition;

    [Header("Glom Mothership")]
    public float GlomMothershipHealth;
    public float GlomMothershipSpawnDistance; // Spawn when the player is this distance from the portal
    public Vector3 GlomMothershipSpawnPosition; // Location to spawn the mothership
    public float GlomMothershipDamageEffects1; // Level of health of mothership to trigger these damage effects
    public float GlomMothershipDamageEffects2;
    public float GlomMothershipDamageEffects3;
    public float GlomMothershipSpawnRate; // Rate at which to spawn enemies

	[Header("Main Menu")]
	public float MainMenuRotationSpeed;
	public string ConfigFile;

	[Header("TCP Server")]
	public int TCPListenPort;
	public int TCPMaxReceivedMessagesPerInterval;

	[Header("UDP Server")]
	public int UDPListenPort;
	public int UDPClientPort;
	public int UDPMaxReceivedMessagesPerInterval;

	[Header("Outposts")]
	public GameObject OutpostModel1Prefab;
	public float OutpostResourceCollectionDistance; // The distance from the outpost the ship has to be in order to collect resources
	public int EasyOutposts;
    public int MediumOutposts;
    public int HardOutposts;
    public int EasyMultiplier;
    public int MediumMultiplier;
    public int HardMultiplier;
    public int EasyMinEnemies;
    public int EasyMaxEnemies;
    public int MediumMinEnemies;
    public int MediumMaxEnemies;
    public int HardMinEnemies;
    public int HardMaxEnemies;
	public int OutpostMinDistance; // The minimum distance between outposts
	public int OutpostGuardTriggerDistance; // The distance at which the guards are triggered
	public float OutpostMinAsteroidFieldSize;
	public float OutpostMaxAsteroidFieldSize;
	public float OutpostMinAsteroidFieldDensity;
	public float OutpostMaxAsteroidFieldDensity;
    public List<Vector3> OutpostSpawnLocations; //Warning: it is possible to spawn two outposts in the same position with this
    public List<DifficultyEnum> OutpostDifficulties;   //Difficulties should be in the same list positions as the corresponding outpost in the above list
                                                       //(So should be same size as above)
    public float OutpostSpawnLocationsVariance; //This only applies to outposts spawned with a set position
	[Header("Ship Movement")]
	public float PlayerShipTurnSpeed;
	public float PlayerShipSlowDown;
	public float PlayerShipShieldDelay; // Delay in seconds to wait before recharging shield

    [Header("Ship Properties")]
    public Vector3 PlayerStartingPosition;
    public float PlayerShipStartingHealth;
    public float PlayerShipStartingShields;
    public float PlayerShipStartingRechargeRate;
    public float PlayerShipStartingSpeed;
	public float PlayerShipStartingMaxTurnSpeed;
    public int PlayerShipStartingResources;
    public int PlayerShipStartingCivilians;
    public float PlayerShipComponentHealth;
	public float PlayerShipInitialResourceBonus;
	public float PlayerShipInitialResourceInterest;
	public float PlayerShipStartingFiringDelay;
	public int PlayerShipStartingBulletDamage;

    [Header("Ship Upgrade Properties")]
    public UpgradeProperties[] upgradeProperties;


	[Header("Player Shooting")]
	public Texture2D PlayerHitmarker; // Hitmarker texture
	public AudioClip PlayerFireSound; // Sound to make when firing
	public bool PlayerFireSoundRandomPitch;
	public float PlayerBulletAccuracy;
	public float PlayerBulletSpeed;
	public float WiimoteInterpolationFactor;
	public int PlayerMaxAmmo;
	public int PlayerShootingAmmoCost;       // The ammount of ammo needed to fire a bullet
	public int PlayerAmmoRechargeValue;      // The ammount of ammo recharged per interval
	public float PlayerAmmoRechargeInterval; // The interval at which ammo recharges.

    [Header("Mission Settings")]
    public MissionManager.Mission[] missionProperties;

    [Header("Game Scoring Settings")]
    public int civilianWeighting;
    public int resourcesWeighting;
    public int playerScoreWeighting;
    public int hullWeighting;
    public int droneWeighting;
    public int engineWeighting;
    public int storageWeighting;
    public int shieldsWeighting;
    public int turretWeighting;
}
