using UnityEngine;
using System.Collections;

public class OutpostTarget : MonoBehaviour 
{
    private GameObject player;
    private float distance;
    private Renderer myRenderer;

	// Use this for initialization
	void Start () 
    {
	    player = GameObject.Find("CameraManager(Clone)");
        StartCoroutine("UpdateDistance");

	}
	
	// Update is called once per frame
	void Update () 
    {
        if(distance < 600)
            HideTarget();
        else
        {
            Vector3 v3 = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-v3);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, player.transform.eulerAngles.z);
        }
	}

    private void GetRenderer()
    {
        myRenderer = GetComponent<Renderer>();
    }

    public void StartMission()
    {
        myRenderer.material.color = Color.yellow;
    }

    public void EndMission()
    {
        myRenderer.material.color = Color.red;
    }

    private IEnumerator UpdateDistance()
    {
        distance = Vector3.Distance(player.transform.position, transform.position);
        yield return new WaitForSeconds(1f);
        StartCoroutine("UpdateDistance");
    }

    public void ShowTarget()
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.enabled = true;
    }

    public void HideTarget()
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.enabled = false;
    }
}
