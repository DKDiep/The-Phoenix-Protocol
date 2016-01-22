using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;

public class ServerLobby : MonoBehaviour {
    ServerManager serverManager;
    [SerializeField]
    public GameObject canvasObject;
    private Button startButton;

    private int count = 0;

    // Use this for initialization
    void Start ()
    {
        serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();
        startButton = canvasObject.transform.Find("StartButton").gameObject.GetComponent<Button>();
        startButton.onClick.AddListener(() => onClickStartButton() );
    }
	
	public void onClickStartButton ()
    {
        // Pass lobby information to server
        serverManager.StartGame();
        startButton.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }

    public void playerJoin(uint netId)
    {
        // Create token for new player
        GameObject playerToken = Instantiate(Resources.Load("Prefabs/PlayerToken", typeof(GameObject))) as GameObject;
        playerToken.transform.parent = canvasObject.transform;
        playerToken.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); ;
        playerToken.transform.localPosition = new Vector3(0.0f,0.0f,0.0f);

        playerToken.transform.Find("UserId").GetComponent<Text>().text = "NetId: "+netId.ToString();
        //player.setCount(count);
    }
}
