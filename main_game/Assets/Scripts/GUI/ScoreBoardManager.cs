using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreBoardManager : MonoBehaviour 
{

    [SerializeField] private GameObject[] scoreBoardItems;
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


