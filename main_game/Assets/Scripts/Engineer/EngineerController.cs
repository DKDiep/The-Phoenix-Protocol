using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class EngineerController : NetworkBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float walkSpeed;

	private float runSpeed;
	private float jumpSpeed;
	private float upMultiplier;
	private bool isWalking;
	private float stepInterval;

    private Text upgradeText;
    private Text dockText;
    private PlayerController playerController;
    private new Camera camera;
    private MouseLook mouseLook;
    private Vector2 input;
    private float m_StepCycle = 0f;
    private float m_NextStep;
    private float engineerMaxDistance;

    private float componentHealth;
    private int componentUpgradeLevel;

    private string upgradeString;
    private string repairString;
    private string dockString;

    private bool canUpgrade;
    private bool canRepair;
    private bool pressedUpgrade;
    private bool pressedRepair;
    private bool isDocked = false;
    private bool jump;

    private List<GameObject> engines;
    private List<GameObject> turrets;
    private List<GameObject> bridge;

    private GameObject playerShip;
    private GameObject dockCanvas;
    private GameObject engineerCanvas;
    private GameObject lastLookedAt;

    private Texture emptyProgressBar;
    private Texture filledProgressBar;

    private EngineerInteraction interactiveObject;
    private NetworkStartPosition startPosition;

    private Dictionary<InteractionKey, float> keyPressTime;

    // The repair and upgrade time in seconds
    // TODO: Make these depend on Engineer upgrade level
    private const float REPAIR_TIME = 5;
    private const float UPGRADE_TIME = 5;

    [SerializeField] Material defaultMat;
    [SerializeField] Material repairMat;
    [SerializeField] Material upgradeMat;


    private enum InteractionKey
    {
        Repair,
        Upgrade
    }


	// Use this for initialization
    void Start()
    {
        //Initialize with default values
        if (isServer)
            gameObject.transform.rotation = Quaternion.identity;

		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        runSpeed     = walkSpeed * 2;
        jumpSpeed    = walkSpeed;
        upMultiplier = jumpSpeed / 2;

		int enumElements = Enum.GetNames(typeof(InteractionKey)).Length;
		keyPressTime     = new Dictionary<InteractionKey, float>(enumElements);

        // Remove crosshair from this scene. 
        GameObject.Find("CrosshairCanvas(Clone)").SetActive(false);
    }

	private void LoadSettings()
	{
		walkSpeed = settings.EngineerWalkSpeed;
        engineerMaxDistance = settings.EngineerMaxDistance;
	}

    [Command]
    void CmdSetRotation(Quaternion rotation)
    {
        gameObject.transform.rotation = rotation;
    }

    [Command]
    void CmdMove(Vector2 movement, bool jumping, bool sprinting)
    {
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * movement.y + transform.right * movement.x +
            transform.up * (movement.y * gameObject.transform.rotation.x * upMultiplier);

        float speed = sprinting ? runSpeed : walkSpeed;
        Vector3 actualMove;
        actualMove.x = desiredMove.x * speed;
        actualMove.z = desiredMove.z * speed;
        actualMove.y = desiredMove.y * speed;

        if (jumping)
        {
            actualMove.y += jumpSpeed;
        }

        gameObject.transform.Translate(actualMove);
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
            upgradeString = "Press LT to upgrade";
            repairString = "Press RT to repair";
            dockString = "Press B to dock";
        }
        else
        {
            upgradeString = "Press Mouse1 to upgrade";
            repairString = "Press Mouse2 to repair";
            dockString = "Press L Shift to dock";
        }

        // Get a reference to the player ship
        playerShip = GameObject.Find("PlayerShip(Clone)");

        // The engineer hasn't looked at anything yet
        lastLookedAt = null;

        // Create the upgrade text object to use
        engineerCanvas = Instantiate(Resources.Load("Prefabs/UpgradeText")) as GameObject;
        Text[] tmp = engineerCanvas.GetComponentsInChildren<Text>();

        if (tmp[0].name.Equals("Upgrade Text"))
        {
            upgradeText = tmp[0];
            dockText = tmp[1];
        }
        else
        {
            upgradeText = tmp[1];
            dockText = tmp[0];
        }
        dockText.text = dockString;

        // Create the docked canvas, and start the engineer in the docked state
        dockCanvas = Instantiate(Resources.Load("Prefabs/DockingCanvas")) as GameObject;

        // We need a reference to the engineer start position as this is where
        // we anchor the engineer
        startPosition = playerShip.GetComponentInChildren<NetworkStartPosition>();
        gameObject.transform.parent = startPosition.transform;

        Dock();

        // Get the components of the main ship that can be upgraded and/or repaired
        EngineerInteraction[] interactionObjects = playerShip.GetComponentsInChildren<EngineerInteraction>();

        engines = new List<GameObject>();
        turrets = new List<GameObject>();
        bridge = new List<GameObject>();
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
			}
            // TODO: add shield generator
        }
    }

    /// <summary>
    /// Sets the upgradeable and repairable property
    /// for each game object in the list to value
    /// </summary>
    /// <param name="isUpgrade">Wether the job is an upgrade or a repair</param>
    /// /// <param name="value">The value to set Upgradeable/Repairable property to</param>
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
    /// Adds the uprade/repair job to the engineer's list
    /// </summary>
    /// <param name="isUpgrade">Wether the job is an upgrade or a repair</param>
    /// <param name="part">The part of the ship this job applies to</param>
	public void AddJob(bool isUpgrade, ComponentType part)
    {
		if (part == ComponentType.Turret)
        {
            this.ProcessJob(isUpgrade, true, turrets);
        }
		else if (part == ComponentType.Engine)
        {
            this.ProcessJob(isUpgrade, true, engines);
        }
		else if (part == ComponentType.Bridge)
        {
            this.ProcessJob(isUpgrade, true, bridge);
        }

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
        if (part == ComponentType.Turret)
        {
            this.ProcessJob(isUpgrade, false, turrets);
        }
        else if (part == ComponentType.Engine)
        {
            this.ProcessJob(isUpgrade, false, engines);
        }
        else if (part == ComponentType.Bridge)
        {
            this.ProcessJob(isUpgrade, false, bridge);
        }

        // Un-highlight the appropriate components
        Highlight(part);
    }

    /// <summary>
    /// Sets the health and upgrade level of the component
    /// that the engineer is currently looking at
    /// </summary>
    /// <param name="health">The health of the component</param>
    /// <param name="level">The upgrade level of the component</param>
    public void SetComponentStatus(float health, int level)
    {
        componentHealth = health;
        componentUpgradeLevel = level;
    }

    private void Update()
    {
        // Make sure this only runs on the client
        if (playerController == null || !playerController.isLocalPlayer)
            return;

        RotateView();
        if (Input.GetButton("Dock"))
        {
            Dock();
            return;
        }

        jump = Input.GetButton("Jump");
        pressedUpgrade = Input.GetButton("Upgrade");
        pressedRepair = Input.GetButton("Repair");

        // Deal with how long Upgrade and Repair have been pressed
        if (pressedUpgrade)
            keyPressTime[InteractionKey.Upgrade] += Time.deltaTime;
        else
            keyPressTime[InteractionKey.Upgrade] = 0;

        if (pressedRepair)
            keyPressTime[InteractionKey.Repair] += Time.deltaTime;
        else
            keyPressTime[InteractionKey.Repair] = 0;

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

                    // If the object we are currently looking at is not the same one
                    // we last looked at we need to request health and upgrade level values
                    if (lastLookedAt != objectLookedAt)
                    {
                        playerController.CmdGetComponentStatus(interactiveObject.Type);
                        lastLookedAt = objectLookedAt;
                    }
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
            lastLookedAt = null;
        }
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
            //CmdMove(input, jump, !isWalking);  UNCOMMENT LATER

            // If the engineer is docked we undock first
            UnDock();

            // Display health and upgrade level of the component that the engineer is looking at
            if (lastLookedAt != null)
            {
                //TODO: Replace with UI things
                Debug.Log("Health: " + componentHealth + ", UpgradeLevel: " + componentUpgradeLevel);
            }

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;

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

        // Do upgrades/repairs
        // Force engineer to repair before upgrading if
        // both are possible
        if (canRepair && keyPressTime[InteractionKey.Repair] >= REPAIR_TIME)
        {
            FinishJob(false, interactiveObject.Type);
            playerController.CmdDoRepair(interactiveObject.Type);
        }
        else if (canUpgrade && keyPressTime[InteractionKey.Upgrade] >= UPGRADE_TIME)
        {
            FinishJob(true, interactiveObject.Type);
            playerController.CmdDoUpgrade(interactiveObject.Type);
        }

        ProgressStepCycle(speed);
    }

    /// <summary>
    /// Docks the engineer if it is not docked
    /// </summary>
    private void Dock()
    {
        if (!isDocked)
        {
            isDocked = true;
            dockCanvas.SetActive(isDocked);
            engineerCanvas.SetActive(!isDocked);
            gameObject.transform.parent = startPosition.transform;
            gameObject.transform.localPosition = new Vector3(0,0,0);
            gameObject.transform.rotation = startPosition.transform.rotation;
        }
    }

    /// <summary>
    /// Un-docks the engineer if it is docked
    /// </summary>
    private void UnDock()
    {
        if (isDocked)
        {
            isDocked = false;
            dockCanvas.SetActive(isDocked);
            engineerCanvas.SetActive(!isDocked);
            gameObject.transform.parent = playerShip.transform;
        }
    }

    private void ProgressStepCycle(float speed)
    {
        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + stepInterval;
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

    private void RotateView()
    {
        mouseLook.LookRotation(transform, camera.transform);
        // Send the rotaion to the server
        //CmdSetRotation(transform.rotation);  UNCOMMENT TO SEE NETWORK ISSUES
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
        List<GameObject> toHighlight = null;

        switch (component)
        {
            case ComponentType.Engine:
                toHighlight = engines;
                break;
            case ComponentType.Turret:
                toHighlight = turrets;
                break;
            case ComponentType.Bridge:
                toHighlight = bridge;
                break;
            default:
                Debug.Log("ERROR: Failed to identify component type for highlighting");
                return;
        }

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

    private void OnGUI()
    {
        if (canRepair && keyPressTime[InteractionKey.Repair] > 0)
        {
            float progress = keyPressTime[InteractionKey.Repair] / REPAIR_TIME;
            GUI.DrawTexture(new Rect(0, 0, 100, 50), emptyProgressBar);
            GUI.DrawTexture(new Rect(0, 0, 100 * progress, 50), filledProgressBar);
        }
        else if (canUpgrade && keyPressTime[InteractionKey.Upgrade] > 0)
        {
            float progress = keyPressTime[InteractionKey.Upgrade] / UPGRADE_TIME;
            GUI.DrawTexture(new Rect(0, 0, 100, 50), emptyProgressBar);
            GUI.DrawTexture(new Rect(0, 0, 100 * progress, 50), filledProgressBar);
        }
    }
}
