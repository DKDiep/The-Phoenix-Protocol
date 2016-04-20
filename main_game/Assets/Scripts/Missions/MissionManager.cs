using System;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour 
{
    private GameState gameState;
    private GameSettings settings;
    private Mission[] missions;
    private bool[] activeList;              //These could be in missions, but this avoids the problem of having to edit gamesettings when you want to change them.
    private PlayerController playerController;
    private OutpostManager outpostManager;
    private float startTime;
	private bool timerStarted;
    private bool missionInit = false;

    void Start () 
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        outpostManager = GameObject.Find("OutpostManager(Clone)").GetComponent<OutpostManager>();
        LoadSettings();
    }

    public void ResetMissions() 
    {
        StopAllCoroutines();

        for (int i = 0; i < missions.Length; i++)
        {
            activeList[i] = missions[i].active;
            missions[i].reset();
        }

        StartCoroutine(WaitThenStartUpdateMissions());
    }

    private void LoadSettings()
    {
        missions = settings.missionProperties;
        activeList = new bool[missions.Length];
        for (int i = 0; i < missions.Length; i++)
        {
            activeList[i] = missions[i].active;
        }
    }

    void Update ()
    {
        if (Input.GetKeyDown("z"))
        {
            print("ShieldGen health: " + gameState.GetComponentHealth((ComponentType)(int)(UpgradableComponentIndex.ShieldGen)));
            print("Turrets health: " + gameState.GetComponentHealth((ComponentType)(int)(UpgradableComponentIndex.Turrets)));
            print("Engines health: " + gameState.GetComponentHealth((ComponentType)(int)(UpgradableComponentIndex.Engines)));
            print("Hull health: " + gameState.GetComponentHealth((ComponentType)(int)(UpgradableComponentIndex.Hull)));
            print("Drone health: " + gameState.GetComponentHealth((ComponentType)(int)(UpgradableComponentIndex.Drone)));
            print("ResourceStorage health: " + gameState.GetComponentHealth((ComponentType)(int)(UpgradableComponentIndex.ResourceStorage)));
        }
        if (Input.GetKeyDown("x"))
        {
            print("ShieldGen Level: " + gameState.upgradableComponents[(int)UpgradableComponentIndex.ShieldGen].Level);
            print("Turrets Level: " + gameState.upgradableComponents[(int)UpgradableComponentIndex.Turrets].Level);
            print("Engines Level: " + gameState.upgradableComponents[(int)UpgradableComponentIndex.Engines].Level);
            print("Hull Level: " + gameState.upgradableComponents[(int)UpgradableComponentIndex.Hull].Level);
            print("Drone Level: " + gameState.upgradableComponents[(int)UpgradableComponentIndex.Drone].Level);
            print("ResourceStorage Level: " + gameState.upgradableComponents[(int)UpgradableComponentIndex.ResourceStorage].Level);

        }

        if (gameState.Status == GameState.GameStatus.Started)
        {
            // Initialise any variables for missions
            if(!missionInit)
            {
                InitialiseMissions();
                StartCoroutine(UpdateMissions());
            }
        }
    }

    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }


    private void InitialiseMissions()
    {
        for(int id = 0; id < missions.Length; id++)
        {
            foreach (MissionCompletion completeCondition in missions[id].completionConditions)
            {
                // If the missions completion type is an outpost, we randomly assign it a close outpost.
                if(completeCondition.completionType == CompletionType.Outpost)
                {
                    completeCondition.completionValue = outpostManager.GetClosestOutpost();
                    // If we have successfully initialised the completion value.
                    if(completeCondition.completionValue != -1)
                        missionInit = true;
                }
            }
        }
    }

	/// <summary>
	/// Sets the mission start time to the current time.
	/// </summary>
	public void StartTimer()
	{
		startTime    = Time.time;
		timerStarted = true;
	}

    private IEnumerator UpdateMissions()
    {
        CheckMissionTriggers();
        CheckMissionsCompleted(); 
        yield return new WaitForSeconds(1f);
        StartCoroutine(UpdateMissions());
    }

    IEnumerator WaitThenSetActive(int[] missionIds)
    {
        foreach (int missionId in missionIds)
        {
            yield return new WaitForSeconds(3.0f);
            activeList[missionId] = true;
        }
    }

    // The wait part is here because without it we get some weird behaviour from the reset period - 
    // missions were being triggered in the new game because trigger conditions were being met in the previous game
    IEnumerator WaitThenStartUpdateMissions() 
    {
        yield return new WaitForSeconds(5.0f);
        InitialiseMissions();
        StartCoroutine(UpdateMissions());
    }


    /// <summary>
    /// Checks if any of the missions have triggered. 
    /// </summary>
    private void CheckMissionTriggers() 
    {
        // Loop through mission ids
        for(int id = 0; id < missions.Length; id++)
        {
            if (missions[id].hasStarted() == false && activeList[id])
                if (CheckTrigger(id))
                    {
                        StartMission(id);
                    }
        }
    }

    private void CheckMissionsCompleted() 
    {
        for(int id = 0; id < missions.Length; id++)
        {
            if (missions[id].hasStarted() == true && missions[id].isComplete() == false)
            {
                if (CheckCompletion(id))
                {
                    CompleteMission(id);
                }
            }
        }
    }

    private void StartMission(int missionId)
    {
        missions[missionId].start();
        int i = 0;
        int[] missionCompletions = new int[missions[missionId].completionConditions.Length];
        int[] ids = new int[missions[missionId].completionConditions.Length]; //ids can be the id of the outpost, or the id of the upgradable component
        foreach (MissionCompletion completeCondition in missions[missionId].completionConditions)
        {
            if (completeCondition.completionType == CompletionType.Outpost)
            {
                ids[i] = completeCondition.completionValue;
                if(ids[i] != -1)outpostManager.setMissionTarget(ids[i]);
            }
            if (completeCondition.completionType == CompletionType.Upgrade || completeCondition.completionType == CompletionType.Repair)
            {
                ids[i] = (int)completeCondition.componentIndex;
            }
            missionCompletions[i] = (int)completeCondition.completionType;
            i++;
        }   
        playerController.RpcStartMission(missions[missionId].name, missions[missionId].description, missionCompletions, missions[missionId].objectiveList, ids);
    }

    private void CompleteMission(int missionId)
    {
        missions[missionId].completeMission();
        int i = 0;
        int[] missionCompletions = new int[missions[missionId].completionConditions.Length];
        int[] ids = new int[missions[missionId].completionConditions.Length]; //ids can be the id of the outpost, or the id of the upgradable component
        foreach (MissionCompletion completeCondition in missions[missionId].completionConditions)
        {
            if (completeCondition.completionType == CompletionType.Outpost)
            {
                ids[i] = completeCondition.completionValue;
                if(ids[i] != -1) outpostManager.endMission(ids[i]);
            }
            if (completeCondition.completionType == CompletionType.Upgrade || completeCondition.completionType == CompletionType.Repair)
            {
                ids[i] = (int)completeCondition.componentIndex;
            }
            missionCompletions[i] = (int)completeCondition.completionType;
            i++;
        }
        playerController.RpcCompleteMission(missions[missionId].completedDescription, missionCompletions, missions[missionId].objectiveList, ids);
        StartCoroutine(WaitThenSetActive(missions[missionId].activates));
    }

    /// <summary>
    /// Checks if the mission can be started based on a trigger type and value
    /// </summary>
    /// <returns><c>true</c>, if trigger was checked, <c>false</c> otherwise.</returns>
    /// <param name="trigger">Trigger.</param>
    /// <param name="value">Value.</param>
    private bool CheckTrigger(int missionId)
    {
        // Loop through each trigger condition
        foreach (MissionTrigger trigger in missions[missionId].triggerConditions)
        {
            switch(trigger.triggerType)
            {
                case TriggerType.Health:
                    if(gameState.GetShipHealth() < trigger.triggerValue)
                    {
                        if(missions[missionId].triggerOnAny) return true;
                    }
                    else
                    {
                        if(!missions[missionId].triggerOnAny) return false;
                    }
                    break;
                case TriggerType.OutpostDistance:
                    if(outpostManager.GetClosestOutpostDistance() < trigger.triggerValue)
                    {
                        if(missions[missionId].triggerOnAny) return true;
                    }   
                    else
                    {
                        if(!missions[missionId].triggerOnAny) return false;
                    }
                    break;
                case TriggerType.Resources:
                    if(gameState.GetShipResources() > trigger.triggerValue)
                    {
                        if(missions[missionId].triggerOnAny) return true;
                    }
                    else
                    {
                        if(!missions[missionId].triggerOnAny) return false;
                    }
                    break;
                case TriggerType.Shields:
                    if(gameState.GetShipShield() < trigger.triggerValue)
                    {
                        if(missions[missionId].triggerOnAny) return true;
                    }
                    else
                    {
                        if(!missions[missionId].triggerOnAny) return false;
                    }
                    break;
                case TriggerType.Time:
					if(timerStarted && (Time.time - startTime) > trigger.triggerValue)
                    {
                        if(missions[missionId].triggerOnAny) return true;
                    }
                    else
                    {
                        if(!missions[missionId].triggerOnAny) return false;
                    }
                    break;
            }
        }

        if(missions[missionId].triggerOnAny)
            return false;
        else 
            return true;
    }

    private bool CheckCompletion(int missionId)
    {
        // Loop through each completion condition
        foreach (MissionCompletion completeCondition in missions[missionId].completionConditions)
        {
            switch(completeCondition.completionType)
            {
                case CompletionType.Enemies:     
                    if(gameState.GetTotalKills() >= completeCondition.completionValue)
                    {
                        if(missions[missionId].completeOnAny) return true;
                    }
                    else
                    {
                        if(!missions[missionId].completeOnAny) return false;
                    }
                    break;
                case CompletionType.Outpost:
                    if(completeCondition.completionValue != -1)
                    {
                        GameObject outpost = gameState.GetOutpostById(completeCondition.completionValue);
                        if(outpost != null && outpost.GetComponentInChildren<OutpostLogic>().resourcesCollected == true)
                        {
                            if(missions[missionId].completeOnAny) return true;
                        }
                        else
                        {
                            if(!missions[missionId].completeOnAny) return false;
                        }
                    }
                    break;
                case CompletionType.Upgrade:
                    if (gameState.upgradableComponents[(int)completeCondition.componentIndex].Level == completeCondition.completionValue)
                    {
                        if (missions[missionId].completeOnAny) return true;
                    }
                    else
                    {
                        if (!missions[missionId].completeOnAny) return false;
                    }
                    break;
                case CompletionType.Repair:
                    if (gameState.GetComponentHealth((ComponentType)(int)(completeCondition.componentIndex)) == completeCondition.completionValue)
                    {
                        if (missions[missionId].completeOnAny) return true;
                    }
                    else
                    {
                        if (!missions[missionId].completeOnAny) return false;
                    }
                    break;
            }
        }
        //If completeOnAny is true then this section is only reached if none of the complete conditions are met.
        //Otherwise this section is only reached if all of the complete conditions are met. Confusingly.
        if (missions[missionId].completeOnAny)
            return false;
        else
            return true;
    }
        
    [System.Serializable] 
    public class Mission
    {
        public bool active;
        public string name;

        [Multiline]
        public string description;
        [Multiline]
        public string completedDescription;

        public String[] objectiveList;

        // If true then any trigger condiiton will trigger this mission;
        // If false then ALL trigger conditions will have to be true to trigger the mission;
        public bool triggerOnAny;
        public MissionTrigger[] triggerConditions;

        // If true then any completion condition will complete this mission;
        // If false then ALL completion conditions will have to be true to complete the mission;
        public bool completeOnAny;
        public MissionCompletion[] completionConditions;

        //list of missions which are set active by this one;
        public int[] activates;

        // Functions can be added to be triggered 'onStart' or 'onComplete'
        public UnityEvent onStart;
        public UnityEvent onCompletion;

        private bool started = false;
        private bool complete = false;

        //index of a mission 

        public bool isComplete()
        {
            return complete;
        }
        public void completeMission()
        {
            Debug.Log("Completed Mission " + name);
            if(onCompletion != null)
                onCompletion.Invoke();

            complete = true;
        }
        public bool hasStarted()
        {
            return started;
        }
        public void reset()
        {
            started = false;
            complete = false;
        }
        public void start()
        {
            Debug.Log("Starting Mission " + name);
            if(onStart != null)
                onStart.Invoke();
            started = true;
        }
    }

    public void setActive(int missionId, bool status)
    {
        activeList[missionId] = status;
    }

    [System.Serializable]
    public class MissionTrigger
    {
        public TriggerType triggerType;
        public int triggerValue;
    }

    [System.Serializable]
    public class MissionCompletion
    {
        public CompletionType completionType;
        public int completionValue;
        public UpgradableComponentIndex componentIndex;
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
    Outpost,                 // Complete mission if outpost is visited
    Upgrade,
    Repair
}