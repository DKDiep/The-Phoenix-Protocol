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