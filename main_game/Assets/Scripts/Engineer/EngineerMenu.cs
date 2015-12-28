using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EngineerMenu : MonoBehaviour {

    [SerializeField] GameObject menuCam;
    [SerializeField] GameObject debug;

    NetworkClient client;
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
    void Update()
    {
        if (ship != null)
        {
            Destroy(menuCam);
            debug.SetActive(true);
            Destroy(this);
        }
        else
        {
            ship = GameObject.Find("PlayerShip(Clone)");
        }
    }

    void OnGUI()
	{
        if (client == null)
        {
            if (GUI.Button(new Rect((Screen.width / 2) - 75, (Screen.height / 2) - 50, 150, 100), "Start Game"))
            {
                client = manager.StartClient();

                //if (client != null)
                //{
                //    Debug.Log("Waiting for game to start");
                //    System.Threading.Thread.Sleep(5000);
                //    ship = GameObject.Find("PlayerShip(Clone)");
                //}
                //
                //Debug.Log("Game started");
                //if (ship != null)
                //{
                //    Destroy(menuCam);
                //    debug.SetActive(true);
                //    Destroy(this);
                //
                //    //Disable all cameras so that the engineer camera will be the main camera
                //    foreach (Camera cam in Camera.allCameras)
                //    {
                //        cam.enabled = false;
                //    }
                //
                //    //Disable all audiolisteners associated with the ship
                //    foreach (AudioListener l in ship.GetComponentsInChildren<AudioListener>())
                //    {
                //        l.enabled = false;
                //    }
                //
                //    joinHandler.CmdEngineerJoin();
                //}
            }
        }
	}
}
