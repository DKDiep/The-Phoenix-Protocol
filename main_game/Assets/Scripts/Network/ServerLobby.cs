using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ServerLobby : MonoBehaviour {
    private ServerManager serverManager;
    private TCPServer tcpServer;
    private UDPServer udpServer;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject canvasObject;
    [SerializeField] private GameObject cameraPanel;
    [SerializeField] private GameObject engineerPanel;
    [SerializeField] private GameObject commandPanel;
    [SerializeField] private Button startButton;
	#pragma warning restore 0649

    private uint serverId;
    private int centreIndex;

    // For camera index on join
    private int left = 0, right = -1;

    // Store left and right value of camera rotation to align screens
    public struct CameraOrientation
    {
        // Key for dictionary to access adjacent left/right
        public int leftId;
        public int rightId;

        // Own left/right normalised Y values
        public float left;
        public float right;
    }

    private Dictionary<uint, CameraOrientation> OrientationDictionary;

    // Use this for initialization
    void Awake ()
    {
        GameObject server = GameObject.Find("GameManager");
        if (server != null)
        {
            serverManager = server.GetComponent<ServerManager>();
            tcpServer = server.GetComponent<TCPServer>();
            udpServer = server.GetComponent<UDPServer>();
            startButton.onClick.AddListener(() => OnClickStartButton());
            canvasObject.transform.Find("IPAddress").GetComponent<Text>().text = Network.player.ipAddress;
        }
    }

    public void OnClickStartButton ()
    {
        // Pass lobby information to server
        PlayerTokenController[] engTokens = engineerPanel.transform.GetComponentsInChildren<PlayerTokenController>();
        uint[] engControllerIds = new uint[engTokens.Length];

        int i = 0;
        foreach (PlayerTokenController engToken in engTokens)
        {
            engControllerIds[i] = engToken.GetPlayerController().netId.Value;
            i++;
        }

        // Tell the server which clients will be engineers
        serverManager.SetEngineers(engControllerIds);

        // Get cameras
        PlayerTokenController[] cameraTokens = cameraPanel.transform.GetComponentsInChildren<PlayerTokenController>();
        uint[] cameraControllerIds = new uint[cameraTokens.Length];

        i = 0;
        foreach (PlayerTokenController cameraToken in cameraTokens)
        {
            uint currControllerId = cameraToken.GetPlayerController().netId.Value;

            // Make sure we don't send the server's ID to be associated as a camera
            // This is because the server implicitly knows that it is a camera
            // And it should not treat itself as a client
            if (currControllerId != serverId)
                cameraControllerIds[i] = currControllerId;

            i++;
        }

        // Tell the server manager which clients will be cameras
        serverManager.SetCameras(cameraControllerIds);

        PlayerTokenController commandConsole = commandPanel.transform.GetComponentInChildren<PlayerTokenController>();
        if (commandConsole != null) 
        {
            // Tell the server which client will be the commander
            serverManager.SetCommander(commandConsole.GetPlayerController().netId.Value);
        }

		serverManager.SetServerId(serverId);

        // Populate dictionary matching screen IDs of player controllers to canvas objects
        for (i = 0; i < cameraPanel.transform.childCount; i++)
        {
            int index = i - centreIndex;
            // Instantiate crosshair to be spawned
            GameObject crosshairObject = Instantiate(Resources.Load("Prefabs/CrosshairCanvas", typeof(GameObject))) as GameObject;
           
			// Network spawn a crosshair canvas for each camera
            ServerManager.NetworkSpawn(crosshairObject);
            
			// Add after spawning
            serverManager.RpcAddCrosshairObject(index, crosshairObject);
        }
			
		Instantiate(Resources.Load("Prefabs/GameTimerCanvas", typeof(GameObject)));

        startButton.onClick.RemoveAllListeners();
        // Start game only spawns, call begin to play
        serverManager.StartGame();
        
        // Init the Network Servers
        udpServer.Initialise();
        tcpServer.Initialise();
        
        // Instantiate ready screen then remove listener and destroy self
        GameObject readyScreen = Instantiate(Resources.Load("Prefabs/ReadyCanvas", typeof(GameObject))) as GameObject;
        ServerManager.NetworkSpawn(readyScreen);
        serverManager.SetReadyScreen(readyScreen);
        Destroy(this.gameObject);
    }

    // This method is only called on server via command
    public void PlayerJoin(GameObject playerObject, RoleEnum role)
    {
        // Get player and set chosen role
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
		playerController.RpcSetRole(role);

        // Create token for new player
        GameObject playerToken = Instantiate(Resources.Load("Prefabs/PlayerToken", typeof(GameObject))) as GameObject;

        // Set parent based on role
        if (role == RoleEnum.Camera)
        {
            playerToken.transform.SetParent(cameraPanel.transform, false);

            if (right == -1)
            {
                right = 0;
                serverId = playerObject.GetComponent<PlayerController>().netId.Value;
            }
            else
            {
                if (right == -left)
                {
                    right = right + 1;
                    playerToken.transform.SetAsLastSibling();
                }
                else
                {
                    left = left - 1;
                    playerToken.transform.SetAsFirstSibling();
                }
            }
        }
        else if (role == RoleEnum.Engineer)
        {
            playerToken.transform.SetParent(engineerPanel.transform, false);
            playerToken.transform.SetAsLastSibling();
        }
        else
        {
            playerToken.transform.SetParent(commandPanel.transform, false);
        }

        playerToken.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // Reference player controller to change variables with token
        playerToken.GetComponent<PlayerTokenController>().SetPlayerController(playerObject);
        playerToken.transform.Find("UserId").GetComponent<Text>().text = "NetId: "+playerController.netId.ToString();
        
        EventTrigger trigger = playerToken.GetComponent<EventTrigger>();
        EventTrigger.Entry entryRelease = new EventTrigger.Entry();
        entryRelease.eventID = EventTriggerType.EndDrag;
        entryRelease.callback.AddListener((eventData) => { SortToken(playerToken); });
        trigger.triggers.Add(entryRelease);

        EventTrigger.Entry entryDrag = new EventTrigger.Entry();
        entryDrag.eventID = EventTriggerType.Drag;
        entryDrag.callback.AddListener((eventData) => { playerToken.GetComponent<PlayerTokenController>().OnDrag(); });
        trigger.triggers.Add(entryDrag);

        UpdateCameras();
    }

    public void SortToken(GameObject playerToken)
    {
        // Assign role based on proximity to panel
        Vector3 position = playerToken.transform.position;
        float distanceCamera = Vector3.Distance(position, cameraPanel.transform.position);
        float distanceEngineer = Vector3.Distance(position, engineerPanel.transform.position);
        float distanceCommand = Vector3.Distance(position, commandPanel.transform.position);
        if (distanceCamera < distanceEngineer && distanceCamera < distanceCommand)
        {
            // Parent to closest panel
            playerToken.transform.SetParent(cameraPanel.transform, false);

            // Set new role using referenced player controller
			playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole(RoleEnum.Camera);

            // Sort into closest order
            int index = 0;
            for (int i = 0; i < cameraPanel.transform.childCount; i++)
            {
                Vector3 childPosition = cameraPanel.transform.GetChild(i).position;
                if (position.x > childPosition.x)
                {
                    index = i;
                }
            }

            // Place in order
            playerToken.transform.SetSiblingIndex(index);
        }
        else if (serverId != playerToken.GetComponent<PlayerTokenController>().GetPlayerController().netId.Value) // Server must be main camera
        {
            if (distanceEngineer < distanceCommand)
            {
                playerToken.transform.SetParent(engineerPanel.transform, false);
				playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole(RoleEnum.Engineer);
            }
            else if (commandPanel.transform.childCount == 0) // Only assign if there is no commander
            {
                playerToken.transform.SetParent(commandPanel.transform, false);
				playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole(RoleEnum.Commander);
            }
        }

        // Rebuild auto layout
        LayoutRebuilder.MarkLayoutForRebuild(playerToken.transform as RectTransform);

        // Update cameras for preview
        UpdateCameras();
    }

    private void UpdateCameras()
    {
        // Work out standard rotation for each index
        GameObject playerCamera = GameObject.Find("CameraManager(Clone)");

        // Get camera frustum planes
        Camera cam = playerCamera.GetComponent<Camera>();

        // Calculate frustum height at far clipping plane using field of view
        //float frustumHeight = 2.0f * cam.farClipPlane * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float frustumHeight = 2.0f * cam.farClipPlane * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        var distance = frustumHeight * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float fov = 2.0f * Mathf.Atan(frustumHeight * 0.5f / distance) * Mathf.Rad2Deg;

        // Calculate frustum width using height and camera aspect
        float frustumWidth = frustumHeight * cam.aspect;

        // Calculate left and right vectors of frustum
        Vector3 of = (Vector3.forward * cam.farClipPlane);
        Vector3 ofr = of + (Vector3.right * frustumWidth / 2.0f);
        Vector3 ofl = of + (Vector3.left * frustumWidth / 2.0f);
        Quaternion q = Quaternion.FromToRotation(ofl, ofr);
        float borderRatio = 1.10f;
        // Multiple by size including border percentage
        float y = q.eulerAngles.y*borderRatio;
        float rotateAngle;

        // Uncomment to instantiate test cameras locally
        /*GameObject leftCam = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
        Quaternion leftQ = Quaternion.Euler(new Vector3(0, -y, 0));
        leftCam.transform.localRotation = leftQ;
        GameObject rightCam = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
        Quaternion rightQ = Quaternion.Euler(new Vector3(0, y, 0));
        rightCam.transform.localRotation = rightQ;*/

        for (int i = 0; i < cameraPanel.transform.childCount; i++)
        {
            PlayerController playerController = cameraPanel.transform.GetChild(i).gameObject.GetComponent<PlayerTokenController>().GetPlayerController();
            
			// Get index of centre server)
            if (playerController.netId.Value == serverId)
            {
                centreIndex = i;
            }
        }

        for (int i = 0; i < cameraPanel.transform.childCount; i++)
        {
            int index = i - centreIndex;
            PlayerController playerController = cameraPanel.transform.GetChild(i).gameObject.GetComponent<PlayerTokenController>().GetPlayerController();
            playerController.RpcSetCameraIndex(index);
            rotateAngle = y * (float)index;
            playerController.RpcRotateCamera(rotateAngle, playerController.netId.Value);
        }

        // Reset camera for other roles
        for (int i = 0; i < engineerPanel.transform.childCount; i++)
        {
            PlayerController playerController = engineerPanel.transform.GetChild(i).gameObject.GetComponent<PlayerTokenController>().GetPlayerController();
            playerController.RpcSetCameraIndex(0);
            playerController.RpcRotateCamera(0.0f, playerController.netId.Value);
        }
        
        for (int i = 0; i < commandPanel.transform.childCount; i++)
        {
            PlayerController playerController = commandPanel.transform.GetChild(i).gameObject.GetComponent<PlayerTokenController>().GetPlayerController();
            playerController.RpcSetCameraIndex(0);
            playerController.RpcRotateCamera(0.0f, playerController.netId.Value);
        }
    }
}
