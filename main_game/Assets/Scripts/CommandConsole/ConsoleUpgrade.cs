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
    public UpgradeProperties properties;

    private Transform levelIndicator;

    private bool setupDone = false;
    private GameObject repairButton ;
    private Image repairButtonImage;
    private List<Color> YellowToRed = new List<Color>();
    private bool pending = false;
    private int currentLevel = 0;
    private List<GameObject> levelIndicators = new List<GameObject>();
    static private Color offWhite = new Color(176f / 255f, 176f / 255f, 176f / 255f, 1);
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
    }

    public void Reset()
    {
        for (int i = 0; i < properties.numberOfLevels; i++)
        {
            levelIndicators[i].GetComponent<Image>().color = new Color(0, 0, 0, 86f/255f);
        }
        // Hide repair button
        repairButton.SetActive(false);
        // Reset stats
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
                }
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

    public void showAffordable(bool affordable)
    {
        if (affordable)
            upgradeCostTxt.color = offWhite;
        else upgradeCostTxt.color = Color.red;
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
        if(currentLevel < levelIndicators.Count)levelIndicators[currentLevel].GetComponent<Image>().color = color;
    }

    public void UpdateLevelIndicator(int level)
    {
        levelIndicators[level-1].GetComponent<Image>().color = new Vector4(1, 1, 1, 86f/255f);
        currentLevel++;
    }

    public void HideRepairButton()
    {
        repairButton.SetActive(false);
    }

}


