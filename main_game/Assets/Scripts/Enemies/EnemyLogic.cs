/*
    Control enemy ship attributes and AI
		
    Relevant Documentation:
	  * Enemy AI:    https://bitbucket.org/pyrolite/game/wiki/Enemy%20AI
      * Enemy Types: https://bitbucket.org/pyrolite/game/wiki/Enemy%20Types
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLogic : MonoBehaviour, DestructibleObject, DestructionListener
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private int shootPeriodPercentageVariation; // Percentage variation +/- in the length of the shooting period
	private float shieldDelay; 					// Delay in seconds to wait before recharging shield
	private float shieldRechargeRate; 			// Units of shield to increase per second
	private AudioClip fireSnd;
	private bool randomPitch;
    private GameObject empEffect;

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
	internal EnemyType type;

	private AudioSource mySrc;
	private GameState gameState;
	private GameObject player;

	private bool shoot = false, angleGoodForShooting = false;
	private bool rechargeShield;
    
	private bool hacked = false;
	private GameObject hackedAttackTraget = null;

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

	// This is the amount of resources dropped by the enemy when killed. It is calculated based on the enemy's max health and shield
	private int droppedResources;
	private const int DROP_RESOURCE_RANGE = 100;

	// The current state of the ship's AI
	internal EnemyAIState state;

	// Waypoints are used to move around the player when close enough
	private List<GameObject> aiWaypoints;
	private GameObject currentWaypoint               = null;
	private const float AI_WAYPOINT_ROTATION_SPEED   = 1.3f;   // Turning speed when following waypoints
	private const float AI_WAYPOINT_REACHED_DISTANCE = 20f;    // Distance when a waypoint is considered reached
	private const float AI_SHOOT_MAX_ANGLE           = 50f;    // Maximum angle with the player when shooting is possible
	private float lastYRot;
	private bool reachedFrontOfPlayer = false;

	private List<GameObject> playerShipTargets;
	private GameObject currentTarget;

	// Parameters for raycasting obstacle detection
	// Two rays are shot forwards, on the left and right side of the ship to detect incoming obstacles
	private const int AI_OBSTACLE_RAY_FRONT_LENGTH = 30;
	private const string AI_OBSTACLE_TAG_DEBRIS    = "Debris";
	private const string AI_OBSTACLE_TAG_ENEMY     = "EnemyShip";
	private const int AI_OBSTACLE_AVOID_ROTATION   = 45;
	private int previousAvoidDirection             = 0;
	private int avoidDirection;
	private float aiObstacleRayFrontOffset;

    private ObjectPoolManager bulletManager;
    private ObjectPoolManager gnatBulletManager;
    private ObjectPoolManager fireflyBulletManager;
    private ObjectPoolManager hornetBulletManager;
    private ObjectPoolManager blackWidowBulletManager;
    private ObjectPoolManager logicManager;
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

	private Vector3 guardLocation = Vector3.zero; // If this enemy is an outpost guard, this will be set to a non-zero value
	private const int AI_GUARD_TURN_BACK_DISTANCE = 500; // The distance at which guards stop engaging the player and turn back to the outpost
	private const int AI_GUARD_PROTECT_DISTANCE   = 100; // The distance from the outpost at which to stop and wait when returning to guard
	private int guardTriggerDistance = 100; // The distance at which a player triggers the guard to attack

    private Renderer meshRenderer;

	private List<DestructionListener> destructionListeners;

	void Start ()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		GameObject server = settings.GameManager;
		gameState         = server.GetComponent<GameState>();

        gnatBulletManager     = GameObject.Find("GnatBulletManager").GetComponent<ObjectPoolManager>();
        fireflyBulletManager     = GameObject.Find("FireflyBulletManager").GetComponent<ObjectPoolManager>();
        hornetBulletManager = GameObject.Find("HornetBulletManager").GetComponent<ObjectPoolManager>();
        blackWidowBulletManager = GameObject.Find("BlackWidowBulletManager").GetComponent<ObjectPoolManager>();
        logicManager      = GameObject.Find("EnemyBulletLogicManager").GetComponent<ObjectPoolManager>();
        impactManager     = GameObject.Find("BulletImpactManager").GetComponent<ObjectPoolManager>();
        explosionManager  = GameObject.Find("EnemyExplosionManager").GetComponent<ObjectPoolManager>();
        enemyLogicManager = GameObject.Find("EnemyLogicManager").GetComponent<ObjectPoolManager>();

		destructionListeners = new List<DestructionListener>();
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
		speedUpdateDelay		   	   = settings.EnemySuicidalSpeedUpdateInterval;
		suicidalExtraSpeed 			   = settings.EnemySuicidalExtraSpeed;
	}

    IEnumerator UpdateDelay()
    {
        yield return new WaitForSeconds(3f);

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
			StartCoroutine("MatchPlayerSpeed");
        StartCoroutine("UpdateTransform");
    }

    IEnumerator UpdateTransform()
    {
        //Debug.Log("My type is " + type + " and manager " + enemyManager.gameObject.name);
        enemyManager.UpdateTransform(controlObject.transform.position, controlObject.transform.rotation, controlObject.name);
        yield return new WaitForSeconds(0.1f);
        StartCoroutine("UpdateTransform");
    }

	/// <summary>
	/// Keeps the enemy always moving faster than the player.
	/// 
	/// This should only be used on suicidal enemies.
	/// </summary>
	IEnumerator MatchPlayerSpeed()
	{
		float playerSpeed = gameState.GetShipSpeed();
		if (speed < playerSpeed + suicidalExtraSpeed)
			speed = playerSpeed + suicidalExtraSpeed;

		yield return new WaitForSeconds(speedUpdateDelay);
		StartCoroutine("MatchPlayerSpeed");
	}

    void OnEnable()
    {
        // Decide the resource drop for this ship to be within DROP_RESOURCE_RANGE range of its max health + shield
        if(gnatManager == null)
            gnatManager      = GameObject.Find("GnatManager").GetComponent<ObjectPoolManager>();

        if(fireflyManager == null)
            fireflyManager      = GameObject.Find("FireflyManager").GetComponent<ObjectPoolManager>();

        if(termiteManager == null)
            termiteManager      = GameObject.Find("TermiteManager").GetComponent<ObjectPoolManager>();

        if(lightningBugManager == null)
            lightningBugManager      = GameObject.Find("LightningBugManager").GetComponent<ObjectPoolManager>();

         if(hornetManager == null)
            hornetManager      = GameObject.Find("HornetManager").GetComponent<ObjectPoolManager>();

        if(blackWidowManager == null)
            blackWidowManager      = GameObject.Find("BlackWidowManager").GetComponent<ObjectPoolManager>();

        StartCoroutine("UpdateDelay");
        droppedResources = System.Convert.ToInt32(maxHealth + maxShield + Random.Range (0, DROP_RESOURCE_RANGE)); 
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }


    public void SetControlObject(GameObject newControlObject)
    {
        controlObject = newControlObject;
        transform.parent.gameObject.GetComponent<EnemyCollision>().collisionDamage = collisionDamage;
		aiObstacleRayFrontOffset = controlObject.GetComponent<Collider>().bounds.extents.y / 2.0f;
        meshRenderer = controlObject.GetComponent<MeshRenderer>();
    }

    // This function is run when the object is spawned
    public void SetPlayer(GameObject temp)
	{
        mySrc 	   = GetComponent<AudioSource>();
        mySrc.clip = fireSnd;

		player = temp;

		Transform playerCommanderShootAnchor = player.transform.Find("CommanderShootAnchor"); // The CommanderShootAnchor is basically the front point of the ship
		suicidalMinFrontDist 				 = playerCommanderShootAnchor.transform.localPosition.z;

		state = EnemyAIState.SeekPlayer;
        controlObject.transform.eulerAngles = new Vector3(controlObject.transform.eulerAngles.x, controlObject.transform.eulerAngles.y, randomZ);
        randomZ = Random.Range(0f,359f);

        if(maxShield > 0)
		{
			shield = maxShield;
			lastShieldCheck = shield;
			StartCoroutine ("RechargeShields");
		}

        // Find location to spawn bullets at
        foreach(Transform child in this.transform.parent)
		{
			if(child.gameObject.name.Equals("ShootAnchor")) shootAnchor = child.gameObject;
		}

		StartCoroutine ("ShootManager");
		StartCoroutine("DrawDelay");
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

	// Make this enemy guard location
	public void SetGuarding(Vector3 location, int distance)
	{
		state         		 = EnemyAIState.Wait;
		guardLocation 		 = location;
		guardTriggerDistance = distance;
	}

    // Avoids null reference during initial spawning
	IEnumerator DrawDelay()
	{
		yield return new WaitForSeconds(1f);
	}

	void Update ()
	{
		// If this enemy has gone mad, its sole purpose is to crash into the player
		if (isSuicidal)
		{

			MoveTowardsPlayer ();
			return;
		}

		prevPos    = currentPos;
		currentPos = player.transform.position;
		distance   = Vector3.Distance(transform.position, player.transform.position);

        if(distance > 1000)
            meshRenderer.enabled = false;
        else
            meshRenderer.enabled = true;

		// Check if about to collide with something
		// Ignore the outpost if returning towards its location, because the guard distance might be smaller than the avoid distance.
		// We will not hit the outpost as long as the guard distance accounts for the the outpost's size
		AvoidInfo obstacleInfo = CheckObstacleAhead();
		if (!obstacleInfo.IsNone() && (state != EnemyAIState.ReturnToGuardLocation || !obstacleInfo.ObstacleTag.Equals("Outpost")))
		{
			// If already avoiding an obsctale or returning to an outpost, clear the previous waypoint before creating another one
			if (state == EnemyAIState.AvoidObstacle || state == EnemyAIState.ReturnToGuardLocation)
				Destroy(currentWaypoint);

			// If about to collide with an enemy, go towards a different waypoint - it's very likely the other guy will not go the same way
			// Otherwise, temporarily change direction
			if (obstacleInfo.ObstacleTag.Equals(AI_OBSTACLE_TAG_ENEMY))
			{
				state                  = EnemyAIState.EngagePlayer;
				previousAvoidDirection = 0;
				currentWaypoint        = GetNextWaypoint();
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
				GameObject avoidWaypoint         = new GameObject ();
				avoidWaypoint.name               = "AvoidWaypoint";
				avoidWaypoint.transform.position = controlObject.transform.position;
				avoidWaypoint.transform.rotation = controlObject.transform.rotation;
				avoidWaypoint.transform.Rotate (0, avoidDirection * AI_OBSTACLE_AVOID_ROTATION, 0);
				avoidWaypoint.transform.Translate (Vector3.forward * AI_OBSTACLE_RAY_FRONT_LENGTH);
				currentWaypoint = avoidWaypoint;

				// Uncomment this to visualise the waypoint
				// If there is more than one enemy, this probably doesn't help much
				/*GameObject visibleWaypoint = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				visibleWaypoint.transform.position   = currentWaypoint.transform.position;
				visibleWaypoint.transform.localScale = visibleWaypoint.transform.localScale * 5;*/
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
				state                  = EnemyAIState.SeekPlayer;
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
				}
				else
					MoveTowardsPlayer();
				
				return;
			}

			// Hacked enemies can move to a location or attack another enemy
			// When attacking another enemy, try to get behind him so we get a better shot
			if (hacked)
			{
				if (currentWaypoint != null)
				{
					bool reached = MoveTowardsCurrentWaypoint();

					// When a hacked enemy reaches its destination, it stops moving, unless it's following an enemy
					if (reached && hackedAttackTraget == null)
						currentWaypoint = null;
				}

				return; 
			}

			// If this enemy is a guard and is too far from its guarding location, turn back towards it
			if (guardLocation != Vector3.zero && state != EnemyAIState.ReturnToGuardLocation &&
				Vector3.Distance(controlObject.transform.position, guardLocation) >= AI_GUARD_TURN_BACK_DISTANCE)
			{
				state                             = EnemyAIState.ReturnToGuardLocation;
				GameObject returnWaypoint         = new GameObject ();
				returnWaypoint.name               = "GuardReturnWaypoint";
				returnWaypoint.transform.position = guardLocation;
				currentWaypoint                   = returnWaypoint;
			}
			// Engage player when close enough, otherwise catch up to them
			else if ((state == EnemyAIState.SeekPlayer && distance <= engageDistance) ||
				(state == EnemyAIState.Wait && distance <= guardTriggerDistance))
			{
				currentWaypoint = GetNextWaypoint();
				state = EnemyAIState.EngagePlayer;
			}
			else if (state == EnemyAIState.EngagePlayer && distance > engageDistance)
			{
				state = EnemyAIState.SeekPlayer;
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
			else if (state == EnemyAIState.ReturnToGuardLocation)
			{
				MoveTowardsCurrentWaypoint();
				if (Vector3.Distance(controlObject.transform.position, guardLocation) < AI_GUARD_PROTECT_DISTANCE)
				{
					state = EnemyAIState.Wait;
					Destroy(currentWaypoint);
				}
			}
			// if (state == EnemyAIState.Wait) do nothing
		}
	}

    public IEnumerator EMPEffect()
    {
        empEffect = transform.parent.Find("EMPEffect").gameObject;
        float originalSpeed = speed;
        speed = 0;
        empEffect.SetActive(true);
        yield return new WaitForSeconds(settings.empDuration);
        empEffect.SetActive(false);
        speed = originalSpeed;
    }

	// When not engaged, try and get closer to the player
	private void MoveTowardsPlayer()
	{
		controlObject.transform.LookAt(player.transform.position);
		controlObject.transform.Translate (controlObject.transform.forward * Time.deltaTime * speed);
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

			Vector3 waypointRelativeToPlayer = player.transform.InverseTransformPoint(nextWaypoint.transform.position);
			if (isSuicidal && waypointRelativeToPlayer.z < suicidalMinFrontDist)
				getAnother = true;
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
			Time.deltaTime * AI_WAYPOINT_ROTATION_SPEED);
		float yRot = Mathf.Clamp (lastYRot - controlObject.transform.localEulerAngles.y, 0, 3);
		controlObject.transform.Rotate (0, 0, yRot, Space.Self);
		lastYRot = controlObject.transform.localEulerAngles.y;

		controlObject.transform.Translate (Vector3.forward * Time.deltaTime * speed);

		float distanceToWaypoint = Vector3.Distance(controlObject.transform.position, currentWaypoint.transform.position);
		if (distanceToWaypoint < AI_WAYPOINT_REACHED_DISTANCE)
		{
			// If the reached waypoint is an avoid or a hacked move waypoint, it is not needed any more
			if (state == EnemyAIState.AvoidObstacle || (hacked && hackedAttackTraget == null))
				Destroy(currentWaypoint);
			
			// If this enemy is hacked to attack a target, never stop following that target
			if (hackedAttackTraget != null)
				currentWaypoint = hackedAttackTraget;
			else
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
		hitLeft = Physics.Raycast (objectTransform.position - aiObstacleRayFrontOffset * objectTransform.right, objectTransform.forward,
			out hitInfoLeft, AI_OBSTACLE_RAY_FRONT_LENGTH);
		hitRight = Physics.Raycast (objectTransform.position + aiObstacleRayFrontOffset * objectTransform.right, objectTransform.forward,
			out hitInfoRight, AI_OBSTACLE_RAY_FRONT_LENGTH);

		// If an obstacle is found, return its tag
		// Uncomment to show a ray when a collision is detected
		if (hitLeft)
		{
			/*Debug.DrawRay (objectTransform.position - aiObstacleRayFrontOffset * objectTransform.right,
				AI_OBSTACLE_RAY_FRONT_LENGTH * objectTransform.forward, Color.magenta, 3, false);*/
			return new AvoidInfo(hitInfoLeft.collider.gameObject.tag, AvoidInfo.AvoidSide.Left);
		}
		else if (hitRight)
		{
			/*Debug.DrawRay(objectTransform.position + aiObstacleRayFrontOffset*objectTransform.right,
				AI_OBSTACLE_RAY_FRONT_LENGTH*objectTransform.forward, Color.magenta, 3, false);*/
			return new AvoidInfo(hitInfoRight.collider.gameObject.tag, AvoidInfo.AvoidSide.Right);
		}
		else
		{
			// Uncomment to debug raycasting parameters
			/*Debug.DrawRay(objectTransform.position - aiObstacleRayFrontOffset*objectTransform.right,
				AI_OBSTACLE_RAY_FRONT_LENGTH*objectTransform.forward, Color.green, 0, false);
			Debug.DrawRay(objectTransform.position + aiObstacleRayFrontOffset*objectTransform.right,
				AI_OBSTACLE_RAY_FRONT_LENGTH*objectTransform.forward, Color.green, 0, false);*/
			/*Debug.DrawRay (objectTransform.position + AI_OBSTACLE_RAY_BACK_OFFSET*objectTransform.up,
				-AI_OBSTACLE_RAY_BACK_LENGTH*objectTransform.right, Color.yellow, 0, false);
			Debug.DrawRay (objectTransform.position + AI_OBSTACLE_RAY_BACK_OFFSET*objectTransform.up,
				+AI_OBSTACLE_RAY_BACK_LENGTH*objectTransform.right, Color.yellow, 0, false);*/

			return AvoidInfo.None();
		}
	}

	// Control shooting based on attributes
	IEnumerator ShootManager()
	{
		// Suicidal enemies don't shoot
		if (isSuicidal)
			yield break; // this means "return" in coroutine speak
		
		if(!shoot && angleGoodForShooting)
		{
			yield return new WaitForSeconds(0.1f);
			shoot = true;
			StartCoroutine ("Shoot");
		}
		else
		{
			yield return new WaitForSeconds(shootPeriod * (Random.Range (100-shootPeriodPercentageVariation, 100+shootPeriodPercentageVariation) / 100f));
			shoot = false;
		}

		StartCoroutine ("ShootManager");
	}

    // Spawn bullet, use predication, and player sound
	IEnumerator Shoot()
	{
		yield return new WaitForSeconds((1f/ shotsPerSec) + Random.Range (0.01f, 0.1f/shotsPerSec));

        GameObject obj = bulletManager.RequestObject();
        obj.transform.position = shootAnchor.transform.position;

        GameObject logic = logicManager.RequestObject();
		logic.transform.parent = obj.transform;
		logic.transform.localPosition = Vector3.zero;

        BulletLogic bulletLogic = logic.GetComponent<BulletLogic>();
		BulletMove bulletMove   = obj.GetComponent<BulletMove>();

		float bulletSpeed = 0f;
		if (type == EnemyType.Gnat)
		{
			bulletSpeed = 600f;
			bulletLogic.SetParameters(0.3f, 1f);
		}
		else if (type == EnemyType.Firefly)
		{
			bulletSpeed = 600f;
			bulletLogic.SetParameters(0.15f, 2f);
		}
         else if(type == EnemyType.Hornet)
		{
			bulletSpeed = 800f;
			bulletLogic.SetParameters(0.05f, 5f);
		}
         else if(type == EnemyType.BlackWidow)
		{
			bulletSpeed = 800f;
			bulletLogic.SetParameters(0.05f, 10f);
		}
		bulletMove.Speed = bulletSpeed;
		bulletManager.SetBulletSpeed(obj.name, bulletSpeed);

		Vector3 destination = currentTarget.transform.position + ((currentPos - prevPos) * (distance / 10f));

		bulletLogic.SetDestination (destination, false, player, bulletManager, logicManager, impactManager);

        bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);

        if(randomPitch) mySrc.pitch = Random.Range(0.7f, 1.3f);
        if(distance < 300f) mySrc.PlayOneShot(fireSnd);
		if(shoot) StartCoroutine ("Shoot");
	}

	IEnumerator RechargeShields()
	{
		if(lastShieldCheck == shield)
		{
			shield += shieldRechargeRate / 10f;
			lastShieldCheck = shield;
			yield return new WaitForSeconds(0.1f);
			StartCoroutine ("RechargeShields");
		}
		else
		{
			lastShieldCheck = shield;
			yield return new WaitForSeconds(shieldDelay);
			StartCoroutine ("RechargeShields");
		}
	}

    // Detect collisions with other game objects
	public void collision(float damage, int playerId)
	{
        if(enemyManager != null)
        {
    		if (shield > damage)
    		{
    			shield -= damage;
    		}
    		else if (shield > 0)
    		{
    			float remDamage = damage - shield;
    			shield = 0;

    			health -= remDamage;
    		}
    		else if(health > damage)
    		{
    			health -= damage;
    		}
    		else if (transform.parent != null) // The null check prevents trying to destroy an object again while it's already being destroyed
    		{
    			if(playerId != -1)
    			{
    				// Update player score
    				gameState.AddToPlayerScore(playerId, 10);
    			}

				NotifyDestructionListeners(); // Notify registered listeners that this object has been destroyed

                string removeName = transform.parent.gameObject.name;

    			// Automatically collect resources from enemy ship
    			gameState.AddShipResources(droppedResources);

    			// Destroy Object
                GameObject temp = explosionManager.RequestObject();
                temp.transform.position = transform.position;

                explosionManager.EnableClientObject(temp.name, temp.transform.position, temp.transform.rotation, temp.transform.localScale);

                //GameObject temp = Instantiate(destroyEffect, transform.position, transform.rotation) as GameObject;

    			gameState.RemoveEnemy (controlObject.gameObject);
                //GetComponent<bl_MiniMapItem>().DestroyItem();
               
                transform.parent = null;
                enemyManager.DisableClientObject(removeName);
                enemyManager.RemoveObject(removeName);
                enemyLogicManager.RemoveObject(gameObject.name);
    		}
        }
	}

    /// <summary>
    /// Sets the hacked attribute of this object
    /// to val
    /// </summary>
    /// <param name="val">The boolean value that hacked should take</param>
    public void setHacked(bool val)
    {
        hacked 			   = val;
		hackedAttackTraget = null;

		// When an enemy becomes hacked, it stops moving and waits for orders
		if (hacked)
			currentWaypoint = null;
    }

    /// <summary>
    /// Moves the enemy to the position (x,z)
    /// </summary>
    /// <param name="posX">The X coordinate to move to</param>
    /// <param name="posZ">The Z coordinate to move to</param>
    public void HackedMove (float posX, float posZ)
    {
		// Uncomment this to see waypoints as spheres
		/*if (Debug.isDebugBuild)
	    {
			waypoint = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			waypoint.GetComponent<Renderer>().material.color = Color.blue;
	    }
		else*/
		currentWaypoint = new GameObject("HackWaypoint");
		currentWaypoint.transform.position = new Vector3(posX, controlObject.transform.transform.position.y, posZ);
    }

    /// <summary>
    /// Makes the enemy attack the specified target
    /// </summary>
    /// <param name="target">The target to attack</param>
    public void HackedAttack(GameObject target)
    {
		hackedAttackTraget = currentWaypoint = currentTarget = target;

		EnemyLogic targetLogic = hackedAttackTraget.GetComponentInChildren<EnemyLogic>();
		if (targetLogic != null)
			targetLogic.RegisterDestructionListener(this);
    }

	/// <summary>
	/// Registers a listener to be notified when this object is destroyed.
	/// </summary>
	/// <param name="listener">The listener.</param>
	public void RegisterDestructionListener(DestructionListener listener)
	{
		destructionListeners.Add(listener);
	}

	/// <summary>
	/// Notifies the registered destruction listeners and clears the list.
	/// </summary>
	private void NotifyDestructionListeners()
	{
		foreach (DestructionListener listener in destructionListeners)
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
			hackedAttackTraget = currentWaypoint = null;
	}

	// Class that shows obstacle detection info to be used for avoiding moves
	private class AvoidInfo
	{
		public enum AvoidSide { None, Left, Right }

		public string ObstacleTag { get; private set; } // The tag of the detected obstacle
		public AvoidSide Side { get; private set; }     // The side of the ship that the obstacle is coming up on

		private static AvoidInfo noneInfo = null;

		// Create an object showing no obstacle info
		private AvoidInfo()
		{
			this.ObstacleTag = null;
			this.Side        = AvoidSide.None;
		}

		// Create an object containing info about a collision
		public AvoidInfo(string obstacleTag, AvoidSide side)
		{
			this.ObstacleTag = obstacleTag;
			this.Side        = side;
		}
			
		// Get an object containing no obstacle info
		public static AvoidInfo None()
		{
			if (noneInfo == null)
				noneInfo = new AvoidInfo();

			return noneInfo;
		}

		// Check if this object contains no collision info
		public bool IsNone()
		{
			return this == noneInfo;
		}
	}
}

public enum EnemyAIState
{
	SeekPlayer,
	AvoidObstacle,
	EngagePlayer,
	Wait,
	ReturnToGuardLocation
}
