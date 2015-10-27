using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class GameState : MonoBehaviour {
    public Canvas canvas;
    public int TotalPower;
    public int Health;
    public Text PowerText;
    private int remPower;
    private bool EnginOn = true;
    private bool ShieldsOn = true;
    // Use this for initialization
    void Start () {
        UpdatePower();
    }

    public void Engin(bool isOn)
    {
        EnginOn = isOn;
        UpdatePower();
    }

    public void Shields(bool isOn)
    {
        ShieldsOn = isOn;
        UpdatePower();
    }

    void UpdatePower()
    {
        remPower = TotalPower;
        if (EnginOn) remPower -= 3;
        if (ShieldsOn) remPower -= 2;
        PowerText.text = remPower.ToString();
    }

        // Update is called once per frame
        void Update () {
        
	}
}
