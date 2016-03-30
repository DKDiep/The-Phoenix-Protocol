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
	private GameObject crosshairCanvas;

    void Start()
    {
        GameObject server = GameObject.Find("GameManager");
        serverManager = server.GetComponent<ServerManager>();
        if (ClientScene.localPlayers[0].IsValid)
            playerController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        if (playerController.netId.Value != serverManager.GetServerId())
        {
            goButton.gameObject.SetActive(false);
        }
        else
        {
			if (crosshairCanvas == null)
				crosshairCanvas = GameObject.Find("CrosshairCanvas(Clone)");
            serverManager = server.GetComponent<ServerManager>();
            goButton.onClick.AddListener(() => OnClickStartButton());
            Reset();
        }
    }

    public void Reset()
    {
        GameObject musicObect = GameObject.Find("MusicManager(Clone)");
        if (musicObect != null)
        {
            musicManager = musicObect.GetComponent<MusicManager>();
            musicManager.PlayMusic(1);
        }
        if (playerController.netId.Value == serverManager.GetServerId())
            RpcShow();  
    }

    public void OnClickStartButton()
    {
        InitialiseGame();
    }
    
    public void InitialiseGame()
    {
        // Reset cutscene manager variables and play
        serverManager.cutsceneManager.GetComponent<LoadingText>().Reset();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Reset();
        serverManager.cutsceneManager.GetComponent<LoadingText>().Play();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Play();
        
		// Setup shoot logic now that dependencies are ready
        GameObject.Find("PlayerShootLogic(Clone)").GetComponent<PlayerShooting>().Setup();
       
		// Start the game
        GameObject.Find("GameManager").GetComponent<GameState>().Status = GameState.GameStatus.Started;

        // Show the crosshairs (they might have been hidden before a reset)
        if (playerController.GetRole() == RoleEnum.Camera && crosshairCanvas != null)
            crosshairCanvas.SetActive(true);
       
		// Disable self until restart
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
