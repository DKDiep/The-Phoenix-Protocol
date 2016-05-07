using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ConsoleUpgrade : MonoBehaviour
{
    private GameState gameState;
    // The text fields in the upgrade box
    private Text upgradeNameTxt;
    private Text upgradeCostTxt;

    private GameObject sideUpgradeButtonObject;
    private Button sideUpgradeButton;
    private Text sideUpgradeButtonText;
    private Image sideUpgradeButtonImage;
    private GameObject sideRepairButtonObject;
    private Button sideRepairButton;
    private Text sideRepairButtonText;
    private Image sideRepairButtonImage;
    public Image backGround;

    // Information about the upgrade
    public UpgradeProperties properties;

    private Transform levelIndicator;

    private bool setupDone = false;
    private GameObject repairIcon ;

    private Image repairIconImage;
    private List<Color> yellowToReds = new List<Color>();
    private bool upgradePending = false;
    private bool levelsInitialised = false;
    private bool maxLevel = false;
    private bool damaged = false;
    private bool fullHealth = true;
    private bool affordable = false;
    public float maxHealth = 100;
    private List<GameObject> levelIndicators = new List<GameObject>();
    static private Color offWhite = new Color(176f / 255f, 176f / 255f, 176f / 255f, 1);
    static private Color whiteA200 = new Color(1, 1, 1, 200f / 255f);
    static private Color whiteA50 = new Color(1, 1, 1, 50f / 255f);

    void Start ()
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        for(int i = 0; i < 10; i++)
        {
            yellowToReds.Add(new Color(1, (float)(((i * 25.6))/256), 0, 128));
        }

        // Hide the repair button from the start
        repairIcon = gameObject.transform.Find("RepairIcon").gameObject;
        repairIconImage = repairIcon.GetComponent<Image>();
        repairIcon.SetActive(false);
        sideUpgradeButtonObject = gameObject.transform.Find("SideUpgradeArea").GetChild(0).gameObject;
        sideUpgradeButtonText = sideUpgradeButtonObject.GetComponentInChildren<Text>();
        sideUpgradeButtonImage = sideUpgradeButtonObject.GetComponent<Image>();
        sideUpgradeButton = sideUpgradeButtonObject.GetComponent<Button>();
        sideUpgradeButtonObject.SetActive(false);
        sideRepairButtonObject = gameObject.transform.Find("SideUpgradeArea").GetChild(1).gameObject;
        sideRepairButton = sideRepairButtonObject.GetComponent<Button>();
        sideRepairButtonText = sideRepairButtonObject.GetComponentInChildren<Text>();
        sideRepairButtonImage = sideRepairButtonObject.GetComponent<Image>();
        sideRepairButtonObject.SetActive(false);
    }

    public void Reset()
    {
        for (int i = 0; i < properties.numberOfLevels; i++)
        {
            levelIndicators[i].GetComponent<Image>().color = new Color(0, 0, 0, 86f/255f);
        }
        // Hide repair button
        repairIcon.SetActive(false);
        setRepairPending(false);
        setUpgradePending(false);
    }

    void Update()
    {
        int index;
        if(setupDone)
        {
            // If this component is repairable.
            if(properties.repairable)
            {
                float componentHealth = gameState.GetComponentHealth(properties.type);
                if (properties.type == ComponentType.Hull) maxHealth = 50 + 50 * properties.currentLevel;
                else maxHealth = 100;
                if (componentHealth < maxHealth * 0.8)
                {
                    index = Mathf.RoundToInt((componentHealth/maxHealth) * 10);
                    if (index < 0) index = 0;
                    if (index > 9) index = 9;
                    Color yellowToRed = yellowToReds[index];
                    yellowToRed.a = repairIconImage.color.a;
                    repairIconImage.color = yellowToRed;
                    repairIcon.SetActive(true);
                    fullHealth = false;
                }
                else
                {
                    repairIcon.SetActive(false);
                    fullHealth = true;
                }
            }
        }
    }
        
    public void SetUpgradeInfo(UpgradeProperties properties)
    {
        this.properties = properties;
        if (!levelsInitialised)
        {
            InitialiseLevels();
            levelsInitialised = true;
        }
        UpdateTextFields();
        setupDone = true;
    }

    public void UpdateCost(int cost)
    {
        properties.cost = cost;
        upgradeCostTxt.text = properties.cost.ToString();
    }

    private void UpdateTextFields()
    {
        upgradeNameTxt = gameObject.transform.Find("UpgradeLabel").GetComponent<Text>();
        upgradeNameTxt.text = properties.name;

        upgradeCostTxt = gameObject.transform.Find("UpgradeCostText").GetComponent<Text>();
        upgradeCostTxt.text = properties.cost.ToString();
    }

    private void InitialiseLevels()
    {
        GameObject level;
        levelIndicator = gameObject.transform.Find("LevelIndicatorWrapper");

        for(int i = 0; i < properties.numberOfLevels; i++)
        {
            level = Instantiate(Resources.Load("Prefabs/levelIndicator", typeof(GameObject))) as GameObject;
            level.transform.SetParent(levelIndicator);
            level.transform.localPosition = new Vector3(15*i, 0, 0);
            level.transform.localScale = new Vector3(1,1,1);
            level.GetComponent<Image>().color = new Color(0, 0, 0, 86f/255f);
            levelIndicators.Add(level);
        }
    }

    public void setPendingUpgradeColor(Color color) //Used by command console to set pulsating color
    {
        if(properties.currentLevel < levelIndicators.Count)levelIndicators[properties.currentLevel-1].GetComponent<Image>().color = color;
    }

    public void setPendingRepairAlpha(float alpha)
    {
        Color yellowToRed = repairIconImage.color;
        yellowToRed.a = alpha;
        repairIconImage.color = yellowToRed;
    }

    public void UpdateLevelIndicator(int level)
    {
        levelIndicators[level-1].GetComponent<Image>().color = new Vector4(1, 1, 1, 86f/255f);
        if (properties.currentLevel == properties.numberOfLevels)
        {
            maxLevel = true;
            setUpgradePending(false);
        }
    }

    public void HideRepairButton()
    {
        repairIcon.SetActive(false);
    }

    // For the side repair button, need to rename the other confusing gameObject name
    public void SetRepairButtonActive(bool active)
    {
        sideRepairButtonObject.SetActive(active);
    }
    public void SetUpgradeButtonActive(bool active)
    {
        sideUpgradeButtonObject.SetActive(active);
    }

    public void setRepairPending(bool pending)
    {
        if (fullHealth)
        {
            //sideRepairButtonText.text = "Undamaged";
            sideRepairButtonImage.color = whiteA50;
            sideRepairButtonText.color = whiteA50;
            sideRepairButton.interactable = false;
            return;
        }
        if (pending)
        {
            //sideRepairButtonText.text = "Waiting";
            sideRepairButtonImage.color = whiteA50;
            sideRepairButtonText.color = whiteA50;
            sideRepairButton.interactable = false;
        }
        else
        {
            //sideRepairButtonText.text = "Repair";
            sideRepairButtonImage.color = whiteA200;
            sideRepairButtonText.color = whiteA200;
            sideRepairButton.interactable = true;
        }
    }

    public void setUpgradePending(bool pending)
    {
        upgradePending = pending;
        if (maxLevel)
        {
            //sideUpgradeButtonText.text = "Max Level";
            setUpgradeButtonInteractable(false);
            return;
        }
        if (pending)
        {
            //sideUpgradeButtonText.text = "Waiting";
            setUpgradeButtonInteractable(false);
        }
        else
        {
            //sideUpgradeButtonText.text = "Upgrade";
            showAffordable(affordable);
        }
    }

    public void showAffordable(bool affordable)
    {
        if (affordable)
        {
            upgradeCostTxt.color = offWhite;
            if (!upgradePending)
                setUpgradeButtonInteractable(true);
            else
                setUpgradeButtonInteractable(false);
        }
        else
        {
            upgradeCostTxt.color = Color.red;
            setUpgradeButtonInteractable(false);
        }
        this.affordable = affordable;
    }

    private void setUpgradeButtonInteractable(bool isInteractable)
    {
        Color backGroundColor = backGround.color;
        if (isInteractable)
        {
            sideUpgradeButtonImage.color = whiteA200;
            sideUpgradeButtonText.color = whiteA200;
            sideUpgradeButton.interactable = true;
            backGroundColor.a = 1;
            backGround.color = backGroundColor;
        }
        else
        {
            sideUpgradeButtonImage.color = whiteA50;
            sideUpgradeButtonText.color = whiteA50;
            sideUpgradeButton.interactable = false;
            backGroundColor.a = 0.4f;
            backGround.color = backGroundColor;
        }
    }

}


