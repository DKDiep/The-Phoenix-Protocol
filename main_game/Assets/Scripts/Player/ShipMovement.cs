using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour, INavigatable
{

	[SerializeField] float speed = 10f;
	[SerializeField] float turnSpeed = 1f;
	[SerializeField] float slowDown;
	float pitchVelocity = 0f;
	float rollVelocity = 0f;
	float pitchOld;
	float rollOld;
	float slowTime = 0f;
	float slowTime2 = 0f;

    private GameObject controlObject;

    public void SetControlObject(GameObject newControlObject)
    {
        controlObject = newControlObject;
    }

	void Update () 
	{
        float joyH = Input.GetAxis("Horizontal"), joyV = Input.GetAxis("Vertical");

        // Detect key presses, ensure velocity is less than some maximum, ensure the angle is constrained between some limits to avoid the player flying backwards
        bool canPitchUp = pitchVelocity < 1f;
        bool canPitchDown = pitchVelocity > -1f;
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

        bool canRollRight = rollVelocity < 1f;
        bool canRollLeft = rollVelocity > -1f;
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

	// If I hit something, check what it is and react accordingly
	void OnTriggerEnter (Collider col)
	{
		if(col.gameObject.tag == "Debris")
		{
			Debug.Log ("I hit some debris");
		}
		else if(col.gameObject.tag == "EnemyBullet")
		{
			Debug.Log ("I was shot by an enemy");
		}
	}
}
