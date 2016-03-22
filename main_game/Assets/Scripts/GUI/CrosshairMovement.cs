using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using WiimoteApi;
using System;

public class CrosshairMovement : NetworkBehaviour
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
	private float oldAccel, newAccel;
    private GameObject[] crosshairs;
	private WiiRemoteManager wii;

	private int screenControlling = 0;

	private GameObject gameManager;
	private ServerManager serverManager;
	private PlayerController playerController;

	// Autoaiming looks for objects inside a sphere in front of the player
	private const int AUTOAIM_OFFSET              = 570; // The offset between the player and the sphere's centre
	private const int AUTOAIM_RADIUS              = 500; // The sphere's radius
	private const int AUTOAIM_DISTANCE_THRESHOLD  = 50;  // The maximum distance between an autoaim target and the aiming direction, i.e. the snap distance
	private const int AUTOAIM_ADVANCE_OFFSET      = 2;  // The distance at which to aim in front of the target to account for bullet speed

    //8 floats for 4 2D positions
    public SyncListFloat position = new SyncListFloat();

    // Use this for initialization
    void Start ()
    {

		gameManager = GameObject.Find("GameManager");
		serverManager = gameManager.GetComponent<ServerManager>();


		if (ClientScene.localPlayers[0].IsValid)
			playerController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();


        //Populate sync list with 8 floats
        for (int i = 0; i < 8; i++)
            position.Add(0.0f);

		GameObject remoteManager = GameObject.Find("WiiRemoteManager");
		wii = remoteManager.GetComponent<WiiRemoteManager>();

        // Get number of connected wii remotes
        numberOfCrossHairs = wii.GetNumberOfRemotes();
        
		// If there are no wii remotes connected, set the default to 2
		// Um... Why? Let's set it to 1 instead (to help fix turret drift)
		if(numberOfCrossHairs == 0) numberOfCrossHairs = 1;

		crosshairPosition = new Vector3[numberOfCrossHairs];

		init = new bool[4];
		crosshairs = new GameObject[4];
			
        // Find crosshairs
		for(int i = 0; i < 4; ++i)
        {
            crosshairs[i] = gameObject.transform.Find("Crosshairs").GetChild(i).gameObject;
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


        // If the current instance of crosshairMovement is on the server.
		if(playerController.netId.Value == serverManager.GetServerId())
        {
			// If there is no wii remotes connected.
			if (WiimoteManager.Wiimotes.Count < 1) 
			{
                SwitchPlayers();
                ChangeScreenManually();
			}
		} 

		// Update position of crosshairs
		for(int i = 0; i < 4; i++) {
			selectedCrosshair = crosshairs[i].transform;
			selectedCrosshair.position = GetPosition(i);
		}
    }
        
	void FixedUpdate ()
    {
		if(playerController.netId.Value == serverManager.GetServerId())
        {
			// Control crosshair by mouse if there is no wii remote connected.
			if (WiimoteManager.Wiimotes.Count < 1) 
			{
                SetCrosshairPositionMouse();
			} 
			else 
			{
                DetermineCrosshairScreen();
                SetCrosshairPositionWiimote();
			}
		}
    }

    /// <summary>
    /// Sets the crosshair position using the current mouse x and y position. 
    /// Sends the crosshair position to the correct screen.
    /// </summary>
    private void SetCrosshairPositionMouse()
    {
        // Update its position to the current mouse position
        Vector2 currentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // If there's an autoaim target in range, use that instead of the cursor position
        Target target = GetClosestTarget(currentPosition);
        if (!target.IsNone())
        {
            serverManager.SetCrosshairPosition(controlling, screenControlling, Camera.main.WorldToScreenPoint(target.GetAimPosition()));
        }
        else
        {
            serverManager.SetCrosshairPosition(controlling, screenControlling, currentPosition);
        }
    }
        
    /// <summary>
    /// Switch between players using keys 4-7, for debugging different player shooting.
    /// </summary>
    void SwitchPlayers() 
    {
        // Loop through 4 players
        for (int i = 4; i <= 7; i++) 
        {
            if (Input.GetKeyDown (i.ToString ())) 
            {
                controlling = i-4;
            }
        }
    }
        
    /// <summary>
    /// Sets the crosshair position using the wii remote
    /// Sends the crosshair position to the correct screen.
    /// </summary>
    private void SetCrosshairPositionWiimote()
    {
        int remoteId = 0;
        foreach(Wiimote remote in WiimoteManager.Wiimotes) 
        {
            // Get the currently controlled crosshair
            Transform selectedCrosshair = crosshairs[remoteId].transform;
            if (this.init[remoteId]) 
            {
                wii.SetPlayerLeds(remote, remoteId);

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

                                Vector3 position = selectedCrosshair.position;
                                position.x = pointer[0] * Screen.width;
                                position.y = pointer[1] * Screen.height;

                                // If there's an autoaim target in range, use that instead of the pointing position
                                // TODO: this is not tested with a Wiimote and might interfere with smoothing
                                Target target = GetClosestTarget(position);
                                if (!target.IsNone())
                                {
                                    serverManager.SetCrosshairPosition(remoteId, screenControlling, Camera.main.WorldToScreenPoint(target.GetAimPosition()));
                                }
                                else 
                                {
                                    serverManager.SetCrosshairPosition(remoteId, screenControlling, position);
                                }


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

    private void DetermineCrosshairScreen()
    {
        int remoteId = 0;
        foreach(Wiimote remote in WiimoteManager.Wiimotes)
        {
            if(remote.Ir.ir[2, 0] == -1)
            {
                screenControlling = 0;
            }
            else
            {
                int apex = 0;
                float midx = 0;
                float midy = 0;

                // Points of the triangle
                float x1 = (float)remote.Ir.ir[0, 0];
                float y1 = (float)remote.Ir.ir[0, 1];
                float x2 = (float)remote.Ir.ir[1, 0];
                float y2 = (float)remote.Ir.ir[1, 1];
                float x3 = (float)remote.Ir.ir[2, 0];
                float y3 = (float)remote.Ir.ir[2, 1];

                // Calculate the length of each edge of the triangle
                double e1 = Math.Sqrt(Math.Pow(x1 - x2,2) + Math.Pow(y1 - y2,2));
                double e2 = Math.Sqrt(Math.Pow(x2 - x3,2) + Math.Pow(y2 - y3,2));
                double e3 = Math.Sqrt(Math.Pow(x3 - x1,2) + Math.Pow(y3 - y1,2));

                // Which lines are bigger
                if(e2 > e1 && e2 > e3)
                {
                    apex = 0;
                    midx = (x2+x3)/2;
                    midy = (y2+y3)/2;
                }
                else if(e3 > e2 && e3 > e1)
                {
                    apex = 1;
                    midx = (x1+x3)/2;
                    midy = (y1+y3)/2;
                }
                else if(e1 > e2 && e1 > e3)
                {
                    apex = 2;
                    midx = (x2+x1)/2;
                    midy = (y2+y1)/2;
                }
                    
                if(e1 < 400 && e2 < 400 && e3 < 400)
                {
                    // If the apex is greater than the y value of the mid point of the two points then it
                    // is on the right screen.
                    screenControlling = -1;
                    if(remote.Ir.ir[apex, 1] > midy)
                    {
                        screenControlling = 1;
                    }
                }
            }
            remoteId++;
        }
    }


    /// <summary>
    /// Changes the screen manually using the 1-3 keys, this is for debugging when wiimotes are not connected
    /// </summary>
    private void ChangeScreenManually()
    {
        // Change Screen Buttons
        for (int i = 1; i <= 3; i++) 
        {
            if (Input.GetKeyDown (i.ToString ())) 
            {
                // Subtract 2 here as the screen ids are from -1 to 1
                screenControlling = i - 2;
            }
        }
    }

    public void SetPosition(int crosshairId, Vector2 newPosition)
    {
        int i = crosshairId * 2;
        position[i] = newPosition.x;
        position[i + 1] = newPosition.y;
    }

	private Vector2 GetPosition(int crosshairId)
	{
		int i = crosshairId * 2;
		return new Vector2(position[i], position[i + 1]);
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

		// targetGizmoLoc = Vector3.zero; // Uncomment this to use with target gizmos
		return Target.None;
	}

	// Uncomment below for visual debug cues
	// private Vector3 targetGizmoLoc;
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
				// TODO: We might need to aim a bit more in front
				return this.Position + this.Object.transform.forward * AUTOAIM_ADVANCE_OFFSET;
			else
				return this.Position;
		}
	}
}
