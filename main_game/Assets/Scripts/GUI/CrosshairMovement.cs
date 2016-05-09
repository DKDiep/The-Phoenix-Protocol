using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
//using WiimoteApi;
using System;

public class CrosshairMovement : NetworkBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float wiimoteInterpolationFactor;
	private float autoaimHoldDelay;
	private float autoaimHoldMaxDistance;

	private int controlling = 0;
	private int numberOfCrossHairs;
	private bool[] init;
	private Vector3 initPos;
	private float movx;
	private float movy;
	public Vector3[] crosshairPosition;
	private float oldAccel, newAccel;
    private GameObject[] crosshairs;

	private int screenControlling = 0;
    private bool usingMouse = false;

	private GameObject gameManager;
	private ServerManager serverManager;

	// Autoaiming looks for objects inside a sphere in front of the player
	private const int AUTOAIM_OFFSET              = 570; // The offset between the player and the sphere's centre
	private const int AUTOAIM_RADIUS              = 500; // The sphere's radius
	private const int AUTOAIM_DISTANCE_THRESHOLD  = 20;  // The maximum distance between an autoaim target and the aiming direction, i.e. the snap distance
	private const int AUTOAIM_ADVANCE_OFFSET      = 2;  // The distance at which to aim in front of the target to account for bullet speed
	private CrosshairAutoaimAssist[] autoaimScripts;
	private Camera mainCamera;
    private PlayerController localController;

    private GameObject playerShip;

    //8 floats for 4 2D positions
    public SyncListFloat position = new SyncListFloat();
	private SyncListBool visibleCrosshairs = new SyncListBool();
	private SyncListFloat lastShootTime = new SyncListFloat();

	private Target[] lastTargets = new Target[4];

    // Use this for initialization
    void Start ()
    {
        playerShip  = GameObject.Find("PlayerShip(Clone)");
		gameManager = GameObject.Find("GameManager");

		serverManager = gameManager.GetComponent<ServerManager>();
        if (ClientScene.localPlayers[0].IsValid)
            localController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        //Populate sync list with 8 floats
        for (int i = 0; i < 8; i++)
            position.Add(0.0f);

		for (int i = 0; i < 4; i++)
		{
			visibleCrosshairs.Add(false);
			lastShootTime.Add(0f);
			lastTargets[i] = Target.None;
		}
                    
		// If there are no wii remotes connected, set the default to 2
		// Um... Why? Let's set it to 1 instead (to help fix turret drift)
		if(numberOfCrossHairs == 0) numberOfCrossHairs = 1;

		crosshairPosition = new Vector3[4];

		init 		   = new bool[4];
		crosshairs 	   = new GameObject[4];
		autoaimScripts = new CrosshairAutoaimAssist[4];
			
        // Find crosshairs
		for(int i = 0; i < 4; ++i)
        {
            crosshairs[i] 	  = gameObject.transform.Find("Crosshairs").GetChild(i).gameObject;
			autoaimScripts[i] = crosshairs[i].GetComponent<CrosshairAutoaimAssist>();

			// Hide crosshairs we are not using
			if(i >= numberOfCrossHairs) crosshairs[i].SetActive(false);

			init[i] = true;
        }

		mainCamera = Camera.main;
			
		//StartCoroutine(FindRemotes());
	}

	private void LoadSettings()
	{
		wiimoteInterpolationFactor = settings.WiimoteInterpolationFactor;
		autoaimHoldDelay 		   = settings.PlayerAutoaimSwitchDelay;
		autoaimHoldMaxDistance     = settings.PlayerAutoaimHoldDistance;
    }

    
	// Update is called once per frame
    void Update()
	{
		Transform selectedCrosshair;

		// Toggle usage of mouse when pressing 'c'
		if (Input.GetKeyDown("c"))
		{
			usingMouse = !usingMouse;
			Debug.Log("Using Mouse? " + usingMouse);
		}

		if (usingMouse)
		{
			// SwitchPlayers(); // Uncomment to be able to switch turrets using the keys
			ChangeScreenManually();
			SetCrosshairPositionMouse();
		}

        Vector3[] targets = new Vector3[4];
        Vector3[] rays = new Vector3[8];

        // Update position of crosshairs
        for (int i = 0; i < 4; i++)
		{
			selectedCrosshair = crosshairs[i].transform;

			// Interpolate towards the current aiming position
			Vector2 currentPosition = selectedCrosshair.position, newPosition = GetPosition(i);
			selectedCrosshair.position = Vector2.Lerp(currentPosition, newPosition, Time.deltaTime * wiimoteInterpolationFactor);

			// Disable the crosshair on this screen if it's on another screen
			crosshairs[i].SetActive(visibleCrosshairs[i]);

            Ray ray = GetAimRay(selectedCrosshair.position);
            rays[i * 2] = ray.origin;
            rays[i * 2 + 1] = ray.direction;
			Target target = GetClosestTarget(ray);

			// Aim at a new target only if the player is not shooting at the current one or has move the crosshair far enough from the previous point
			// This causes slightly weird behaviour, including auto-snapping not working properly, and we've decided to remove it
			/*if (Time.time - lastShootTime[i] > autoaimHoldDelay || lastTargets[i].IsNone())
				target = lastTargets[i] = GetClosestTarget(ray);
			else
				target = lastTargets[i];*/
			
			if (!target.IsNone())
				selectedCrosshair.position = mainCamera.WorldToScreenPoint(target.GetAimPosition());

            targets[i] = mainCamera.ScreenToWorldPoint(new Vector3(selectedCrosshair.position.x, selectedCrosshair.position.y, 1000));
        }
        
        localController.CmdUpdateTargets(gameObject, targets, rays);
    }

    /// <summary>
    /// Sets the crosshair position wii remote.
    /// This is called by the UDPServer
    /// </summary>
    /// <param name="playerId">Player identifier.</param>
    /// <param name="screenId">Screen identifier.</param>
    /// <param name="position">Position.</param>
    public void SetCrosshairPositionWiiRemote(int playerId, int screenId, Vector2 position)
    {
        Vector2 oldPosittion = GetPosition(controlling);
        serverManager.SetCrosshairPosition(playerId, screenId, position);
    }

    /// <summary>
    /// Sets the crosshair position using the current mouse x and y position. 
    /// Sends the crosshair position to the correct screen.
    /// </summary>
    private void SetCrosshairPositionMouse()
    {
        // Update its position to the current mouse position
        Vector2 currentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        serverManager.SetCrosshairPosition(controlling, screenControlling, currentPosition);
    }
        
    /// <summary>
    /// Switch between players using keys 4-7, for debugging different player shooting.
    /// </summary>
    private void SwitchPlayers() 
    {
		// TODO: these keys conflict with the commander abilities

        // Loop through 4 players
        for (int i = 4; i <= 7; i++) 
        {
            if (Input.GetKeyDown (i.ToString ())) 
            {
                controlling = i-4;
				break;
            }
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
				break;
            }
        }
    }

	public void SetPosition(int crosshairId, bool visible, Vector2 newPosition)
    {
		visibleCrosshairs[crosshairId] = visible;

		if (visible)
		{
			int i = crosshairId * 2;
			position[i] = newPosition.x;
			position[i + 1] = newPosition.y;
		}
    }

	public Vector2 GetPosition(int crosshairId)
	{
		int i = crosshairId * 2;
		return new Vector2(position[i], position[i + 1]);
	}

    public Ray GetAimRay(Vector3 aimPosition)
    {
        return mainCamera.ScreenPointToRay(aimPosition);
    }

    public Target GetClosestTarget(Ray ray)
    {
        // Find the objects in a sphere in front of the player
        int layerColMask = LayerMask.GetMask("Enemy");
        Collider[] cols = Physics.OverlapSphere(ray.origin + ray.direction * AUTOAIM_OFFSET, AUTOAIM_RADIUS, layerColMask);
        Collider closestCol = null;
        float minDistance = AUTOAIM_DISTANCE_THRESHOLD;
        foreach (Collider col in cols)
        {
            // Find the enemy closest to the aiming direction and within the distance threshold from the aiming direction
            float aimDirectionDistance = Vector3.Cross(ray.direction, col.transform.position - ray.origin).magnitude;

            if (aimDirectionDistance < minDistance)
            {
				// Don't auto aim at hacked enemies
				EnemySyncParams enemyParams = col.gameObject.GetComponent<EnemySyncParams>();
				if (enemyParams == null || !enemyParams.GetHacked())
				{
					closestCol = col;
					minDistance = aimDirectionDistance;
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
        
	/// <summary>
	/// Updates the time a specified player last took a shot.
	/// </summary>
	/// <param name="playerId">The player's ID.</param>
	/// <param name="time">The last shot timestamp on the server.</param>
	public void UpdateLastShotTime(int playerId, float time)
	{
		lastShootTime[playerId] = Time.time;
	}

    public int GetControlling()
    {
        return controlling;
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

    public class Target
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
			this.Position = this.Object.transform.position;

			if (this.Object.CompareTag("EnemyShip"))
				return this.Position + this.Object.transform.forward * AUTOAIM_ADVANCE_OFFSET;
			else
				return this.Position;
		}
	}
}
