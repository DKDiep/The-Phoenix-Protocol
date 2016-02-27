using UnityEngine;
using System.Collections;

public class GameStatsManager : MonoBehaviour
{
    private GameState state;

    // Use this for initialization
    void Start()
    {
        state = this.gameObject.GetComponent<GameState>();
        if (MainMenu.startServer)
        {
            StartCoroutine(SendRequest());
        }
    }

    IEnumerator SendRequest()
    {
        while (true)
        {
            if (state != null)
            {
                int[] playerScores = state.GetPlayerScores();
                string jsonMsg = "{\"playerscores\":[";
                if (playerScores != null)
                {
                    foreach (uint playerScore in playerScores)
                    {
						jsonMsg += playerScore + ",";
                    }
                    jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
                    jsonMsg += "],";
                    jsonMsg += "\"totalShipResources\": " + state.GetTotalShipResources() + ",";
                    jsonMsg += "\"shipResources\": " + state.GetShipResources() + ",";
                    jsonMsg += "\"shipHealth\": " + state.GetShipHealth();
                    jsonMsg += "}";

                    string url = "http://localhost:8080/game_data";
                    WWWForm form = new WWWForm();
                    form.AddField("JSON:", jsonMsg);
                    WWW www = new WWW(url, form);
                    yield return www;
                }
            }
            yield return new WaitForSeconds(5);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
