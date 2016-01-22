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

    // Use this for initialization
    void Start ()
    {
        serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();
        startButton = canvasObject.transform.Find("StartButton").gameObject.GetComponent<Button>();
        startButton.onClick.AddListener(() => onClickStartButton() );
    }
	
	public void onClickStartButton ()
    {
        serverManager.StartGame();
        startButton.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }
}
