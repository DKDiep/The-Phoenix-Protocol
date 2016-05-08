using UnityEngine;
using System.Collections;

public class PortalLogic : MonoBehaviour 
{
	private GameState gameState;
    private Camera mainCam;
    private GameObject player;
    float distance;

	void Start () 
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        mainCam = Camera.main;
        player = gameState.PlayerShip;
	}   

    void Update()
    {
        distance = Vector3.Distance(player.transform.position, transform.position);
        if(distance < 444 && distance > 150f)
            mainCam.fov = (20000f / distance);
    }

    void OnTriggerEnter (Collider col)
    {
        if (gameState.Status == GameState.GameStatus.Started)
        {
            // Debug.Log("You have reached the portal. You have won then game!");
            gameState.Status = GameState.GameStatus.Won;
        }
    }
}
