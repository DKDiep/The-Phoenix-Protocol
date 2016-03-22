/*
    Turrets aim at player reticule position
*/

using UnityEngine;
using System.Collections;

public class FollowReticule : MonoBehaviour 
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private int controlledByPlayerId;
	#pragma warning restore 0649

	// Empty game object to use as the target position
    private GameObject targetPoint;
    private GameObject crosshair;

    // Draws line between turret position and aim position if true
    private bool debug = false;

    // Initialise temporary object
    void Start()
    {
        targetPoint = new GameObject();
        targetPoint.name = "AimTarget" + controlledByPlayerId.ToString();

        crosshair = GameObject.Find("Crosshairs").transform.GetChild(controlledByPlayerId).gameObject;
    }

    void FixedUpdate()
    {
        if (crosshair != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(crosshair.transform.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && (hit.transform.gameObject.tag.Equals("Debris") || hit.transform.gameObject.tag.Equals("EnemyShip")))
                targetPoint.transform.position = hit.transform.position;
            else
            {
                targetPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshair.transform.position.x, crosshair.transform.position.y, 1000));
                targetPoint.transform.Translate(transform.parent.transform.forward * (-10f));
            }

            //transform.LookAt(targetPoint.transform.position);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint.transform.position - transform.position);
            transform.rotation = targetRotation;

            // Angle correction
            transform.localEulerAngles = new Vector3(270f, transform.localEulerAngles.y - 90f, transform.localEulerAngles.z);

            // Draw line between turret position and aim position. Gizmos needs to be enabled in Game View to see the line (click Gizmos in top right)
            if(debug)
                Debug.DrawRay(transform.position, targetPoint.transform.position - transform.position, Color.green);
        }
    }
}
