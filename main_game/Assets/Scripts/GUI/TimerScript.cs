/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Counts number of seconds survived
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour 
{
    private Text counterText;
    public float minutes, seconds;

	// Use this for initialization
	void Start () 
    {
        counterText = GetComponent<Text>();
	}

    void Update()
    {
        seconds = Time.timeSinceLevelLoad % 60f;
        minutes = Time.timeSinceLevelLoad / 60f;
        counterText.text = minutes.ToString("00") + ":" + seconds.ToString("00.00");
    }
}
