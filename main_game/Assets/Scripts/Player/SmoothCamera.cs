/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Smooth camera movement
*/

using UnityEngine;
using System.Collections;

public class SmoothCamera : MonoBehaviour 
{

GameObject parent;
[SerializeField] float damping;

	/*********
    NOTE:
    Functionally removed as this script prevents unique camera transform
    Remove or update when camera implementation is complete
    **********/
	/*void Start () 
	{
        if (transform.parent != null)
		    parent = transform.parent.gameObject;
		transform.parent = null; // unlink from parent
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
        if (parent != null)
        {
            transform.position = parent.transform.position;
            Quaternion rotation = parent.transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        }
	}*/
}
