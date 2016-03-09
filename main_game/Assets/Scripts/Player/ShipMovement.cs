/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene, Andrei Poenaru
    Description: Control ship flying
*/

using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float turnSpeed;
	private float maxTurnSpeed;
	private float slowDown;
	private float shieldDelay; // Delay in seconds to wait before recharging shield

	private float shieldRechargeValue; // The value by which to recharge the shields each tick
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

    // Initialise object
    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		GameObject server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();

		GameObject remoteManager = GameObject.Find("WiiRemoteManager");
		wii = remoteManager.GetComponent<WiiRemoteManager>();

    	controlObject = transform.parent.gameObject;
		// TODO: I'm not sure where the 10f is from, it was in the code when I refactored it. This value should probably be related to the recherge rate
		shieldRechargeValue = gameState.GetShipShieldRechargeRate() / 10f;
		lastShieldCheck = gameState.GetShipShield();
		StartCoroutine ("RechargeShields");
    }

	private void LoadSettings()
	{
		turnSpeed = settings.PlayerShipTurnSpeed;
		maxTurnSpeed = settings.PlayerShipMaxTurnSpeed;
		slowDown = settings.PlayerShipSlowDown;
		shieldDelay = settings.PlayerShipShieldDelay;
	}

	IEnumerator RechargeShields()
	{
		if(lastShieldCheck == gameState.GetShipShield() && 
		   gameState.GetShipShield() < gameState.GetShipMaxShields()) // Ensure shield is below max value and the player hasn't been hit
		{
			gameState.RechargeShield(shieldRechargeValue);
			lastShieldCheck = gameState.GetShipShield();
			yield return new WaitForSeconds(0.1f);
			StartCoroutine ("RechargeShields");
		}
		else
		{
			lastShieldCheck = gameState.GetShipShield();
			yield return new WaitForSeconds(shieldDelay);
			StartCoroutine ("RechargeShields");
		}
	}
		
    public void StartGame()
    {
        gameState.myShield = GameObject.Find("Shield(Clone)").GetComponent<ShieldEffects>();
    }

	void Update () 
	{
        if (gameState.Status != GameState.GameStatus.Started)
            return;

        float joyH = Input.GetAxis("Horizontal"), joyV = Input.GetAxis("Vertical");

        // Detect key presses, ensure velocity is less than some maximum, ensure the angle is constrained between some limits to avoid the player flying backwards
		bool canPitchUp = pitchVelocity < maxTurnSpeed;
		bool canPitchDown = pitchVelocity > maxTurnSpeed * (-1f);

        controlObject.transform.eulerAngles = new Vector3(controlObject.transform.rotation.eulerAngles.x, controlObject.transform.rotation.eulerAngles.y, controlObject.transform.rotation.eulerAngles.z - sideRoll);

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
			controlObject.transform.Translate(Vector3.forward * gameState.GetShipSpeed() * Time.deltaTime);
        }

            //Debug.Log(sideRoll);
        controlObject.transform.eulerAngles = new Vector3(controlObject.transform.rotation.eulerAngles.x, controlObject.transform.rotation.eulerAngles.y, controlObject.transform.rotation.eulerAngles.z + sideRoll);
	
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
            myDamage = Camera.main.gameObject.GetComponent<DamageEffects>();

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
		if(up && !(left || right))
		{
			myDamage.Damage(1,damage, gameState.GetShipHealth());
		}
		else if(down && !(up || down))
		{
			myDamage.Damage(3,damage, gameState.GetShipHealth());
		}
			
		// Check to see if the hull is hit, otherwise damage component
		if (component == ComponentType.None)
		{
			gameState.DamageShip(damage);
		}
        else
            gameState.ReduceComponentHealth(component, damage);
	}
}
