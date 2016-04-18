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

	private int controlling = 0;
	private int numberOfCrossHairs;
	private bool[] init;
	private Vector3 initPos;
	private float movx;
	private float movy;
	public Vector3[] crosshairPosition;
	private float oldAccel, newAccel;
    private GameObject[] crosshairs;
	//private WiiRemoteManager wii;

	private int screenControlling = 0;
    private bool usingMouse = true;

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

    //8 floats for 4 2D positions
    public SyncListFloat position = new SyncListFloat();

    // Use this for initialization
    void Start ()
    {

		gameManager = GameObject.Find("GameManager");
		serverManager = gameManager.GetComponent<ServerManager>();
        if (ClientScene.localPlayers[0].IsValid)
            localController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        //Populate sync list with 8 floats
        for (int i = 0; i < 8; i++)
            position.Add(0.0f);
                    
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

		// Update position of crosshairs
		for (int i = 0; i    < 4; i++)
		{
			selectedCrosshair = crosshairs[i].transform;
			selectedCrosshair.position = GetPosition(i);

            Target target = GetClosestTarget(selectedCrosshair.position);
            GameObject targetObject = null;
            if (!target.IsNone())
            {
                //targets[i] = target.GetAimPosition();
                selectedCrosshair.position = mainCamera.WorldToScreenPoint(target.GetAimPosition());
                targetObject = target.Object;
            }
            else
            {
                Vector2 oldPosittion = GetPosition(controlling); 
            }
            autoaimScripts[i].Target = targetObject;

            targets[i] = mainCamera.ScreenToWorldPoint(new Vector3(selectedCrosshair.position.x, selectedCrosshair.position.y, 1000));
        }
        
        localController.CmdUpdateTargets(gameObject, targets);
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
        // If a playerId is used that requires a crosshair, enable the crosshair
        if(playerId > numberOfCrossHairs-1)
        {
            numberOfCrossHairs = playerId+1;
            for(int i = 0; i < 4; ++i)
            {
                // Show new crosshairs
                if(i >= numberOfCrossHairs) crosshairs[i].SetActive(true);
            }
        }
        // If there's an autoaim target in range, use that instead of the wii remote position
        /*Target target = GetClosestTarget(position);
        GameObject targetObject = null;
        if (!target.IsNone())
        {
			serverManager.SetCrosshairPosition(playerId, screenId, mainCamera.WorldToScreenPoint(target.GetAimPosition()));
            targetObject = target.Object;
        }
        else
        {
			Vector2 oldPosittion = GetPosition(controlling);
            serverManager.SetCrosshairPosition(playerId, screenId, position);

        }
        autoaimScripts[playerId].Target = targetObject;*/
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

        // If there's an autoaim target in range, use that instead of the cursor position
        /*Target target = GetClosestTarget(currentPosition);
		GameObject targetObject = null;
        if (!target.IsNone())
        {
			serverManager.SetCrosshairPosition(controlling, screenControlling, mainCamera.WorldToScreenPoint(target.GetAimPosition()));
			targetObject = target.Object;
        }
        else
        {
            serverManager.SetCrosshairPosition(controlling, screenControlling, currentPosition);
        }
		autoaimScripts[controlling].Target = targetObject;*/
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
        
	// Get the target closest to where the player is aiming (within bounds)
	private Target GetClosestTarget(Vector3 aimPosition)
	{
		// Cast a ray from the crosshair
		Ray ray = mainCamera.ScreenPointToRay(aimPosition);

		// Find the objects in a sphere in front of the player
		int layerColMask    = LayerMask.GetMask("Enemy");
		Collider[] cols     = Physics.OverlapSphere(ray.origin + ray.direction * AUTOAIM_OFFSET, AUTOAIM_RADIUS, layerColMask);
		Collider closestCol = null;
		float minDistance   = AUTOAIM_DISTANCE_THRESHOLD;
		foreach (Collider col in cols)
		{
			// Find the enemy closest to the aiming direction and within the distance threshold from the aiming direction
			float aimDirectionDistance = Vector3.Cross(ray.direction, col.transform.position - ray.origin).magnitude;

			// If we previously found an asteroid but there is also an enemy in range, prioritise the enemy
			if (aimDirectionDistance < minDistance)
			{
				closestCol   = col;
				minDistance  = aimDirectionDistance;
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
