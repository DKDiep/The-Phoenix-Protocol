using UnityEngine;
using System.Collections;

public class OutpostTarget : MonoBehaviour 
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
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if(distance < 600)
            HideTarget();
        else
        {
            Vector3 v3 = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-v3);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, player.transform.eulerAngles.z);
        }
	}

    public void ShowTarget()
    {
        GetComponent<Renderer>().enabled = true;
    }

    public void HideTarget()
    {
        GetComponent<Renderer>().enabled = false;
    }
}
