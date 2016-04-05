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

		GameObject crosshairs = GameObject.Find("Crosshairs");
		if (crosshairs != null)
			crosshair = crosshairs.transform.GetChild(controlledByPlayerId).gameObject;
    }

    void FixedUpdate()
    {
		if (crosshair != null && crosshair.activeSelf)
        {
			// Get the point the crosshair is pointing at
			targetPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshair.transform.position.x,
				crosshair.transform.position.y, Camera.main.farClipPlane));

			// Project the shooting direction on the ship's XZ plane (the turret only rotates around the ship's Y direction)
			Vector3 turretToCrosshairDirection = targetPoint.transform.position - transform.position;
			Vector3 projectionOnNormal = Vector3.Project(turretToCrosshairDirection, transform.parent.transform.up);
			Vector3 projectedAimingDirection = targetPoint.transform.position - projectionOnNormal;

			// Align the turret's X axis (the guns direction) with the (projected) shooting direction
			transform.rotation = Quaternion.FromToRotation(Vector3.right, projectedAimingDirection);

            // Keep the turret aligned horizontaly with the ship
			transform.localEulerAngles = new Vector3(270f, transform.localEulerAngles.y, transform.localEulerAngles.z);

            // Draw line between turret position and aim position. Gizmos needs to be enabled in Game View to see the line (click Gizmos in top right)
			/*if (debug)
			{
				Debug.DrawRay(transform.position, targetPoint.transform.position - transform.position, Color.green);
				Debug.DrawLine(transform.position, projectedAimingDirection, Color.red);
			}*/
        }
    }

	private Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPosition);
		Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
		float distance;
		xy.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}
}
