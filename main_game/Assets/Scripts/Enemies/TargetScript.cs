using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour 
{
    private GameObject player;
    private float distance; // Distance to the player
    private new Renderer renderer;
    private bool isEngineer;

    void OnEnable()
    {
        if(player == null)
        {
            if(GameObject.Find("CameraManager(Clone)") != null)
                player = GameObject.Find("CameraManager(Clone)");
            else
                Destroy(this);
        }

        if(renderer == null)
            renderer = GetComponent<Renderer>();
        StartCoroutine(UpdateDistance()); 
    }

    void Update()
    {
        transform.LookAt(transform.position + player.transform.rotation * Vector3.forward,
            player.transform.rotation * Vector3.up);
    }
        

    IEnumerator UpdateDistance()
    {
        if(player != null)
        {
            distance = Vector3.Distance(transform.position, player.transform.position);
            if(distance > 650f || distance < 125f)
                renderer.enabled = false;
            else
            {
                renderer.enabled = true;
                if(Random.Range(0,2500) == 0)
                    AIVoice.SendCommand(Random.Range(19,22));
            }
            yield return new WaitForSeconds(Mathf.Clamp(distance / 750f, 0.1f, 1f));
            StartCoroutine(UpdateDistance());
        }
    }
}
