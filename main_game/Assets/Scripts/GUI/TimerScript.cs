/*
    Counts number of seconds survived
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour 
{
    private Text counterText;
    private int minutes;
    private float seconds;

    private float startTime;
    private float timeDiff;

    // If the timer is enabled (disabled by default at the moment)
    private bool enabled = false;

	// Use this for initialization
	void Start () 
    {
        counterText = GetComponent<Text>();
        ResetTimer();
	}

    public void ResetTimer() 
    {
        startTime = Time.time;
    }

    void Update()
    {
        // Toggle if the T button is pressed.
        if (Input.GetKeyDown(KeyCode.T)) 
        {
            enabled = !enabled;
        }

        // Only display the timer if it is enabled
        if(enabled)
        {
            timeDiff = Time.time - startTime;
            seconds = timeDiff % 60f;
            minutes = (int)Mathf.Floor(timeDiff / 60f);
            counterText.text = minutes.ToString("00") + ":" + seconds.ToString("00.00");
        } 
        else 
        {
            counterText.text = "";
        }
    }
}
