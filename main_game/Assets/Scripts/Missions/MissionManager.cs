using System;
using UnityEngine;

public class MissionManager : MonoBehaviour 
{
    private GameState gameState;
    private GameSettings settings;
    private Mission[] missions;
    void Start () 
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        LoadSettings();
    }

    private void LoadSettings()
    {
        missions = settings.missionProperties;
    }

    void Update () 
    {
        if(gameState.Status == GameState.GameStatus.Started)
        {
            CheckMissionTriggers();
        }
    }

    /// <summary>
    /// Checks if any of the missions have triggered. 
    /// </summary>
    private void CheckMissionTriggers() 
    {
        // Loop through mission ids
        for(int id = 0; id < missions.Length; id++)
        {
            if(CheckTrigger(missions[id].triggerType, missions[id].triggerValue) && missions[id].hasStarted() == false)
            {
                StartMission(id);
            }
        }
    }

    private void StartMission(int missionId)
    {
        missions[missionId].start();
        Debug.Log("Starting Mission " + missions[missionId].description);
    }

    /// <summary>
    /// Checks if the mission can be started based on a trigger type and value
    /// </summary>
    /// <returns><c>true</c>, if trigger was checked, <c>false</c> otherwise.</returns>
    /// <param name="trigger">Trigger.</param>
    /// <param name="value">Value.</param>
    private bool CheckTrigger(TriggerType trigger, int value)
    {
        switch(trigger)
        {
            case TriggerType.Health:
                if(gameState.GetShipHealth() < value)
                    return true;
                break;
            case TriggerType.OutpostDistance:
                // Check outpost distance
                break;
            case TriggerType.Resources:
                if(gameState.GetShipResources() > value)
                    return true;
                break;
            case TriggerType.Shields:
                if(gameState.GetShipShield() < value)
                    return true;
                break;
            case TriggerType.Time:
                // Check game time.
                break;
        }
        return false;
    }

        
    [System.Serializable] 
    public class Mission
    {
        public string name, description;
        public TriggerType triggerType;
        public int triggerValue;
        private bool started = false;
        public Mission(string name, string description, TriggerType triggerType, int triggerValue) 
        {
            this.name         = name;
            this.description  = description;
            this.triggerType  = triggerType;
            this.triggerValue = triggerValue;
        }
        public bool hasStarted()
        {
            return started;
        }
        public void start()
        {
            started = true;
        }
    }
}

public enum TriggerType
{
    Time,
    Health,
    Shields,
    Resources,
    OutpostDistance
}