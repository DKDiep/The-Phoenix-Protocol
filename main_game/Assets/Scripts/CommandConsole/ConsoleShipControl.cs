using UnityEngine;
using System.Collections;

public class ConsoleShipControl : MonoBehaviour {

	private const float rotationSpeed = 300;

	private Quaternion desiredRotation;
	private Quaternion currentRotation;
	private Quaternion rotation;
	private float xDeg, yDeg;

	void Start () {

 
	}
		
	void Update () {
		
		if (Input.GetMouseButton(0))
		{
			// Get mouse x position and rotate the ship about the y axis
			xDeg += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
			desiredRotation = Quaternion.Euler(0, xDeg, 0);
			currentRotation = transform.rotation;
			rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime);
			transform.rotation = rotation;
		}
	}
        
}
