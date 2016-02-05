using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ServerLobby : MonoBehaviour {
    private ServerManager serverManager;
    private GameState gameState;

    [SerializeField]
    private GameObject canvasObject;
    [SerializeField]
    private GameObject cameraPanel;
    [SerializeField]
    private GameObject engineerPanel;
    [SerializeField]
    private GameObject commandPanel;
    [SerializeField]
    private Button startButton;

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
            gameState = server.GetComponent<GameState>();
            startButton.onClick.AddListener(() => OnClickStartButton());
        }
    }
	
	public void OnClickStartButton ()
    {
        // Pass lobby information to server
        PlayerTokenController[] engTokens = engineerPanel.transform.GetComponentsInChildren<PlayerTokenController>();
        short[] engControllerIds = new short[engTokens.Length];

        int i = 0;
        foreach (PlayerTokenController engToken in engTokens)
        {
            engControllerIds[i] = engToken.GetPlayerController().playerControllerId;
            i++;
        }
        serverManager.SetEngineers(engControllerIds);

        serverManager.StartGame();
        startButton.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }

    // This method is only called on server via command
    public void PlayerJoin(GameObject playerObject)
    {
        // Get player and set default role
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        playerController.RpcSetRole("camera");

        // Create token for new player
        GameObject playerToken = Instantiate(Resources.Load("Prefabs/PlayerToken", typeof(GameObject))) as GameObject;
        playerToken.transform.SetParent(cameraPanel.transform, false);
        playerToken.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        // Reference player controller to change variables with token
        playerToken.GetComponent<PlayerTokenController>().SetPlayerController(playerObject);
        playerToken.transform.Find("UserId").GetComponent<Text>().text = "NetId: "+playerController.netId.ToString();

        EventTrigger trigger = playerToken.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.EndDrag;
        entry.callback.AddListener((eventData) => { SortToken(playerToken); });
        trigger.triggers.Add(entry);

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

        if (right == -1)
        {
            right = 0;
        }
        else
        {
            if (right == -left)
            {
                right = right + 1;
                playerController.RpcSetCameraIndex(right);
                rotateAngle = y * right;
                playerToken.transform.SetAsLastSibling();
            }
            else
            {
                left = left - 1;
                playerController.RpcSetCameraIndex(left);
                rotateAngle = y * left;
                playerToken.transform.SetAsFirstSibling();
            }
            Vector3 r = q.eulerAngles;
            playerController.RpcRotateCamera(rotateAngle, playerController.netId.Value);
        }
    }

    public void SortToken(GameObject playerToken)
    {
        //playerToken.transform.parent = null;
        Debug.Log(playerToken.transform.position);
        // assign role based on proximity to panel
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
            playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole("camera");
            // Sort into closest order
            /*int index = 0;
            for (int i = 0; i < cameraPanel.transform.childCount; i++)
            {
                Vector3 childPosition = cameraPanel.transform.GetChild(i).position;
                if (position.x > childPosition.x )
                {
                    index = i;
                }
            }
            // Set order
            playerToken.transform.SetSiblingIndex(index);
            Debug.Log(index);*/
        }
        else if (distanceEngineer < distanceCommand)
        {
            Debug.Log("engineer");
            playerToken.transform.SetParent(engineerPanel.transform, false); 
            playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole("engineer");
        }
        else
        {
            Debug.Log("commander");
            playerToken.transform.SetParent(commandPanel.transform, false);
            playerToken.GetComponent<PlayerTokenController>().GetPlayerController().RpcSetRole("commander");
        }
        // Rebuild auto layout
        LayoutRebuilder.MarkLayoutForRebuild(playerToken.transform as RectTransform);
    }
}
