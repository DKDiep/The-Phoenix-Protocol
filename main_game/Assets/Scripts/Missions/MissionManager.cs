using System;
using UnityEngine;

public class MissionManager : MonoBehaviour 
{
    private GameState gameState;
    private GameSettings settings;
    private Mission[] missions;
    private PlayerController playerController;
    private OutpostManager outpostManager;
    private float startTime;

    void Start () 
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        outpostManager = GameObject.Find("OutpostManager(Clone)").GetComponent<OutpostManager>();
        LoadSettings();
    }

    public void ResetMissions() 
    {
        startTime = Time.time;
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
            CheckMissionsCompleted();
        }
    }

    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
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

    private void CheckMissionsCompleted() 
    {
        for(int id = 0; id < missions.Length; id++)
        {
            if(CheckCompletion(missions[id].completionType, missions[id].completionValue) && 
               missions[id].hasStarted() == true && missions[id].isComplete() == false)
            {
                CompleteMission(id);
            }
        }
    }

    private void StartMission(int missionId)
    {
        missions[missionId].start();
        playerController.RpcStartMission(missions[missionId].name, missions[missionId].description);
    }

    private void CompleteMission(int missionId)
    {
        missions[missionId].completeMission();
        playerController.RpcCompleteMission(missions[missionId].completedDescription);
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
                if(outpostManager.GetClosestOutpostDistance() < value)
                    return true;
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
                if((Time.time - startTime) > value)
                    return true;
                break;
        }
        return false;
    }

    private bool CheckCompletion(CompletionType type, int value)
    {
        switch(type)
        {
        case CompletionType.Enemies:
            if(gameState.GetTotalKills() > value)
                return true;
            break;
        case CompletionType.Outpost:
            // Check if we have visited the outpost with id 'value'
            break;
        }
        return false;
    }
        
    [System.Serializable] 
    public class Mission
    {
        public string name, description, completedDescription;
        public TriggerType triggerType;
        public CompletionType completionType;
        public int triggerValue, completionValue;
        private bool started = false;
        private bool complete = false;

        public Mission(string name, string description, TriggerType triggerType, int triggerValue) 
        {
            this.name         = name;
            this.description  = description;
            this.triggerType  = triggerType;
            this.triggerValue = triggerValue;
        }
        public bool isComplete()
        {
            return complete;
        }
        public void completeMission()
        {
            complete = true;
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
    Time,                   // Trigger after a certain time
    Health,                 // Trigger if the ships health is below a specific value
    Shields,                // Trigger if the ships shields are below a specific value
    Resources,              // Trigger if the player has collected a certain amount of resources.
    OutpostDistance         // Trigger if the player is a certain distence from any outpost
}

public enum CompletionType
{
    Enemies,                // Complete mission if x enemies are destroyed
    Outpost                 // Complete mission if outpost is visited
}