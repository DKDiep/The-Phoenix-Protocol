using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;

public class ServerLobby : MonoBehaviour {
    ServerManager serverManager;
    [SerializeField]
    public GameObject canvasObject;
    private Button button;

    // Use this for initialization
    void Start ()
    {
        serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();
        button = canvasObject.transform.Find("StartButton").gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => onClickStartButton() );
    }
	
	public void onClickStartButton ()
    {
        serverManager.StartGame();
        button.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }
}
