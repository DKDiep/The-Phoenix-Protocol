using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour, INavigatable
{

	[SerializeField] float speed = 10f;
	[SerializeField] float turnSpeed = 0.01f;
	[SerializeField] float maxTurnSpeed = 1f;
	[SerializeField] float slowDown;
	[SerializeField] float shield;
	[SerializeField] float health;
	GameObject controlObject;
	float pitchVelocity = 0f;
	float rollVelocity = 0f;
	float pitchOld;
	float rollOld;
	float slowTime = 0f;
	float slowTime2 = 0f;

    void Start()
    {
    	controlObject = transform.parent.gameObject;
    }

    public float GetHealth()
    {
    	return health;
    }

	void Update () 
	{
        float joyH = Input.GetAxis("Horizontal"), joyV = Input.GetAxis("Vertical");

        // Detect key presses, ensure velocity is less than some maximum, ensure the angle is constrained between some limits to avoid the player flying backwards
		bool canPitchUp = pitchVelocity < maxTurnSpeed;
		bool canPitchDown = pitchVelocity > maxTurnSpeed * (-1f);
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

        if (controlObject != null)
        {
            controlObject.transform.Rotate(Vector3.right * pitchVelocity * Time.deltaTime * turnSpeed);
            controlObject.transform.Rotate(Vector3.up * rollVelocity * Time.deltaTime * turnSpeed);
            controlObject.transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
	
	}

    public void Up()
    {

    }

    public void Down()
    {

    }

    public void Left()
    {

    }

    public void Right()
    {

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
			Destroy(transform.parent.gameObject);
		}
		Debug.Log ("Player was hit, has " + shield + " shield and " + health + " health");
	}
}
