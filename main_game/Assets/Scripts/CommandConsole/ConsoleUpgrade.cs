using UnityEngine;
using UnityEngine.UI;



public class ConsoleUpgrade : MonoBehaviour
{
    private Text upgradeNameTxt;
    private Text upgradeCostTxt;

    private string upgradeName;
    private int upgradeCost;
    private int maxLevels;

    void Start ()
    {
    }

    void Update()
    {

    }

    public void SetUpgradeInfo(string name, int cost, int levels)
    {
        upgradeName = name;
        upgradeCost = cost;
        maxLevels = levels;

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
        // Use maxLevels to change number of level cells.
    }

}


