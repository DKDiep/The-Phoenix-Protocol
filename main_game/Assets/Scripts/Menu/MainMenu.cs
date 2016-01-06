using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MainMenu : NetworkBehaviour 
{

    public static bool startServer;
    NetworkManager manager;
    NetworkClient client = null;

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

    public void JoinAsEngineer()
    {
        startServer = false;
        client = manager.StartClient();

        // Register a handler for Owner messages. This message
        // tells the client which object it controls
        client.RegisterHandler(890, OnServerOwner);

        // Register handler for connection message. This is received when the
        // server and client establish a connection
        client.RegisterHandler(MsgType.Connect, OnClientConnect);
    }

    private void OnClientConnect(NetworkMessage netMsg)
    {
        // Tell the server we want to be an engineer
        PickRoleMessage message = new PickRoleMessage();
        message.role = (int)RoleEnum.ENGINEER;
        client.Send(789, message);
    }

    private void OnServerOwner(NetworkMessage netMsg)
    {
        if (ClientScene.localPlayers[0].IsValid)
            ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>().SetRole("engineer");

        GameObject obj = netMsg.reader.ReadGameObject();
    }

    public void ShowOptions()
    {

    }
}
