/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene, Dillon Diep
    Description: Basic menu system
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MainMenu : NetworkBehaviour 
{

    public static bool startServer;
    NetworkManager manager;
    MessageHandler messageHandler;

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
