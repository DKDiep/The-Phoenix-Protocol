using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EngineerMenu : MonoBehaviour {

    [SerializeField] GameObject menuCam;
    [SerializeField] GameObject debug;

    GameObject ship;
    GameState gameState;
    NetworkManager manager;

	// Use this for initialization
	void Start ()
    {
        gameState = gameObject.GetComponent<GameState>();
        manager = gameObject.GetComponent<NetworkManager>();
    }
	
	// Sketchy method to detect when client game has started. Needs replacing.
	void Update () 
	{
		ship = GameObject.Find ("PlayerShip(Clone)");
	}

    void OnGUI()
	{
        if (GUI.Button(new Rect((Screen.width / 2) - 75, (Screen.height / 2) - 50, 150, 100), "Start Game") && ship != null)
        {
            Destroy(menuCam);
            debug.SetActive(true);
            Destroy(this);
        }
	}
}
