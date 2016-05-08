using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
        Dictionary<uint, Officer> officerMap = gameState.GetOfficerMap();

        for(int i = 0; i < 4; i++) {
            scoreBoardItems[i].transform.FindChild("playerScore").GetComponent<Text>().text = gameState.GetPlayerScore(i).ToString();

            if((i+1) > gameState.GetOfficerCount())
            {
                scoreBoardItems[i].SetActive(false);
            }
            else
            {
                scoreBoardItems[i].SetActive(true);
                if(officerMap != null)
                    scoreBoardItems[i].transform.FindChild("PlayerLabel").GetComponent<Text>().text = officerMap[(uint)i].Name;
            }



        }
    }
}


