/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Manage network initialisation
*/

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
    bool gameStarted;
    GameObject spawner;

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
        Cursor.visible = true; //leave as true for development, false for production
        //networkManager = gameObject.GetComponent<NetworkManager>();

        if(MainMenu.startServer)
        {
            clientIds = new List<int>();
            // Host is client Id #0
            clientIds.Add(0);
            clientId = 0;
            gameStarted = false;
            // assign clients
            gameState = gameObject.GetComponent<GameState>();
            this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            CreateServerSetup();
        }
    }

    void CreateServerSetup()
    {
        //Spawn server lobby
        GameObject serverSetupCamera = Instantiate(Resources.Load("Prefabs/ServerSetupCamera", typeof(GameObject))) as GameObject;
    }

    public void StartGame()
    {
        //Get player controller
        if (ClientScene.localPlayers[0].IsValid)
            thePlayer = ClientScene.localPlayers[0].gameObject;

        //Spawn networked ship
        GameObject playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
        gameState.SetPlayerShip(playerShip);
        ServerManager.NetworkSpawn(playerShip);

        //Instantiate local cameras for players after spawning ship
        thePlayer.GetComponent<PlayerController>().CallSetCamera();

        //Spawn shield
        GameObject playerShield = Instantiate(Resources.Load("Prefabs/Shield", typeof(GameObject))) as GameObject;
        ServerManager.NetworkSpawn(playerShield);

        //Instantiate ship logic on server only
        GameObject playerShipLogic = Instantiate(Resources.Load("Prefabs/PlayerShipLogic", typeof(GameObject))) as GameObject;
        //playerShipLogic.GetComponent<ShipMovement>().SetControlObject(playerShip);
        playerShipLogic.transform.parent = playerShip.transform;
            
        //Instantiate ship shoot logic on server only
        GameObject playerShootLogic = Instantiate(Resources.Load("Prefabs/PlayerShootLogic", typeof(GameObject))) as GameObject;
        playerShootLogic.transform.parent = playerShip.transform;

        //Instantiate crosshairs
        GameObject crosshairCanvas = Instantiate(Resources.Load("Prefabs/CrosshairCanvas", typeof(GameObject))) as GameObject;

        //GameObject minimap = Instantiate(Resources.Load("Prefabs/MiniMap", typeof(GameObject))) as GameObject;
       // minimap.GetComponentInChildren<bl_MiniMap>().m_Target = playerShip;
        //minimap.GetComponentInChildren<bl_MMCompass>().Target = playerShip.transform;

        //Set up the game state
        thePlayer.GetComponent<PlayerController>().SetControlledObject(playerShip);
        gameState.Setup();

        //Start the game
        playerShip.GetComponentInChildren<ShipMovement>().StartGame();
        gameState.SetStatus(GameState.Status.Started);
        gameStarted = true;
    }

}
