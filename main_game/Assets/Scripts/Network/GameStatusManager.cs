﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameStatusManager : MonoBehaviour
{
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
		if((gameState.Status == GameState.GameStatus.Died ||
			gameState.Status == GameState.GameStatus.Won) && !gameOverScreen)
		{
			gameOverCanvas = Instantiate(Resources.Load("Prefabs/GameOverCanvas", typeof(GameObject))) as GameObject;
			if(gameState.Status == GameState.GameStatus.Died) 
			{
				gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "Your ship and the crew were killed.";
			}
			else
			{
				gameOverCanvas.transform.Find("StatusText").gameObject.GetComponent<Text>().text = "You reached the portal!";
			}

			gameOverScreen = true;
		}
	}
}
