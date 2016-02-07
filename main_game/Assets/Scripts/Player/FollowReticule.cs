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

	[SerializeField] int controlledByPlayerId;

	// Empty game object to use as the target position
    GameObject targetPoint;
    GameObject crosshairContainer;
    GameObject[] crosshairs;

    // Initialise temporary object
    void Start()
    {
        //targetPoint = new GameObject();
        targetPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetPoint.name = "AimTarget" + controlledByPlayerId.ToString();

		crosshairContainer = GameObject.Find("Crosshairs");

        if (crosshairContainer != null)
        {
            crosshairs = new GameObject[4];

            // Find crosshair images
            for (int i = 0; i < 4; ++i)
            {
                crosshairs[i] = crosshairContainer.transform.GetChild(i).gameObject;
            }
        }

    }

    void FixedUpdate()
    {
        if (crosshairContainer != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(crosshairs[controlledByPlayerId].transform.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.transform.gameObject.tag.Equals("Player"))
            {
                targetPoint.transform.position = hit.transform.position;
            }
            else
            {
                targetPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshairs[controlledByPlayerId].transform.position.x, crosshairs[controlledByPlayerId].transform.position.y, 1000));
                targetPoint.transform.Translate(transform.parent.transform.forward * (-10f));
            }

            transform.LookAt(targetPoint.transform.position);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint.transform.position - transform.position);

            transform.rotation = targetRotation;

            // Angle correction
            transform.localEulerAngles = new Vector3(270f, transform.localEulerAngles.y - 90f, transform.localEulerAngles.z);
        }
    }
}
