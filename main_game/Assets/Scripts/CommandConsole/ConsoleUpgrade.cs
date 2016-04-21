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

    private GameObject sideUpgradeButton;
    private Text sideUpgradeButtonText;
    private Image sideUpgradeButtonImage;
    private GameObject sideRepairButton;
    private Text sideRepairButtonText;
    private Image sideRepairButtonImage;

    // Information about the upgrade
    public UpgradeProperties properties;

    private Transform levelIndicator;

    private bool setupDone = false;
    private GameObject repairButton ;

    private Image repairButtonImage;
    private List<Color> YellowToRed = new List<Color>();
    private bool upgradePending = false;
    private bool levelsInitialised = false;
    private bool maxLevel = false;
    private bool damaged = false;
    private bool fullHealth = true;
    private bool affordable = false;
    private List<GameObject> levelIndicators = new List<GameObject>();
    static private Color offWhite = new Color(176f / 255f, 176f / 255f, 176f / 255f, 1);
    static private Color whiteA200 = new Color(1, 1, 1, 200f / 255f);
    static private Color whiteA50 = new Color(1, 1, 1, 50f / 255f);

    void Start ()
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
        for(int i = 0; i < 10; i++)
        {
            YellowToRed.Add(new Color(1, (float)(((i * 25.6))/256), 0, 128));
        }

        // Hide the repair button from the start
        repairButton = gameObject.transform.Find("RepairButton").gameObject;
        repairButtonImage = repairButton.GetComponent<Image>();
        repairButton.SetActive(false);

        sideUpgradeButton = gameObject.transform.Find("SideUpgradeArea").GetChild(0).gameObject;
        sideUpgradeButtonText = sideUpgradeButton.GetComponentInChildren<Text>();
        sideUpgradeButtonImage = sideUpgradeButton.GetComponent<Image>();
        sideUpgradeButton.SetActive(false);
        sideRepairButton = gameObject.transform.Find("SideUpgradeArea").GetChild(1).gameObject;
        sideRepairButtonText = sideRepairButton.GetComponentInChildren<Text>();
        sideRepairButtonImage = sideRepairButton.GetComponent<Image>();
        sideRepairButton.SetActive(false);
    }

    public void Reset()
    {
        for (int i = 0; i < properties.numberOfLevels; i++)
        {
            levelIndicators[i].GetComponent<Image>().color = new Color(0, 0, 0, 86f/255f);
        }
        // Hide repair button
        repairButton.SetActive(false);
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
                if (componentHealth < 80)
                {
                    index = Mathf.RoundToInt(componentHealth / 10);
                    if (index < 0) index = 0;
                    if (index > 9) index = 9;
                    repairButtonImage.color = YellowToRed[index];
                    repairButton.SetActive(true);
                    fullHealth = false;
                }
                else
                {
                    repairButton.SetActive(false);
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

    public void showAffordable(bool affordable)
    {
        if (affordable)
        {
            upgradeCostTxt.color = offWhite;
            if (!upgradePending)
            {
                sideUpgradeButtonImage.color = whiteA200;
                sideUpgradeButtonText.color = whiteA200;
            }
            else
            {
                sideUpgradeButtonImage.color = whiteA50;
                sideUpgradeButtonText.color = whiteA50;
            }
            this.affordable = affordable;
        }
        else
        {
            upgradeCostTxt.color = Color.red;
            sideUpgradeButtonImage.color = whiteA50;
            sideUpgradeButtonText.color = whiteA50;
            this.affordable = affordable;
        }
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

    public void setPendingColor(Color color) //Used by command console to set pulsating color
    {
        if(properties.currentLevel < levelIndicators.Count)levelIndicators[properties.currentLevel-1].GetComponent<Image>().color = color;
    }

    public void UpdateLevelIndicator(int level)
    {
        levelIndicators[level-1].GetComponent<Image>().color = new Vector4(1, 1, 1, 86f/255f);
        if (properties.currentLevel == properties.numberOfLevels)
        {
            maxLevel = true;
            setUpgradePending(true);
        }
    }

    public void HideRepairButton()
    {
        repairButton.SetActive(false);
    }

    // For the side repair button, need to rename the other confusing gameObject name
    public void SetRepairButtonActive(bool active)
    {
        sideRepairButton.SetActive(active);
    }
    public void SetUpgradeButtonActive(bool active)
    {
        sideUpgradeButton.SetActive(active);
    }

    public void setRepairPending(bool pending)
    {
        if (fullHealth)
        {
            sideRepairButtonText.text = "Undamaged";
            sideRepairButtonImage.color = whiteA50;
            sideRepairButtonText.color = whiteA50;
            return;
        }
        if (pending)
        {
            sideRepairButtonText.text = "Waiting";
            sideRepairButtonImage.color = whiteA50;
            sideRepairButtonText.color = whiteA50;
        }
        else
        {
            sideRepairButtonText.text = "Repair";
            sideRepairButtonImage.color = whiteA200;
            sideRepairButtonText.color = whiteA200;
        }
    }

    public void setUpgradePending(bool pending)
    {
        upgradePending = pending;
        if (maxLevel)
        {
            sideUpgradeButtonText.text = "Max Level";
            sideUpgradeButtonImage.color = whiteA50;
            sideUpgradeButtonText.color = whiteA50;
            return;
        }
        if (pending)
        {
            sideUpgradeButtonText.text = "Waiting";
            sideUpgradeButtonImage.color = whiteA50;
            sideUpgradeButtonText.color = whiteA50;
        }
        else
        {
            sideUpgradeButtonText.text = "Upgrade";
            showAffordable(affordable);
        }
    }
}


