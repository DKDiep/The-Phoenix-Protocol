using UnityEngine;
using System.Collections;

public class PortalLogic : MonoBehaviour {

	public float distanceToWin = 0.9f;
	GameState gameState;
	// Use this for initialization
	void Start () {
		GameObject server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void collision() 
	{
		Debug.Log("You have reached the portal. You have won then game!");
		gameState.SetStatus(GameState.Status.Won);
	}
}
