using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;

public class ServerLobby : MonoBehaviour {
    ServerManager serverManager;
    [SerializeField]
    private GameObject canvasObject;
    [SerializeField]
    private GameObject cameraPanel;
    [SerializeField]
    private GameObject engineerPanel;
    [SerializeField]
    private Button startButton;

    private int count = 0;

    // Use this for initialization
    void Start ()
    {
        serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();
        //startButton = canvasObject.transform.Find("StartButton").gameObject.GetComponent<Button>();
        startButton.onClick.AddListener(() => onClickStartButton() );
    }
	
	public void onClickStartButton ()
    {
        // Pass lobby information to server
        serverManager.StartGame();
        startButton.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }

    public void playerJoin(GameObject playerObject)
    {
        // Get player and set default role
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        playerController.SetRole("camera");

        // Create token for new player
        GameObject playerToken = Instantiate(Resources.Load("Prefabs/PlayerToken", typeof(GameObject))) as GameObject;
        playerToken.transform.parent = cameraPanel.transform;
        playerToken.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); ;
        //playerToken.transform.localPosition = new Vector3(0.0f,0.0f,0.0f);

        playerToken.transform.Find("UserId").GetComponent<Text>().text = "NetId: "+playerController.netId.ToString();
        //player.setCount(count);
    }
}
