using UnityEngine;
using UnityEngine.UI;



public class CommandConsoleState : MonoBehaviour {
    public Canvas canvas;
    public int TotalPower;
    public int Health;
    public Text PowerText;
    public Text MassText;
    public Text EngineLabel;
    public Text ShieldsLabel;
    public Text ShieldsUpgradeLabel;
    public Text GunsUpgradeLabel;
    public Text EngineUpgradeLabel;
    public Text PopUpText;
    public GameObject ShieldsButton;
    public GameObject GunsButton;
    public GameObject EngineButton;
    public GameObject PopUp;
    public GameObject LevelCounter1;
    public GameObject LevelCounter2;
    public GameObject LevelCounter3;
    public GameObject LevelCounter4;
    private PlayerController playerControlScript;
    private int remPower;
    private int mass;
    private bool upgrade = true;
    private bool engineOn = true;
    private bool shieldsOn = true;
    private bool gunsOn = true;
    private int shieldsLevel = 1;
    private int engineLevel = 1;
    private int gunsLevel = 1;
    private double second = 0; 
	GameObject ship;
    // Use this for initialization
    void Start () {

		ship = Instantiate(Resources.Load("Prefabs/CommandShip", typeof(GameObject))) as GameObject;
		ship.AddComponent<ConsoleShipControl>();

        mass = 0;
        remPower = 0;
        UpdatePower();
        UpdateMass();
        ShieldsUpgradeLabel.text = shieldsLevel * 100 + "M";
        GunsUpgradeLabel.text = gunsLevel * 100 + "M";
        EngineUpgradeLabel.text = engineLevel * 100 + "M";
        print("setting popuptext");
        LevelCounter1.SetActive(true);
        LevelCounter2.SetActive(false);
        LevelCounter3.SetActive(false);
        LevelCounter4.SetActive(false);
        PopUpText.text = "nufink";
        PopUp.SetActive(false);
        //ShieldsLabel.text = "Component got";
        ShieldsButton.SetActive(true);
        GunsButton.SetActive(true);
        EngineButton.SetActive(true);
        mass = 100;
        print("command console started");
    }

    public void givePlayerControllerReference(PlayerController playerControl)
    {
        playerControlScript = playerControl;
        playerControlScript.test();
    }

    public void test()
    {
        print("blehhrsh");
    }

    public void Engin(bool isOn)
    {
        if (upgrade)
        {
            EngineLabel.text = EngineLabel.text + " I";
            upgrade = false;
        }
        //enginOn = isOn;
        UpdatePower();
    }

    //Called whenever power is toggled on a system  0 = Shields, 1 = Guns, 2 = Engines
    public void PowerToggle(int system)
    {
        switch(system){
            case 0:
                shieldsOn = !shieldsOn;
                break;
            case 1:
                gunsOn = !gunsOn;
                break;
            case 2:
                engineOn = !engineOn;
                break;
        }
        UpdatePower();
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
                print("calling showLevelCounters(gunslevel) = " + gunsLevel);
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
        UpdatePower();
    }
    

    //Called whenever an upgrade is purchased (by clicking yellow button) 0 = Shields, 1 = Guns, 2 = Engines
    public void ThingUpgrade(int where)
    {
        switch (where)
        {
            case 0:
                if(mass >= 100 * shieldsLevel)
                {
                    mass -= 100 * shieldsLevel;
                    shieldsLevel++;
                    ShieldsUpgradeLabel.text = shieldsLevel * 100 + "M";
                    showLevelCounters(shieldsLevel);
                    playerControlScript.CmdUpgrade(0);
                }
                break;
            case 1:
                if(mass >= 100 * gunsLevel)
                {
                    mass -= 100 * gunsLevel;
                    gunsLevel++;
                    GunsUpgradeLabel.text = gunsLevel * 100 + "M";
                    showLevelCounters(gunsLevel);

                }
                break;
            case 2:
                if (mass >= 100 * engineLevel)
                {
                    mass -= 100 * engineLevel;
                    engineLevel++;
                    playerControlScript.CmdUpgrade(2);
                    EngineUpgradeLabel.text = engineLevel * 100 + "M";
                    showLevelCounters(gunsLevel);
                }
                break;
        }
        upgrade = false;
        UpdateMass();
    }

   /* private GameObject Level(int where)
    {
        switch (where)
        {
            case 0:
                return shieldsLevel;
                break;
            case 1:
                return gunsLevel;
                break;
            case 2:
                return engineLevel;
                break;
        }
    }*/


    void UpdatePower()
    {
        remPower = TotalPower;
        if (engineOn) remPower -= 3;
        if (shieldsOn) remPower -= 2;
        if (gunsOn) remPower -= 5;
        PowerText.text = "Power: " + remPower;
        //player = GetComponentInParent<PlayerControl>();
        //if (player != null) print("player != null");
        //else print("player is null");
    }

    void UpdateMass()
    {
        MassText.text = "Mass:  " + mass;
    }


    void FixedUpdate ()
    { 
        second += Time.deltaTime;
        if(second >= 1)
        {
            mass += 5;
            second = 0;
            UpdateMass();
        }
    }

    void onGui()
    {
       
    }

        // Update is called once per frame
    void Update () {
        
	}
}
