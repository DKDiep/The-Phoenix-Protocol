using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EngineerController : NetworkBehaviour {

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] private float upMultiplier;
    [SerializeField] private bool isWalking;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float m_StepInterval;
	#pragma warning restore 0649

    private Text upgradeText;
    private Text dockText;
    private PlayerController myController;
    private new Camera camera;
    private MouseLook mouseLook;
    private bool jump;
    private Vector2 input;
    private float m_StepCycle = 0f;
    private float m_NextStep;
    private string upgradeString;
    private string repairString;
    private string dockString;
    private bool canUpgrade;
    private bool canRepair;
    private bool pressedUpgrade;
    private bool pressedRepair;
    private bool isDocked = false;
    private EngineerInteraction interactiveObject;
    private GameObject playerShip;
    private GameObject dockCanvas;
    private GameObject engineerCanvas;
    private NetworkStartPosition startPosition;

    private List<GameObject> engines;
    private List<GameObject> turrets;
    private GameObject bridge;


	// Use this for initialization
    void Start()
    {
        //Initialize with default values
        if (isServer)
        {
            gameObject.transform.rotation = Quaternion.identity;
        }

        walkSpeed = 2;
        runSpeed = walkSpeed * 2;
        jumpSpeed = walkSpeed;
        upMultiplier = jumpSpeed / 2;
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

    //Initialize player instance and local game here. Kind of a replacement for Start()
    public void Initialize(GameObject cam, PlayerController controller)
    {
        camera = cam.GetComponent<Camera>();
        mouseLook = gameObject.GetComponent<MouseLook>();
        mouseLook.Init(transform, camera.transform);

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
        foreach (EngineerInteraction interaction in interactionObjects)
        {
			switch (interaction.Type)
			{
			case ComponentType.Engine:
				engines.Add (interaction.gameObject);
				break;
			case ComponentType.Bridge:
				bridge = interaction.gameObject;
				break;
			case ComponentType.Turret:
				turrets.Add (interaction.gameObject);
				break;
			}
            // TODO: add shield generator
        }
    }

    // Helper for AddJob
    private void ProcessJob(bool upgrade, List<GameObject> parts)
    {
        foreach (GameObject obj in parts)
        {
            EngineerInteraction interaction = obj.GetComponent<EngineerInteraction>();

            if (upgrade)
                interaction.Upgradeable = true;
            else
                interaction.Repairable = true;
        }
    }

    // Adds a job for the engineer to do
	public void AddJob(bool upgrade, ComponentType part)
    {
		if (part == ComponentType.Turret)
        {
            this.ProcessJob(upgrade, turrets);
        }
		else if (part == ComponentType.Engine)
        {
            this.ProcessJob(upgrade, engines);
        }
		else if (part == ComponentType.Bridge)
        {
            EngineerInteraction interaction = bridge.GetComponent<EngineerInteraction>();
            if (upgrade)
                interaction.Upgradeable = true;
            else
                interaction.Repairable = true;
        }
    }

    // Update is called once per frame
    public void EngUpdate()
    {
        RotateView();

        if (Input.GetButton("Dock"))
        {
            Dock();
            return;
        }

        jump = Input.GetButton("Jump");
        pressedUpgrade = Input.GetButton("Upgrade");
        pressedRepair = Input.GetButton("Repair");

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
                interactiveObject = hitInfo.collider.gameObject.GetComponent<EngineerInteraction>();

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
        }
    }

    public void EngFixedUpdate()
    {
        float speed;
        GetInput(out speed);

        // Move the player if they have moved
        if (input.x != 0 || input.y != 0 || jump == true)
        {
            //CmdMove(input, jump, !isWalking);  UNCOMMENT LATER

            // If the engineer is docked we undock first
            UnDock();

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

            transform.position += actualMove;
        }

        // Do upgrades/repairs
        // Force engineer to repair before upgrading if
        // both are possible
        if (canRepair && pressedRepair)
            Debug.Log("Repair");
        else if (canUpgrade && pressedUpgrade)
            Debug.Log("Upgrade");

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
            gameObject.transform.parent = null;
        }
    }

    private void ProgressStepCycle(float speed)
    {
        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;
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
}
