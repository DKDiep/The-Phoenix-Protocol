/*
    Smooth camera movement
*/

using UnityEngine;
using System.Collections;

public class SmoothCamera : MonoBehaviour 
{
	private GameObject parent;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] float damping;
	#pragma warning restore 0649
    
    // Update is called once per frame
    void LateUpdate () 
    {
        if (parent != null)
        {
            transform.position = parent.transform.position;
            Quaternion rotation = parent.transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        }
        else
        {
            transform.parent = null; // unlink from parent
            parent = GameObject.Find("PlayerShip(Clone)");
        }
    }
}