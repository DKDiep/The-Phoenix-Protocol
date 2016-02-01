/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Turrets aim at player reticule position
*/

using UnityEngine;
using System.Collections;

public class FollowReticule : MonoBehaviour 
{

    GameObject targetPoint; // Empty game object to use as the target position

    // Initialise temporary object
    void Start()
    {
        targetPoint = new GameObject();
        targetPoint.name = "AimTarget";
    }
 
    void FixedUpdate () 
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit) && !hit.transform.gameObject.tag.Equals("Player"))
        {
            targetPoint.transform.position = hit.transform.position;
        }
        else
        {
            targetPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000));
            targetPoint.transform.Translate (transform.parent.transform.forward * (-10f));
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetPoint.transform.position - transform.position);

        transform.rotation = targetRotation;

        // Angle correction
        transform.localEulerAngles = new Vector3(270f,  transform.localEulerAngles.y + 270f,  transform.localEulerAngles.z);
        }
    }
