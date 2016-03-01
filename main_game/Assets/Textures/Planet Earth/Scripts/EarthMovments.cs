using UnityEngine;
using System.Collections;

public class EarthMovments : MonoBehaviour {
	
	public float rotationSpeed;
	public Transform linkedObject;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(0f,0f, rotationSpeed*Time.deltaTime);
		if(linkedObject){
			linkedObject.Rotate(0f,0f, rotationSpeed*Time.deltaTime);
		}
	}
}
