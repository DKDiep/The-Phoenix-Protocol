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
            goButton.onClick.AddListener(() => OnClickStartButton());
            goButton.gameObject.SetActive(false);
            StartCoroutine(DelayButton());
            Reset();
        }


    }

    void Update()
    {
        if(goButton.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickStartButton();
            }
        }
    }

    IEnumerator DelayButton()
    {
        yield return new WaitForSeconds(2f);
        goButton.gameObject.SetActive(true);
    }

    public void Reset()
    {
        GameObject musicObect = GameObject.Find("MusicManager(Clone)");
        Camera.main.fov = 45;
		Cursor.visible = true; //leave as true for development, false for production
        if (musicObect != null)
        {
            musicManager = musicObect.GetComponent<MusicManager>();
            musicManager.PlayMusic(0);
        }
        if (playerController.netId.Value == serverManager.GetServerId())
            RpcShow();  
    }

    public void OnClickStartButton()
    {
		Cursor.visible = Debug.isDebugBuild; //leave as true for development, false for production
        InitialiseGame();
    }
        
    public void InitialiseGame()
    {
        // Reset cutscene manager variables and play
        serverManager.cutsceneManager.GetComponent<LoadingText>().Reset();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Reset();
        serverManager.cutsceneManager.GetComponent<LoadingText>().Play();
        serverManager.cutsceneManager.GetComponent<LoadingText>().MuteAudio();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Play();
		serverManager.spawner.GetComponent<EnemySpawner>().StartDifficultyTimer();

		// Setup shoot logic now that dependencies are ready
        for(int i = 0; i < 3; i++)
            GameObject.Find("PlayerShooting"+i).GetComponent<PlayerShooting>().Setup();
       
		// Start the game
        GameObject.Find("GameManager").GetComponent<GameState>().Status = GameState.GameStatus.Started;

        // Show the crosshairs (they might have been hidden before a reset)
        if (playerController.GetRole() == RoleEnum.Camera)
        {
            // Get the local crosshair
            crosshairCanvas = serverManager.GetCrosshairObject(playerController.GetScreenIndex());
            crosshairCanvas.SetActive(true);
        }
       
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
