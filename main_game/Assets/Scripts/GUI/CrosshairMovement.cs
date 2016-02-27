using UnityEngine;
using System.Collections;
using WiimoteApi;
using System;

public class CrosshairMovement : MonoBehaviour
{
    private int controlling = 0;
	private int numberOfCrossHairs;
	private float posReadDelay = 0.0001f;
	private bool[] init;
	private Vector3 initPos;
	private float movx;
	private float movy;
	private bool canMove = true;
	public Vector3[] crosshairPosition;
	private Vector3[] oldCrosshairPosition;
	private Vector3[] crosshairPositionTmp;
	private float oldAccel, newAccel;
    private GameObject[] crosshairs;
	private WiiRemoteManager wii;

	// Autoaiming looks for objects inside a sphere in front of the player
	private const int AUTOAIM_OFFSET              = 570; // The offset between the player and the sphere's centre
	private const int AUTOAIM_RADIUS              = 500; // The sphere's radius
	private const int AUTOAIM_DISTANCE_THRESHOLD  = 50;  // The maximum distance between an autoaim target and the aiming direction, i.e. the snap distance
	private const int AUTOAIM_ADVANCE_OFFSET      = 10;  // The distance at which to aim in front of the target to account for bullet speed

	// Use this for initialization
	void Start ()
    {
		GameObject crosshairContainer = GameObject.Find("Crosshairs");

		GameObject remoteManager = GameObject.Find("WiiRemoteManager");
		wii = remoteManager.GetComponent<WiiRemoteManager>();

        // Get number of connected wii remotes
        numberOfCrossHairs = wii.GetNumberOfRemotes();
        
		// If there are no wii remotes connected, set the default to 2
		if(numberOfCrossHairs == 0) numberOfCrossHairs = 2;

		crosshairPosition = new Vector3[numberOfCrossHairs];
		oldCrosshairPosition = new Vector3[numberOfCrossHairs];
		crosshairPositionTmp = new Vector3[numberOfCrossHairs];

		init = new bool[4];
		crosshairs = new GameObject[4];
			
        // Find crosshairs
		for(int i = 0; i < 4; ++i)
        {
			crosshairs[i] = GameObject.Find("CrosshairImage"+i);
			// Hide crosshairs we are not using
			if(i >= numberOfCrossHairs) crosshairs[i].SetActive(false);

			init[i] = true;
        }
			
		StartCoroutine(FindRemotes());
	}
	
	// Update is called once per frame
    void Update()
    {
		Transform selectedCrosshair;

		// If there is a wii remote connected.
		if (WiimoteManager.Wiimotes.Count > 0) 
		{
			// Loop through each wii remote id
			for(int remoteId = 0; remoteId < WiimoteManager.Wiimotes.Count; remoteId++) 
			{
				selectedCrosshair = crosshairs[remoteId].transform;

				// Fixes some strange bug where the z value gets set to a rediculous value.
				oldCrosshairPosition[remoteId].z = 0.0f;
				crosshairPosition[remoteId].z = 0.0f;

				// Do some interpolation to help smoothing
				if(crosshairPositionTmp[remoteId] == oldCrosshairPosition[remoteId]) 
				{
					if(Math.Abs(selectedCrosshair.position.x) < Math.Abs(crosshairPosition[remoteId].x) &&
					   Math.Abs(selectedCrosshair.position.y) < Math.Abs(crosshairPosition[remoteId].y)) 
					{
						selectedCrosshair.position = selectedCrosshair.position + (crosshairPosition[remoteId]/50);
					}
				} 
				else 
				{
					selectedCrosshair.position = oldCrosshairPosition[remoteId];
					crosshairPositionTmp[remoteId] = oldCrosshairPosition[remoteId];
				}
			}
		} 
		else 
		{
			// Check to see if any of the crosshair keys have been pressed
			for (int i = 1; i <= numberOfCrossHairs; i++) 
			{
				if (Input.GetKeyDown (i.ToString ())) 
				{
					controlling = i-1;
					Debug.Log ("Controlling " + controlling);
				}
			}
		}
    }

	void FixedUpdate ()
    {
        // Get the currently controlled crosshair
        Transform selectedCrosshair = crosshairs[controlling].transform;

		// Control crosshair by mouse if there is no wii remote connected.
		if (WiimoteManager.Wiimotes.Count < 1) 
		{
			// Update its position to the current mouse position
			Vector3 currentPosition = selectedCrosshair.position;
			currentPosition.x = Input.mousePosition.x;
			currentPosition.y = Input.mousePosition.y;

			// If there's an autoaim target in range, use that instead of the cursor position
			Target target = GetClosestTarget(currentPosition);
			if (!target.IsNone())
				selectedCrosshair.position = Camera.main.WorldToScreenPoint(target.GetAimPosition());
			else
				selectedCrosshair.position = currentPosition;
		} 
		else 
		{
			int remoteId = 0;
			foreach(Wiimote remote in WiimoteManager.Wiimotes) 
			{
				selectedCrosshair = crosshairs[remoteId].transform;
				if (this.init[remoteId]) 
				{
					// Set the LEDs on each wii remote to indicate which player is which
					if(remoteId == 0) remote.SendPlayerLED (true, false, false, false);
					if(remoteId == 1) remote.SendPlayerLED (false, true, false, false);
					if(remoteId == 2) remote.SendPlayerLED (false, false, true, false);
					if(remoteId == 3) remote.SendPlayerLED (false, false, false, true);

					// Set up the IR camera on the wii remote
					remote.SetupIRCamera (IRDataType.BASIC);
					this.init[remoteId] = false;
				}
				try 
				{
					if (remote.ReadWiimoteData () > 0) 
					{ 
						float[] pointer = remote.Ir.GetPointingPosition ();

						// If the delay is over and the crosshair can be updated
						if(canMove) 
						{
							if(pointer[0] != -1 && pointer[1] != -1) 
							{
								oldAccel = newAccel;

								// Get data from the accelerometer
								newAccel = remote.Accel.GetCalibratedAccelData()[1] + remote.Accel.GetCalibratedAccelData()[2];

								// If there is little movement, don't bother doing this. (Should stop shaking)
								if(Math.Abs(newAccel - oldAccel) > 0.03) 
								{
									oldCrosshairPosition[remoteId] = crosshairPosition[remoteId];

									Vector3 position = selectedCrosshair.position;
									position.x = pointer[0] * Screen.width;
									position.y = pointer[1] * Screen.height;

									// If there's an autoaim target in range, use that instead of the pointing position
									// TODO: this is not tested with a Wiimote and might interfere with smoothing
									Target target = GetClosestTarget(position);
									if (!target.IsNone())
										crosshairPosition[remoteId] = Camera.main.WorldToScreenPoint(target.GetAimPosition());
									else
										crosshairPosition[remoteId] = position;

									canMove = false;
									StartCoroutine("Delay");
								}
							}
						}
					}
				} 
				catch (Exception) 
				{
					// Sometimes the wii remote will disconnect so for this we re connect the remote and run the initialise function again.
					WiimoteManager.FindWiimotes ();
					this.init[remoteId] = true;
				}  
				remoteId++;
			}
		}
    }

	IEnumerator FindRemotes()
	{	
		WiimoteManager.FindWiimotes ();
		yield return new WaitForSeconds(5f);
	}

	IEnumerator Delay()
	{
		yield return new WaitForSeconds(posReadDelay);
		canMove = true;
	}

	// Get the target closest to where the player is aiming (within bounds)
	private Target GetClosestTarget(Vector3 aimPosition)
	{
		// Cast a ray from the crosshair
		Ray ray = Camera.main.ScreenPointToRay(aimPosition);

		// Find the objects in a sphere in front of the player
		// TODO: use layers to remove the physics cost
		Collider[] cols     = Physics.OverlapSphere(ray.origin + ray.direction * AUTOAIM_OFFSET, AUTOAIM_RADIUS);
		Collider closestCol = null;
		float minDistance   = AUTOAIM_DISTANCE_THRESHOLD;
		bool foundAnEnemy   = false;
		foreach (Collider col in cols)
		{
			// Find the enemy closest to the aiming direction and within the distance threshold from the aiming direction
			if (col.CompareTag("EnemyShip"))
			{
				float aimDirectionDistance = Vector3.Cross(ray.direction, col.transform.position - ray.origin).magnitude;

				// If we previously found an asteroid but there is also an enemy in range, prioritise the enemy
				if (aimDirectionDistance < minDistance || (closestCol != null && closestCol.CompareTag("Debris") &&
					aimDirectionDistance < AUTOAIM_DISTANCE_THRESHOLD))
				{
					closestCol   = col;
					minDistance  = aimDirectionDistance;
					foundAnEnemy = true;
				}
			}
			// If we haven't found any enemy, try to aim for the closest asteroid
			else if (!foundAnEnemy && col.CompareTag("Debris"))
			{
				float aimDirectionDistance = Vector3.Cross(ray.direction, col.transform.position - ray.origin).magnitude;

				if (aimDirectionDistance < minDistance)
				{
					closestCol   = col;
					minDistance  = aimDirectionDistance;
				}
			}
		}
			
		// If a target is found, return it 
		if (closestCol != null)
		{
			// targetGizmoLoc = closestCol.transform.position; // Uncomment this to use with target gizmos
			return new Target(closestCol.gameObject, minDistance);
		}

		targetGizmoLoc = Vector3.zero;
		return Target.None;
	}
		
	// Uncomment below for visual debug cues
	private Vector3 targetGizmoLoc;
	void OnDrawGizmos()
	{
		// Debug aim sphere
		/*Ray ray = Camera.main.ScreenPointToRay(crosshairs[controlling].transform.position);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(ray.origin + ray.direction * AUTOAIM_OFFSET, AUTOAIM_RADIUS);*/

		// Debug target
		/*Gizmos.color = Color.cyan;
		if (targetGizmoLoc != Vector3.zero)
			Gizmos.DrawSphere(targetGizmoLoc, 20);*/
	}

	private class Target
	{
		public GameObject Object { get; private set; }
		public Vector3 Position { get; private set; }
		public float Distance { get; private set; }

		private static readonly Target noTarget;
		public static Target None
		{
			get { return noTarget; }
		}

		private Target() : this(null, 0) { }

		static Target()
		{
			noTarget = new Target();
		}

		public Target (GameObject obj, float distance)
		{
			this.Object   = obj;
			this.Distance = distance;

			if (obj != null)
				this.Position = obj.transform.position;
			else
				this.Position = Vector3.zero;
		}

		// Check if this object doesn't represent any target
		public bool IsNone()
		{
			return this.Equals(noTarget);
		}

		// Get the aim position of this object. For ships, this will be a little in front of their current position to account for their movement
		public Vector3 GetAimPosition()
		{
			if (this.Object.CompareTag("EnemyShip"))
				// TODO: When we get a proper enemy ship, update the forward direction
				// TODO: We might need to aim a bit more in front
				return this.Position - this.Object.transform.up * AUTOAIM_ADVANCE_OFFSET;
			else
				return this.Position;
		}
	}
}
