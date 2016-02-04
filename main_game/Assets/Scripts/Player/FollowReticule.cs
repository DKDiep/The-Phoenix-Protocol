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
	// Empty game object to use as the target position
    GameObject targetPoint; 

	GameObject[] crosshairs;

    // Initialise temporary object
    void Start()
    {
        targetPoint = new GameObject();
        targetPoint.name = "AimTarget";

		GameObject crosshairContainer = GameObject.Find("Crosshairs");

		crosshairs = new GameObject[4];

		// Find crosshair images
		for(int i = 0; i < 4; ++i)
		{
			crosshairs[i] = crosshairContainer.transform.GetChild(i).gameObject;
		}


    }
 
    void FixedUpdate () 
    {
		// Currently only for player 0, needs to be updated for each player. 
		Ray ray = Camera.main.ScreenPointToRay(crosshairs[0].transform.position);
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit) && !hit.transform.gameObject.tag.Equals("Player"))
        {
            targetPoint.transform.position = hit.transform.position;
        }
        else
        {
			targetPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshairs[0].transform.position.x, crosshairs[0].transform.position.y, 1000));
            targetPoint.transform.Translate (transform.parent.transform.forward * (-10f));
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetPoint.transform.position - transform.position);

        transform.rotation = targetRotation;

        // Angle correction
        transform.localEulerAngles = new Vector3(270f,  transform.localEulerAngles.y - 90f,  transform.localEulerAngles.z);
        }
    }
