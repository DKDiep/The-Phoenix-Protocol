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
        messageHandler = manager.transform.GetComponent<MessageHandler>();
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

    public void JoinAsEngineer()
    {
        startServer = false;
        NetworkClient client = manager.StartClient();

        //Set the client for the message handler
        messageHandler.SetClient(client);

        // Register a handler for Owner messages. This message
        // tells the client which object it controls
        client.RegisterHandler(890, messageHandler.OnServerOwner);

        // Register handler for connection message. This is received when the
        // server and client establish a connection
        client.RegisterHandler(MsgType.Connect, messageHandler.OnClientConnect);
    }

    public void ShowOptions()
    {

    }
}
