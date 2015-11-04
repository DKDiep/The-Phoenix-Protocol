using UnityEngine;
using System.Collections;

public class DestinationTracker : MonoBehaviour {

Vector3 destination;
float distance;

	// Use this for initialization
	void Start () 
	{
		destination = GameObject.Find ("Destination").transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		distance = Vector3.Distance (transform.position, destination);
	}
}
