using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class EngineerController : NetworkBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float walkSpeed;
    private bool runOnce;

	private float runSpeed;
	private float jumpSpeed;
	private bool isWalking;

    private Text upgradeText;
    private Text dockText;
    private Text popupText;
    private PlayerController playerController;
    private new Camera camera;
    private MouseLook mouseLook;
    private Vector2 input;
    private float engineerMaxDistance;

    private string upgradeString;
    private string repairString;
    private string dockString;
    private string popupString;

    private bool canUpgrade;
    private bool canRepair;
    private bool pressedAction;
    private bool isDocked = false;
    private bool jump;
    private bool showPopup = false;
    private bool collideFront;
    private bool collideLeft;
    private bool collideBack;
    private bool collideRight;

	private GameState gameState = null;

    private List<GameObject> engines;
    private List<GameObject> turrets;
    private List<GameObject> bridge;
	private List<GameObject> shieldGen;
	private List<GameObject> resourceStorage;

    private GameObject playerShip;
    private GameObject dockCanvas;
    private GameObject engineerCanvas;

    private Texture emptyProgressBar;
    private Texture filledProgressBar;

    private Vector2 progressBarLocation;

    private EngineerInteraction interactiveObject;
    private NetworkStartPosition startPosition;

    private Dictionary<InteractionKey, float> keyPressTime;

	private float workTime; // The repair and upgrade time in seconds

	private Material defaultMat;
    private Material repairMat;
    private Material upgradeMat;

    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private bool updateRotation;

    private enum InteractionKey
    {
        Action,
        Popup
    }


	// Use this for initialization
    void Start()
    {
        runOnce = false;
        currentPosition = previousPosition = Vector3.zero;
        updateRotation = false;
        StartCoroutine(SetParent());
        Setup();
    }

    IEnumerator SetParent()
    {
        yield return new WaitForSeconds(0.1f);
        playerShip = GameObject.Find("PlayerShip(Clone)");
        if(playerShip != null)
            transform.parent = playerShip.transform;
        else
            StartCoroutine(SetParent());
    }

    public void Reset()
    {
        RpcReset();
    }

    [ClientRpc]
    private void RpcReset()
    {
        // Re-initialise (which would also dock)
        if (camera != null && playerController != null)
        {
            Setup();
            Initialize(camera.gameObject, playerController);
        }
    }

    public void Setup()
    {
        //Initialize with default values
        if (isServer)
            gameObject.transform.rotation = Quaternion.identity;

        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        LoadSettings();

        runSpeed = walkSpeed * 2;
        jumpSpeed = walkSpeed;

        int enumElements = Enum.GetNames(typeof(InteractionKey)).Length;
        keyPressTime = new Dictionary<InteractionKey, float>(enumElements);

        // Initialize with keys
        keyPressTime[InteractionKey.Action] = 0f;
        keyPressTime[InteractionKey.Popup] = 0f;
    }

    private void LoadSettings()
	{
        engineerMaxDistance = settings.EngineerMaxDistance;
        emptyProgressBar = settings.EmptyProgressBar;
        filledProgressBar = settings.FilledProgressBar;
        defaultMat = settings.EngineerDefaultMat;
        repairMat = settings.EngineerRepairMat;
        upgradeMat = settings.EngineerUpgradeMat;
	}

    /// <summary>
    /// Use this to initialize the Engineer once the main_game
    /// scene has been loaded. This acts as a replacement for Unity's
    /// Start() method. The Start() method can still be used to perform
    /// any initialization that does not require the main_game scene to be loaded
    /// </summary>
    /// <param name="cam">The camera object of the engineer</param>
    /// <param name="controller">The player controller of the engineer</param>
    public void Initialize(GameObject cam, PlayerController controller)
    {
        // Initialize the camera and the MouseLook script
        camera = cam.GetComponent<Camera>();
        mouseLook = gameObject.GetComponent<MouseLook>();
        mouseLook.Init(transform, camera.transform);
        playerController = controller;

        // Set the upgrade and repair strings depending on wheter
        // a controller is used or the keyboard is used
        if (Input.GetJoystickNames().Length > 0)
        {
            upgradeString = "Hold A to upgrade";
            repairString = "Hold A to repair";
            dockString = "";
            //popupString = "Job finished. Press B to dock, or continue doing jobs";
        }
        else
        {
            upgradeString = "Hold Mouse1 to upgrade";
            repairString = "Hold Mouse2 to repair";
            dockString = "Press L Shift to dock";
            popupString = "";
        }

        // Set the progress bar location
        progressBarLocation = new Vector2((Screen.width / 2) - 50, (Screen.height / 2) + 130);

        // Get a reference to the player ship
        playerShip = GameObject.Find("PlayerShip(Clone)");

        // Create the upgrade text object to use
        engineerCanvas = Instantiate(Resources.Load("Prefabs/EngineerCanvas")) as GameObject;
        Text[] tmp = engineerCanvas.GetComponentsInChildren<Text>();

        foreach (Text t in tmp)
        {
            if (t.name.Equals("Upgrade Text"))
                upgradeText = t;
            else if (t.name.Equals("Dock Text"))
                dockText = t;
            else if (t.name.Equals("Popup Text"))
                popupText = t;
        }
        dockText.text = dockString;

        // Create the docked canvas, and start the engineer in the docked state
        dockCanvas = Instantiate(Resources.Load("Prefabs/DockingCanvas")) as GameObject;

        // We need a reference to the engineer start position as this is where
        // we anchor the engineer
        startPosition = playerShip.GetComponentInChildren<NetworkStartPosition>();
        gameObject.transform.parent = playerShip.transform;

        Dock();

        // Get the components of the main ship that can be upgraded and/or repaired
        EngineerInteraction[] interactionObjects = playerShip.GetComponentsInChildren<EngineerInteraction>();

        engines 		= new List<GameObject>();
        turrets 		= new List<GameObject>();
        bridge 			= new List<GameObject>();
		shieldGen 		= new List<GameObject>();
		resourceStorage = new List<GameObject>();
		foreach (EngineerInteraction interaction in interactionObjects)
        {
            // Ensure that the properties of the Interaction
            // script are initialized as normally they are only
            // initialized on the server side
            interaction.Initialize();

			switch (interaction.Type)
			{
			case ComponentType.Engine:
				engines.Add (interaction.gameObject);
				break;
			case ComponentType.Bridge:
                bridge.Add (interaction.gameObject);
				break;
			case ComponentType.Turret:
				turrets.Add (interaction.gameObject);
				break;
			case ComponentType.ShieldGenerator:
				shieldGen.Add(interaction.gameObject);
				break;
			case ComponentType.ResourceStorage:
				resourceStorage.Add(interaction.gameObject);
				break;
			}
        }
    }

    /// <summary>
    /// Sets the upgradeable and repairable property
    /// for each game object in the list to value
    /// </summary>
    /// <param name="isUpgrade">Wether the job is an upgrade or a repair</param>
    /// <param name="value">The value to set Upgradeable/Repairable property to</param>
    /// <param name="parts">The list of parts this job applies to</param>
    private void ProcessJob(bool isUpgrade, bool value, List<GameObject> parts)
    {
        foreach (GameObject obj in parts)
        {
            EngineerInteraction interaction = obj.GetComponent<EngineerInteraction>();

            if (isUpgrade)
                interaction.Upgradeable = value;
            else
                interaction.Repairable = value;
        }
    }

	/// <summary>
	/// Gets list of parts of a specified type.
	/// </summary>
	/// <returns>The part list.</returns>
	/// <param name="type">The component type.</param>
	private List<GameObject> GetPartListByType(ComponentType type)
	{
		List<GameObject> partList = null;

		switch (type)
		{
		case ComponentType.Turret:
			partList = turrets;
			break;
		case ComponentType.Engine:
			partList = engines;
			break;
		case ComponentType.Bridge:
			partList = bridge;
			break;
		case ComponentType.ShieldGenerator:
			partList = shieldGen;
			break;
		case ComponentType.ResourceStorage:
			partList = resourceStorage;
			break;
		}

		return partList;
	}

    /// <summary>
    /// Adds the uprade/repair job to the engineer's list
    /// </summary>
    /// <param name="isUpgrade">Wether the job is an upgrade or a repair</param>
    /// <param name="part">The part of the ship this job applies to</param>
	public void AddJob(bool isUpgrade, ComponentType part)
    {
		List<GameObject> partList = GetPartListByType(part);

		this.ProcessJob(isUpgrade, true, partList);

        // Highlight the appropriate components
        Highlight(part);
    }

    /// <summary>
    /// Resets the Upgradeable/Repairable attribute of the object
    /// that has been upgraded/repaired thus taking the job off the queue
    /// </summary>
    /// <param name="isUpgrade"></param>
    /// <param name="part"></param>
    private void FinishJob(bool isUpgrade, ComponentType part)
    {
		List<GameObject> partList = GetPartListByType(part);

		this.ProcessJob(isUpgrade, false, partList);

        // Un-highlight the appropriate components
        Highlight(part);
    }

    [Command]
    public void CmdUpdatePosition(Vector3 pos)
    {
        RpcUpdatePosition(pos);
    }

    [ClientRpc]
    public void RpcUpdatePosition(Vector3 pos)
    {
        if(playerController == null || !playerController.isLocalPlayer)
            transform.localPosition = pos;
    }

    IEnumerator UpdateRotation()
    {
        CmdUpdateRotation(transform.rotation);
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(UpdateRotation());
    }

    [Command]
    public void CmdUpdateRotation(Quaternion rotation)
    {
        RpcUpdateRotation(rotation);
    }

    [ClientRpc]
    public void RpcUpdateRotation(Quaternion rotation)
    {
        if(playerController == null || !playerController.isLocalPlayer)
            transform.rotation = rotation;
    }


    private void Update()
    {
        // Make sure this only runs on the client
        if (playerController == null || !playerController.isLocalPlayer)
            return;

        if(!updateRotation)
        {
            StartCoroutine(UpdateRotation());
            updateRotation = true;
        }

        currentPosition = transform.localPosition;

        if(Vector3.Distance(currentPosition, previousPosition) > 0.01f)
        {
            CmdUpdatePosition(currentPosition);
            previousPosition = currentPosition;
        }

        if(!runOnce)
        {
            Camera.main.gameObject.GetComponent<VideoGlitches.VideoGlitchVHSPause>().enabled = true;
            Camera.main.gameObject.GetComponent<VignetteAndChromaticAberration>().enabled = true;
            runOnce = true;
        }


        RotateView();
        if (Input.GetButton("Dock"))
        {
            Dock();
            return;
        }

        jump = Input.GetButton("Jump");
        pressedAction = Input.GetButton("Action");

        // Deal with how long the action button has been pressed
        if (pressedAction && interactiveObject != null)
            keyPressTime[InteractionKey.Action] += Time.deltaTime;
        else
            keyPressTime[InteractionKey.Action] = 0f;

        // Artificial way of having the popup show for 2 seconds
        if (showPopup)
        {
            keyPressTime[InteractionKey.Popup] += Time.deltaTime;
            popupText.text = popupString;
        }
        else
        {
            keyPressTime[InteractionKey.Popup] = 0;
            popupText.text = "";
        }

        // Do forward raycast from camera to the center of the screen to see if an upgradeable object is in front of the player
        int x = Screen.width / 2;
        int y = Screen.height / 2;
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hitInfo;
        canUpgrade = false;
        canRepair = false;

        if (Physics.Raycast(ray, out hitInfo, 10.0f))
        {
            if (hitInfo.collider.CompareTag("Player"))
            {
                // Get the game object the engineer is currently looking at
                // and get the EngineerInteraction script attached if it has one
                GameObject objectLookedAt = hitInfo.collider.gameObject;
                interactiveObject = objectLookedAt.GetComponent<EngineerInteraction>();

                // If the object being looked at has an EngineerInteraction
                // script we use it
                if (interactiveObject != null)
                {
                    canUpgrade = interactiveObject.Upgradeable;
                    canRepair = interactiveObject.Repairable;
                }
            }

            if (canRepair)
                upgradeText.text = repairString;
            else if (canUpgrade)
                upgradeText.text = upgradeString;
            else
                ResetUpgradeText();
        }
        else
        {
            ResetUpgradeText();
            interactiveObject = null;
        }

        // Cast rays for checking collisions
        // Forward ray
        if (Physics.Raycast(transform.position, transform.forward, 9f))
            collideFront = true;
        else
            collideFront = false;

        // Left ray
        if (Physics.Raycast(transform.position, -transform.right, 9f))
            collideLeft = true;
        else
            collideLeft = false;

        // Back ray
        if (Physics.Raycast(transform.position, -transform.forward, 9f))
            collideBack = true;
        else
            collideBack = false;

        // Right ray
        if (Physics.Raycast(transform.position, transform.right, 9f))
            collideRight = true;
        else
            collideRight = false;
    }

    private void FixedUpdate()
    {
        // Make sure this only runs on the client
        if (playerController == null || !playerController.isLocalPlayer)
            return;

        float speed;
        GetInput(out speed);

        // Move the player if they have moved
        if (input.x != 0 || input.y != 0 || jump == true)
        {
            // If the engineer is docked we undock first
            UnDock();

            // always move along the camera forward as it is the direction that it being aimed at
            // using the collision info to decide wheter to move in a direction or not
            float forwardMul;
            float rightMul;

            if (collideFront && input.y > 0)
                forwardMul = 0;
            else if (collideBack && input.y < 0)
                forwardMul = 0;
            else
                forwardMul = 1;

            if (collideLeft && input.x < 0)
                rightMul = 0;
            else if (collideRight && input.x > 0)
                rightMul = 0;
            else
                rightMul = 1;

            Vector3 desiredMove = transform.forward * forwardMul * input.y + transform.right * rightMul * input.x;

            Vector3 actualMove;
            actualMove.x = desiredMove.x * speed;
            actualMove.z = desiredMove.z * speed;
            actualMove.y = desiredMove.y * speed;

            if (jump)
            {
                actualMove.y += jumpSpeed;
            }

            // Only move the engineer if it is within the bounds of the player ship
            Vector3 newPosition = transform.position + actualMove;
            if (Vector3.Distance(newPosition, playerShip.transform.position) < engineerMaxDistance)
                transform.position = newPosition;
        }

        // If the popup has been show for the required amount of time then
        // we make it disappear
		if (showPopup && keyPressTime[InteractionKey.Popup] >= workTime)
            showPopup = false;

        // Do upgrades/repairs
        // Force engineer to repair before upgrading if
        // both are possible
		if (canRepair && keyPressTime[InteractionKey.Action] >= workTime)
        {
            keyPressTime[InteractionKey.Action] = 0;
            FinishJob(false, interactiveObject.Type);
            playerController.CmdDoRepair(interactiveObject.Type);
            showPopup = true;
        }
		else if (canUpgrade && keyPressTime[InteractionKey.Action] >= workTime)
        {
            keyPressTime[InteractionKey.Action] = 0;
            FinishJob(true, interactiveObject.Type);
            playerController.CmdDoUpgrade(interactiveObject.Type);
            showPopup = true;
        }
    }

    /// <summary>
    /// Docks the engineer if it is not docked
    /// </summary>
    private void Dock()
    {
		if (isDocked)
			return; 
        
        isDocked = true;
        dockCanvas.SetActive(isDocked);
        engineerCanvas.SetActive(!isDocked);
        //gameObject.transform.parent = startPosition.transform;
        gameObject.transform.position = startPosition.transform.position;
        gameObject.transform.rotation = startPosition.transform.rotation;

		// Update the drone stats
		if (gameState == null) // This is needed because the engineer can't always get the game state from the start
			gameState = GameObject.Find("GameManager").GetComponent<GameState>();
		if (gameState != null)
			gameState.GetDroneStats(out walkSpeed, out workTime);
    }

    /// <summary>
    /// Un-docks the engineer if it is docked
    /// </summary>
    private void UnDock()
    {
		if (!isDocked)
			return;
	
        isDocked = false;
        dockCanvas.SetActive(isDocked);
        engineerCanvas.SetActive(!isDocked);
        gameObject.transform.parent = playerShip.transform;
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        isWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = isWalking ? walkSpeed : runSpeed;
        input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }
    }

    /// <summary>
    /// Used to rotate the engineer camera based on mouse movement
    /// </summary>
    private void RotateView()
    {
        mouseLook.LookRotation(transform, camera.transform);
    }

    private void ResetUpgradeText()
    {
        upgradeText.text = "";
    }

    /// <summary>
    /// Highlights all components of type component
    /// </summary>
    /// <param name="component">The components to highlight</param>
    private void Highlight(ComponentType component)
    {
        // The list of game objects that need to be highlighted
		List<GameObject> toHighlight = GetPartListByType(component);

        for(int i = 0; i < toHighlight.Count; i++)
        {
            GameObject part = toHighlight[i];
            EngineerInteraction interaction = part.GetComponent<EngineerInteraction>();

            if (interaction == null)
            {
                Debug.Log("EngineerInteraction component could not be found");
            }
            else
            {
                Renderer renderer = part.GetComponent<Renderer>();
                Material[] mats = renderer.materials;

                if (interaction.Repairable)
                {
                    for(int j = 0; j < mats.Length; ++j)
                        mats[j] = repairMat;
                }
                else if(interaction.Upgradeable)
                {
                    for(int j = 0; j < mats.Length; ++j)
                        mats[j] = upgradeMat;
                }
                else
                {
                    // Default
                    for(int j = 0; j < mats.Length; ++j)
                        mats[j] = defaultMat;
                }

                renderer.materials = mats;
            }
        }
    }

    /// <summary>
    /// Listens for GUI events. Used to draw the upgrade/repair
    /// progress bar
    /// </summary>
    private void OnGUI()
    {
        if (canRepair && keyPressTime[InteractionKey.Action] > 0)
        {
			float progress = keyPressTime[InteractionKey.Action] / workTime;
            GUI.DrawTexture(new Rect(progressBarLocation.x, progressBarLocation.y, 100, 11), emptyProgressBar);
            GUI.DrawTexture(new Rect(progressBarLocation.x+3, progressBarLocation.y+3, 97 * progress, 5), filledProgressBar);
        }
        else if (canUpgrade && keyPressTime[InteractionKey.Action] > 0)
        {
			float progress = keyPressTime[InteractionKey.Action] / workTime;
            GUI.DrawTexture(new Rect(progressBarLocation.x, progressBarLocation.y, 100, 11), emptyProgressBar);
            GUI.DrawTexture(new Rect(progressBarLocation.x+3, progressBarLocation.y+3, 97 * progress, 5), filledProgressBar);
        }
    }
}
