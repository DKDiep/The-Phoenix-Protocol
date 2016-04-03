using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour 
{
    private GameObject player;
    private float distance; // Distance to the player
    private new Renderer renderer;

    void OnEnable()
    {
        if(player == null)
            player = GameObject.Find("CameraManager(Clone)");
        if(renderer == null)
            renderer = GetComponent<Renderer>();
        StartCoroutine(UpdateDistance()); 
    }
        

    IEnumerator UpdateDistance()
    {
        yield return new WaitForSeconds(0.1f);
        distance = Vector3.Distance(transform.position, player.transform.position);
        if(distance > 650f || distance < 100f)
            renderer.enabled = false;
        else
        {
            Vector3 v3 = player.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-v3);
            renderer.enabled = true;
        }
        StartCoroutine(UpdateDistance());
    }
}
