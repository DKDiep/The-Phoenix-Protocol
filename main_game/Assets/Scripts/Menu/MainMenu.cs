using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MainMenu : NetworkBehaviour 
{

    public static bool startServer;
    NetworkManager manager;

    void Start()
    {
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect((Screen.width / 2) - 75, (Screen.height / 2) - 75, 150, 50), "Start Server"))
        {
            startServer = true;
            manager.StartServer();
        }
        else if (GUI.Button(new Rect((Screen.width / 2) - 75, (Screen.height / 2) - 0, 150, 50), "Start Client"))
        {
            startServer = false;
            manager.StartClient();
        }
    }

}
