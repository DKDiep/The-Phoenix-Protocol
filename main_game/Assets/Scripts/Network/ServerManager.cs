using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ServerManager : NetworkBehaviour {

    private GameState gameState;
    private NetworkManager networkManager;
    private int clientId = 0;
    private List<int> clientIds;

    [ClientRpc]
    public void RpcSpawn(string type)
    {
        Debug.Log("Spawn:" + type);
    }

    public int clientIdCount()
    {
        return clientIds.Count;
    }

    void Start()
    {
        clientIds = new List<int>();
        // Host is client Id #0
        clientIds.Add(0);
        clientId = 0;
        //assign clients

        gameState = gameObject.GetComponent<GameState>();
        networkManager = gameObject.GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 400, 150, 100), "Start Game"))
        {
            if (networkManager != null)
            {
                networkManager.StartServer();
                //networkManager.OnStartServer();
                GameObject thePlayer = Instantiate(Resources.Load("Prefabs/PlayerController", typeof(GameObject))) as GameObject;
                NetworkServer.Spawn(thePlayer);

                gameState.Setup();

                GameObject playerShip = gameState.getPlayerShip();
                NetworkServer.Spawn(playerShip);
                gameState.setStatus(GameState.Status.Started);
            }
        }
    }

}
