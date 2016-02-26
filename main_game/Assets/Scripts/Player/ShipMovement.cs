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
	[SerializeField] float turnSpeed = 0.01f;
	[SerializeField] float maxTurnSpeed = 1f;
	[SerializeField] float slowDown;
	[SerializeField] float shieldDelay; // Delay in seconds to wait before recharging shield

	bool rechargeShield;
	float lastShieldCheck; // Temp variable allows us to see whether I've taken damage since last checking
	GameObject controlObject;
	private GameState gameState;
	private WiiRemoteManager wii;

	float pitchVelocity = 0f;
	float rollVelocity = 0f;
	float pitchOld;
	float rollOld;
	float slowTime = 0f;
	float slowTime2 = 0f;
	bool left, right, up, down;
	DamageEffects myDamage;
    ShieldEffects myShield = null;



    // Initialise object
    void Start()
    {
		
		GameObject server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();

		GameObject remoteManager = GameObject.Find("WiiRemoteManager");
		wii = remoteManager.GetComponent<WiiRemoteManager>();

    	controlObject = transform.parent.gameObject;
		gameState.SetShipShield(gameState.GetShipMaxShields());
		lastShieldCheck = gameState.GetShipShield();
		StartCoroutine ("RechargeShields");


    }

	IEnumerator RechargeShields()
	{
		if(lastShieldCheck == gameState.GetShipShield() && 
		   gameState.GetShipShield() < gameState.GetShipMaxShields()) // Ensure shield is below max value and the player hasn't been hit
		{
			gameState.SetShipShield(gameState.GetShipShield() + (gameState.GetShipShieldRechargeRate() / 10f));
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
        myShield = GameObject.Find("Shield(Clone)").GetComponent<ShieldEffects>();
    }

	void Update () 
	{
        float joyH = Input.GetAxis("Horizontal"), joyV = Input.GetAxis("Vertical");

        // Detect key presses, ensure velocity is less than some maximum, ensure the angle is constrained between some limits to avoid the player flying backwards
		bool canPitchUp = pitchVelocity < maxTurnSpeed;
		bool canPitchDown = pitchVelocity > maxTurnSpeed * (-1f);

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

        if (Input.GetKey (KeyCode.D) && canRollRight)
		{
			rollVelocity += Time.deltaTime * turnSpeed;
			slowTime2 = 0f;
			rollOld = rollVelocity;
		}
		else if(Input.GetKey (KeyCode.A) && canRollLeft)
		{
			rollVelocity -= Time.deltaTime * turnSpeed;
			slowTime2 = 0f;
			rollOld = rollVelocity;
		}
        else if ((joyH < 0 && canRollLeft) || (joyH > 0 && canRollRight))
        {
            rollVelocity += joyH * Time.deltaTime * turnSpeed;
			slowTime2 = 0f;
			rollOld = rollVelocity;
        }
		else
		{
			rollVelocity = Mathf.Lerp(rollOld, 0f, slowTime2);
			slowTime2 += slowDown * Time.deltaTime;
		}

        // Move parent object
        if (controlObject != null)
        {
            controlObject.transform.Rotate(Vector3.right * pitchVelocity * Time.deltaTime * turnSpeed);
            controlObject.transform.Rotate(Vector3.up * rollVelocity * Time.deltaTime * turnSpeed);
			controlObject.transform.Translate(Vector3.forward * gameState.GetShipSpeed() * Time.deltaTime);
        }
	
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

        if(myDamage == null) myDamage = Camera.main.gameObject.GetComponent<DamageEffects>();

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

        // Control damage to player
		if (gameState.GetShipShield() > damage)
		{
			gameState.SetShipShield(gameState.GetShipShield() - damage);
			myShield.Impact(gameState.GetShipShield());
		}
		else if (gameState.GetShipShield() > 0)
		{
			float remDamage = damage - gameState.GetShipShield();
			gameState.SetShipShield(0);
			myShield.ShieldDown();
			gameState.ReduceShipHealth(remDamage);
		}
		else if(gameState.GetShipHealth() > damage)
		{
			// TODO: check to see if the hull is hit, otherwise damage component
            if(component == ComponentType.None)
                gameState.ReduceShipHealth(damage);
            else
                gameState.ReduceComponentHealth(component, damage);
		}
		else
		{
			gameState.SetShipHealth(0);
			gameState.SetStatus(GameState.Status.Died);
			//Destroy(transform.parent.gameObject);
		}
		//Debug.Log ("Player was hit, has " + shield + " shield and " + gameState.GetShipHealth() + " health");
	}
}
