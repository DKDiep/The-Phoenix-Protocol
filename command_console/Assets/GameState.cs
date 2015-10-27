using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class GameState : MonoBehaviour {
    public Canvas canvas;
    public int TotalPower;
    public int Health;
    public Text PowerText;
    public Text EnginLabel;
    public Text ShieldsLabel;
    private int remPower;
    private bool upgrade = false;
    private bool enginOn = true;
    private bool shieldsOn = true;
    // Use this for initialization
    void Start () {
        UpdatePower();
        EnginLabel.text = "Engin";
    }

    public void Upgrade(bool isOn)
    {
        upgrade = isOn;
    }

    public void Engin(bool isOn)
    {
        if (upgrade)
        {
            EnginLabel.text = EnginLabel.text + "I";
            upgrade = false;
        }
        enginOn = isOn;
        UpdatePower();
    }

    public void Shields(bool isOn)
    {
        shieldsOn = isOn;
        UpdatePower();
    }

    void UpdatePower()
    {
        remPower = TotalPower;
        if (enginOn) remPower -= 3;
        if (shieldsOn) remPower -= 2;
        PowerText.text = remPower.ToString();
    }

        // Update is called once per frame
        void Update () {
        
	}
}
