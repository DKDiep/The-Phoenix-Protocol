using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour 
{
    private GameObject player;
    private float distance; // Distance to the player
    private new Renderer renderer;

	// Use this for initialization
	void Start () 
    {
	    player = GameObject.Find("CameraManager(Clone)");
        renderer = GetComponent<Renderer>();
        StartCoroutine("UpdateDistance");
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(distance > 650f || distance < 25f)
            renderer.enabled = false;
        else
        {
            Vector3 v3 = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-v3);
            renderer.enabled = true;
        }    
	}

    private IEnumerator UpdateDistance()
    {
        yield return new WaitForSeconds(0.5f);
        distance = Vector3.Distance(transform.position, player.transform.position);
        StartCoroutine("UpdateDistance");
    }
}
