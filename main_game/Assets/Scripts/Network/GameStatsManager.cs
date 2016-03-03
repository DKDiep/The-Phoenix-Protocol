using UnityEngine;
using System.Collections;

public class GameStatsManager : MonoBehaviour
{
    private GameState gameState;

    // Use this for initialization
    void Start()
    {
        gameState = this.gameObject.GetComponent<GameState>();
        if (MainMenu.startServer)
        {
            StartCoroutine(SendRequest());
        }
    }

    public int CalculateGameScore()
    {
        float totalScore = 0;

        // totalScore += value / weighting;

        // Total civilians saved
        totalScore += gameState.GetCivilians() / 100;

        // Total resources collected
        totalScore += gameState.GetTotalShipResources() / 1000;

        // Total score for each player
        for(int playerId = 0; playerId < 4; playerId++)
        {
            totalScore += gameState.GetPlayerScore(playerId) / 1000;
        }

        // Scores for each of the upgrades
        totalScore += gameState.GetUpgradableComponent(ComponentType.Bridge).Level * 100;
        totalScore += gameState.GetUpgradableComponent(ComponentType.Drone).Level * 150;
        totalScore += gameState.GetUpgradableComponent(ComponentType.Engine).Level * 200;
        totalScore += gameState.GetUpgradableComponent(ComponentType.ResourceStorage).Level * 50;
        totalScore += gameState.GetUpgradableComponent(ComponentType.ShieldGenerator).Level * 200;
        totalScore += gameState.GetUpgradableComponent(ComponentType.Turret).Level * 100;

        return (int)totalScore;
    }
    IEnumerator SendRequest()
    {
        while (true)
        {
            if (gameState.Status == GameState.GameStatus.Started)
            {
                string jsonMsg = "{\"playerscores\":[";

                for (int i = 0; i < 4; i++)
                {
                    jsonMsg += gameState.GetPlayerScore(i) + ",";
                }
                jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
                jsonMsg += "],";
                jsonMsg += "\"totalShipResources\": " + gameState.GetTotalShipResources() + ",";
                jsonMsg += "\"shipResources\": " + gameState.GetShipResources() + ",";
                jsonMsg += "\"shipHealth\": " + gameState.GetShipHealth();
                jsonMsg += "}";

                string url = "http://localhost:8080/game_data";
                WWWForm form = new WWWForm();
                form.AddField("JSON:", jsonMsg);
                WWW www = new WWW(url, form);
                yield return www;

            }
            yield return new WaitForSeconds(5);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
