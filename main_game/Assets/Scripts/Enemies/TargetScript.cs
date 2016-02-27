using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour 
{
    private GameObject player;

	// Use this for initialization
	void Start () 
    {
	    player = GameObject.Find("CameraManager(Clone)");
	}
	
	// Update is called once per frame
	void Update () 
    {
		Vector3 v3 = player.transform.position - transform.position;
		transform.rotation = Quaternion.LookRotation(-v3);
	}
}
