using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class CommandConsoleState : MonoBehaviour {

#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] private Canvas canvas;
    [SerializeField] private Text civiliansText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text resourcesText;
    [SerializeField] private Text shieldsText;

    [SerializeField] private Text upgradeButtonLabel;
    [SerializeField] private Text repairButtonLabel;
    [SerializeField] private Text costLabel;
    [SerializeField] private Text upgradeDescription;

    [SerializeField] Material defaultMat;
    [SerializeField] Material highlightMat;

    [SerializeField] private GameObject objectiveFeed;
    [SerializeField] private GameObject popupWindow;

    [SerializeField] private GameObject newsFeedBG;
    [SerializeField] private GameObject upgradeInfoBG;
    [SerializeField] private Text upgradeInfoName;
    [SerializeField] private GameObject mapBG;
    [SerializeField] private GameObject mapBGBG;
    [SerializeField] private GameObject missionWindowBG;
    [SerializeField] private GameObject healthBarPanel;
    [SerializeField] private GameObject shieldBarPanel;
    [SerializeField] private List<GameObject> abilityObjects;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color highlightColor;
#pragma warning restore 0649

    private GameObject eventSystem;
    private PlayerController playerController;
    private GameObject ship;
    private GameObject upgradeArea;
    private GameObject portal;
    private StratMap stratMap;
    private GameState gameState;
    private GameSettings settings;
    private List<ConsoleUpgrade> consoleUpgrades = new List<ConsoleUpgrade>();
    private List<GameObject> upgradeBoxes;
    private List<GameObject> healthSegments = new List<GameObject>();
    private List<GameObject> shieldSegments = new List<GameObject>();

    private List<MissionText> missionTexts = new List<MissionText>();
    private List<string> currentObjectives = new List<string>();
    private Image[] pulsateableImages;
    private Image[] abilityImages;
    private Image[] abilityCooldownImages;
    private Button[] abilityButtons;
    private bool[] pulsateToggle;
    private float[] abilityCooldowns;
    private UpgradeProperties[] upgradeProperties;

    private Image newsFeedImage;
    private Image upgradeInfoImage;
    private Image mapImage;       //this is the black area on the minimap
    private Image mapBGImage;     //this is the blue background behind the minimap
    private Image missionWindowImage;

    private int componentToUpgrade = 0;
    private double second = 0;
    private static Color uiColor = new Color(0.75f, 0.75f, 0.75f, 1);
    private static Color transpWhite = new Color(1, 1, 1, 86f / 255f);
    private static Color transpBlack = new Color(0, 0, 0, 86f / 255f);
    // Indicates which upgrade is in progress.
    private int[] upgradeProgress = new int[6] { 0, 0, 0, 0, 0, 0 };
    private int[] repairProgress = new int[6] { 0, 0, 0, 0, 0, 0 };
    private ConsoleShipControl shipControl;

    void Start() 
    {
        AddHealthAndShields();
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        upgradeArea = GameObject.Find("UpgradeInfo");
        stratMap = GameObject.Find("Map").GetComponent<StratMap>();

        pulsateToggle = new bool[Enum.GetNames(typeof(UIElementEnum)).Length];
        pulsateableImages = new Image[Enum.GetNames(typeof(UIElementEnum)).Length];
        pulsateableImages[(int)UIElementEnum.MapBG] = mapBGBG.GetComponent<Image>();
        pulsateableImages[(int)UIElementEnum.MissionWindow] = missionWindowBG.GetComponent<Image>();
        pulsateableImages[(int)UIElementEnum.UpgradeInfo] = upgradeInfoBG.GetComponent<Image>();
        pulsateableImages[(int)UIElementEnum.NewsFeed] = newsFeedBG.GetComponent<Image>();

        abilityCooldowns = new float[Enum.GetNames(typeof(AbilityEnum)).Length];
        abilityButtons = new Button[Enum.GetNames(typeof(AbilityEnum)).Length];
        abilityImages  = new Image[Enum.GetNames(typeof(AbilityEnum)).Length];
        abilityCooldownImages = new Image[Enum.GetNames(typeof(AbilityEnum)).Length];


        for (int i = 0; i < Enum.GetNames(typeof(AbilityEnum)).Length; i++)
        {
            abilityButtons[i] = abilityObjects[i].GetComponent<Button>();
            abilityImages[i] = abilityObjects[i].GetComponent<Image>();
            abilityCooldownImages[i] = abilityObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>();
            abilityCooldowns[i] = 60.0f;
        }

        for (int i = 0; i < pulsateToggle.Length; i++)
        {
            pulsateToggle[i] = false;
        }

        stratMap.Portal = portal;
        LoadSettings();

        Camera.main.GetComponent<ToggleGraphics>().UpdateGraphics();
        Camera.main.GetComponent<ToggleGraphics>().SetCommandGraphics();
        Camera.main.transform.position = new Vector3(0, 0, -2000);
        Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);

        LoadShipModel();

        UpdateAllText();

        popupWindow.SetActive(false);

        upgradeArea.SetActive(false);
        objectiveFeed.GetComponent<Text>().text = "";
        AddUpgradeBoxes();
    }



    public void Reset()
    {
        LoadSettings();
        upgradeProgress = new int[6] { 0, 0, 0, 0, 0, 0 };
        repairProgress = new int[6] { 0, 0, 0, 0, 0, 0 };

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
            consoleUpgrades[component].setUpgradePending(false);
            consoleUpgrades[component].setRepairPending(false);
            consoleUpgrades[component].SetRepairButtonActive(false);
            consoleUpgrades[component].SetUpgradeButtonActive(false);
        }
        for (int i = 0; i < pulsateToggle.Length; i++)
        {
            pulsateToggle[i] = false;
        }
        UpdateAllText();
        popupWindow.SetActive(false);
        missionTexts.Clear();
        objectiveFeed.GetComponent<Text>().text = "";
        stratMap.Reset();
        currentObjectives.Clear();
        EventSystem.current.SetSelectedGameObject(null);    //deselect all the buttons

        // Reset all highlighting
        shipControl.HighlightComponent(-1);
    }

    private void LoadSettings()
    {
        // Copy values manually so original settings won't change
        upgradeProperties = new UpgradeProperties[settings.upgradeProperties.Length];
        for (int i = 0; i < upgradeProperties.Length; i++)
        {
            upgradeProperties[i] = new UpgradeProperties();
            upgradeProperties[i].type = settings.upgradeProperties[i].type;
            upgradeProperties[i].description = settings.upgradeProperties[i].description;
            upgradeProperties[i].cost = settings.upgradeProperties[i].cost;
            upgradeProperties[i].numberOfLevels = settings.upgradeProperties[i].numberOfLevels;
            upgradeProperties[i].repairable = settings.upgradeProperties[i].repairable;
            upgradeProperties[i].currentLevel = settings.upgradeProperties[i].currentLevel;
            upgradeProperties[i].name = settings.upgradeProperties[i].name;
        }
    }
    void Update()
    {
        //pulsate UI elements
        Color lerpedColor = Color.white;
        lerpedColor = Color.Lerp(Color.white, Color.grey, Mathf.PingPong(Time.time, 1));
        float lerpedAlpha = 0.1f;
        lerpedAlpha = Mathf.Lerp(0.1f, 1, Mathf.PingPong(Time.time, 1));
        //commented for now because it's incompatible with button changes in it's current state. don't delete.
        /*for (int i = 0; i < pulsateToggle.Length; i++)
        {
            if (pulsateToggle[i])
            {
                pulsateableImages[i].color = lerpedColor;
            }
            else pulsateableImages[i].color = uiColor;
        }*/
        lerpedColor = transpBlack;
        lerpedColor = Color.Lerp(transpBlack, transpWhite, Mathf.PingPong(Time.time, 1));
        for (int i = 0; i < upgradeProgress.Length; i++)
        {
            if (upgradeProgress[i] == 1)
            {
                consoleUpgrades[i].setPendingUpgradeColor(lerpedColor);
            }
            else consoleUpgrades[i].setPendingUpgradeColor(transpBlack);
            if (repairProgress[i] == 1)
            {
                consoleUpgrades[i].setPendingRepairAlpha(lerpedAlpha);
            }
        }

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
        if(second >= 0.2)
        {
            UpdateAllText();
            UpdateCostColors();
            UpdateHealthShieldBars();
            for (int ability = 0; ability < Enum.GetNames(typeof(AbilityEnum)).Length; ability++)
            {
                abilityCooldownImages[(int)ability].fillAmount = Mathf.Max(0, 1 - abilityCooldowns[(int)ability]/60);
                abilityCooldowns[(int)ability] += (float)second;
                if (abilityCooldowns[(int)ability] > 60) abilityButtons[(int)ability].interactable = true;
            }
            second = 0;
        }
    }

    private void LoadShipModel()
    {
        // Load the ship model into the scene. 
        ship = Instantiate(Resources.Load("Prefabs/CommandShip", typeof(GameObject))) as GameObject;
        ship.transform.position = new Vector3(17f, 0, -1945f);
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
            upgradeBox.transform.localPosition = new Vector3(-513, 180 - (component*80), 0);
            upgradeBox.GetComponent<ConsoleUpgrade>().SetUpgradeInfo(upgradeProperties[component]);
            upgradeBox.transform.GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(delegate{OnClickUpgrade(component);});
            upgradeBox.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Button>().onClick.AddListener(delegate{UpgradeShip();});
            upgradeBox.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Button>().onClick.AddListener(delegate{RepairShip();});
            //upgradeBox.transform.Find("RepairButton").GetComponent<Button>().onClick.AddListener(delegate{OnClickRepair(component);});
            consoleUpgrades.Add(upgradeBox.GetComponent<ConsoleUpgrade>());
            pulsateableImages[component] = upgradeBox.transform.Find("UpgradePicture").GetComponent<Image>();
            consoleUpgrades.Last<ConsoleUpgrade>().backGround = pulsateableImages[component];
        }
    }

    private void AddHealthAndShields()
    {
        RectTransform shieldBarPanelTransform = (RectTransform)shieldBarPanel.transform;
        RectTransform healthBarPanelTransform = (RectTransform)healthBarPanel.transform;
        for (int i = 0; i < 25; i++)
        {
            GameObject shieldSeg = Instantiate(Resources.Load("Prefabs/ShieldSeg", typeof(GameObject))) as GameObject;
            shieldSeg.transform.localScale = new Vector3(1, 1, 1);
            shieldSeg.transform.SetParent(shieldBarPanelTransform, false);
            shieldSeg.transform.localPosition = new Vector3(56 - shieldBarPanelTransform.sizeDelta.x/2 + i * 20, 0, 0);
            shieldSegments.Add(shieldSeg);
        }
        for (int i = 0; i < 25; i++)
        {
            GameObject healthSeg = Instantiate(Resources.Load("Prefabs/HealthSeg", typeof(GameObject))) as GameObject;
            healthSeg.transform.localScale = new Vector3(1, 1, 1);
            healthSeg.transform.SetParent(healthBarPanelTransform, false);
            healthSeg.transform.localPosition = new Vector3(56 - healthBarPanelTransform.sizeDelta.x/2 + i * 20, 0, 0);
            healthSegments.Add(healthSeg);
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
    private bool CheckUpgradeCost(int baseCost,  int level) 
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
    private bool UpgradeComponent(int componentId, int cost)
    {
        if(cost <= gameState.GetShipResources())
        {
            // Update the ships resources
            gameState.UseShipResources(cost);

            // Send request to engineer to upgrade
            playerController.CmdAddUpgrade((ComponentType)componentId);

            // Update resources text with new value.
            UpdateAllText();
            return true;
        }
        return false;
    }

    public void UseAbility(int ability)
    {
        AbilityEnum abilityEnum = (AbilityEnum)ability;
        abilityCooldowns[ability] = 0;
        abilityButtons[ability].interactable = false;
        switch (abilityEnum)
        {
            case AbilityEnum.OverDrive:
                playerController.CmdUseOverdrive();
                break;
            case AbilityEnum.Boost:
                playerController.CmdUseBoost();
                break;
            case AbilityEnum.EMP:
                playerController.CmdUseEMP();
                break;
            case AbilityEnum.SmartBomb:
                playerController.CmdUseSmartBomb();
                break;
        }
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
        consoleUpgrades[(int)type].setUpgradePending(false);
        consoleUpgrades[(int)type].UpdateLevelIndicator(upgradeProperties[(int)type].currentLevel);
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
        consoleUpgrades[(int)type].setRepairPending(false);
        repairProgress[(int)type] = 0;
        UpdateNewsFeed("[Engineer] " + upgradeProperties[(int)type].name + " has been repaired.");
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
        upgradeInfoName.text = upgradeProperties[component].name;

        //costLabel.text = GetUpgradeCost(upgradeProperties[component].cost, upgradeProperties[component].currentLevel).ToString();
        // Upgrade the cost text to display in red if the player does not have enough resources.
        //UpdateCostTextColor();

        bool upgradePending;
        bool repairPending;
        
        if (upgradeProgress[component] == 1) upgradePending = true;
        else upgradePending = false;
        if (repairProgress[component] == 1) repairPending = true;
        else repairPending = false;

        consoleUpgrades[component].setUpgradePending(upgradePending);
        consoleUpgrades[component].setRepairPending(repairPending);

        for (int i = 0; i < consoleUpgrades.Count; i++)
        {
            if(i == component)
            {
                consoleUpgrades[i].SetRepairButtonActive(true);
                consoleUpgrades[i].SetUpgradeButtonActive(true);
            }
            else
            {
                consoleUpgrades[i].SetRepairButtonActive(false);
                consoleUpgrades[i].SetUpgradeButtonActive(false);
            }
        }
    }

    public void OnClickRepair(int component)
    {
        if (repairProgress[componentToUpgrade] == 1 || gameState.GetComponentHealth((ComponentType)component) > 80 || component > 3) //components 4 and 5 can't be repaired
            return;
        playerController.CmdAddRepair((ComponentType)component);
        repairProgress[componentToUpgrade] = 1;
        consoleUpgrades[component].setRepairPending(true);
    }

    //Called whenever an upgrade is purchased
    public void UpgradeShip()
    {
        // If we are already waiting then we don't want to upgrade again.
        if(upgradeProgress[componentToUpgrade] == 1)
            return;

        // Try to upgrade the component
        if(!UpgradeComponent(componentToUpgrade, upgradeProperties[componentToUpgrade].cost))
            return;

        // Update the cost of the component
        consoleUpgrades[componentToUpgrade].UpdateCost(GetUpgradeCost(upgradeProperties[componentToUpgrade].cost, upgradeProperties[componentToUpgrade].currentLevel + 1));

        upgradeProgress[componentToUpgrade] = 1;
        consoleUpgrades[componentToUpgrade].setUpgradePending(true);
    }

    public void RepairShip()
    {
        OnClickRepair(componentToUpgrade);
    }

    /// <summary>
    /// Update all text values on screen.
    /// </summary>
    private void UpdateAllText()
	{
		// Update the text with values from gamestate.
        civiliansText.text = gameState.GetCivilians().ToString();;
        resourcesText.text = gameState.GetShipResources().ToString();;
    }

    private void UpdateCostColors()
    {
        for (int i = 0; i < 6; i++)
        {
            if (consoleUpgrades[i].properties.cost > gameState.GetShipResources())
            {
                consoleUpgrades[i].showAffordable(false);
            }
            else
            {
                consoleUpgrades[i].showAffordable(true);
            }
        }
    }

    private void UpdateHealthShieldBars()
    {
        int health = ((int)Math.Round(gameState.GetShipHealth(), 0));
        int shield = health > 0 ?((int)Math.Round(gameState.GetShipShield(), 0)) : 0;
        int healthBarsToShow = health / 10;
        int shieldBarsToShow = shield / 10;
        if (healthBarsToShow > 25)
        {
            healthBarsToShow = 25;
            print("health value too high. It should be capped at 250");
        }
        if (healthBarsToShow < 0) healthBarsToShow = 0;
        if (shieldBarsToShow > 25)
        {
            shieldBarsToShow = 25;
            print("shield value too high. It should be capped at 250");
        }
        if (shieldBarsToShow < 0) shieldBarsToShow = 0;
        for (int i = 0; i < healthBarsToShow; i++)
        {
            healthSegments[i].SetActive(true);
        }
        for (int i = healthBarsToShow; i < 25; i++)
        {
            healthSegments[i].SetActive(false);
        }
        for (int i = 0; i < shieldBarsToShow; i++)
        {
            shieldSegments[i].SetActive(true);
        }
        for (int i = shieldBarsToShow; i < 25; i++)
        {
            shieldSegments[i].SetActive(false);
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

    public void AddObjectives(string[] objectives)
    {
        currentObjectives.AddRange(objectives.ToList());
        UpdateObjectives(); 
    }

    public void RemoveObjectives(string[] completedObjectives)
    {
        foreach (string completedObjective in completedObjectives)
        {
            currentObjectives.Remove(completedObjective);
        }
        UpdateObjectives();
    }

    public void addMissionPopupToQueue(string title, string descrption)
    {
        MissionText missionText = new MissionText();
        missionText.title = title;
        missionText.description = descrption;
        missionTexts.Add(missionText);
        if (popupWindow.active == false) showMissionPopup();
    }

    public void showMissionPopup()
    {
        MissionText mission;
        if (missionTexts.Count > 0)
        {
            mission = missionTexts[0];
            popupWindow.SetActive(true);
            popupWindow.transform.Find("MissionTitle").GetComponent<Text>().text = mission.title;
            popupWindow.transform.Find("MissionDescription").GetComponent<Text>().text = mission.description;
        }
    }

    public void ShowObjectiveOnMap(int id)
    {
        if (stratMap == null) print("stratmap == null");
        else stratMap.startMission(id);
        pulsateToggle[(int)UIElementEnum.MapBG] = true;
        pulsateToggle[(int)UIElementEnum.NewsFeed] = true;
        List<UIElementEnum> UIElementEnums = new List<UIElementEnum> { UIElementEnum.MapBG, UIElementEnum.NewsFeed };
        StartCoroutine(WaitThenStopPulsate(UIElementEnums,10.0f));
    }

    IEnumerator WaitThenStopPulsate(List<UIElementEnum> elements, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        foreach (UIElementEnum element in elements)
        {
            pulsateToggle[(int)element] = false;
        }
    }

    public void ShowUpgradeObjective(UpgradableComponentIndex componentIndex)   //also called for repair objectives
    {
        //UpgradableComponentIndex elements correspond to UIElementEnum elements
        pulsateToggle[(int)componentIndex] = true;
    }

    public void RemoveUpgradeObjective(UpgradableComponentIndex componentIndex) //also called for repair objectives
    {
        //UpgradableComponentIndex elements correspond to UIElementEnum elements
        pulsateToggle[(int)componentIndex] = false;
    }


    public void RemoveObjectiveFromMap(int id)
    {
        stratMap.endMission(id);
        pulsateToggle[(int)UIElementEnum.MapBG] = false;
    }

    public void ClosePopupWindow()
    {
        popupWindow.SetActive(false);
        missionTexts.RemoveAt(0);
        showMissionPopup();
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
                playerController.CmdDoUpgrade(type);
            }
        }
    }

    private void UpdateObjectives()
    {
        objectiveFeed.GetComponent<Text>().text = "";
        foreach (string objective in currentObjectives)
        {
            objectiveFeed.GetComponent<Text>().text = objective + "\n" + objectiveFeed.GetComponent<Text>().text;
        }
    }

    private void UpdateNewsFeed(string message)
    {
        //I don't want to remove the NewsFeed functionality completely, in case we want it back. So I'm just making this do nothing for now.
        //newsFeed.GetComponent<Text>().text = message + "\n" + newsFeed.GetComponent<Text>().text;
    }

    public class MissionText
    {
        public string title;
        public string description;
        public List<string> objectives;
    }
}
