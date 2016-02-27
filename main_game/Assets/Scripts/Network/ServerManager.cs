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
using UnityEngine.Networking.NetworkSystem;

public class ServerManager : NetworkBehaviour {
    private GameState gameState;
    private NetworkManager networkManager;
    private int clientId = 0;
    private Dictionary<uint, RoleEnum> netIdToRole;
    private Dictionary<uint, NetworkConnection> netIdToConn;
    private PlayerController playerController;
    private NetworkMessageDelegate originalAddPlayerHandler;
    bool gameStarted;
    GameObject spawner;
    [SerializeField] GameObject poolManager;

    public int clientIdCount()
    {
        return netIdToRole.Count;
    }
    // Used to spawn network objects
    public static void NetworkSpawn(GameObject spawnObject)
    {
        NetworkServer.Spawn(spawnObject);
    }

    void Awake()
    {
        foreach(Transform child in poolManager.transform)
        {
            child.gameObject.GetComponent<ObjectPoolManager>().SpawnObjects();
        }
    }

    void Start()
    {
        Cursor.visible = true; //leave as true for development, false for production
        //networkManager = gameObject.GetComponent<NetworkManager>();

        if(MainMenu.startServer)
        {
            // Spawn Object Pooling Controller first
            // Save the original add player handler to save ourselves some work
            // and register a new one
            originalAddPlayerHandler = NetworkServer.handlers[MsgType.AddPlayer];
            NetworkServer.RegisterHandler(MsgType.AddPlayer, OnClientAddPlayer);

			netIdToRole = new Dictionary<uint, RoleEnum>();
            netIdToConn = new Dictionary<uint, NetworkConnection>();
            // Host is client Id #0. Using -1 as the role for the Host
			netIdToRole.Add(0, RoleEnum.Host);
            clientId = 0;
            gameStarted = false;
            // assign clients
            gameState = gameObject.GetComponent<GameState>();
            // Set up the game state
            gameState.Setup();
            this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            CreateServerSetup();
        }
    }

    // Called automatically by Unity when a player joins
    private void OnClientAddPlayer(NetworkMessage netMsg)
    {
        originalAddPlayerHandler(netMsg);

        uint netId = netMsg.conn.playerControllers[0].gameObject.GetComponent<NetworkIdentity>().netId.Value;
        netIdToConn.Add(netId, netMsg.conn);
        Debug.Log(netId);
    }

    // Takes the list of player controller IDs that will be engineers
    // and updates the map of <ControllerID, Role>
    public void SetEngineers(uint[] playerControllerIds)
    {
        foreach (uint id in playerControllerIds)
        {
            if (id != 0)
				netIdToRole.Add(id, RoleEnum.Engineer);
            else
                Debug.LogError("The host cannot be an engineer!");
        }
    }

    // Notifies an engineer of a new upgrade or repair job
	public void NotifyEngineer(bool upgrade, ComponentType part)
    {
        // Notify every engineer of the upgrade/repair
        foreach (KeyValuePair<uint, RoleEnum> client in netIdToRole)
		{
            if (client.Value == RoleEnum.Engineer)
            {
                // Create the message to send
                EngineerJobMessage msg = new EngineerJobMessage();
                msg.upgrade = upgrade;
                msg.part = part;

                // Send to the engineer
                NetworkServer.SendToClient(netIdToConn[client.Key].connectionId, 123, msg);
            }
        }
    }

    private void CreateServerSetup()
    {
        //Spawn server lobby
        GameObject serverLobby = Instantiate(Resources.Load("Prefabs/ServerLobby", typeof(GameObject))) as GameObject;
    }

    public void StartGame()
    {

        // Spawn Cutscene Manager
        GameObject cutsceneManager = Instantiate(Resources.Load("Prefabs/CutsceneManager", typeof(GameObject))) as GameObject;
        ServerManager.NetworkSpawn(cutsceneManager);

        // Spawn networked ship
        GameObject playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
        gameState.SetPlayerShip(playerShip);
        ServerManager.NetworkSpawn(playerShip);

        // Get the engineer start position
        NetworkStartPosition engineerStartPos = playerShip.GetComponentInChildren<NetworkStartPosition>();

        // Spawn the engineers at the engineer start position
        foreach (KeyValuePair<uint, RoleEnum> client in netIdToRole)
        {
            if (client.Value == RoleEnum.Engineer)
            {
                // Create the engineer object
                GameObject engineer = Instantiate(Resources.Load("Prefabs/Engineer", typeof(GameObject)),
                    engineerStartPos.transform.position, engineerStartPos.transform.rotation) as GameObject;
                gameState.AddEngineerList(engineer);

                // Spawn the engineer with local authority
                NetworkServer.SpawnWithClientAuthority(engineer, netIdToConn[client.Key]);

                // Let the client know of the object it controls
                ControlledObjectMessage response = new ControlledObjectMessage();
                response.controlledObject = engineer;
                NetworkServer.SendToClient(netIdToConn[client.Key].connectionId, 890, response);
            }
        }

        //On start, RPC every player to mount camera on ship
        if (ClientScene.localPlayers[0].IsValid)
            playerController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        if (playerController != null)
            print("calling rpcroleinit");
            playerController.RpcRoleInit();

        // Spawn music controller only on server
        Instantiate(Resources.Load("Prefabs/MusicManager", typeof(GameObject)));
        
        //Spawn shield
        GameObject playerShield = Instantiate(Resources.Load("Prefabs/Shield", typeof(GameObject))) as GameObject;
        ServerManager.NetworkSpawn(playerShield);

		// Spawn portal
		GameObject portal = Instantiate(Resources.Load("Prefabs/Portal", typeof(GameObject))) as GameObject;
        portal.transform.position = new Vector3(0,0,3000);
		gameState.SetPortal(portal);
		ServerManager.NetworkSpawn(portal);
        portal.AddComponent<PortalLogic>();

        //Instantiate ship logic on server only
        GameObject playerShipLogic = Instantiate(Resources.Load("Prefabs/PlayerShipLogic", typeof(GameObject))) as GameObject;
        //playerShipLogic.GetComponent<ShipMovement>().SetControlObject(playerShip);
        playerShipLogic.transform.parent = playerShip.transform;
            
        //Instantiate ship shoot logic on server only
        GameObject playerShootLogic = Instantiate(Resources.Load("Prefabs/PlayerShootLogic", typeof(GameObject))) as GameObject;
        playerShootLogic.transform.parent = playerShip.transform;

        //Instantiate crosshairs
        GameObject crosshairCanvas = Instantiate(Resources.Load("Prefabs/CrosshairCanvas", typeof(GameObject))) as GameObject;

        GameObject minimap = Instantiate(Resources.Load("Prefabs/MiniMap", typeof(GameObject))) as GameObject;
        minimap.GetComponentInChildren<bl_MiniMap>().m_Target = playerShip;
        minimap.GetComponentInChildren<bl_MMCompass>().target = playerShip.transform;

        //Set up the game state
        playerController.SetControlledObject(playerShip);

		//Reset Player's scores
		gameState.ResetPlayerScores();

        //Start the game
        playerShip.GetComponentInChildren<ShipMovement>().StartGame();
        gameState.SetStatus(GameState.Status.Started);
        gameStarted = true;
    }
}
