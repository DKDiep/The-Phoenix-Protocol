using UnityEngine;
using UnityEngine.UI;



public class CommandConsoleState : MonoBehaviour {

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Canvas canvas;
	[SerializeField] private Text civiliansText;
	[SerializeField] private Text healthText;
	[SerializeField] private Text resourcesText;
    [SerializeField] private Text shieldsText;
	
    [SerializeField] private Text shieldsLabel;
    [SerializeField] private Text turretsLabel;
	[SerializeField] private Text engineLabel;
    [SerializeField] private Text hullLabel;
    [SerializeField] private Text droneLabel;
    [SerializeField] private Text storageLabel;

	[SerializeField] private Text shieldsUpgradeLabel;
	[SerializeField] private Text turretsUpgradeLabel;
	[SerializeField] private Text engineUpgradeLabel;
    [SerializeField] private Text hullUpgradeLabel;
    [SerializeField] private Text droneUpgradeLabel;
    [SerializeField] private Text storageUpgradeLabel;

	[SerializeField] private Text popUpText;
	[SerializeField] private GameObject shieldsButton;
	[SerializeField] private GameObject turretsButton;
	[SerializeField] private GameObject engineButton;
	[SerializeField] private GameObject popUp;
	[SerializeField] private GameObject levelCounter1;
	[SerializeField] private GameObject levelCounter2;
	[SerializeField] private GameObject levelCounter3;
	[SerializeField] private GameObject levelCounter4;
	[SerializeField] private GameObject newsFeed;
	#pragma warning restore 0649

    private PlayerController playerController;
	private GameObject ship;
	private GameState gameState;
    private GameSettings settings;

	// Local copies of the ships values
    private int shipResources;
	private int shipCivilians;
	private float shipHealth;
	private float shipShields;

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

        shieldsUpgradeLabel.text = shieldsInitialCost.ToString();
        turretsUpgradeLabel.text = turretsInitialCost.ToString();
        engineUpgradeLabel.text = enginesInitialCost.ToString();
        hullUpgradeLabel.text = hullInitialCost.ToString();
        droneUpgradeLabel.text = droneInitialCost.ToString();
        storageUpgradeLabel.text = storageInitialCost.ToString();

        levelCounter1.SetActive(true);
        levelCounter2.SetActive(false);
        levelCounter3.SetActive(false);
        levelCounter4.SetActive(false);
        newsFeed.SetActive(false);

		// Hide the popup by default
        popUpText.text = "";
        popUp.SetActive(false);
        
        shieldsButton.SetActive(true);
        turretsButton.SetActive(true);
        engineButton.SetActive(true);

        // Remove crosshair from this scene. 
        GameObject.Find("CrosshairCanvas(Clone)").SetActive(false);
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

    public void Engin(bool isOn)
    {
        if (upgrade)
        {
            engineLabel.text = engineLabel.text + " I";
            upgrade = false;
        }
    }
		
    public void systemPopUp(int system)
    {
        switch (system)
        {
            case 0:
                popUpText.text = "Shields";
                showLevelCounters(shieldsLevel);
                break;
            case 1:
                popUpText.text = "Turrets";
                showLevelCounters(turretsLevel);
                break;
            case 2:
                showLevelCounters(engineLevel);
                popUpText.text = "Engines";
                break;
        }
        popUp.SetActive(true);
    }

    //Enables/disables level counter objects such that there are <level> of them visible
    private void showLevelCounters(int level)
    {
        levelCounter1.SetActive(false);
        levelCounter2.SetActive(false);
        levelCounter3.SetActive(false);
        levelCounter4.SetActive(false);
        if (level > 0) levelCounter1.SetActive(true);
        if (level > 1) levelCounter2.SetActive(true);
        if (level > 2) levelCounter3.SetActive(true);
        if (level > 3) levelCounter4.SetActive(true);
    }

    /// <summary>
    /// Checks the upgrade cost of a component
    /// </summary>
    /// <returns><c>true</c>, if upgrade cost was checked, <c>false</c> otherwise.</returns>
    /// <param name="baseCost">Base cost of component to be upgraded</param>
    /// <param name="level">Level of the component</param>
    private bool CheckUpgradeCost(int baseCost, int level) 
    {
        return (shipResources >= GetUpgradeCost(baseCost, level));
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
    private void UpgradeComponent(ComponentType type, int baseCost, int level)
    {
        if(CheckUpgradeCost(baseCost, level))
        {
            shipResources -= GetUpgradeCost(baseCost, level);
            showLevelCounters(level);
            playerController.CmdAddUpgrade(type);
        }
    }

    //Called whenever an upgrade is purchased (by clicking yellow button)
    public void UpgradeShip(int component)
    {
        switch (component)
        {
            // Shields Upgrade
            case 0: 
                UpgradeComponent(ComponentType.ShieldGenerator, shieldsInitialCost, shieldsLevel);
                shieldsUpgradeLabel.text = GetUpgradeCost(shieldsInitialCost, shieldsLevel + 1).ToString();
                break;
            // Turrets Upgrade
            case 1:
                UpgradeComponent(ComponentType.Turret, turretsInitialCost, turretsLevel);
                turretsUpgradeLabel.text = GetUpgradeCost(turretsInitialCost, turretsLevel + 1).ToString();
                break;
            // Engine Upgrade
            case 2:
                UpgradeComponent(ComponentType.Engine, enginesInitialCost, engineLevel);
                engineUpgradeLabel.text = GetUpgradeCost(enginesInitialCost, engineLevel + 1).ToString();
                break;
            // Hull Upgrade
            case 3:
                UpgradeComponent(ComponentType.Bridge, hullInitialCost, hullLevel);
                hullUpgradeLabel.text = GetUpgradeCost(hullInitialCost, hullLevel + 1).ToString();
                break;
            // Drone Upgrade
            case 4:
                UpgradeComponent(ComponentType.Drone, droneInitialCost, droneLevel);
                droneUpgradeLabel.text = GetUpgradeCost(droneInitialCost, droneLevel + 1).ToString();
                break;
            // Resource Storage Upgrade
            case 5:
                UpgradeComponent(ComponentType.ResourceStorage, storageInitialCost, storageLevel);
                storageUpgradeLabel.text = GetUpgradeCost(storageInitialCost, storageLevel + 1).ToString();
                break;
        }
        upgrade = false;
		UpdateAllText();
    }
        
	/// <summary>
	/// Update all text values on screen.
	/// </summary>
	private void UpdateAllText()
	{
		// Get resources and health from the gamestate.
		int shipCivilians = gameState.GetCivilians();
		int shipResources = gameState.GetShipResources();
		float shipHealth  = gameState.GetShipHealth();
		float shipShields = gameState.GetShipShield();

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
