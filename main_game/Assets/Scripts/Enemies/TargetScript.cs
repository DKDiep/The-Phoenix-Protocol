using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour 
{
    private GameObject player;
    private float distance; // Distance to the player
    private Renderer renderer;

	// Use this for initialization
	void Start () 
    {
	    player = GameObject.Find("CameraManager(Clone)");
        renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        distance = Vector3.Distance(transform.position, player.transform.position);

        if(distance > 800f || distance < 25f)
            renderer.enabled = false;
        else
        {
            Vector3 v3 = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-v3);
            renderer.enabled = true;
        }    
	}
}
