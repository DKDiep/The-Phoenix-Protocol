using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour 
{

    GameObject player;

	// Use this for initialization
	void Start () 
    {
	    player = GameObject.Find("PlayerShip(Clone)");
	}
	
	// Update is called once per frame
	void Update () 
    {
            Vector3 v3 = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-v3);
	}
}
