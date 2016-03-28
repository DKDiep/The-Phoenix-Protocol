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

    private int[] componentLevels = new int[6] {1,1,1,1,1,1};
    private int componentToUpgrade = 0;

    private double second = 0; 

    private string[] upgradeNames = new string[6] {"SHIELDS", "TURRETS", "ENGINES", "HULL", "DRONE", "STORAGE"};
    private int[] upgradeCosts = new int[6];
    private string[] upgradeDescriptions = new string[6];
    private int[] upgradeMaxLevels = new int[6];

    // Indicates which upgrade is in progress.
    private int[] upgradeProgress = new int[6] {0,0,0,0,0,0};
    private bool[] componentRepairable = new bool[6] {true, true, true, true, false, false};

    private ConsoleShipControl shipControl;
   
    void Start () {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        upgradeArea = GameObject.Find("UpgradeInfo");
        stratMap = GameObject.Find("Map").GetComponent<StratMap>();
        stratMap.Portal = portal;
        LoadSettings();
       
        Camera.main.GetComponent<ToggleGraphics>().UpdateGraphics();
        Camera.main.GetComponent<ToggleGraphics>().SetCommandGraphics();

        LoadShipModel();

		UpdateAllText();
      
        // Remove crosshair from this scene. 
        GameObject crosshairCanvas = GameObject.Find("CrosshairCanvas(Clone)");
        if(crosshairCanvas != null)
            crosshairCanvas.SetActive(false);
        
        ClosePopupWindow();

        upgradeArea.SetActive(false);
        newsFeed.GetComponent<Text>().text = "";
        AddUpgradeBoxes();
    }

    private void LoadSettings() 
    {
        upgradeCosts[0]  = settings.ShieldsInitialCost;
        upgradeCosts[1]  = settings.TurretsInitialCost;
        upgradeCosts[2]  = settings.EnginesInitialCost;
        upgradeCosts[3]  = settings.HullInitialCost;
        upgradeCosts[4]  = settings.DroneInitialCost;
        upgradeCosts[5]  = settings.StorageInitialCost;

        upgradeDescriptions[0] = settings.ShieldsDescription;
        upgradeDescriptions[1] = settings.TurretsDescription;
        upgradeDescriptions[2] = settings.EnginesDescription;
        upgradeDescriptions[3] = settings.HullDescription;
        upgradeDescriptions[4] = settings.DroneDescription;
        upgradeDescriptions[5] = settings.StorageDescription;

        upgradeMaxLevels[0] = settings.ShieldsUpgradeLevels;
        upgradeMaxLevels[1] = settings.TurretsUpgradeLevels;
        upgradeMaxLevels[2] = settings.EnginesUpgradeLevels;
        upgradeMaxLevels[3] = settings.HullUpgradeLevels;
        upgradeMaxLevels[4] = settings.DroneUpgradeLevels;
        upgradeMaxLevels[5] = settings.StorageUpgradeLevels;

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
            upgradeBox.GetComponent<ConsoleUpgrade>().SetUpgradeInfo(type, upgradeNames[component], upgradeCosts[component], upgradeMaxLevels[component], componentRepairable[component]);
            upgradeBox.GetComponent<Button>().onClick.AddListener(delegate{OnClickUpgrade(component);});
            upgradeBox.transform.Find("UpgradeRepairButton").GetComponent<Button>().onClick.AddListener(delegate{OnClickRepair(component);});
            consoleUpgrades.Add(upgradeBox.GetComponent<ConsoleUpgrade>());
        }
    }
        
    public void givePlayerControllerReference(PlayerController controller)
    {
        playerController = controller;
    }
        
    private int GetIdFromComponentType(ComponentType type)
    {
        switch(type)
        {
            case ComponentType.ShieldGenerator:
                return 0;
            case ComponentType.Turret:
                return 1;
            case ComponentType.Engine:
                return 2;
            case ComponentType.Bridge:
                return 3;
            case ComponentType.Drone:
                return 4;
            case ComponentType.ResourceStorage:
                return 5;
        }
        return 0;
    }

    private ComponentType GetComponentTypeFromId(int id)
    {
        switch(id)
        {
        case 0:
            return ComponentType.ShieldGenerator;
        case 1:
            return ComponentType.Turret;
        case 2:
            return ComponentType.Engine;
        case 3:
            return ComponentType.Bridge;
        case 4:
            return ComponentType.Drone;
        case 5:
            return ComponentType.ResourceStorage;
        }
        return 0;
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
            playerController.CmdAddUpgrade(GetComponentTypeFromId(componentId));

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
        upgradeProgress[GetIdFromComponentType(type)] = 0;
        upgradeButtonLabel.text = "Upgrade";
        componentLevels[GetIdFromComponentType(type)]++;
        UpdateNewsFeed("[Engineer] " + upgradeNames[GetIdFromComponentType(type)] + " upgrade is complete.");
    }

    /// <summary>
    /// Confirms the repair, is called when the engineer has completed the repair.
    /// </summary>
    /// <param name="type">Type.</param>
    public void ConfirmRepair(ComponentType type)
    {
        consoleUpgrades[(int)type].HideRepairButton();
        UpdateNewsFeed("[Engineer] " + upgradeNames[GetIdFromComponentType(type)] + " has been repaired.");
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
        upgradeDescription.text = upgradeDescriptions[component];
        costLabel.text = GetUpgradeCost(upgradeCosts[component], componentLevels[component]).ToString();
        // Upgrade the cost text to display in red if the player does not have enough resources.
        UpdateCostTextColor();

        if(upgradeProgress[component] == 1) 
            upgradeButtonLabel.text = "Waiting";
        else
            upgradeButtonLabel.text = "Upgrade";
    }

    public void OnClickRepair(int component)
    {
        playerController.CmdAddRepair(GetComponentTypeFromId(component));
    }
    //Called whenever an upgrade is purchased (by clicking yellow button)
    public void UpgradeShip()
    {
        // If we are already waiting then we don't want to upgrade again.
        if(upgradeProgress[componentToUpgrade] == 1)
            return;

        // Try to upgrade the component
        if(!UpgradeComponent(componentToUpgrade, upgradeCosts[componentToUpgrade], componentLevels[componentToUpgrade]))
            return;

        // Update the cost of the component
        consoleUpgrades[componentToUpgrade].UpdateCost(GetUpgradeCost(upgradeCosts[componentToUpgrade], componentLevels[componentToUpgrade] + 1));

        upgradeProgress[componentToUpgrade] = 1;
        upgradeButtonLabel.text = "Waiting";
    }
        
	/// <summary>
	/// Update all text values on screen.
	/// </summary>
	private void UpdateAllText()
	{
		// Get resources and health from the gamestate.
        int shipCivilians   = gameState.GetCivilians();
		int shipResources   = gameState.GetShipResources();
		float shipHealth    = gameState.GetShipHealth();
		float shipShields   = gameState.GetShipShield();

		// Update the text
        civiliansText.text = shipCivilians.ToString();;
        resourcesText.text = shipResources.ToString();;
        healthText.text    = shipHealth.ToString();;
        shieldsText.text   = shipShields.ToString();;
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
    
    public void FoundOutpost(GameObject outpost, int id)
    {
        UpdateNewsFeed("[Outpost] Outpost Discovered!");
        stratMap.NewOutpost(outpost,id);
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
            if(upgradeProgress[GetIdFromComponentType(type)] == 1)
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
