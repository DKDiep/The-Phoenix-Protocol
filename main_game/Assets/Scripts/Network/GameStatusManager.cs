using UnityEngine;
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
	// Use this for initialization
	void Start () {
		server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();

        if (ClientScene.localPlayers[0].IsValid)
            playerController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		if((gameState.Status == GameState.GameStatus.Died ||
			gameState.Status == GameState.GameStatus.Won) && !gameOverScreen)
		{
            GameObject.Find("Portal(Clone)").SetActive(false);

            // Set an overlay on the screen
			gameOverCanvas = Instantiate(Resources.Load("Prefabs/GameOverCanvas", typeof(GameObject))) as GameObject;
			if(gameState.Status == GameState.GameStatus.Died) 
                gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "Your ship and the crew were killed.";
			else
                gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "You reached the portal!";

            if(playerController.netId.Value != server.GetComponent<ServerManager>().GetServerId())
            {
                gameOverCanvas.transform.Find("StatusText").gameObject.SetActive(false);
                gameOverCanvas.transform.Find("GameOverText").gameObject.SetActive(false);
            }

			gameOverScreen = true;
		}
	}
}
