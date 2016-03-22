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
        timeDiff = Time.time - startTime;
        seconds = timeDiff % 60f;
        minutes = (int)Mathf.Floor(timeDiff / 60f);
        counterText.text = minutes.ToString("00") + ":" + seconds.ToString("00.00");
    }
}
