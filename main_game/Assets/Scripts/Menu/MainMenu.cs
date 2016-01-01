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

    public void CreateGame()
    {
        startServer = true;
        manager.StartHost();
    }

    public void JoinGame()
    {
        startServer = false;
        manager.StartClient();
    }

    public void ShowOptions()
    {

    }
}
