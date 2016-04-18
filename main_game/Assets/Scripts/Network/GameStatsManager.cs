using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStatsManager : MonoBehaviour
{
    private GameState gameState;
    private GameSettings settings;
    // Use this for initialization
    void Start()
    {
        gameState = this.gameObject.GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        if (MainMenu.startServer)
        {
            StartCoroutine(SendRequest());
        }
    }

    public int CalculateAndSendGameScore()
    {
        float totalScore = 0;

        // totalScore += value / weighting;

        // Total civilians saved
        totalScore += gameState.GetCivilians() / settings.civilianWeighting;

        // Total resources collected
        totalScore += gameState.GetTotalShipResources() / settings.resourcesWeighting;

        // Total score for each player
        for(int playerId = 0; playerId < 4; playerId++)
        {
            totalScore += gameState.GetPlayerScore(playerId) / settings.playerScoreWeighting;
        }

        // Scores for each of the upgrades
        totalScore += gameState.GetUpgradableComponent(ComponentType.Hull).Level * settings.hullWeighting;
        totalScore += gameState.GetUpgradableComponent(ComponentType.Drone).Level * settings.droneWeighting;
        totalScore += gameState.GetUpgradableComponent(ComponentType.Engine).Level * settings.engineWeighting;
        totalScore += gameState.GetUpgradableComponent(ComponentType.ResourceStorage).Level * settings.storageWeighting;
        totalScore += gameState.GetUpgradableComponent(ComponentType.ShieldGenerator).Level * settings.shieldsWeighting;
        totalScore += gameState.GetUpgradableComponent(ComponentType.Turret).Level * settings.turretWeighting;

        //this should be changed to take player input;
        string teamName = "\"cockpit spacenauts\"";
        string jsonMsg = "{\"team_name\":" + teamName + ",";
        jsonMsg += "\"score\":" + (int)totalScore;
        jsonMsg += "}";
        StartCoroutine(SendFinalRequest(jsonMsg));
        return (int)totalScore;
}

    IEnumerator SendRequest()
    {
        while (true)
        {
            if (gameState.Status == GameState.GameStatus.Started)
            {
                string jsonMsg = "{\"playerscores\":[";
                Dictionary<uint, Officer> officerMap = gameState.GetOfficerMap();
                int i = 0;
                foreach(KeyValuePair<uint, Officer> officer in officerMap)
                {
                    jsonMsg += "{\"name\": \"" + officer.Value.Name + "\", \"score\":" + gameState.GetPlayerScore(i) + "},";
                    i++;
                }
                // Remove last comma from json 
                if(i != 0) jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
                
                jsonMsg += "],";
                jsonMsg += "\"totalShipResources\": " + gameState.GetTotalShipResources() + ",";
                jsonMsg += "\"shipResources\": " + gameState.GetShipResources() + ",";
                jsonMsg += "\"shipHealth\": " + (int)gameState.GetShipHealth();
                jsonMsg += "}";

                string url = "http://localhost:8081/game_data";
                WWWForm form = new WWWForm();
                form.AddField("JSON:", jsonMsg);
                WWW www = new WWW(url, form);
                //Send request
                yield return www;

            }
            yield return new WaitForSeconds(5);
        }
    }

    IEnumerator SendFinalRequest(string jsonMsg)
    {
        string url = "http://localhost:8081/save_game_data";
        WWWForm form = new WWWForm();
        form.AddField("JSON:", jsonMsg);
        WWW www = new WWW(url, form);
        //Send request
        yield return www;
    }
    // Update is called once per frame
    void Update () {
	
	}
}
