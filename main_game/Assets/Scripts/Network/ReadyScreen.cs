using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class ReadyScreen : NetworkBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Button goButton;
	#pragma warning restore 0649

    private ServerManager serverManager;
    private MusicManager musicManager;
    private PlayerController playerController;

    private bool blackScreen = false;

    void Awake()
    {
        GameObject server = GameObject.Find("GameManager");

        if (server != null)
        {
            serverManager = server.GetComponent<ServerManager>();
            goButton.onClick.AddListener(() => OnClickStartButton());
        }
        if (ClientScene.localPlayers[0].IsValid)
            playerController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        if (playerController.netId.Value != serverManager.GetServerId())
            goButton.gameObject.SetActive(false);
    }

    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        musicManager = GameObject.Find("MusicManager(Clone)").GetComponent<MusicManager>();
        if (musicManager != null)
        {
            musicManager.PlayMusic(1);
        }
        RpcShow();  
    }

    public void OnClickStartButton()
    {
        // Reset cutscene manager variables and play
        serverManager.cutsceneManager.GetComponent<LoadingText>().Reset();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Reset();
        serverManager.cutsceneManager.GetComponent<LoadingText>().Play();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Play();
        // Start the game
        serverManager.Reset();

        // Disable self until restart
        //gameObject.SetActive(false);
        RpcHide();
    }

    [ClientRpc]
    void RpcHide()
    {
        gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcShow()
    {
        gameObject.SetActive(true);
    }
}
