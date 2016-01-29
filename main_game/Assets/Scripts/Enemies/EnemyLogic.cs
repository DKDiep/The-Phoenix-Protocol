/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Control enemy ship attributes and AI
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLogic : MonoBehaviour 
{

	[SerializeField] float speed = 15f;
	[SerializeField] float health;
	[SerializeField] float shotsPerSec = 1f;
	[SerializeField] float collisionDamage;
	[SerializeField] float shootPeriod; // How long in seconds the enemy should shoot for when it fires
	[SerializeField] int percentageVariation; // Percentage variation +/- in the length of the shooting period
	[SerializeField] float maxShield; // Max recharging shield level. Set to 0 to disable shields
	[SerializeField] float shieldDelay; // Delay in seconds to wait before recharging shield
	[SerializeField] float shieldRechargeRate; // Units of shield to increase per second
	[SerializeField] bool isSuicidal; // Attempt to crash into player?
	[SerializeField] GameObject bullet;
	[SerializeField] GameObject bulletLogic;
    [SerializeField] GameObject destroyEffect;
    [SerializeField] AudioClip fireSnd;
    AudioSource mySrc;

	public GameObject player;
	bool shoot = false, angleGoodForShooting = false;
	bool rechargeShield;
    public bool draw = false;
	float shield;
    public float distance;
	float lastShieldCheck; // Temp variable allows us to see whether I've taken damage since last checking   
	float randomZ;

	GameObject shootAnchor;
	Vector3 prevPos, currentPos;
	Renderer myRender;
	private GameObject controlObject;

	int state;
	private const int STATE_SEEK_PLAYER    = 0;
	private const int STATE_AVOID_OBSTACLE = 1;
	private const int STATE_COOLDOWN       = 2;
	private const int STATE_ENGAGE_PLAYER  = 3;
	private const int ENGAGE_DISTANCE      = 650;

	private List<GameObject> aiWaypoints;
	private GameObject currentWaypoint               = null;
	private const float AI_WAYPOINT_ROTATION_SPEED   = 1f;
	private const float AI_WAYPOINT_REACHED_DISTANCE = 2.5f;
	private const float AI_SHOOT_MAX_ANGLE           = 50f;
	private float lastYRot;

    public void SetControlObject(GameObject newControlObject)
    {
        controlObject = newControlObject;
        transform.parent.gameObject.GetComponent<EnemyCollision>().collisionDamage = collisionDamage;
    }

    // This function is run when the object is spawned
    public void SetPlayer(GameObject temp)
	{
        mySrc = GetComponent<AudioSource>();
        mySrc.clip = fireSnd;
		player = temp;
		state = STATE_SEEK_PLAYER;
		myRender = transform.parent.gameObject.GetComponent<Renderer>();
        controlObject.transform.eulerAngles = new Vector3(controlObject.transform.eulerAngles.x, controlObject.transform.eulerAngles.y, randomZ);
        randomZ = Random.Range(0f,359f);
		if(maxShield > 0)
		{
			shield = maxShield;
			lastShieldCheck = shield;
			StartCoroutine ("Recharge Shields");
		}

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

		if (Debug.isDebugBuild)
		{
			Color randomColor = new Color (Random.value, Random.value, Random.value, 1.0f);
			foreach (GameObject waypoint in aiWaypoints)
				waypoint.GetComponent<MeshRenderer> ().material.color = randomColor;
		}
	}

	IEnumerator DrawDelay()
	{
		yield return new WaitForSeconds(1f);
		draw = true;
	}
	
	void Update () 
	{
		prevPos    = currentPos;
		currentPos = player.transform.position;
		distance   = Vector3.Distance(transform.position, player.transform.position);

		// Engage player when close enough, otherwise catch up to them
		if (state == STATE_SEEK_PLAYER && distance <= ENGAGE_DISTANCE)
		{
			state = STATE_ENGAGE_PLAYER;
			currentWaypoint = GetNextWaypoint ();
		} 
		else if (state == STATE_ENGAGE_PLAYER && distance > ENGAGE_DISTANCE)
		{
			state = STATE_SEEK_PLAYER;
		} 

		if (state == STATE_ENGAGE_PLAYER)
		{
			MoveTowardsCurrentWaypoint ();

			// Check if the angle is good for shooting
			Vector3 direction    = player.transform.position - controlObject.transform.position;
			float angle          = Vector3.Angle (-controlObject.transform.up, direction);
			angleGoodForShooting = (angle < AI_SHOOT_MAX_ANGLE) || (angle > (180-AI_SHOOT_MAX_ANGLE));
		}
		else // if (state == STATE_SEEK_PLAYER)
		{
			angleGoodForShooting = false;
			MoveTowardsPlayer ();
		}

	}

	// When not engaged, try and get closer to the player
	void MoveTowardsPlayer()
	{
		controlObject.transform.LookAt(player.transform.position);
		controlObject.transform.Translate (controlObject.transform.right*Time.deltaTime * speed);
		controlObject.transform.eulerAngles = new Vector3(controlObject.transform.eulerAngles.x - 90, controlObject.transform.eulerAngles.y,
			controlObject.transform.eulerAngles.z);
	}

	// Get the next engagement waypoint to follow, which should be different from the previous one
	GameObject GetNextWaypoint()
	{
		GameObject nextWaypoint;

		do
		{
			int r = Random.Range (0, aiWaypoints.Count);
			nextWaypoint = aiWaypoints [r];
		} while (nextWaypoint.Equals (currentWaypoint));

		return nextWaypoint;
	}

	// When engaged with the player ship, move between waypoints
	void MoveTowardsCurrentWaypoint()
	{
		
		Vector3 relativePos = currentWaypoint.transform.position - controlObject.transform.position;
		Quaternion rotation = Quaternion.LookRotation (relativePos, Vector3.forward); // TODO: when we get a proper ship, remove the upwards parameter

		// Turn and bank
		controlObject.transform.rotation = Quaternion.Lerp (controlObject.transform.rotation, rotation,
			Time.deltaTime * AI_WAYPOINT_ROTATION_SPEED);
		float yRot = Mathf.Clamp (lastYRot - controlObject.transform.localEulerAngles.y, 0, 3);
		controlObject.transform.Rotate (0, 0, yRot, Space.Self);
		lastYRot = controlObject.transform.localEulerAngles.y;

		controlObject.transform.Translate (0, Time.deltaTime * speed * (-1), 0); // TODO: when we get a proper ship, move it forwards

		if (Vector3.Distance (controlObject.transform.position, currentWaypoint.transform.position) < AI_WAYPOINT_REACHED_DISTANCE)
			currentWaypoint = GetNextWaypoint ();
	}
	
	IEnumerator ShootManager()
	{
		if(!shoot && angleGoodForShooting)
		{
			yield return new WaitForSeconds(0.1f);
			shoot = true;
			StartCoroutine ("Shoot");
		}
		else
		{
			yield return new WaitForSeconds(shootPeriod * (Random.Range (100-percentageVariation, 100+percentageVariation) / 100f));
			shoot = false;
		}

		StartCoroutine ("ShootManager");
	}
	
	IEnumerator Shoot()
	{
		yield return new WaitForSeconds((1f/ shotsPerSec) + Random.Range (0.01f, 0.1f/shotsPerSec));
		GameObject obj = Instantiate (bullet, shootAnchor.transform.position, Quaternion.identity) as GameObject;
		GameObject logic = Instantiate (bulletLogic, shootAnchor.transform.position, Quaternion.identity) as GameObject;
		logic.transform.parent = obj.transform;
		logic.transform.localPosition = Vector3.zero;

		Vector3 destination = player.transform.position + ((currentPos - prevPos) * (distance / 10f));

		logic.GetComponent<BulletLogic>().SetDestination (destination);
		ServerManager.NetworkSpawn(obj);
        mySrc.Play();
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
	
	public void collision(float damage)
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
		else
		{
            GameObject temp = Instantiate(destroyEffect, transform.position, transform.rotation) as GameObject;
            ServerManager.NetworkSpawn(temp);
			Destroy(transform.parent.gameObject);
		}

		//Debug.Log ("Glom fighter was hit, has " + shield + " shield and " + health + " health");
	}
}
