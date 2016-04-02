using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class CommandConsoleState : MonoBehaviour {

#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] private Canvas canvas;
    [SerializeField] private Text civiliansText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text resourcesText;
    [SerializeField] private Text shieldsText;

    [SerializeField] private Text upgradeButtonLabel;
    [SerializeField] private Text costLabel;
    [SerializeField] private Text upgradeDescription;

    [SerializeField] Material defaultMat;
    [SerializeField] Material highlightMat;

    [SerializeField] private GameObject newsFeed;
    [SerializeField] private GameObject popupWindow;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color highlightColor;
#pragma warning restore 0649

    private PlayerController playerController;
    private GameObject ship;
    private GameObject upgradeArea;
    private GameObject portal;
    private StratMap stratMap;
    private GameState gameState;
    private GameSettings settings;
    private List<ConsoleUpgrade> consoleUpgrades = new List<ConsoleUpgrade>();
    private ConsoleUpgrade.UpgradeProperties[] upgradeProperties;
    private Dictionary<string, uint> currentOfficers;

    private int componentToUpgrade = 0;
    private double second = 0;

    // Indicates which upgrade is in progress.
    private int[] upgradeProgress = new int[6] { 0, 0, 0, 0, 0, 0 };

    private ConsoleShipControl shipControl;

    void Start() {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        upgradeArea = GameObject.Find("UpgradeInfo");
        stratMap = GameObject.Find("Map").GetComponent<StratMap>();
        stratMap.Portal = portal;
        currentOfficers = new Dictionary<string, uint>();
        LoadSettings();

        Camera.main.GetComponent<ToggleGraphics>().UpdateGraphics();
        Camera.main.GetComponent<ToggleGraphics>().SetCommandGraphics();

        LoadShipModel();

        UpdateAllText();

        ClosePopupWindow();

        upgradeArea.SetActive(false);
        newsFeed.GetComponent<Text>().text = "";
        AddUpgradeBoxes();
    }

    public void Reset()
    {
        LoadSettings();
        upgradeProgress = new int[6] { 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < consoleUpgrades.Count; i++)
        {
            consoleUpgrades[i].Reset();
            //upgradeProperties[i].currentLevel = 1;
            consoleUpgrades[i].UpdateCost(GetUpgradeCost(upgradeProperties[i].cost, upgradeProperties[i].currentLevel));
        }
        foreach (ComponentType type in Enum.GetValues(typeof(ComponentType)))
        {
            if (type == ComponentType.None)
                continue;

            int component = (int)type;
            consoleUpgrades[component].SetUpgradeInfo(upgradeProperties[component]);
            upgradeButtonLabel.text = "Upgrade";
        }
        UpdateAllText();
        ClosePopupWindow();
        upgradeArea.SetActive(false);
        newsFeed.GetComponent<Text>().text = "";
        stratMap.Reset();
    }

    private void LoadSettings()
    {
        // Copy values manually so original settings won't change
        upgradeProperties = new ConsoleUpgrade.UpgradeProperties[settings.upgradeProperties.Length];
        for (int i = 0; i < upgradeProperties.Length; i++)
        {
            upgradeProperties[i] = new ConsoleUpgrade.UpgradeProperties();
            upgradeProperties[i].type = settings.upgradeProperties[i].type;
            upgradeProperties[i].description = settings.upgradeProperties[i].description;
            upgradeProperties[i].cost = settings.upgradeProperties[i].cost;
            upgradeProperties[i].numberOfLevels = settings.upgradeProperties[i].numberOfLevels;
            upgradeProperties[i].repairable = settings.upgradeProperties[i].repairable;
            upgradeProperties[i].currentLevel = settings.upgradeProperties[i].currentLevel;
        }
    }
    void Update()
    {
        // Cheat code! But seriously, this will ease development so much.
        // Upgrades all in progress upgrade requests, basically makes it so you don't need to run the engineer. 
        if (Input.GetKeyDown ("c") && Input.GetKeyDown ("u")) 
        {
            EngineerUpgradeAllCheat();
        }
    }

    void FixedUpdate ()
    { 
        second += Time.deltaTime;
        if(second >= 1)
        {
            UpdateAllText();
            UpdateCostTextColor();
            second = 0;
        }
    }

    private void LoadShipModel()
    {
        // Load the ship model into the scene. 
        ship = Instantiate(Resources.Load("Prefabs/CommandShip", typeof(GameObject))) as GameObject;
        ship.transform.position = new Vector3(18f, -2.5f, -1961f);
        ship.transform.eulerAngles = new Vector3(0, 250f, 0);
        ship.AddComponent<ConsoleShipControl>();
        shipControl = ship.GetComponent<ConsoleShipControl>();
        shipControl.SetMaterials(defaultMat, highlightMat);
    }
    private void AddUpgradeBoxes()
    {
        Transform canvas = gameObject.transform.Find("Canvas");
        foreach(ComponentType type in Enum.GetValues(typeof(ComponentType)))
        {
            if(type == ComponentType.None)
                continue;
            
            int component = (int)type;
            GameObject upgradeBox = Instantiate(Resources.Load("Prefabs/UpgradeBox", typeof(GameObject))) as GameObject;
            upgradeBox.transform.SetParent(canvas);
            upgradeBox.transform.localScale = new Vector3(1,1,1);
            upgradeBox.transform.localPosition = new Vector3(-483, 200 - (component*80), 0);
            upgradeBox.GetComponent<ConsoleUpgrade>().SetUpgradeInfo(upgradeProperties[component]);
            upgradeBox.GetComponent<Button>().onClick.AddListener(delegate{OnClickUpgrade(component);});
            upgradeBox.transform.Find("UpgradeRepairButton").GetComponent<Button>().onClick.AddListener(delegate{OnClickRepair(component);});
            consoleUpgrades.Add(upgradeBox.GetComponent<ConsoleUpgrade>());
        }
    }
        
    public void givePlayerControllerReference(PlayerController controller)
    {
        playerController = controller;
    }
        

    /// <summary>
    /// Checks the upgrade cost of a component
    /// </summary>
    /// <returns><c>true</c>, if upgrade cost was checked, <c>false</c> otherwise.</returns>
    /// <param name="baseCost">Base cost of component to be upgraded</param>
    /// <param name="level">Level of the component</param>
    private bool CheckUpgradeCost(int baseCost, int level) 
    {
        return (gameState.GetShipResources() >= GetUpgradeCost(baseCost, level));
    }

    /// <summary>
    /// Calculates the upgrade cost of a component
    /// </summary>
    /// <returns>The upgrade cost.</returns>
    /// <param name="baseCost">Base cost.</param>
    /// <param name="level">Level.</param>
    private int GetUpgradeCost(int baseCost, int level)
    {
        // Current simplistic cost model. Should be updated for proper usage in game.
        return baseCost * level;
    }

    /// <summary>
    /// Sends the upgrade request for a component of the ship
    /// </summary>
    /// <param name="type">Type of component</param>
    /// <param name="baseCost">Base cost of the component</param>
    /// <param name="level">Level of the component</param>
    private bool UpgradeComponent(int componentId, int baseCost, int level)
    {
        if(CheckUpgradeCost(baseCost, level))
        {
            // Update the ships resources
            gameState.UseShipResources(GetUpgradeCost(baseCost, level));

            // Show level indictor for new level.
            consoleUpgrades[componentId].UpdateLevelIndicator(level);

            // Send request to engineer to upgrade
            playerController.CmdAddUpgrade((ComponentType)componentId);

            // Update resources text with new value.
            UpdateAllText();
            return true;
        }
        return false;
    }

    public void OutpostVisitNotify(int resources, int civilians, int id)
    {
        UpdateNewsFeed("[Outpost] Collected " + resources + " Resources");
        UpdateNewsFeed("[Outpost] Saved " + civilians + " Civilians");
        if (stratMap != null) stratMap.outpostVisitNotify(id);
    }

    /// <summary>
    /// Confirms the upgrade, is called when the engineer has completed the upgrade.
    /// </summary>
    /// <param name="type">Type.</param>
    public void ConfirmUpgrade(ComponentType type)
    {
        upgradeProgress[(int)type] = 0;
        upgradeButtonLabel.text = "Upgrade";
        upgradeProperties[(int)type].currentLevel++;
        UpdateNewsFeed("[Engineer] " + upgradeProperties[(int)type].name + " upgrade is complete.");
    }

    /// <summary>
    /// Confirms the repair, is called when the engineer has completed the repair.
    /// </summary>
    /// <param name="type">Type.</param>
    public void ConfirmRepair(ComponentType type)
    {
        consoleUpgrades[(int)type].HideRepairButton();
        UpdateNewsFeed("[Engineer] " + upgradeProperties[(int)type].name + " has been repaired.");
    }

    /// <summary>
    /// Updates the map of officerName->officerId which
    /// holds the officers for the current game
    /// </summary>
    /// <param name="officerData"></param>
    public void UpdateOfficerList(string officerData)
    {
        // Clear the previous game's officers
        currentOfficers.Clear();

        string[] comma = {","};
        string[] colon = {":"};

        // Splits the officer data to return a list of mappings "officer_name:officer_id"
        // Ignores the last entry which should be empty
        string[] officerList = officerData.Split(comma, StringSplitOptions.RemoveEmptyEntries);

        // Add each officer's data to the map
        foreach (string officer in officerList)
        {
            string[] mapping = officer.Split(colon, StringSplitOptions.None);
            currentOfficers[mapping[0]] = UInt32.Parse(mapping[1]);
        }
    }
        
    public void HighlightComponent(int component)
    {
        shipControl.HighlightComponent(component);
    }

    public void OnClickUpgrade(int component)
    {
        // Set the upgrade area active.
        upgradeArea.SetActive(true);
        componentToUpgrade = component;
        // Highlight the selected component on the ship model.
        HighlightComponent(component);
        // Upgrade description and cost labels
        upgradeDescription.text = upgradeProperties[component].description;
        costLabel.text = GetUpgradeCost(upgradeProperties[component].cost, upgradeProperties[component].currentLevel).ToString();
        // Upgrade the cost text to display in red if the player does not have enough resources.
        UpdateCostTextColor();

        if(upgradeProgress[component] == 1) 
            upgradeButtonLabel.text = "Waiting";
        else
            upgradeButtonLabel.text = "Upgrade";
    }

    public void OnClickRepair(int component)
    {
        playerController.CmdAddRepair((ComponentType)component);
    }

    //Called whenever an upgrade is purchased
    public void UpgradeShip()
    {
        // If we are already waiting then we don't want to upgrade again.
        if(upgradeProgress[componentToUpgrade] == 1)
            return;

        // Try to upgrade the component
        if(!UpgradeComponent(componentToUpgrade, upgradeProperties[componentToUpgrade].cost, upgradeProperties[componentToUpgrade].currentLevel))
            return;

        // Update the cost of the component
        consoleUpgrades[componentToUpgrade].UpdateCost(GetUpgradeCost(upgradeProperties[componentToUpgrade].cost, upgradeProperties[componentToUpgrade].currentLevel + 1));

        upgradeProgress[componentToUpgrade] = 1;
        upgradeButtonLabel.text = "Waiting";
    }
        
	/// <summary>
	/// Update all text values on screen.
	/// </summary>
	private void UpdateAllText()
	{
		// Update the text with values from gamestate.
        civiliansText.text = gameState.GetCivilians().ToString();;
        resourcesText.text = gameState.GetShipResources().ToString();;
        healthText.text    = ((int)Math.Round(gameState.GetShipHealth(), 0)).ToString();;
        shieldsText.text   = ((int)Math.Round(gameState.GetShipShield(), 0)).ToString();;
	}

    private void UpdateCostTextColor()
    {
        if(int.Parse(costLabel.text) > gameState.GetShipResources())
        {
            costLabel.color = new Color(95f/255f, 24f/255f, 24f/255f, 1);
        }
        else
        {
            costLabel.color = new Color(176f/255f, 176f/255f, 176f/255f, 1);
        }
    }
    
    public void FoundOutpost(GameObject outpost, int id, int difficulty)
    {
        UpdateNewsFeed("[Outpost] Outpost Discovered!");
        stratMap.NewOutpost(outpost,id,difficulty);
    }

    public void PortalInit(GameObject portal)
    {
        this.portal = portal;
    }

    public void ShowMissionPopup(string title, string descrption)
    {
        popupWindow.SetActive(true);
        popupWindow.transform.Find("MissionTitle").GetComponent<Text>().text = title;
        popupWindow.transform.Find("MissionDescription").GetComponent<Text>().text = descrption;
    }

    public void StartMissionOnMap(int id)
    {
        if (stratMap == null) print("stratmap == null");
        else stratMap.startMission(id);
    }

    public void EndMissionOnMap(int id)
    {
        stratMap.endMission(id);
    }


    public void ClosePopupWindow()
    {
        popupWindow.SetActive(false);
    }

    /// <summary>
    /// This is a cheat, it confirms the upgrade of all in progress upgrade requests. 
    /// It saves loading up another instance of the game for the engineer.
    /// </summary>
    private void EngineerUpgradeAllCheat()
    {
        foreach(ComponentType type in ComponentType.GetValues(typeof(ComponentType)))
        {
            if(type == ComponentType.None)
                continue;
            
            if(upgradeProgress[(int)type] == 1)
            {
                Debug.Log("Cheating engineer upgrade for " + type.ToString());
                ConfirmUpgrade(type);
            }
        }
    }

    private void UpdateNewsFeed(string message)
    {
        newsFeed.GetComponent<Text>().text = message + "\n" + newsFeed.GetComponent<Text>().text;
    }
}
