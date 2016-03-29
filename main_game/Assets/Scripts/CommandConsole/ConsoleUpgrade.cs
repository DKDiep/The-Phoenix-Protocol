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

    // Information about the upgrade
    private UpgradeProperties properties;

    private Transform levelIndicator;

    private bool setupDone = false;
    private GameObject repairButton;

    private List<GameObject> levelIndicators = new List<GameObject>();
    void Start ()
    {
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();

        // Hide the repair button from the start
        repairButton = gameObject.transform.Find("UpgradeRepairButton").gameObject;
        repairButton.SetActive(false);
    }

    void Update()
    {
        if(setupDone)
        {
            // If this component is repairable.
            if(properties.repairable)
            {
                if(gameState.GetComponentHealth(properties.type) < 80) 
                    repairButton.SetActive(true);
                else
                    repairButton.SetActive(false);

            }
        }
    }
        
    public void SetUpgradeInfo(UpgradeProperties properties)
    {
        this.properties = properties;
        InitialiseLevels();
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

    public void UpdateLevelIndicator(int level)
    {
        levelIndicators[level-1].GetComponent<Image>().color = new Vector4(1, 1, 1, 86f/255f);
    }

    public void HideRepairButton()
    {
        repairButton.SetActive(false);
    }

    [System.Serializable]
    public class UpgradeProperties
    {
        public string name;
        public ComponentType type;
        public string description;
        public int cost;
        public int numberOfLevels;
        public bool repairable;
        public int currentLevel;
    }
}


