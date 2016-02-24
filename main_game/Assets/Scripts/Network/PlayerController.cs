/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene, Luke Bryant
    Description: Networked player entity, input management and RPCs
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{

    private GameObject controlledObject;
    private string role = "camera";
    private int orientation = 0;
    private GameObject playerCamera;
    private GameObject multiCamera;
    private EngineerController engController;
    private PlayerController localController = null;
    private int index = 0;

    public GameObject commandConsoleGameObject;
    // Private to each instance of script
    private GameObject ship;
    private CommandConsoleState commandConsoleState;
    private GameObject gameManager;
    private GameState gameState;
    private ServerManager serverManager;
    private bool gameStarted = false;
    ShipMovement shipMovement;

    public GameObject GetControlledObject()
    {
        return controlledObject;
    }

    public void SetControlledObject(GameObject newControlledObject)
    {
        controlledObject = newControlledObject;
    }

    [ClientRpc]
    public void RpcRoleInit()
    {
        if (ClientScene.localPlayers[0].IsValid)
            localController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        // This RPC is only called once the game has started so we update the variable
        gameStarted = true;
        localController.gameStarted = true;

        playerCamera = GameObject.Find("CameraManager(Clone)");
        if (localController.role == "camera")
        {
            Transform shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
            playerCamera.transform.parent = shipTransform;
        }
        else if (localController.role == "engineer")
        {
            // Reset the camera rotation which was set in the lobby
            playerCamera.transform.localRotation = Quaternion.identity;

            // Set the camera's parent as the engineer instance
            playerCamera.transform.localPosition = new Vector3(0f, 0.8f, 0f);  // May need to be changed/removed
            playerCamera.transform.parent = localController.controlledObject.transform;

            // Set the controlled object for the server side PlayerController
            controlledObject = localController.controlledObject;
            engController = controlledObject.GetComponent<EngineerController>();

            // Set values for the client side PlayerController
            localController.engController = localController.controlledObject.GetComponent<EngineerController>();
            localController.engController.Initialize(playerCamera, localController);
        }
        else if(localController.role == "commander")
        {
            commandConsoleGameObject = Instantiate(Resources.Load("Prefabs/CommanderManager", typeof(GameObject))) as GameObject;
            commandConsoleState = commandConsoleGameObject.GetComponent<CommandConsoleState>();
            commandConsoleState.givePlayerControllerReference(this);
        }
    }
    
    public void CreateCamera()
    {
        playerCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
    }

    [Command]
    public void CmdUpgrade(int where) //0 = shields, 1 = guns, 2 = engines
    {
		// Send request to command console to perform upgrade	
    }
		
    private void Update()
    {
        // Make sure the server doesn't execute this and that the game has started
        if (!isLocalPlayer || !gameStarted)
            return;

        if (role == "engineer")
            engController.EngUpdate();
    }

    private void FixedUpdate()
    {
        // Make sure the server doesn't execute this and that the game has started
        if (!isLocalPlayer || !gameStarted)
            return;

        if (role == "engineer")
            engController.EngFixedUpdate();
    }

    [ClientRpc]
    public void RpcSetRole(string newRole)
    {
        role = newRole;
        Debug.Log("Role set: "+ role);
    }

    // RpcRotateCamera sets orientation using the yRotate, it is not a relative rotation
    [ClientRpc]
    public void RpcRotateCamera(float yRotate, uint receivedId)
    {
        // Change only local camera
        if (isLocalPlayer && netId.Value == receivedId)
        {
            Debug.Log("setting yRotate: " + yRotate);
            Quaternion q = Quaternion.Euler(new Vector3(0, yRotate, 0));
            playerCamera.transform.localRotation = q;
        }
    }

    [ClientRpc]
    public void RpcSetCameraIndex(int newIndex)
    {
        index = newIndex;
        Debug.Log("netId " + netId + " now has index " + index);
    }

    void Start()
    {
        // Get the game manager and game state at start
        gameManager = GameObject.Find("GameManager");
        gameState = gameManager.GetComponent<GameState>();
        serverManager = gameManager.GetComponent<ServerManager>();

        //Each client request server command
        if (isServer)
        {
            //ship = GameObject.Find("PlayerShipLogic(Clone)");
            //shipMovement = ship.GetComponent<ShipMovement>();
        }

        if (isLocalPlayer)
        {
            CreateCamera();
            CmdJoin();
        }
    }

    [Command]
    void CmdJoin()
    {
        // Get lobby script
        GameObject lobbyObject = GameObject.Find("ServerLobby(Clone)");
        ServerLobby serverLobby;
        if (lobbyObject != null)
        {
            serverLobby = GameObject.Find("ServerLobby(Clone)").GetComponent<ServerLobby>();
            if (serverLobby != null)
            {
                // Notify manager and lobby of joining
                serverLobby.PlayerJoin(gameObject);
            }
        }
        else
        {
            Debug.Log("Server lobby not found, cannot set up.");
        }
    }

    // Called by the engineer to carry out upgrades
    // and repairs
    [Command]
    public void CmdDoUpgrade()
    {

    }

    [Command]
    public void CmdDoRepair()
    {

    }

    // Called by the command console to add parts for
    // upgrade or repair
    [Command]
    public void CmdAddUpgrade(string part)
    {
        serverManager.NotifyEngineer(true, part);
    }

    [Command]
    public void CmdAddRepair(string part)
    {
        serverManager.NotifyEngineer(false, part);
    }

    // Used to notify the engineer of an upgrade or repair that has been
    // added
    public void AddJob(bool upgrade, string part)
    {
        // If this is somehow invoked on a client that isn't an engineer
        // or something that isn't a client at all it should be ignored
        if (role != "engineer" || !isLocalPlayer)
            return;

        localController.engController.AddJob(upgrade, part);
    }

    // OnGUI draws to screen and is called every few frames
    void OnGUI()
    {
        //GUI.Label(new Rect(50, 250, 200, 20), "Shield Level: " + shieldsLevel);
    }
}
