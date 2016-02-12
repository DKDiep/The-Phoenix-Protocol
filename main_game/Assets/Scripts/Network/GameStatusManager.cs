using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameStatusManager : MonoBehaviour {
	private GameState gameState;
	private GameObject gameOverCanvas;
	private bool gameOverScreen = false;
	// Use this for initialization
	void Start () {
		GameObject server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();
	}
	
	// Update is called once per frame
	void Update () {
		if((gameState.GetStatus() == GameState.Status.Died ||
			gameState.GetStatus() == GameState.Status.Won) && !gameOverScreen)
		{
			gameOverCanvas = Instantiate(Resources.Load("Prefabs/GameOverCanvas", typeof(GameObject))) as GameObject;
			if(gameState.GetStatus() == GameState.Status.Died) 
			{
				gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "Your ship and the crew were killed.";
			} else {
				gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "You reached the portal!";
			}
			gameOverScreen = true;
		}
	}
}
