using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ConsoleUpgrade : MonoBehaviour
{
    // The text fields in the upgrade box
    private Text upgradeNameTxt;
    private Text upgradeCostTxt;

    // Information about the upgrade
    private ComponentType type;
    private string upgradeName;
    private int upgradeCost;
    private int maxLevels;
    private bool repairable;

    private Transform levelIndicator;

    private List<GameObject> levelIndicators = new List<GameObject>();
    void Start ()
    {
        // Hide the repair button from the start
        gameObject.transform.Find("UpgradeRepairButton").gameObject.SetActive(false);
    }

    void Update()
    {

    }

    public void SetUpgradeInfo(ComponentType type, string name, int cost, int levels, bool repariable)
    {
        this.type       = type;
        upgradeName     = name;
        upgradeCost     = cost;
        maxLevels       = levels;
        this.repairable = repairable;

        InitialiseLevels();
        UpdateTextFields();
       
    }

    public void UpdateCost(int cost)
    {
        upgradeCost = cost;
        upgradeCostTxt.text = upgradeCost.ToString();
    }

    private void UpdateTextFields()
    {
        upgradeNameTxt = gameObject.transform.Find("UpgradeLabel").GetComponent<Text>();
        upgradeNameTxt.text = upgradeName;

        upgradeCostTxt = gameObject.transform.Find("UpgradeCostText").GetComponent<Text>();
        upgradeCostTxt.text = upgradeCost.ToString();
    }

    private void InitialiseLevels()
    {
        GameObject level;
        levelIndicator = gameObject.transform.Find("LevelIndicatorWrapper");

        for(int i = 0; i < maxLevels; i++)
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
}


