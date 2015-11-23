using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ServerManager : NetworkBehaviour {

    private GameState gameState;
    private NetworkManager networkManager;
    private int clientId = 0;
    private List<int> clientIds;
    private GameObject thePlayer;

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
                thePlayer = Instantiate(Resources.Load("Prefabs/PlayerController", typeof(GameObject))) as GameObject;
                NetworkServer.Spawn(thePlayer);

                //Spawn networked ship
                GameObject playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
                gameState.SetPlayerShip(playerShip);
                NetworkServer.Spawn(playerShip);
                //Instantiate ship logic on server only
                GameObject playerShipLogic = Instantiate(Resources.Load("Prefabs/PlayerShipLogic", typeof(GameObject))) as GameObject;
                playerShipLogic.GetComponent<ShipMovement>().SetControlObject(playerShip);

                gameState.SetPlayerShip(playerShip);
                thePlayer.GetComponent<PlayerController>().SetControlledObject(playerShip);
                gameState.Setup();
                gameState.SetStatus(GameState.Status.Started);
            }
        }

        if (GUI.Button(new Rect(10, 550, 150, 20), "RpcSend"))
        {
            if (thePlayer != null)
            {
                Debug.Log("Calling Rpc from servermanager");
                thePlayer.GetComponent<RpcManager>().CallRpcSend("boo");
            }
        }
    }

}
