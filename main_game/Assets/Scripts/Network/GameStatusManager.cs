﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class GameStatusManager : NetworkBehaviour
{
	private GameState gameState;
	private GameObject gameOverCanvas;
	private bool gameOverScreen = false;

    private GameObject server;
    private PlayerController playerController;
    private TCPServer tcpServer;
    private MusicManager musicManager;
    private GameObject localPortal;
    private bool endSoundPlayed;

	// Use this for initialization
	void Start () {
		server = GameObject.Find("GameManager");
        endSoundPlayed = false;

		gameState = server.GetComponent<GameState>();
        tcpServer = this.gameObject.GetComponent<TCPServer>();

        if (ClientScene.localPlayers[0].IsValid)
            playerController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
	}

    public void Reset()
    {
        endSoundPlayed = false;
        RpcReset();
    }

    [ClientRpc]
    void RpcReset()
    {
        gameOverScreen = false;

        GameObject crosshairCanvas = server.GetComponent<ServerManager>().GetCrosshairObject(playerController.GetScreenIndex());
        if (crosshairCanvas != null && GameObject.Find("CommanderCamera") == null)
            crosshairCanvas.SetActive(true);
        
        // Remove screen overlays
        GameObject resultCanvas = GameObject.Find("GameOverCanvas(Clone)");
        if (resultCanvas != null)
        {
            // Force if sync var delayed
            if (gameState.Status != GameState.GameStatus.Setup)
                gameState.Status = GameState.GameStatus.Setup;
            Destroy(resultCanvas);
        }

        // Re-enable portals disabled previously by game over
        if (playerController.GetRole() == RoleEnum.Camera)
        {
            // Null check required if restart is before portal deactivation
            if (localPortal != null)
                localPortal.SetActive(true);
        }

		SetShipVisible(true);
    }

    IEnumerator PlayCommanderVoice(int sound)
    {
        yield return new WaitForSeconds(2f);
        CommanderVoice.SendCommand(sound);
    }
	
	// Update is called once per frame
	void Update () {
		if((gameState.Status == GameState.GameStatus.Died ||
			gameState.Status == GameState.GameStatus.Won) && !gameOverScreen)
		{
			SetShipVisible(false);
            DisableThrusterSound();

            GameObject crosshairCanvas = server.GetComponent<ServerManager>().GetCrosshairObject(playerController.GetScreenIndex());
            if (crosshairCanvas != null)
                crosshairCanvas.SetActive(false);
            
            // Set an overlay on the screen
            gameOverCanvas = Instantiate(Resources.Load("Prefabs/GameOverCanvas", typeof(GameObject))) as GameObject;
			if(gameState.Status == GameState.GameStatus.Died) 
            {
                gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "Your ship has been destroyed";
                gameOverCanvas.transform.Find("GameOverText").gameObject.GetComponent<Text>().text = "GAME OVER";
                if(!endSoundPlayed)
                {
                    endSoundPlayed = true;
                    StartCoroutine(PlayCommanderVoice(6));

                }
            }
			else
            {
                if(!endSoundPlayed)
                {
                    endSoundPlayed = true;
                    StartCoroutine(PlayCommanderVoice(7));
                }
                gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "You reached the portal!";
                gameOverCanvas.transform.Find("GameOverText").gameObject.GetComponent<Text>().text = "YOU SURVIVED";
            }

            if(playerController.GetRole() == RoleEnum.Camera)
            {
                // Disable the portal on all screens
                localPortal = GameObject.Find("Portal(Clone)");
                if (localPortal != null)
                    localPortal.SetActive(false);

                // If it is the server
                if (playerController.netId.Value == server.GetComponent<ServerManager>().GetServerId())
                {
                    // Do not display stats on server.
                    gameOverCanvas.transform.Find("GameOverStats").gameObject.SetActive(false);
                    Debug.Log("Final Game Score: " + this.gameObject.GetComponent<GameStatsManager>().CalculateAndSendGameScore());

                    // Disable the enemies to stop them from shooting
                    foreach (GameObject enemy in gameState.GetEnemyList())
                    {
                        EnemyLogic enemyLogic = enemy.GetComponentInChildren<EnemyLogic>();
                        // Some enemy logic is null
                        if (enemyLogic != null)
                            enemyLogic.gameObject.SetActive(false);
                    }
                  
                    // Manage end game music
                    if(musicManager == null)
                        musicManager = GameObject.Find("MusicManager(Clone)").GetComponent<MusicManager>();

                    if(gameState.Status == GameState.GameStatus.Died) 
                        musicManager.PlayMusic(3);
                    else
                        musicManager.PlayMusic(4);

                    // Disable game timer. 
                    GameObject gameTimer = GameObject.Find("GameTimerText");
                    gameTimer.SetActive(false);
                    
                    // Send Game Over signal to the Phone Server
                    tcpServer.SendSignal(TCPServer.MsgType.GameEnded);
                }
                else 
                {
                    // Show stats on side screens.
                    gameOverCanvas.transform.Find("StatusText").gameObject.SetActive(false);
                    gameOverCanvas.transform.Find("GameOverText").gameObject.SetActive(false);
                }
            } 
            else 
            {
                gameOverCanvas.transform.Find("GameOverStats").gameObject.SetActive(false);
            }
           
			gameOverScreen = true;
		}
	}

    private void DisableThrusterSound()
    {
        GameObject playerShip = GameObject.Find("PlayerShip(Clone)");
        if(playerShip != null)
            playerShip.GetComponent<AudioSource>().mute = true;
    }

	/// <summary>
	/// Shows or hides the ship model.
	/// </summary>
	/// <param name="visible">The visibility.</param>
	private void SetShipVisible(bool visible)
	{
		GameObject playerShip = GameObject.Find("PlayerShip(Clone)");
		if(playerShip != null)
		{
			for(int i = 0; i < playerShip.transform.childCount; i++)
			{
				if(playerShip.transform.GetChild(i).name.Contains("Turret") || playerShip.transform.GetChild(i).name == "Model")
					playerShip.transform.GetChild(i).gameObject.SetActive(visible);
			}
		}
	}
}
