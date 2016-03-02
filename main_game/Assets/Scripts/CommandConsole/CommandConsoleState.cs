using UnityEngine;
using UnityEngine.UI;



public class CommandConsoleState : MonoBehaviour {

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Canvas canvas;
	[SerializeField] private Text civiliansText;
	[SerializeField] private Text healthText;
	[SerializeField] private Text resourcesText;
	[SerializeField] private Text shieldsText;
	[SerializeField] private Text engineLabel;
	[SerializeField] private Text shieldsLabel;
	[SerializeField] private Text shieldsUpgradeLabel;
	[SerializeField] private Text gunsUpgradeLabel;
	[SerializeField] private Text engineUpgradeLabel;
	[SerializeField] private Text popUpText;
	[SerializeField] private GameObject shieldsButton;
	[SerializeField] private GameObject gunsButton;
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

	// Local copies of the ships values
    private int shipResources;
	private int shipCivilians;
	private float shipHealth;
	private float shipShields;

    private bool upgrade = true;
    private int shieldsLevel = 1;
    private int engineLevel = 1;
    private int gunsLevel = 1;
    private double second = 0; 

    void Start () {
		GameObject server = GameObject.Find("GameManager");
		gameState = server.GetComponent<GameState>();

		// Load the ship model into the scene. 
		ship = Instantiate(Resources.Load("Prefabs/CommandShip", typeof(GameObject))) as GameObject;
		ship.AddComponent<ConsoleShipControl>();

		UpdateAllText();

        shieldsUpgradeLabel.text = shieldsLevel * 100 + "M";
        gunsUpgradeLabel.text = gunsLevel * 100 + "M";
        engineUpgradeLabel.text = engineLevel * 100 + "M";
        levelCounter1.SetActive(true);
        levelCounter2.SetActive(false);
        levelCounter3.SetActive(false);
        levelCounter4.SetActive(false);
        newsFeed.SetActive(false);

		// Hide the popup by default
        popUpText.text = "";
        popUp.SetActive(false);
        
        shieldsButton.SetActive(true);
        gunsButton.SetActive(true);
        engineButton.SetActive(true);

        // Remove crosshair from this scene. 
        GameObject.Find("CrosshairCanvas(Clone)").SetActive(false);
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
                popUpText.text = "Guns";
                showLevelCounters(gunsLevel);
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

    
    //Called whenever an upgrade is purchased (by clicking yellow button) 0 = Shields, 1 = Guns, 2 = Engines
    public void UpgradeShip(int where)
    {
        switch (where)
        {
            case 0:
			if(shipResources >= 100 * shieldsLevel)
                {
				shipResources -= 100 * shieldsLevel;
                    shieldsLevel++;
                    shieldsUpgradeLabel.text = shieldsLevel * 100 + "M";
                    showLevelCounters(shieldsLevel);
                    playerController.CmdAddUpgrade(ComponentType.ShieldGenerator);
                }
                break;
            case 1:
			if(shipResources >= 100 * gunsLevel)
                {
				shipResources -= 100 * gunsLevel;
                    gunsLevel++;
                    gunsUpgradeLabel.text = gunsLevel * 100 + "M";
                    showLevelCounters(gunsLevel);

                }
                break;
            case 2:
			if (shipResources >= 100 * engineLevel)
                {
				shipResources -= 100 * engineLevel;
                    engineLevel++;
                    playerController.CmdAddUpgrade(ComponentType.Engine);
                    engineUpgradeLabel.text = engineLevel * 100 + "M";
                    showLevelCounters(gunsLevel);
                }
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
		civiliansText.text = "Civilians:  " + shipCivilians;
		resourcesText.text = "Resources:  " + shipResources;
		healthText.text    = "Health:  " + shipHealth;
		shieldsText.text   = "Shields:  " + shipShields;
	}
    
    public void foundOutpost(string message)
    {
        newsFeed.SetActive(true);
        newsFeed.GetComponent<Text>().text = message;
    }
}
