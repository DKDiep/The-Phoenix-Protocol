using UnityEngine;
using System.Collections;

public class GameStatsManager : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        if (MainMenu.startServer)
        {
            StartCoroutine(SendRequest());
        }
    }

    IEnumerator SendRequest()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            string url = "http://localhost:8080/bar";
            WWWForm form = new WWWForm();
            form.AddField("Future metacritic rating", "10");
            WWW www = new WWW(url, form);
            yield return www;
            if (www.error == null)
            {
                //Debug.Log("WWW Ok!: " + www.data);
            }
            else
            {
                Debug.Log("WWW Error: " + www.error);
            }
            new WaitForSeconds(5.0f);
        }
    }


    // Update is called once per frame
    void Update () {
	
	}
}
