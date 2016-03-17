using UnityEngine;
using UnityEngine.UI;



public class CommandConsoleState : MonoBehaviour {

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Canvas canvas;
	[SerializeField] private Text civiliansText;
	[SerializeField] private Text healthText;
	[SerializeField] private Text resourcesText;
    [SerializeField] private Text shieldsText;

    [SerializeField] private Text upgradeButtonLabel;
    [SerializeField] private Text costLabel;


 

	[SerializeField] private GameObject newsFeed;

    [SerializeField] private GameObject[] levelIndicator;
    [SerializeField] private GameObject[] backgrounds;
    [SerializeField] private Text[] upgradeCostLabel;
	#pragma warning restore 0649

    private PlayerController playerController;
	private GameObject ship;
    private GameObject upgradeArea;
    private StratMap stratMap;
	private GameState gameState;
    private GameSettings settings;

    private bool upgrade = true;
    private int shieldsLevel = 1;
    private int engineLevel = 1;
    private int turretsLevel = 1;
    private int hullLevel = 1;
    private int droneLevel = 1;
    private int storageLevel = 1;
    private int componentToUpgrade = 0;

    private double second = 0; 

    private int shieldsInitialCost;
    private int turretsInitialCost;
    private int enginesInitialCost;
    private int hullInitialCost;
    private int droneInitialCost;
    private int storageInitialCost;

    Color upgradeDefaultColor = new Vector4(27f/255f, 46f/255f, 91f/255f, 200f/255f);

    void Start () {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        upgradeArea = GameObject.Find("UpgradeInfo");
        stratMap = GameObject.Find("Map").GetComponent<StratMap>();
        LoadSettings(); 

		// Load the ship model into the scene. 
		ship = Instantiate(Resources.Load("Prefabs/CommandShip", typeof(GameObject))) as GameObject;
        ship.transform.position = new Vector3(15, -7, 50);
        ship.transform.eulerAngles = new Vector3(0, -140f, 0);
		ship.AddComponent<ConsoleShipControl>();

		UpdateAllText();

        upgradeCostLabel[0].text = shieldsInitialCost.ToString();
        upgradeCostLabel[1].text = turretsInitialCost.ToString();
        upgradeCostLabel[2].text = enginesInitialCost.ToString();
        upgradeCostLabel[3].text = hullInitialCost.ToString();
        upgradeCostLabel[4].text = droneInitialCost.ToString();
        upgradeCostLabel[5].text = storageInitialCost.ToString();

        newsFeed.SetActive(false);

        // Remove crosshair from this scene. 
        GameObject.Find("CrosshairCanvas(Clone)").SetActive(false);

        upgradeArea.SetActive(false);

        for(int i = 0; i < 6; i++)
            backgrounds[i].GetComponent<Image>().color = upgradeDefaultColor;
       

        // Hide all level indicators.
        for(int i = 0; i < 6; i++)
            for(int k = 0; k < 3; k++)
                levelIndicator[i].transform.GetChild(k).gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 86f/255f);
        
    }

    private void LoadSettings() 
    {
        shieldsInitialCost  = settings.ShieldsInitialCost;
        turretsInitialCost  = settings.TurretsInitialCost;
        enginesInitialCost  = settings.EnginesInitialCost;
        hullInitialCost     = settings.HullInitialCost;
        droneInitialCost    = settings.DroneInitialCost;
        storageInitialCost  = settings.StorageInitialCost;
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

    /// <summary>
    /// Updates the level indicator under each upgrade menu item.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="level">Level.</param>
    private void UpdateLevelIndicator(ComponentType type, int level)
    {
        levelIndicator[GetIdFromComponentType(type)].transform.GetChild(level - 1).gameObject.GetComponent<Image>().color = new Vector4(1, 1, 1, 86f/255f);
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
    private bool UpgradeComponent(ComponentType type, int baseCost, int level)
    {
        if(CheckUpgradeCost(baseCost, level))
        {
            // Update the ships resources
            gameState.UseShipResources(GetUpgradeCost(baseCost, level));

            // Show level indictor for new level.
            UpdateLevelIndicator(type, level);

            // Send request to engineer to upgrade
            playerController.CmdAddUpgrade(type);

            // Update resources text with new value.
            UpdateAllText();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Confirms the upgrade, is called when the engineer has completed the upgrade.
    /// </summary>
    /// <param name="type">Type.</param>
    public void ConfirmUpgrade(ComponentType type)
    {
        //upgradeButtonLabels[GetIdFromComponentType(type)].text = "Upgrade";
        switch(type)
        {
        case ComponentType.ShieldGenerator:
            shieldsLevel++;
            break;
        case ComponentType.Turret:
            turretsLevel++;
            break;
        case ComponentType.Engine:
            engineLevel++;
            break;
        case ComponentType.Bridge:
            hullLevel++;
            break;
        case ComponentType.Drone:
            droneLevel++;
            break;
        case ComponentType.ResourceStorage:
            storageLevel++;
            break;
        }

    }

    public void HighlightComponent(int component)
    {
        for(int i = 0; i < 6; i++)
        {
            backgrounds[i].GetComponent<Image>().color = upgradeDefaultColor;
        }
        backgrounds[component].GetComponent<Image>().color = new Vector4(28f/255f, 47f/255f, 98f/255f, 1);
    }
    public void OnClickUpgrade(int component)
    {
        upgradeArea.SetActive(true);
        componentToUpgrade = component;
        HighlightComponent(component);
        switch (component)
        {
            // Shields Upgrade
            case 0: 
                costLabel.text = GetUpgradeCost(shieldsInitialCost, shieldsLevel).ToString();
                break;
            // Turrets Upgrade
            case 1:
                costLabel.text = GetUpgradeCost(turretsInitialCost, turretsLevel).ToString();
                break;
            // Engine Upgrade
            case 2:
                costLabel.text = GetUpgradeCost(enginesInitialCost, engineLevel).ToString();
                break;
            // Hull Upgrade
            case 3:
                costLabel.text = GetUpgradeCost(hullInitialCost, hullLevel).ToString();
                break;
            // Drone Upgrade
            case 4:
                costLabel.text = GetUpgradeCost(droneInitialCost, droneLevel).ToString();
                break;
            // Resource Storage Upgrade
            case 5:
                costLabel.text = GetUpgradeCost(storageInitialCost, storageLevel).ToString();
                break;
        }
        UpdateCostTextColor();
    }

    public void RepairShip(int component)
    {

    }
    //Called whenever an upgrade is purchased (by clicking yellow button)
    public void UpgradeShip()
    {
        // If we are already waiting then we don't want to upgrade again.
        //if(upgradeButtonLabels[component].text == "Waiting")
        //    return;
        
        switch (componentToUpgrade)
        {
            // Shields Upgrade
            case 0: 
                if(!UpgradeComponent(ComponentType.ShieldGenerator, shieldsInitialCost, shieldsLevel))
                    return;
                upgradeCostLabel[0].text = GetUpgradeCost(shieldsInitialCost, shieldsLevel + 1).ToString();
                break;
            // Turrets Upgrade
            case 1:
                if(!UpgradeComponent(ComponentType.Turret, turretsInitialCost, turretsLevel))
                    return;
                upgradeCostLabel[1].text = GetUpgradeCost(turretsInitialCost, turretsLevel + 1).ToString();
                break;
            // Engine Upgrade
            case 2:
                if(!UpgradeComponent(ComponentType.Engine, enginesInitialCost, engineLevel))
                    return;
                upgradeCostLabel[2].text = GetUpgradeCost(enginesInitialCost, engineLevel + 1).ToString();
                break;
            // Hull Upgrade
            case 3:
                if(!UpgradeComponent(ComponentType.Bridge, hullInitialCost, hullLevel))
                    return;
                upgradeCostLabel[3].text = GetUpgradeCost(hullInitialCost, hullLevel + 1).ToString();
                break;
            // Drone Upgrade
            case 4:
                if(!UpgradeComponent(ComponentType.Drone, droneInitialCost, droneLevel))
                    return;
                upgradeCostLabel[4].text = GetUpgradeCost(droneInitialCost, droneLevel + 1).ToString();
                break;
            // Resource Storage Upgrade
            case 5:
                if(!UpgradeComponent(ComponentType.ResourceStorage, storageInitialCost, storageLevel))
                    return;
                upgradeCostLabel[5].text = GetUpgradeCost(storageInitialCost, storageLevel + 1).ToString();
                break;
        }
        //upgradeButtonLabel.text = "Waiting";
    }
        
	/// <summary>
	/// Update all text values on screen.
	/// </summary>
	private void UpdateAllText()
	{
		// Get resources and health from the gamestate.
        int shipCivilians   = gameState.GetCivilians();
		int shipResources   = gameState.GetShipResources();
		float shipHealth      = gameState.GetShipHealth();
		float shipShields     = gameState.GetShipShield();

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
    
    public void FoundOutpost(GameObject outpost)
    {
        newsFeed.SetActive(true);
        //newsFeed.GetComponent<Text>().text = message;
        stratMap.NewOutpost(outpost);
    }
}
