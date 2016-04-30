/*
    Control enemy ship attributes and AI

    Relevant Documentation:
	  * Enemy AI:    https://bitbucket.org/pyrolite/game/wiki/Enemy%20AI
      * Enemy Types: https://bitbucket.org/pyrolite/game/wiki/Enemy%20Types
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLogic : MonoBehaviour, IDestructibleObject, IDestructionListener
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private int shootPeriodPercentageVariation; // Percentage variation +/- in the length of the shooting period
	private float shieldDelay; 					// Delay in seconds to wait before recharging shield
	private float shieldRechargeRate; 			// Units of shield to increase per second
	private AudioClip fireSnd;
	private bool randomPitch;
    private GameObject empEffect;
    private bool empEnabled = false;

	/* These fields are set when assigning a type to the enemy. They should not be initilaised manually.
	 * Please use the internal modifier for all of them to make it clear that they are set somewhere else in code,
	 * and to prevent them from showing up in the Unity Editor, which would cause confusion. */
	internal float speed;
	internal float maxHealth;
	internal float maxShield; // Max recharging shield level. Set to 0 to disable shields
	internal float collisionDamage;
    internal bool isSuicidal;
    internal float shotsPerSec;
    internal float shootPeriod;                  // How long in seconds the enemy should shoot for when it fires
    internal float engageDistance;
	internal float accuracy;
	internal float bulletDamage;
	internal float bulletSpeed;
	internal EnemyType type;

	private AudioSource mySrc;
	private GameState gameState;
    private TCPServer tcpServer;
	private GameObject player; // This is parent object, so the transfor position will be a bit above the model
	private GameObject playerShip; // This is the actual model of the ship

	private bool shoot = false, angleGoodForShooting = false;
	private bool rechargeShield;

	private bool hacked = false;
    private uint controllingPlayerId = 0;
    private uint accumulatedPlayerScore = 0;
	private GameObject hackedAttackTraget = null;
	private GameObject hackedWaypoint;
	private const string HACK_WAYPOINT_NAME = "HackWaypoint";
	private float hackedBulletDamage;
	private float hackedFollowMaxY;
	private bool wasSuicidalBeforeHack;

	// These should be constants, but you can't know the value at compile time, and we can't use the constructor
	// Please, treat them as constants
	private LayerMask LAYER_PLAYER_BULLET;
	private LayerMask LAYER_ENEMY_BULLET;
	private int LAYER_MASK_OBSTACLES;

	internal float health;
	private float shield;
    private float distance;
	private float lastShieldCheck; // Temp variable allows us to see whether I've taken damage since last checking
	private float randomZ;         // A random Z angle to give the enemies some uniqueness

	private float speedUpdateDelay;
	private float suicidalExtraSpeed;
	private float suicidalMinFrontDist; // The minimum distance in front of the ship a suicidal enemy has to reach before going for the player

	private GameObject shootAnchor;
	private Vector3 prevPos, currentPos;
	private GameObject controlObject;
	private Collider controlObjectCollider;
	private EnemySyncParams controlObjectSyncParams;

	// This is the amount of resources dropped by the enemy when killed. It is calculated based on the enemy's max health and shield
	private int droppedResources;

	// The current state of the ship's AI
	internal EnemyAIState state;

	// Waypoints are used to move around the player when close enough
	private List<GameObject> aiWaypoints;
	private GameObject currentWaypoint               = null;
	private const float AI_WAYPOINT_REACHED_DISTANCE = 20f;    // Distance when a waypoint is considered reached
	private const float AI_SHOOT_MAX_ANGLE           = 50f;    // Maximum angle with the player when shooting is possible
	private float aiWaypointRotationSpeed;   // Turning speed when following waypoints
	private float lastYRot;
	private bool reachedFrontOfPlayer = false;
	private bool collisionsEnabled = true;

	private List<GameObject> playerShipTargets;
	private GameObject currentTarget;

	// Parameters for raycasting obstacle detection
	// Two rays are shot forwards, on the left and right side of the ship to detect incoming obstacles
	private const int AI_OBSTACLE_RAY_FRONT_LENGTH  = 45;
	private const string AI_OBSTACLE_TAG_DEBRIS     = "Debris";
	private const string AI_OBSTACLE_TAG_ENEMY      = "EnemyShip";
	private const string AI_OBSTACLE_TAG_MOTHERSHIP = "GlomMothership";
	private const int AI_OBSTACLE_AVOID_ROTATION    = 45;
	private int previousAvoidDirection              = 0;
	private const string AVOID_WAYPOINT_NAME        = "AvoidWaypoint";
	private int avoidDirection;
	private float aiObstacleRayFrontOffset;

    private ObjectPoolManager bulletManager;
    private ObjectPoolManager gnatBulletManager;
    private ObjectPoolManager fireflyBulletManager;
    private ObjectPoolManager hornetBulletManager;
    private ObjectPoolManager blackWidowBulletManager;
	private ObjectPoolManager bulletLogicManager;
    private ObjectPoolManager impactManager;
    private ObjectPoolManager explosionManager;
    private ObjectPoolManager enemyLogicManager;
    private ObjectPoolManager enemyManager;
    private ObjectPoolManager gnatManager;
    private ObjectPoolManager fireflyManager;
    private ObjectPoolManager termiteManager;
    private ObjectPoolManager lightningBugManager;
    private ObjectPoolManager hornetManager;
    private ObjectPoolManager blackWidowManager;

	private const int AI_GUARD_TURN_BACK_DISTANCE = 500; // The distance at which guards stop engaging the player and turn back to the outpost
	private const int AI_GUARD_PROTECT_DISTANCE   = 100; // The distance from the outpost at which to stop and wait when returning to guard
	private int guardTriggerDistance = 100; // The distance at which a player triggers the guard to attack
	private const string GUARD_RETURN_WAYPOINT_NAME = "GuardReturnWaypoint";

    private Renderer meshRenderer;

	private List<IDestructionListener> destructionListeners;

    private AIVoice aiVoice;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] Material hackedMaterial;
    [SerializeField] Material hackedTargetMaterial;
    [SerializeField] Material targetMaterial;
	#pragma warning restore 0649

    Material originalGlow;

	void Start ()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		GameObject server = settings.GameManager;
		gameState         = server.GetComponent<GameState>();
        tcpServer         = server.GetComponent<TCPServer>();

        gnatBulletManager     = GameObject.Find("GnatBulletManager").GetComponent<ObjectPoolManager>();
        fireflyBulletManager     = GameObject.Find("FireflyBulletManager").GetComponent<ObjectPoolManager>();
        hornetBulletManager = GameObject.Find("HornetBulletManager").GetComponent<ObjectPoolManager>();
        blackWidowBulletManager = GameObject.Find("BlackWidowBulletManager").GetComponent<ObjectPoolManager>();
        bulletLogicManager      = GameObject.Find("EnemyBulletLogicManager").GetComponent<ObjectPoolManager>();
        impactManager     = GameObject.Find("BulletImpactManager").GetComponent<ObjectPoolManager>();
        explosionManager  = GameObject.Find("EnemyExplosionManager").GetComponent<ObjectPoolManager>();
        enemyLogicManager = GameObject.Find("EnemyLogicManager").GetComponent<ObjectPoolManager>();

		destructionListeners = new List<IDestructionListener>();

		LAYER_PLAYER_BULLET  = LayerMask.NameToLayer("Player Bullet");
		LAYER_ENEMY_BULLET   = LayerMask.NameToLayer("Enemy Bullet");
		LAYER_MASK_OBSTACLES = LayerMask.GetMask("Asteroid", "Water", "Player");
	}

	private void LoadSettings()
	{
		shotsPerSec 				   = settings.EnemyShotsPerSec;
		shootPeriod 				   = settings.EnemyShootPeriod;
		shootPeriodPercentageVariation = settings.EnemyShootPeriodPercentageVariation;
		shieldDelay					   = settings.EnemyShieldDelay;
		shieldRechargeRate			   = settings.EnemyShieldRechargeRate;
		fireSnd						   = settings.EnemyFireSoundPrefab;
		randomPitch					   = settings.EnemyFireSoundRandomPitch;
		aiWaypointRotationSpeed 	   = settings.EnemyTurningSpeed;
		speedUpdateDelay		   	   = settings.EnemySuicidalSpeedUpdateInterval;
		suicidalExtraSpeed 			   = settings.EnemySuicidalExtraSpeed;
		hackedBulletDamage 			   = settings.HackedEnemyBulletDamage;
		hackedFollowMaxY 			   = settings.HackedEnemyMaxY;
	}

	void Update ()
	{
		prevPos    = currentPos;
		currentPos = player.transform.position;
		distance   = Vector3.Distance(controlObject.transform.position, player.transform.position);

		// Waiting enemies don't need to perform any logic until the player moves within their range
		if (state == EnemyAIState.Wait)
		{
			if (distance <= guardTriggerDistance)
			{
				currentWaypoint = GetNextWaypoint();
				state = EnemyAIState.EngagePlayer;
			}

			return;
		}

		// Suicidal and hacked enemies don't go through the regular states, so handle the collider chages here
		if (hacked || (isSuicidal && !reachedFrontOfPlayer))
			controlObjectCollider.enabled = collisionsEnabled = (distance < engageDistance);

		if (collisionsEnabled)
		{
			// Check if about to collide with something
			// Ignore the outpost if returning towards its location, because the guard distance might be smaller than the avoid distance.
			// We will not hit the outpost as long as the guard distance accounts for the the outpost's size
			AvoidInfo obstacleInfo = CheckObstacleAhead();
			if (!obstacleInfo.IsNone())
			{

				// If already avoiding an obsctale, clear the previous waypoint before creating another one
				if (state == EnemyAIState.AvoidObstacle)
					Destroy(currentWaypoint);

				// If about to collide with an enemy, go towards a different waypoint - it's very likely the other guy will not go the same way
				// Otherwise, temporarily change direction
				if (!hacked && obstacleInfo.ObstacleTag.Equals(AI_OBSTACLE_TAG_ENEMY))
				{
					state = EnemyAIState.EngagePlayer;
					previousAvoidDirection = 0;
					currentWaypoint = GetNextWaypoint();
				}
				else
				{
					state = EnemyAIState.AvoidObstacle;

					// If already avoiding an obstacle, keep the same direction
					// Otherwise, decided based on the side the obstacle is closest to
					if (previousAvoidDirection != 0)
						avoidDirection = previousAvoidDirection;
					else
						avoidDirection = obstacleInfo.Side == AvoidInfo.AvoidSide.Left ? -1 : 1;

					// Create a temp waypoint to follow in order to avoid the asteroid
					GameObject avoidWaypoint = new GameObject(AVOID_WAYPOINT_NAME);
					avoidWaypoint.transform.position = controlObject.transform.position;
					avoidWaypoint.transform.rotation = controlObject.transform.rotation;

					float yRotation = avoidDirection * AI_OBSTACLE_AVOID_ROTATION;
					if (obstacleInfo.ObstacleTag.Equals(AI_OBSTACLE_TAG_MOTHERSHIP))
						yRotation *= 2;
					avoidWaypoint.transform.Rotate(0, yRotation, 0);
					avoidWaypoint.transform.Translate(Vector3.forward * AI_OBSTACLE_RAY_FRONT_LENGTH);
					currentWaypoint = avoidWaypoint;

					// Uncomment this to visualise the waypoint
					// If there is more than one enemy, this probably doesn't help much
					/*GameObject visibleWaypoint = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				visibleWaypoint.transform.position   = currentWaypoint.transform.position;
				visibleWaypoint.transform.localScale = visibleWaypoint.transform.localScale * 5;*/
				}
			}
		}

		// Check if this enemy can shoot
		if (hacked)
		{
			// Hacked enemies that aren't set to attack don't shoot at all
			if (hackedAttackTraget == null)
				angleGoodForShooting = false;
			else
			{
				// Check if the angle is good for shooting
				Vector3 direction 	 = hackedAttackTraget.transform.position - controlObject.transform.position;
				float angle 		 = Vector3.Angle(controlObject.transform.forward, direction);
				float targetDistance = Vector3.Distance(controlObject.transform.position, hackedAttackTraget.transform.position);
				angleGoodForShooting = (targetDistance < engageDistance) && (angle < AI_SHOOT_MAX_ANGLE);
			}
		}
		else
		{
			// Check if the angle is good for shooting and the enemy is in front of the player
			Vector3 direction = player.transform.position - controlObject.transform.position;
			float angle = Vector3.Angle(controlObject.transform.forward, direction);
			Vector3 enemyRelativeToPlayer = player.transform.InverseTransformPoint(controlObject.transform.position);
			angleGoodForShooting = (distance < engageDistance) && (angle < AI_SHOOT_MAX_ANGLE) && (enemyRelativeToPlayer.z > 0);
		}

		// Avoid obsctales if needed
		if (state == EnemyAIState.AvoidObstacle)
		{
			bool finishedAvoiding = MoveTowardsCurrentWaypoint();

			// When the temporary avoid waypoint is reached, return to seeking the player
			if (finishedAvoiding)
			{
				state                  = hacked ? EnemyAIState.Hacked : EnemyAIState.SeekPlayer;
				previousAvoidDirection = 0;
				currentWaypoint 	   = null;
			}
		}
		else
		{
			// Suicidal enemies first get in front of the player, then crash into them
			if (isSuicidal)
			{
				if (!reachedFrontOfPlayer)
				{
					if (currentWaypoint == null)
						currentWaypoint = GetNextWaypoint();

					reachedFrontOfPlayer = MoveTowardsCurrentWaypoint();
					if (reachedFrontOfPlayer)
					{
						collisionsEnabled = false;
						state 		  	  = EnemyAIState.SeekPlayer;
					}
				}
				else
					MoveTowardsPlayer();

				return;
			}

			// Hacked enemies can move to a location or attack another enemy
			// When attacking another enemy, try to get behind him so we get a better shot
			if (hacked)
			{
				if (currentWaypoint == null)
				{
					if (hackedAttackTraget != null)
					{
						// Debug.Log("Following target " + hackedAttackTraget.name);
						currentWaypoint = hackedAttackTraget;
					}
					else
						FollowPlayer();
				}

				MoveTowardsCurrentWaypoint();

				return;
			}

			// Engage player when close enough, otherwise catch up to them
			else if ((state == EnemyAIState.SeekPlayer && distance <= engageDistance))
			{
				currentWaypoint = GetNextWaypoint();
				state = EnemyAIState.EngagePlayer;
				collisionsEnabled = controlObjectCollider.enabled = true;
			}
			else if (state == EnemyAIState.EngagePlayer && distance > engageDistance)
			{
				state = EnemyAIState.SeekPlayer;
				collisionsEnabled = controlObjectCollider.enabled = false;
			}

			if (state == EnemyAIState.EngagePlayer)
			{
				MoveTowardsCurrentWaypoint();
			}
			else if (state == EnemyAIState.SeekPlayer)
			{
				angleGoodForShooting = false;
				MoveTowardsPlayer();
			}
		}
	}

	// When not engaged, try and get closer to the player
	private void MoveTowardsPlayer()
	{
		controlObject.transform.LookAt(playerShip.transform.position);
		controlObject.transform.Translate(Vector3.forward * Time.deltaTime * speed);
	}

	// Get the next engagement waypoint to follow, which should be different from the previous one
	private GameObject GetNextWaypoint()
	{
		GameObject nextWaypoint;
		bool getAnother;

		do
		{
			getAnother   = false;
			int r		 = Random.Range (0, aiWaypoints.Count);
			nextWaypoint = aiWaypoints [r];

			if (isSuicidal)
			{
				Vector3 waypointRelativeToPlayer = player.transform.InverseTransformPoint(nextWaypoint.transform.position);
				getAnother = (waypointRelativeToPlayer.z < suicidalMinFrontDist);
			}
			else if (currentWaypoint != null && nextWaypoint.Equals(currentWaypoint))
				getAnother = true;
		} while (getAnother);

		return nextWaypoint;
	}

	// When engaged with the player ship, move between waypoints, returning true when the waypoint is reached
	private bool MoveTowardsCurrentWaypoint()
	{
		Vector3 relativePos = currentWaypoint.transform.position - controlObject.transform.position;
		Quaternion rotation = Quaternion.LookRotation (relativePos);

		// Turn and bank
		controlObject.transform.rotation = Quaternion.Lerp (controlObject.transform.rotation, rotation,
			Time.deltaTime * aiWaypointRotationSpeed);
		float yRot = Mathf.Clamp (lastYRot - controlObject.transform.localEulerAngles.y, 0, 3);
		controlObject.transform.Rotate (0, 0, yRot, Space.Self);
		lastYRot = controlObject.transform.localEulerAngles.y;

		float distanceToWaypoint = Vector3.Distance(controlObject.transform.position, currentWaypoint.transform.position);
		if (distanceToWaypoint > AI_WAYPOINT_REACHED_DISTANCE)
			controlObject.transform.Translate (Vector3.forward * Time.deltaTime * speed);

		distanceToWaypoint = Vector3.Distance(controlObject.transform.position, currentWaypoint.transform.position);
		if (distanceToWaypoint <= AI_WAYPOINT_REACHED_DISTANCE && state != EnemyAIState.Hacked)
		{
			// If the reached waypoint is an avoid or a hacked move waypoint, it is not needed any more
			if (state == EnemyAIState.AvoidObstacle)
				Destroy(currentWaypoint);

			currentWaypoint = GetNextWaypoint();

			return true;
		}

		return false;
	}

	// Check if the enemy is about to collide with an object
	// Returns the tag of the object, or null if there is no collision about to happen
	private AvoidInfo CheckObstacleAhead()
	{
		Transform objectTransform = controlObject.transform;
		bool hitLeft, hitRight;
		RaycastHit hitInfoLeft, hitInfoRight;

		// Cast two rays forward, one on each side of the object, to check for obstaclse
		// If an obstacle is found, return its tag
		hitLeft = Physics.Raycast (objectTransform.position - aiObstacleRayFrontOffset * objectTransform.right, objectTransform.forward,
			out hitInfoLeft, AI_OBSTACLE_RAY_FRONT_LENGTH, LAYER_MASK_OBSTACLES);
		if (hitLeft)
			return AvoidInfo.Get(hitInfoLeft.collider.gameObject.tag, AvoidInfo.AvoidSide.Left);

		hitRight = Physics.Raycast (objectTransform.position + aiObstacleRayFrontOffset * objectTransform.right, objectTransform.forward,
			out hitInfoRight, AI_OBSTACLE_RAY_FRONT_LENGTH, LAYER_MASK_OBSTACLES);
		if (hitRight)
			return AvoidInfo.Get(hitInfoRight.collider.gameObject.tag, AvoidInfo.AvoidSide.Right);

		return AvoidInfo.None();
	}

    IEnumerator UpdateDelay()
    {
		// Wait a moment to ensure the type is set
        yield return new WaitForSeconds(1f);

		droppedResources = System.Convert.ToInt32(Random.Range(0, maxHealth + maxShield) / 5);

        if(type == EnemyType.Gnat )
        {
            enemyManager = gnatManager;
            bulletManager = gnatBulletManager;
        }
        else if(type == EnemyType.Firefly)
        {
            enemyManager = fireflyManager;
            bulletManager = fireflyBulletManager;
        }
        else if(type == EnemyType.Termite)
        {
            enemyManager = termiteManager;
            bulletManager = null;
        }
        else if(type == EnemyType.LightningBug)
        {
            enemyManager = lightningBugManager;
            bulletManager = null;
        }
        else if (type == EnemyType.Hornet)
        {
            enemyManager = hornetManager;
            bulletManager = hornetBulletManager;
        }
        else if(type == EnemyType.BlackWidow)
        {
            enemyManager = blackWidowManager;
            bulletManager = blackWidowBulletManager;
        }

		if (isSuicidal)
			StartCoroutine(MatchPlayerSpeed());
		else
			StartCoroutine(ShootManager());

        StartCoroutine(UpdateTransform());
    }

    IEnumerator UpdateTransform()
    {
        //Debug.Log("My type is " + type + " and manager " + enemyManager.gameObject.name);
        enemyManager.UpdateTransform(controlObject.transform.position, controlObject.transform.rotation, controlObject.name);
        yield return new WaitForSeconds(Mathf.Clamp((distance / 750f) - 0.2f, 0.05f, 1.75f));
        StartCoroutine(UpdateTransform());
    }

	/// <summary>
	/// Enables or disables rendering based on player distance.
	/// </summary>
	/// <returns>The render distance.</returns>
	IEnumerator CheckRenderDistance()
	{
		if (meshRenderer == null)
			meshRenderer = controlObject.GetComponent<MeshRenderer>();
		else
			meshRenderer.enabled = (distance <= 1000);

		yield return new WaitForSeconds(Random.Range(1f, 1.5f));
		StartCoroutine(CheckRenderDistance());
	}

	/// <summary>
	/// Keeps the enemy always moving faster than the player.
	///
	/// This should only be used on suicidal enemies.
	/// </summary>
	IEnumerator MatchPlayerSpeed()
	{
		float playerSpeed = gameState.GetShipBaseSpeed();
		if (speed < playerSpeed + suicidalExtraSpeed)
			speed = playerSpeed + suicidalExtraSpeed;

		yield return new WaitForSeconds(speedUpdateDelay);
		StartCoroutine(MatchPlayerSpeed());
	}

	public IEnumerator EMPEffect()
	{
		empEffect = transform.parent.Find("EMPEffect").gameObject;
        empEnabled = true;
		float originalSpeed = speed;
		speed = 0;
		empEffect.SetActive(true);
		yield return new WaitForSeconds(settings.empDuration);
		empEffect.SetActive(false);
        empEnabled = false;
		speed = originalSpeed;
	}

    void OnEnable()
    {
        // Decide the resource drop for this ship to be within DROP_RESOURCE_RANGE range of its max health + shield
        if(gnatManager == null)
            gnatManager = GameObject.Find("GnatManager").GetComponent<ObjectPoolManager>();

        if(fireflyManager == null)
            fireflyManager = GameObject.Find("FireflyManager").GetComponent<ObjectPoolManager>();

        if(termiteManager == null)
            termiteManager = GameObject.Find("TermiteManager").GetComponent<ObjectPoolManager>();

        if(lightningBugManager == null)
            lightningBugManager = GameObject.Find("LightningBugManager").GetComponent<ObjectPoolManager>();

         if(hornetManager == null)
            hornetManager = GameObject.Find("HornetManager").GetComponent<ObjectPoolManager>();

        if(blackWidowManager == null)
            blackWidowManager = GameObject.Find("BlackWidowManager").GetComponent<ObjectPoolManager>();

        StartCoroutine(UpdateDelay());
    }

    public void SetControlObject(GameObject newControlObject)
    {
        controlObject = newControlObject;
        transform.parent.gameObject.GetComponent<EnemyCollision>().collisionDamage = collisionDamage;

		controlObjectCollider = controlObject.GetComponent<Collider>();
		aiObstacleRayFrontOffset = controlObjectCollider.bounds.extents.y / 2.0f;

		controlObjectSyncParams = controlObject.GetComponent<EnemySyncParams>();

		meshRenderer = controlObject.GetComponent<MeshRenderer>();
		StartCoroutine(CheckRenderDistance());

		if (originalGlow == null)
		{
			GameObject lights = transform.parent.Find("pattern").gameObject;
			originalGlow = lights.GetComponent<Renderer>().material;
		}
    }

    // This function is run when the object is spawned
    public void SetPlayer(GameObject temp)
	{
        mySrc 	   = GetComponent<AudioSource>();
        mySrc.clip = fireSnd;

		player = temp;
		playerShip = player.transform.FindChild("Model").gameObject;

		Transform playerCommanderShootAnchor = player.transform.Find("CommanderShootAnchor"); // The CommanderShootAnchor is basically the front point of the ship
		suicidalMinFrontDist 				 = playerCommanderShootAnchor.transform.localPosition.z * 1.5f;

		// Enemies have collisions disabled until they come close to the player
		state = EnemyAIState.SeekPlayer;
		collisionsEnabled = controlObjectCollider.enabled = false;

		controlObject.transform.eulerAngles = new Vector3(controlObject.transform.eulerAngles.x, controlObject.transform.eulerAngles.y, randomZ);
        randomZ = Random.Range(0f,359f);

        if(maxShield > 0)
		{
			shield = maxShield;
			lastShieldCheck = shield;
			StartCoroutine(RechargeShields());
		}

        // Find location to spawn bullets at
        foreach(Transform child in this.transform.parent)
		{
			if(child.gameObject.name.Equals("ShootAnchor"))
			{
				shootAnchor = child.gameObject;
				break;
			}
		}

		StartCoroutine(DrawDelay());
        StartCoroutine(SendAccumulatedScore());
	}

	// Set the waypoints to follow when engaging the player
	public void SetAIWaypoints(List<GameObject> waypoints)
	{
		aiWaypoints = waypoints;
	}

	// Set the parts of the player's ship that can be targeted
	public void SetPlayerShipTargets(List<GameObject> targets)
	{
		this.playerShipTargets = targets;
		currentTarget = targets[Random.Range(0, targets.Count)]; // For now, choose the target ship part at random
	}

	// Make this enemy guard his spawwn location
	public void SetGuarding(int distance)
	{
		state         		 = EnemyAIState.Wait;
		guardTriggerDistance = distance;
	}

    // Send a periodic update of the accumulated score
    IEnumerator SendAccumulatedScore()
    {
        while(true) {
            if(hacked && accumulatedPlayerScore > 0) {
                bool success = tcpServer.SendSpectatorScoreIncrement(controllingPlayerId, accumulatedPlayerScore);
                if(success) {
                    accumulatedPlayerScore = 0;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // Avoids null reference during initial spawning
	IEnumerator DrawDelay()
	{
		yield return new WaitForSeconds(1f);
	}


	// Control shooting based on attributes
	IEnumerator ShootManager()
	{
		// Suicidal enemies don't shoot
		if (isSuicidal && !hacked)
			yield break; // this means "return" in coroutine speak

		if(!shoot && angleGoodForShooting)
		{
			yield return new WaitForSeconds(0.1f);
			shoot = true;
			StartCoroutine(Shoot());
		}
		else
		{
			yield return new WaitForSeconds(shootPeriod * (Random.Range (100-shootPeriodPercentageVariation, 100+shootPeriodPercentageVariation) / 100f));
			shoot = false;
		}

		StartCoroutine(ShootManager());
	}

    // Spawn bullet, use predication, and player sound
	IEnumerator Shoot()
	{
        if(type == EnemyType.LightningBug || type == EnemyType.Termite)
            yield break;
		yield return new WaitForSeconds((1f/ shotsPerSec) + Random.Range (0.01f, 0.1f/shotsPerSec));

		if (empEnabled)
			yield break;

        GameObject obj = bulletManager.RequestObject();
		if (obj == null)
			yield break;

        obj.transform.position = shootAnchor.transform.position;

        GameObject logic = bulletLogicManager.RequestObject();
		if (logic == null)
		{
			bulletManager.RemoveObject(obj.name);
			yield break;
		}

		logic.transform.parent = obj.transform;
		logic.transform.localPosition = Vector3.zero;

        BulletLogic bulletLogic = logic.GetComponent<BulletLogic>();
		BulletMove bulletMove   = obj.GetComponent<BulletMove>();

		bulletMove.Speed = bulletSpeed;
		bulletManager.SetBulletSpeed(obj.name, bulletSpeed);

		Vector3 destination;

		// Prevent an error when the target is destroyed just before it's time for this enemy to shoot
		if (hacked && currentTarget == null)
			yield break;

		// If the enemy is hacked, it should use targetted bullets, otherwise it's hard to hit its target
		// The bullet also needs to collide with the (enemy) target, which enemy bullets don't do by default
		if (hackedAttackTraget != null)
		{
			if (Random.value > accuracy)
            {
                accumulatedPlayerScore += 5; // It will hit. Right, guys?
				bulletMove.SetTarget(currentTarget);
            }
            
			bulletLogic.SetParameters(accuracy, hackedBulletDamage);

			destination = currentTarget.transform.position;
			obj.layer   = LAYER_PLAYER_BULLET;

		}
		else
		{
			bulletLogic.SetParameters(accuracy, bulletDamage);
			destination = currentTarget.transform.position + ((currentPos - prevPos) * (distance / 10f));
			obj.layer   = LAYER_ENEMY_BULLET;
		}

		bulletLogic.SetDestination(destination, false, player, bulletManager, bulletLogicManager, impactManager);

        bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);

        if(randomPitch)
            mySrc.pitch = Random.Range(0.7f, 1.3f);
        if(distance < 250f)
            mySrc.PlayOneShot(fireSnd);

		if(shoot)
            StartCoroutine(Shoot());
	}

	IEnumerator RechargeShields()
	{
		if(lastShieldCheck == shield)
		{
			shield += shieldRechargeRate / 10f;
			lastShieldCheck = shield;
			yield return new WaitForSeconds(0.1f);
			StartCoroutine(RechargeShields());
		}
		else
		{
			lastShieldCheck = shield;
			yield return new WaitForSeconds(shieldDelay);
			StartCoroutine(RechargeShields());
		}
	}

    // Detect collisions with other game objects
	public void collision(float damage, int playerId)
	{
		if (enemyManager == null)
			return;

		if (shield > damage)
			shield -= damage;
		else
		{
			float remDamage = damage - shield;
			shield = 0;

			health -= remDamage;
		}

		if (health <=0 && transform.parent != null) // The null check prevents trying to destroy an object again while it's already being destroyed
		{
			if(playerId != -1)
			{
				// Update player score
				gameState.AddToPlayerScore(playerId, 10);

                // Automatically collect resources from enemy ship
                gameState.AddShipResources(droppedResources);

                if(Random.Range(0,8) == 0)
                    AIVoice.SendCommand(Random.Range(0,4));
			}

			// Destroy Object
            GameObject temp = explosionManager.RequestObject();
            temp.transform.position = transform.position;
            empEnabled = false;
            if(empEffect != null)
                empEffect.SetActive(false);

            explosionManager.EnableClientObject(temp.name, temp.transform.position, temp.transform.rotation, temp.transform.localScale);

            Despawn();
		}
	}

    public void Despawn()
    {
        NotifyDestructionListeners(); // Notify registered listeners that this object has been destroyed
        ResetGlowColour();
        originalGlow = null;

		if (currentWaypoint != null && currentWaypoint.name.Equals(AVOID_WAYPOINT_NAME))
			Destroy(currentWaypoint);

		SetHacked(false, 0, "");
        accumulatedPlayerScore = 0;
		angleGoodForShooting = shoot = false;

		reachedFrontOfPlayer = false;

        string removeName = transform.parent.gameObject.name;
        gameState.RemoveEnemy(controlObject.gameObject);

		if (enemyManager != null)
		{
			enemyManager.DisableClientObject(removeName);
			enemyManager.RemoveObject(removeName);
		}
		else
		{
			enemyManager = FindManager();
			if (enemyManager != null)
			{
				enemyManager.DisableClientObject(removeName);
				enemyManager.RemoveObject(removeName);
			}
			else
				Debug.LogWarning("Could not find manager for enemy " + removeName + " type " + type + " so it was not despawned.");
		}

		enemyManager = null;

        transform.parent = null;
        enemyLogicManager.RemoveObject(gameObject.name);
    }

	/// <summary>
	/// Trues to find the pool manager that owns the controlled enemy.
	/// </summary>
	/// <returns>The manager, or <c>null</c> if no enemy manager owns this enemy.</returns>
	private ObjectPoolManager FindManager()
	{
		GameObject pooling = GameObject.Find("ObjectPooling");
		string[] enemyManagerNames = new string[] { "GnatManager", "FireflyManager", "TermiteManager", "LightningBugManager",
			"HornetManager", "BlackWidowManager" };

		foreach (string name in enemyManagerNames)
		{
			ObjectPoolManager manager = pooling.transform.Find(name).gameObject.GetComponent<ObjectPoolManager>();
			if (manager.Owns(controlObject))
				return manager;
		}

		return null;
	}

	/// <summary>
	/// Determines whether this enemy is hacked.
	/// </summary>
	/// <returns><c>true</c> if the enemy is hacked; otherwise, <c>false</c>.</returns>
	public bool IsHacked()
	{
		return hacked;
	}

    /// <summary>
    /// Sets the hacked attribute of this object
    /// to val
    /// </summary>
    /// <param name="val">The boolean value that hacked should take</param>
    public void SetHacked(bool val, uint playerId, string hackedName)
    {
        hacked 			    = val;
		hackedAttackTraget  = null;
        controllingPlayerId = playerId;

        if(hackedName.Length > 7)
            hackedName = hackedName.Substring(0, 7) + "...";

		controlObjectSyncParams.SetHacked(hacked);

		if (hacked)
		{
			// When an enemy becomes hacked, it starts following the ship
			FollowPlayer();
			state = EnemyAIState.Hacked;

			if (enemyManager != null)
                enemyManager.RpcSetHackedGlow(transform.parent.gameObject.name, hackedName);
            else
            {
                enemyManager = FindManager();
                if (enemyManager != null)
                    enemyManager.RpcSetHackedGlow(transform.parent.gameObject.name, hackedName);
                else
                    Debug.LogWarning("Could not find manager for enemy " + transform.parent.gameObject.name + " type " + type);
            }

			// Give suicidals enemies a temporary bullet manager so they're not completely useless when hacked
			if (isSuicidal)
			{
				wasSuicidalBeforeHack = true;
				isSuicidal 			  = false;
				bulletManager 		  = gnatBulletManager;
				StartCoroutine(ShootManager());
			}

			//Debug.Log("Hacked: " + controlObject.name + " with logic " + this.gameObject.name);
		}
		else
		{
			if (hackedWaypoint != null)
				Destroy(hackedWaypoint);

			if (wasSuicidalBeforeHack)
			{
				isSuicidal = true;
				bulletManager = null;
			}

			state = EnemyAIState.SeekPlayer;
		}
    }

    private void ChangeGlowColour()
    {
        GameObject lights = transform.parent.Find("pattern").gameObject;
        lights.GetComponent<Renderer>().material = hackedMaterial;
        transform.parent.Find("Target").GetComponent<Renderer>().material = hackedTargetMaterial;
    }

    private void ResetGlowColour()
    {
        if (enemyManager != null)
		{
			enemyManager.RpcResetHackedGlow(transform.parent.gameObject.name);
		}
		else
		{
			enemyManager = FindManager();
			if (enemyManager != null)
			{
				enemyManager.RpcResetHackedGlow(transform.parent.gameObject.name);
			}
			else
				Debug.LogWarning("Could not find manager for enemy " + transform.parent.gameObject.name + " type " + type);
		}
        GameObject lights = transform.parent.Find("pattern").gameObject;
        lights.GetComponent<Renderer>().material = originalGlow;
        transform.parent.Find("Target").GetComponent<Renderer>().material = targetMaterial;
    }

    /// <summary>
    /// Moves the enemy to the position (x,z) in its current Y plane.
    /// </summary>
    /// <param name="posX">The X coordinate to move to</param>
    /// <param name="posZ">The Z coordinate to move to</param>
    public void HackedMove (float posX, float posZ)
    {
		// Uncomment this to see waypoints as spheres
		/*if (Debug.isDebugBuild)
	    {
			currentWaypoint = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			currentWaypoint.GetComponent<Renderer>().material.color = Color.blue;
	    }*/

		float currentY 							= hackedWaypoint.transform.localPosition.y;
		currentWaypoint.transform.localPosition = new Vector3(posX, currentY, posZ);
		currentWaypoint 						= hackedWaypoint;

		// If this enemey was previously issued an attack command, clear it
		hackedAttackTraget = null;
    }

    /// <summary>
    /// Makes the enemy attack the specified target
    /// </summary>
    /// <param name="target">The target to attack</param>
    public void HackedAttack(GameObject target)
    {
		hackedAttackTraget = currentWaypoint = currentTarget = target;

		//Debug.Log("Attacking " + hackedAttackTraget.gameObject.name);

		EnemyLogic targetLogic = hackedAttackTraget.GetComponentInChildren<EnemyLogic>();
		if (targetLogic != null)
			targetLogic.RegisterDestructionListener(this);
    }

	/// <summary>
	/// Follows the player ship keeping the same relative position.
	///
	/// Should be used for hacked enemies awaiting orders.
	/// </summary>
	private void FollowPlayer()
	{
		if (hackedWaypoint == null)
		{
			// Uncomment this to see waypoints as spheres
			/*if (Debug.isDebugBuild)
			{
				hackedWaypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				hackedWaypoint.GetComponent<Renderer>().material.color = Color.blue;
				hackedWaypoint.name = HACK_WAYPOINT_NAME;
			}
			else*/
			hackedWaypoint = new GameObject(HACK_WAYPOINT_NAME);
			hackedWaypoint.transform.position = controlObject.transform.position;
			hackedWaypoint.transform.parent   = player.transform;

			// Make the enemy follow the player close to their Y position
			Vector3 localPosition = hackedWaypoint.transform.localPosition;
			localPosition.y = Random.Range(0, hackedFollowMaxY);
			hackedWaypoint.transform.localPosition = localPosition;
		}

		currentWaypoint = hackedWaypoint;

		// Uncomment to see when the hacked enemy starts following again
		//Debug.Log("Hacked " + controlObject.name + " following: " + currentWaypoint.transform.localPosition);
	}

	/// <summary>
	/// Registers a listener to be notified when this object is destroyed.
	/// </summary>
	/// <param name="listener">The listener.</param>
	public void RegisterDestructionListener(IDestructionListener listener)
	{
		destructionListeners.Add(listener);
	}

	/// <summary>
	/// Notifies the registered destruction listeners and clears the list.
	/// </summary>
	private void NotifyDestructionListeners()
	{
		foreach (IDestructionListener listener in destructionListeners)
			listener.OnObjectDestructed(transform.parent.gameObject);

		destructionListeners.Clear();
	}

	/// <summary>
	/// Receives a notification that an object has been destroyed.
	/// </summary>
	/// <param name="destructed">The destructed object.</param>
	public void OnObjectDestructed(GameObject destructed)
	{
		if (destructed == hackedAttackTraget)
		{
			hackedAttackTraget = null;
			//Debug.Log("Target destroyed.");
			FollowPlayer();
		}
	}

	// Class that shows obstacle detection info to be used for avoiding moves
	private class AvoidInfo
	{
		public enum AvoidSide { None, Left, Right }

		public string ObstacleTag { get; private set; } // The tag of the detected obstacle
		public AvoidSide Side { get; private set; }     // The side of the ship that the obstacle is coming up on

		private static AvoidInfo noneInfo = null;

		private static Dictionary<string, Dictionary<AvoidSide, AvoidInfo>> infoMap =
			new Dictionary<string, Dictionary<AvoidSide, AvoidInfo>>();

		/// <summary>
		/// Create an object showing no obstacle info
		/// </summary>
		private AvoidInfo()
		{
			this.ObstacleTag = null;
			this.Side        = AvoidSide.None;
		}

		/// <summary>
		/// Create an object containing info about a collision
		/// </summary>
		private AvoidInfo(string obstacleTag, AvoidSide side)
		{
			this.ObstacleTag = obstacleTag;
			this.Side        = side;
		}

		/// <summary>
		/// Get an object containing no obstacle info.
		/// </summary>
		/// <returns>An object containing no obstacle info.</returns>
		public static AvoidInfo None()
		{
			if (noneInfo == null)
				noneInfo = new AvoidInfo();

			return noneInfo;
		}

		/// <summary>
		/// Check if this object contains no collision info.
		/// </summary>
		/// <returns><c>true</c> if this object contains no collision info.</returns>
		public bool IsNone()
		{
			return this == noneInfo;
		}

		/// <summary>
		/// Gets an object containing the specified collsion info.
		/// </summary>
		/// <param name="obstacleTag">The obstacle's tag.</param>
		/// <param name="side">The side the obstacle is on.</param>
		public static AvoidInfo Get(string obstacleTag, AvoidSide side)
		{
			Dictionary<AvoidSide, AvoidInfo> mapLevel2;
			if (infoMap.TryGetValue(obstacleTag, out mapLevel2))
			{
				AvoidInfo info;
				if (mapLevel2.TryGetValue(side, out info))
					return info;
				else
				{
					info = new AvoidInfo(obstacleTag, side);
					mapLevel2[side] = info;
					return info;
				}
			}
			else
			{
				mapLevel2            = new Dictionary<AvoidSide, AvoidInfo>();
				infoMap[obstacleTag] = mapLevel2;

				AvoidInfo info = new AvoidInfo(obstacleTag, side);
				mapLevel2[side] = info;
				return info;
			}
		}
	}
}

public enum EnemyAIState
{
	SeekPlayer,
	AvoidObstacle,
	EngagePlayer,
	Wait,
	Hacked
}
