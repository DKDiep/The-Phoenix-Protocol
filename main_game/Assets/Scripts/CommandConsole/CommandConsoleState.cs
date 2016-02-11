using UnityEngine;
using UnityEngine.UI;



public class CommandConsoleState : MonoBehaviour {
	[SerializeField] private Canvas canvas;
	[SerializeField] private Text HealthText;
	[SerializeField] private Text ResourcesText;
	[SerializeField] private Text EngineLabel;
	[SerializeField] private Text ShieldsLabel;
	[SerializeField] private Text ShieldsUpgradeLabel;
	[SerializeField] private Text GunsUpgradeLabel;
	[SerializeField] private Text EngineUpgradeLabel;
	[SerializeField] private Text PopUpText;
	[SerializeField] private GameObject ShieldsButton;
	[SerializeField] private GameObject GunsButton;
	[SerializeField] private GameObject EngineButton;
	[SerializeField] private GameObject PopUp;
	[SerializeField] private GameObject LevelCounter1;
	[SerializeField] private GameObject LevelCounter2;
	[SerializeField] private GameObject LevelCounter3;
	[SerializeField] private GameObject LevelCounter4;

    private PlayerController playerController;
	private GameObject ship;
	private GameState gameState;

	// Local copies of the ships values
    private int shipResources;
	private float shipHealth;

    private bool upgrade = true;
    private bool engineOn = true;
    private bool shieldsOn = true;
    private bool gunsOn = true;
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


		// Get starting values for resources and health
		shipResources = gameState.GetShipResources();
		shipHealth = gameState.GetShipHealth();
        UpdateResources();
		UpdateHealth();


        ShieldsUpgradeLabel.text = shieldsLevel * 100 + "M";
        GunsUpgradeLabel.text = gunsLevel * 100 + "M";
        EngineUpgradeLabel.text = engineLevel * 100 + "M";
        LevelCounter1.SetActive(true);
        LevelCounter2.SetActive(false);
        LevelCounter3.SetActive(false);
        LevelCounter4.SetActive(false);


		// Hide the popup by default
        PopUpText.text = "";
        PopUp.SetActive(false);
        
        ShieldsButton.SetActive(true);
        GunsButton.SetActive(true);
        EngineButton.SetActive(true);

    }

    public void givePlayerControllerReference(PlayerController controller)
    {
        playerController = controller;
    }

    public void Engin(bool isOn)
    {
        if (upgrade)
        {
            EngineLabel.text = EngineLabel.text + " I";
            upgrade = false;
        }
    }
		
    public void systemPopUp(int system)
    {
        switch (system)
        {
            case 0:
                PopUpText.text = "Shields";
                showLevelCounters(shieldsLevel);
                break;
            case 1:
                PopUpText.text = "Guns";
                showLevelCounters(gunsLevel);
                break;
            case 2:
                showLevelCounters(engineLevel);
                PopUpText.text = "Engines";
                break;
        }
        PopUp.SetActive(true);
    }

    //Enables/disables level counter objects such that there are <level> of them visible
    private void showLevelCounters(int level)
    {
        LevelCounter1.SetActive(false);
        LevelCounter2.SetActive(false);
        LevelCounter3.SetActive(false);
        LevelCounter4.SetActive(false);
        if (level > 0) LevelCounter1.SetActive(true);
        if (level > 1) LevelCounter2.SetActive(true);
        if (level > 2) LevelCounter3.SetActive(true);
        if (level > 3) LevelCounter4.SetActive(true);
    }

    public void Shields(bool isOn)
    {
        shieldsOn = isOn;
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
                    ShieldsUpgradeLabel.text = shieldsLevel * 100 + "M";
                    showLevelCounters(shieldsLevel);
					playerController.CmdUpgrade(0);
                }
                break;
            case 1:
			if(shipResources >= 100 * gunsLevel)
                {
				shipResources -= 100 * gunsLevel;
                    gunsLevel++;
                    GunsUpgradeLabel.text = gunsLevel * 100 + "M";
                    showLevelCounters(gunsLevel);

                }
                break;
            case 2:
			if (shipResources >= 100 * engineLevel)
                {
				shipResources -= 100 * engineLevel;
                    engineLevel++;
					playerController.CmdUpgrade(2);
                    EngineUpgradeLabel.text = engineLevel * 100 + "M";
                    showLevelCounters(gunsLevel);
                }
                break;
        }
        upgrade = false;
        UpdateResources();
    }
		
    void UpdateResources()
    {
		ResourcesText.text = "Resources:  " + shipResources;
    }

	void UpdateHealth()
	{
		HealthText.text = "Health:  " + shipHealth;
	}


    void FixedUpdate ()
    { 
        second += Time.deltaTime;
        if(second >= 1)
        {
			// Get resources and health from the gamestate.
			shipResources = gameState.GetShipResources();
			shipHealth = gameState.GetShipHealth();
			UpdateResources();
			UpdateHealth();

            second = 0;
        }
    }

    void onGui()
    {
       
    }

    // Update is called once per frame
    void Update () {
        
	}
}
