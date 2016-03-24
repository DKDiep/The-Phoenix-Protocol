using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreBoardManager : MonoBehaviour 
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] private GameObject[] scoreBoardItems;
	#pragma warning restore 0649

    private GameState gameState;

    void Start() 
    {
        GameObject server = GameObject.Find("GameManager");
        gameState = server.GetComponent<GameState>();

        for(int i = 0; i < 4; i++) {
            scoreBoardItems[i].transform.FindChild("playerScore").GetComponent<Text>().text = gameState.GetPlayerScore(i).ToString();
        }
    }
}


