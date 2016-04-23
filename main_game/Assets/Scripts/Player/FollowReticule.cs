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
	private Camera mainCamera;
	private ServerManager serverManager;

    void Start()
    {
        targetPoint = new GameObject();
        targetPoint.name = "AimTarget" + controlledByPlayerId.ToString();

		GameObject crosshairs = GameObject.Find("Crosshairs");
		if (crosshairs != null)
			crosshair = crosshairs.transform.GetChild(controlledByPlayerId).gameObject;

		serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();

		mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
		if (crosshair != null)
		{
			// Get the point the crosshair is pointing at
			// TODO: this needs to be updated to work when pointing at other screens as well after 6 turrets are implemented
			// Folliwing PlayerShooting should do the job
			GameObject crosshairObject = serverManager.GetCrosshairObject(0);
			Vector3 playerTarget       = serverManager.GetTargetPositions(crosshairObject).targets[controlledByPlayerId];

			// Project the shooting direction on the ship's XZ plane (the turret only rotates around the ship's Y direction)
			Vector3 turretToCrosshairDirection = playerTarget - transform.position;
			Vector3 projectionOnNormal 		   = Vector3.Project(turretToCrosshairDirection, transform.parent.transform.up);
			Vector3 projectedAimingDirection   = (playerTarget - projectionOnNormal) - transform.position;

			// Align the turret's X axis (the guns direction) with the (projected) shooting direction
			transform.rotation = Quaternion.FromToRotation(Vector3.right, projectedAimingDirection);

			// Keep the turret aligned horizontaly with the ship
			transform.localEulerAngles = new Vector3(270f, transform.localEulerAngles.y, transform.localEulerAngles.z);

			// Uncomment to draw line between turret position and aim position
			/*Debug.DrawRay(transform.position, playerTarget - transform.position, Color.green);
			Debug.DrawRay(transform.position, projectedAimingDirection, Color.red);*/
		}
    }

	private Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
	{
		Ray ray = mainCamera.ScreenPointToRay(screenPosition);
		Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
		float distance;
		xy.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}
}
