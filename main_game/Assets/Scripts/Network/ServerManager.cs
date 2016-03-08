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

public class ServerManager : NetworkBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject poolManager;
	#pragma warning restore 0649

    private GameState gameState;
    private NetworkManager networkManager;
    private Dictionary<uint, RoleEnum> netIdToRole;
    private Dictionary<uint, NetworkConnection> netIdToConn;
    private Dictionary<int, GameObject> screenIdToCrosshair;
    private PlayerController playerController;
    private NetworkMessageDelegate originalAddPlayerHandler;
    private GameObject spawner;

	private uint serverId;
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
        // Server and clients need to know screenId matching crosshairs
        screenIdToCrosshair = new Dictionary<int, GameObject>();
        if (MainMenu.startServer)
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

            // Assign clients
            gameState = gameObject.GetComponent<GameState>();

            // Set up the game state
            gameState.Setup();
            this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            CreateServerSetup();
        }
    }
    
    [ClientRpc]
	public void RpcAddCrosshairObject(int screenId, GameObject crosshairObject)
	{
		screenIdToCrosshair.Add(screenId, crosshairObject);

		PlayerController localController = null;
		if (ClientScene.localPlayers[0].IsValid)
			localController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

		if (screenId != localController.GetScreenIndex())
		{
			screenIdToCrosshair[screenId].SetActive(false);
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

    /// <summary>
    /// Notifies all engineers of a new upgrade/repair
    /// job to do
    /// </summary>
    /// <param name="upgrade">Whether the job is an upgrade or a repair</param>
    /// <param name="part">The part to upgrade/repair</param>
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
                NetworkServer.SendToClient(netIdToConn[client.Key].connectionId, MessageID.ENGINEER_JOB, msg);
            }
        }
    }

    /// <summary>
    /// Sends a ComponentStatus message to the specified client
    /// with the specified values
    /// </summary>
    /// <param name="health">The component's health</param>
    /// <param name="level">The component's ugprade level</param>
    /// <param name="netId">The netid of the client we wish to send the message to</param>
    public void SendComponentStatus(float health, int level, uint netId)
    {
        // Create the Component Status message
        ComponentStatusMessage msg = new ComponentStatusMessage();
        msg.health = health;
        msg.level = level;

        // Send the message
        NetworkServer.SendToClient(netIdToConn[netId].connectionId, MessageID.COMPONENT_STATUS, msg);
    }

    private void CreateServerSetup()
    {
        //Spawn server lobby
        Instantiate(Resources.Load("Prefabs/ServerLobby", typeof(GameObject)));
    }

    public void StartGame()
    {

        // Spawn Cutscene Manager
        GameObject cutsceneManager = Instantiate(Resources.Load("Prefabs/CutsceneManager", typeof(GameObject))) as GameObject;
        ServerManager.NetworkSpawn(cutsceneManager);

        // Spawn networked ship
        GameObject playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
        gameState.PlayerShip = playerShip;
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
                gameState.AddToEngineerList(engineer);

                // Spawn the engineer with local authority
                NetworkServer.SpawnWithClientAuthority(engineer, netIdToConn[client.Key]);

                // Let the client know of the object it controls
                ControlledObjectMessage response = new ControlledObjectMessage();
                response.controlledObject = engineer;
                NetworkServer.SendToClient(netIdToConn[client.Key].connectionId, MessageID.OWNER, response);
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
        portal.transform.position = new Vector3(0,1000,3000);
		gameState.Portal = portal;
		ServerManager.NetworkSpawn(portal);
        portal.AddComponent<PortalLogic>();

        //Instantiate ship logic on server only
        GameObject playerShipLogic = Instantiate(Resources.Load("Prefabs/PlayerShipLogic", typeof(GameObject))) as GameObject;
        playerShipLogic.transform.parent = playerShip.transform;
            
        //Instantiate ship shoot logic on server only
        GameObject playerShootLogic = Instantiate(Resources.Load("Prefabs/PlayerShootLogic", typeof(GameObject))) as GameObject;
        playerShootLogic.transform.parent = playerShip.transform;
        playerShootLogic.GetComponent<PlayerShooting>().Setup();

        //Set up the game state
        playerController.SetControlledObject(playerShip);
    }

    public void Play()
    {
        //Reset Player's scores
        gameState.ResetPlayerScores();
        GameObject gameTimer = GameObject.Find("GameTimerText");
        gameTimer.SetActive(true);
        gameTimer.GetComponent<TimerScript>().ResetTimer();
        //Start the game
        gameState.PlayerShip.GetComponentInChildren<ShipMovement>().StartGame();
        gameState.Status = GameState.GameStatus.Started;
    }

	public void SetServerId(uint serverId) 
	{
		this.serverId = serverId;
	}

	public uint GetServerId()
	{
		return serverId;
	}
		
    public void SetCrosshairPosition(int crosshairId, int screenId, Vector2 position)
    {
        GameObject crosshairObject = screenIdToCrosshair[screenId];
        crosshairObject.GetComponent<CrosshairMovement>().SetPosition(crosshairId, position);
    }
}
