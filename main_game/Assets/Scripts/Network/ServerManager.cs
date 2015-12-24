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
    [SerializeField] Camera menuCam;
    [SerializeField] GameObject menuBG;
    [SerializeField] Texture2D play0;
	[SerializeField] Texture2D play1;
    bool gameStarted;

    public int clientIdCount()
    {
        return clientIds.Count;
    }
    // Used to spawn network objects
    public static void NetworkSpawn(GameObject spawnObject)
    {
        NetworkServer.Spawn(spawnObject);
    }

    void Start()
    {
        clientIds = new List<int>();
        // Host is client Id #0
        clientIds.Add(0);
        clientId = 0;
        gameStarted = false;
        // assign clients
        
        Cursor.visible = true;

        gameState = gameObject.GetComponent<GameState>();
        networkManager = gameObject.GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
    if(!gameStarted)
    {
        if (GUI.Button(new Rect((Screen.width / 2) - 75, (Screen.height / 2) - 50, 150, 100), "Start Game"))
        {
            if (networkManager != null)
            {
                networkManager.StartServer();
                thePlayer = Instantiate(Resources.Load("Prefabs/PlayerController", typeof(GameObject))) as GameObject;
                ServerManager.NetworkSpawn(thePlayer);

                //Spawn networked ship
                GameObject playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
                gameState.SetPlayerShip(playerShip);
                ServerManager.NetworkSpawn(playerShip);

                //Instantiate ship logic on server only
                GameObject playerShipLogic = Instantiate(Resources.Load("Prefabs/PlayerShipLogic", typeof(GameObject))) as GameObject;
                //playerShipLogic.GetComponent<ShipMovement>().SetControlObject(playerShip);
                playerShipLogic.transform.parent = playerShip.transform;
                
				//Instantiate ship shoot logic on server only
				GameObject playerShootLogic = Instantiate(Resources.Load("Prefabs/PlayerShootLogic", typeof(GameObject))) as GameObject;
				playerShootLogic.transform.parent = playerShip.transform;

                //Instantiate crosshairs
                GameObject crosshairCanvas = Instantiate(Resources.Load("Prefabs/CrosshairCanvas", typeof(GameObject))) as GameObject;

                //Set up the game state
                thePlayer.GetComponent<PlayerController>().SetControlledObject(playerShip);
                gameState.Setup();

                //Spawn two networked engineers
                GameObject engineer1 = Instantiate(Resources.Load("Prefabs/Engineer", typeof(GameObject))) as GameObject;
                gameState.AddEngineerList(engineer1);
                ServerManager.NetworkSpawn(engineer1);

                GameObject engineer2 = Instantiate(Resources.Load("Prefabs/Engineer", typeof(GameObject))) as GameObject;
                gameState.AddEngineerList(engineer2);
                ServerManager.NetworkSpawn(engineer2);

                GameObject engineerController1 = Instantiate(Resources.Load("Prefabs/EngineerController", typeof(GameObject))) as GameObject;
                GameObject engineerController2 = Instantiate(Resources.Load("Prefabs/EngineerController", typeof(GameObject))) as GameObject;

                engineerController1.transform.parent = engineer1.transform;
                engineerController2.transform.parent = engineer2.transform;

                //Start the game
                gameState.SetStatus(GameState.Status.Started);
                Destroy (menuCam.gameObject);
                Destroy (menuBG.gameObject);
                gameStarted = true;
                Cursor.visible = false;
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

}
