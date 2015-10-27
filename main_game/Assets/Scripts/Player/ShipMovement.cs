using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour 
{

	[SerializeField] float speed = 10f;
	[SerializeField] float sidewaysSpeed = 10f;
	[SerializeField] float turnSpeed = 1f;
	[SerializeField] float pitchMax = 90f;
	[SerializeField] float pitchMin = -90f;
	[SerializeField] float rollMin = -30f;
	[SerializeField] float rollMax = 30f;
	float pitchVelocity = 0f;
	float rollVelocity = 0f;
	float pitchAngle = 0f;
	float rollAngle = 0f;
	float driftVelocity = 0f;
	
	void Update () 
	{
		
		// Detect key presses, ensure velocity is less than some maximum, ensure the angle is constrained between some limits to avoid the player flying backwards
		if(Input.GetKey (KeyCode.W) && pitchVelocity < 1f && ((transform.eulerAngles.x >= 0 && transform.eulerAngles.x < pitchMax) || transform.eulerAngles.x > 270f))
		{
			pitchVelocity += 1f * Time.deltaTime * turnSpeed;
		}
		else if(Input.GetKey (KeyCode.S) && pitchVelocity > -1f && (transform.eulerAngles.x < 90f || transform.eulerAngles.x > 360f - pitchMin))
		{
			pitchVelocity -= 1f * Time.deltaTime * turnSpeed;
		}
		else if(Mathf.Abs (pitchVelocity) > 0) // If no key pressed, decrease velocity
		{
			pitchVelocity *= 0.5f;
			if(Mathf.Abs (pitchVelocity) < 0.001f) pitchVelocity = 0f;
		}
		
		if(Input.GetKey (KeyCode.D) && rollVelocity < 1f && ((transform.eulerAngles.y >= 0 && transform.eulerAngles.y < rollMax) || transform.eulerAngles.y > 270f))
		{
			rollVelocity += 1f * Time.deltaTime * turnSpeed;
		}
		else if(Input.GetKey (KeyCode.A) && rollVelocity > -1f && (transform.eulerAngles.y < 90f || transform.eulerAngles.y > 360f - rollMin))
		{
			rollVelocity -= 1f * Time.deltaTime * turnSpeed;
		}
		else if(Mathf.Abs (rollVelocity) > 0)
		{
			rollVelocity *= 0.5f;
			if(Mathf.Abs (rollVelocity) < 0.001f) rollVelocity = 0f;
		}
		
		pitchAngle = transform.eulerAngles.x + pitchVelocity;
		rollAngle = transform.eulerAngles.y + rollVelocity;
		driftVelocity = transform.eulerAngles.z % 90f; // Fixes wrapping of angles
		if(transform.eulerAngles.z > 310f) driftVelocity *= (-1f * sidewaysSpeed * Time.deltaTime);
		else driftVelocity *= sidewaysSpeed * 1.6f * Time.deltaTime;
		
		transform.eulerAngles = new Vector3(pitchAngle, rollAngle, transform.eulerAngles.z);
		transform.Translate (transform.forward * speed * Time.deltaTime);
	
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
