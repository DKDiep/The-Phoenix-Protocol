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
        messageHandler = manager.GetComponent<MessageHandler>();
    }

    public void CreateGame()
    {
        startServer = true;
        SetHandler(manager.StartHost());
    }

    public void JoinGame()
    {
        startServer = false;
        SetHandler(manager.StartClient());
    }

    private void SetHandler(NetworkClient client)
    {
        messageHandler.SetClient(client);

        // Register handler for the Owner message from the server to the client
        client.RegisterHandler(890, messageHandler.OnServerOwner);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
