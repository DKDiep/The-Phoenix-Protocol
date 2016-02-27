using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ServerLobby : MonoBehaviour {
    private ServerManager serverManager;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject canvasObject;
    [SerializeField] private GameObject cameraPanel;
    [SerializeField] private GameObject engineerPanel;
    [SerializeField] private GameObject commandPanel;
    [SerializeField] private Button startButton;
	#pragma warning restore 0649

    private uint serverId;

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
            startButton.onClick.AddListener(() => OnClickStartButton());
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

        // Start the game
        serverManager.StartGame();
        startButton.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }

    // This method is only called on server via command
    public void PlayerJoin(GameObject playerObject)
    {
        // Get player and set default role
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
		playerController.RpcSetRole(RoleEnum.Camera);

        // Create token for new player
        GameObject playerToken = Instantiate(Resources.Load("Prefabs/PlayerToken", typeof(GameObject))) as GameObject;
        playerToken.transform.SetParent(cameraPanel.transform, false);
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

        UpdateCameras();
    }

    public void SortToken(GameObject playerToken)
    {
        Debug.Log(playerToken.transform.position);

        // Assign role based on proximity to panel
        Vector3 position = playerToken.transform.position;
        float distanceCamera = Vector3.Distance(position, cameraPanel.transform.position);
        float distanceEngineer = Vector3.Distance(position, engineerPanel.transform.position);
        float distanceCommand = Vector3.Distance(position, commandPanel.transform.position);
        if (distanceCamera < distanceEngineer && distanceCamera < distanceCommand)
        {
            Debug.Log("camera");

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
                Debug.Log("engineer");
                playerToken.transform.SetParent(engineerPanel.transform, false);
				playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole(RoleEnum.Engineer);
            }
            else if (commandPanel.transform.childCount == 0) // Only assign if there is no commander
            {
                Debug.Log("commander");
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
        float frustumHeight = 2.0f * cam.farClipPlane * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Calculate frustum width using height and camera aspect
        float frustumWidth = frustumHeight * cam.aspect;

        // Calculate left and right vectors of frustum
        Vector3 of = (playerCamera.transform.localRotation * Vector3.forward * cam.farClipPlane) - playerCamera.transform.localPosition;
        Vector3 ofr = of + (playerCamera.transform.localRotation * Vector3.right * frustumWidth / 2.0f);
        Vector3 ofl = of + (playerCamera.transform.localRotation * Vector3.left * frustumWidth / 2.0f);
        Quaternion q = Quaternion.FromToRotation(ofl, ofr);
        float y = q.eulerAngles.y;
        float rotateAngle;
        int centreIndex = 0;

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
            rotateAngle = y * (index);
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
