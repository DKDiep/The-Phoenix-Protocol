using UnityEngine;
using UnityEngine.UI;



public class CommandConsoleState : MonoBehaviour {

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Canvas canvas;
	[SerializeField] private Text civiliansText;
	[SerializeField] private Text healthText;
	[SerializeField] private Text resourcesText;
    [SerializeField] private Text shieldsText;

    [SerializeField] private Text[] upgradeButtonLabels;

    [SerializeField] private Text shieldsCostLabel;
    [SerializeField] private Text turretsCostLabel;
    [SerializeField] private Text engineCostLabel;
    [SerializeField] private Text hullCostLabel;
    [SerializeField] private Text droneCostLabel;
    [SerializeField] private Text storageCostLabel;

	[SerializeField] private GameObject newsFeed;

    [SerializeField] private GameObject[] levelIndicator;
	#pragma warning restore 0649

    private PlayerController playerController;
	private GameObject ship;
	private GameState gameState;
    private GameSettings settings;

    private bool upgrade = true;
    private int shieldsLevel = 1;
    private int engineLevel = 1;
    private int turretsLevel = 1;
    private int hullLevel = 1;
    private int droneLevel = 1;
    private int storageLevel = 1;

    private double second = 0; 

    private int shieldsInitialCost;
    private int turretsInitialCost;
    private int enginesInitialCost;
    private int hullInitialCost;
    private int droneInitialCost;
    private int storageInitialCost;

    void Start () {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        LoadSettings(); 

		// Load the ship model into the scene. 
		ship = Instantiate(Resources.Load("Prefabs/CommandShip", typeof(GameObject))) as GameObject;
		ship.AddComponent<ConsoleShipControl>();

		UpdateAllText();

        shieldsCostLabel.text = shieldsInitialCost.ToString();
        turretsCostLabel.text = turretsInitialCost.ToString();
        engineCostLabel.text = enginesInitialCost.ToString();
        hullCostLabel.text = hullInitialCost.ToString();
        droneCostLabel.text = droneInitialCost.ToString();
        storageCostLabel.text = storageInitialCost.ToString();

        newsFeed.SetActive(false);

        // Remove crosshair from this scene. 
        GameObject.Find("CrosshairCanvas(Clone)").SetActive(false);


        // Hide all level indicators.
        for(int i = 0; i < 6; i++)
            for(int k = 0; k < 3; k++)
                levelIndicator[i].transform.GetChild(k).gameObject.SetActive(false);
        
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
                break;
            case ComponentType.Turret:
                return 1;
                break;
            case ComponentType.Engine:
                return 2;
                break;
            case ComponentType.Bridge:
                return 3;
                break;
            case ComponentType.Drone:
                return 4;
                break;
            case ComponentType.ResourceStorage:
                return 5;
                break;
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
        levelIndicator[GetIdFromComponentType(type)].transform.GetChild(level - 1).gameObject.SetActive(true);
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
    private void ConfirmUpgrade(ComponentType type)
    {
        upgradeButtonLabels[GetIdFromComponentType(type)].text = "Upgrade";
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

    //Called whenever an upgrade is purchased (by clicking yellow button)
    public void UpgradeShip(int component)
    {
        // If we are already waiting then we don't want to upgrade again.
        if(upgradeButtonLabels[component].text == "Waiting")
            return;
        
        switch (component)
        {
            // Shields Upgrade
            case 0: 
                if(!UpgradeComponent(ComponentType.ShieldGenerator, shieldsInitialCost, shieldsLevel))
                    return;
                shieldsCostLabel.text = GetUpgradeCost(shieldsInitialCost, shieldsLevel + 1).ToString();
                break;
            // Turrets Upgrade
            case 1:
                if(!UpgradeComponent(ComponentType.Turret, turretsInitialCost, turretsLevel))
                    return;
                turretsCostLabel.text = GetUpgradeCost(turretsInitialCost, turretsLevel + 1).ToString();
                break;
            // Engine Upgrade
            case 2:
                if(!UpgradeComponent(ComponentType.Engine, enginesInitialCost, engineLevel))
                    return;
                engineCostLabel.text = GetUpgradeCost(enginesInitialCost, engineLevel + 1).ToString();
                break;
            // Hull Upgrade
            case 3:
                if(!UpgradeComponent(ComponentType.Bridge, hullInitialCost, hullLevel))
                    return;
                hullCostLabel.text = GetUpgradeCost(hullInitialCost, hullLevel + 1).ToString();
                break;
            // Drone Upgrade
            case 4:
                if(!UpgradeComponent(ComponentType.Drone, droneInitialCost, droneLevel))
                    return;
                droneCostLabel.text = GetUpgradeCost(droneInitialCost, droneLevel + 1).ToString();
                break;
            // Resource Storage Upgrade
            case 5:
                if(!UpgradeComponent(ComponentType.ResourceStorage, storageInitialCost, storageLevel))
                    return;
                storageCostLabel.text = GetUpgradeCost(storageInitialCost, storageLevel + 1).ToString();
                break;
        }
        upgradeButtonLabels[component].text = "Waiting";
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
    
    public void foundOutpost(string message)
    {
        newsFeed.SetActive(true);
        newsFeed.GetComponent<Text>().text = message;
    }
}
