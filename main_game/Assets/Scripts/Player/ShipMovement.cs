﻿/*
    Control ship flying
*/

using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float turnSpeed;
	private float slowDown;
	private float shieldDelay; // Delay in seconds to wait before recharging shield

	private bool rechargeShield;
	private float lastShieldCheck;    // Temp variable allows us to see whether I've taken damage since last checking
	private GameObject controlObject;
	private GameState gameState;
	private WiiRemoteManager wii;

	private float pitchVelocity = 0f;
	private float rollVelocity = 0f;
	private float pitchOld;
	private float rollOld;
	private float slowTime = 0f;
	private float slowTime2 = 0f;
	private bool left, right, up, down;
    private float sideRoll = 0f;
    private float sideRollOld = 0f;
	private DamageEffects myDamage;
	private Camera mainCamera;

	private Coroutine rechargeShieldsCoroutine;

    // Initialise object
    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		Reset();

		GameObject server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();

		GameObject remoteManager = GameObject.Find("WiiRemoteManager");
		wii = remoteManager.GetComponent<WiiRemoteManager>();

    	controlObject = transform.parent.gameObject;
		lastShieldCheck = gameState.GetShipShield();
		rechargeShieldsCoroutine = StartCoroutine(RechargeShields());
        gameState.myShield = GameObject.Find("Shield(Clone)").GetComponent<ShieldEffects>();

		mainCamera = Camera.main;
    }

    public void Reset()
    {
        LoadSettings();
        pitchVelocity = 0f;
        rollVelocity = 0f;
        pitchOld = 0f;
        rollOld = 0f;
        slowTime = 0f;
        slowTime2 = 0f;
        sideRoll = 0f;
        sideRollOld = 0f;
}

private void LoadSettings()
	{
		turnSpeed = settings.PlayerShipTurnSpeed;
		slowDown = settings.PlayerShipSlowDown;
		shieldDelay = settings.PlayerShipShieldDelay;
	}

    public void LightningBugEffect()
    {
        myDamage.DistortionEffect();
    }


	IEnumerator RechargeShields()
	{
		if(lastShieldCheck == gameState.GetShipShield()) // Ensure the player hasn't been hit
		{
			gameState.RechargeShield();
			lastShieldCheck = gameState.GetShipShield();
			yield return new WaitForSeconds(0.1f);
			if (gameState.GetShipShield() < gameState.GetShipMaxShields())
				rechargeShieldsCoroutine = StartCoroutine(RechargeShields());
		}
		else
		{
			lastShieldCheck = gameState.GetShipShield();
			yield return new WaitForSeconds(shieldDelay);
			rechargeShieldsCoroutine = StartCoroutine(RechargeShields());
		}
	}

	void Update () 
	{

        if (gameState.Status != GameState.GameStatus.Started)
            return;

		float maxTurnSpeed = gameState.GetShipMaxTurnSpeed();

        float joyH = Input.GetAxis("Horizontal"), joyV = Input.GetAxis("Vertical");
        float speedMultiplier = Input.GetAxis("ShipSpeed");

        // We need to map the speed multiplier from -1 to 1, into the range 0 to 1
        speedMultiplier += 1;
        speedMultiplier /= 2;

        // Make sure the ship doesn't stop by making the minimum multiplier 0.1
        speedMultiplier = speedMultiplier == 0 ? 0.1f : speedMultiplier;

        // Detect key presses, ensure velocity is less than some maximum, ensure the angle is constrained between some limits to avoid the player flying backwards
		bool canPitchUp = pitchVelocity < maxTurnSpeed;
		bool canPitchDown = pitchVelocity > maxTurnSpeed * (-1f);


        //controlObject.transform.eulerAngles = new Vector3(controlObject.transform.rotation.eulerAngles.x, controlObject.transform.rotation.eulerAngles.y, controlObject.transform.rotation.eulerAngles.z - sideRoll);

        // Control movement. slowTime used to make ship smoothly decrease velocity when key unpressed
        if (Input.GetKey (KeyCode.W) && canPitchUp)
		{
			pitchVelocity +=  (Time.deltaTime * turnSpeed);
			slowTime = 0f;
			pitchOld = pitchVelocity;
		}
		else if(Input.GetKey (KeyCode.S) && canPitchDown)
		{
			pitchVelocity -=  (Time.deltaTime * turnSpeed);
			slowTime = 0f;
			pitchOld = pitchVelocity;
		}
        else if ((joyV < 0 && canPitchDown) || (joyV > 0 && canPitchUp)) // If keys aren't used, check the joystick
        {
            pitchVelocity += joyV * Time.deltaTime * turnSpeed;
			slowTime = 0f;
			pitchOld = pitchVelocity;
        }
		else
		{
			pitchVelocity = Mathf.Lerp(pitchOld, 0f, slowTime);
			slowTime += slowDown * Time.deltaTime;
		}

        bool canRollRight = rollVelocity < maxTurnSpeed;
        bool canRollLeft = rollVelocity > maxTurnSpeed * (-1f);

        if (Input.GetKey (KeyCode.D))
		{
            if(sideRoll < 10f)
            {
                sideRoll += Time.deltaTime * 6f;
            }

            slowTime2 = 0f;

            sideRollOld = sideRoll;
            if(canRollRight)
            {
    			rollVelocity += Time.deltaTime * turnSpeed;
    			rollOld = rollVelocity;
            }
		}
		else if(Input.GetKey (KeyCode.A))
		{
            if(sideRoll > -10f)
            {
                sideRoll -= Time.deltaTime * 6f;
            }
            sideRollOld = sideRoll;
            slowTime2 = 0f;

            if(canRollLeft)
            {
                rollVelocity -= Time.deltaTime * turnSpeed;
                rollOld = rollVelocity;
            }
		}
        else if ((joyH < 0 && canRollLeft) || (joyH > 0 && canRollRight))
        {
            rollVelocity += joyH * Time.deltaTime * turnSpeed;
            if(sideRoll < 10f && sideRoll > -10f)
                sideRoll += joyH * Time.deltaTime * 6f;

            sideRollOld = sideRoll;
			slowTime2 = 0f;
			rollOld = rollVelocity;
        }
		else
		{
			rollVelocity = Mathf.Lerp(rollOld, 0f, slowTime2);
            sideRoll = Mathf.Lerp(sideRollOld, 0f, slowTime2);
			slowTime2 += slowDown * Time.deltaTime;
		}

        // Move parent object
        if (controlObject != null)
        {
            controlObject.transform.Rotate(Vector3.right * pitchVelocity * Time.deltaTime * turnSpeed);
            controlObject.transform.Rotate(Vector3.up * rollVelocity * Time.deltaTime * turnSpeed);
			controlObject.transform.Translate(Vector3.forward * gameState.GetShipSpeed() * speedMultiplier * Time.deltaTime);
        }

            //Debug.Log(sideRoll);
        //controlObject.transform.eulerAngles = new Vector3(controlObject.transform.rotation.eulerAngles.x, controlObject.transform.rotation.eulerAngles.y, controlObject.transform.rotation.eulerAngles.z + sideRoll);
	
	}

    public void Death()
    {
        GameObject explosion = Instantiate(Resources.Load("Prefabs/OutpostExplode", typeof(GameObject))) as GameObject;
        explosion.transform.position = gameState.PlayerShip.transform.position;
        explosion.transform.Translate(transform.forward * 5f);
        explosion.SetActive(true);
        ServerManager.NetworkSpawn(explosion);
    }
       
    /// <summary>
    /// Calculate direction of hit and decrement shields or health.
    /// </summary>
    /// <param name="damage">The damage value.</param>
    /// <param name="yRot">The y rotation of the bullet's direction.</param>
    /// <param name="component">The component that was hit.</param>
    public void collision(float damage, float yRot, ComponentType component)
	{
		if(wii != null) wii.RumbleAll(5);

		yRot += transform.eulerAngles.y - 180f;

		if(yRot < 0f) yRot = yRot + (360f * (int) ((yRot / 360f) + 1));
        else if(yRot > 360f) yRot = yRot - (360f * (int) (yRot / 360f));

		if(yRot < 30f || yRot > 330f)
		{
			up = true;
			down = false;
			left = false;
			right = false;
		}
		else if (yRot > 30f && yRot < 60f)
		{
			up = true;
			right = false;
			down = false;
			left = true;
		}
		else if (yRot < 330f && yRot > 300f)
		{
			up = true;
			right = true;
			down = false;
			left = false;
		}
		else if (yRot > 60f && yRot < 120f)
		{
			up = false;
			right = false;
			down = false;
			left = true;
		}
		else if (yRot < 300f && yRot > 270f)
		{
			up = false;
			right = true;
			down = false;
			left = false;
		}
		else if (yRot > 120f && yRot < 150f)
		{
			up = false;
			right = false;
			down = true;
			left = true;
		}
		else if (yRot < 270f && yRot > 240f)
		{
			up = false;
			right = true;
			down = true;
			left = false;
		}
		else if (yRot > 150f && yRot < 180f)
		{
			up = false;
			right = false;
			down = true;
			left = false;
		}
		else if (yRot < 240f && yRot > 210f)
		{
			up = false;
			right = false;
			down = true;
			left = false;
		}

        if(myDamage == null) 
            myDamage = mainCamera.gameObject.GetComponent<DamageEffects>();

        // Check to see if the hull is hit, otherwise damage component
		if (component == ComponentType.None || component == ComponentType.ResourceStorage)
            gameState.DamageShip(damage);
        else
            gameState.DamageComponent(component, damage);

		// Reset the shield recharge delay
		StopCoroutine(rechargeShieldsCoroutine);
		lastShieldCheck = -1;
		rechargeShieldsCoroutine = StartCoroutine(RechargeShields());

        // Show directional damage effect
		if(left && !(up || down))
		{
			myDamage.Damage(0,damage, gameState.GetShipHealth());
		}
		else if(right && !(up || down))
		{
			myDamage.Damage(2,damage, gameState.GetShipHealth());
		}
		else if(left && up)
		{
			myDamage.Damage(4,damage, gameState.GetShipHealth());
		}
		else if(left && down)
		{
			myDamage.Damage(6,damage, gameState.GetShipHealth());
		}
		else if(right && up)
		{
			myDamage.Damage(5,damage, gameState.GetShipHealth());
		}
		else if(right && down)
		{
			myDamage.Damage(7,damage, gameState.GetShipHealth());
		}
		else if(up && !(left || right))
		{
			myDamage.Damage(1,damage, gameState.GetShipHealth());
		}
		else
		{
			myDamage.Damage(3,damage, gameState.GetShipHealth());
		}
	}
}
