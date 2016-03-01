using UnityEngine;
using System.Collections;

public class RotateEarth : MonoBehaviour {

	private float mouseXAmount, mouseYAmount;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetMouseButton(1)){
			mouseXAmount = -Input.GetAxis("Mouse X") * 2f;
			mouseYAmount = Input.GetAxis("Mouse Y") * 2f;
			transform.Rotate(mouseYAmount,mouseXAmount,0,Space.World);
		}

	}
}
