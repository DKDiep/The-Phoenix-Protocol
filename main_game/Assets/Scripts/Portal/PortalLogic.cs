using UnityEngine;
using System.Collections;

public class PortalLogic : MonoBehaviour 
{
	private GameState gameState;

	void Start () 
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
	}   

    void OnTriggerEnter (Collider col)
    {
        if(col.gameObject.tag.Equals ("Player"))
        {
            Debug.Log("You have reached the portal. You have won then game!");
            gameState.Status = GameState.GameStatus.Won;
        }
    }
}
