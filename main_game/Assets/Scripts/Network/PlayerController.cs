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
	private RoleEnum role = RoleEnum.Camera;
    private GameObject playerCamera;
    private GameObject multiCamera;
    private EngineerController engController;
    private PlayerController localController = null;
    private int index = 0;

    public GameObject commandConsoleGameObject;
    private GameObject ship;
    private CommandConsoleState commandConsoleState;
    private GameObject gameManager;
    private GameState gameState;
    private ServerManager serverManager;
    private NetworkClient client;
    private bool gameStarted = false;
    private ShipMovement shipMovement;

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
		if (localController.role == RoleEnum.Camera)
        {
            Transform shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
            playerCamera.transform.parent = shipTransform;
        }
		else if (localController.role == RoleEnum.Engineer)
        {
            // Reset the camera rotation which was set in the lobby
            playerCamera.transform.localRotation = Quaternion.identity;
            playerCamera.transform.parent = localController.controlledObject.transform;

            // Set the camera's parent as the engineer instance
            playerCamera.transform.localPosition = new Vector3(0f, 0.17f, 0.73f);  // May need to be changed/removed


            // Set the controlled object for the server side PlayerController
            controlledObject = localController.controlledObject;
            engController = controlledObject.GetComponent<EngineerController>();

            // Set values for the client side PlayerController
            localController.engController = localController.controlledObject.GetComponent<EngineerController>();
            localController.engController.Initialize(playerCamera, localController);
        }
		else if(localController.role == RoleEnum.Commander)
        {
            commandConsoleGameObject = Instantiate(Resources.Load("Prefabs/CommanderManager", typeof(GameObject))) as GameObject;
            commandConsoleState = commandConsoleGameObject.GetComponent<CommandConsoleState>();
            localController.commandConsoleState = commandConsoleState;
            commandConsoleState.givePlayerControllerReference(localController);
        }
    }
    
	public int GetScreenIndex() 
	{
		return index;
	}
    public void CreateCamera()
    {
        playerCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
    }

    [ClientRpc]
    public void RpcSetRole(RoleEnum newRole)
    {
        role = newRole;
        Debug.Log("Role set: "+ role);
    }


    public RoleEnum GetRole()
    {
        return role;
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

    [ClientRpc]
    public void RpcOutpostNotification(GameObject outpost)
    {
        if (commandConsoleState != null)
        {
            commandConsoleState.FoundOutpost(outpost);
        }
    }

    void Start()
    {
        // Get the game manager and game state at start
        gameManager = GameObject.Find("GameManager");
        gameState = gameManager.GetComponent<GameState>();
        serverManager = gameManager.GetComponent<ServerManager>();

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

	/// <summary>
	/// Call from the engineer to carry out upgrade
    /// of the provided part
	/// </summary>
	/// <param name="part">The part to upgrade</param>
    [Command]
    public void CmdDoUpgrade(ComponentType part)
    {
        gameState.GetUpgradableComponent(part).Upgrade();
        serverManager.SendUpgradeFinished(part);
    }

    /// <summary>
    /// Call from the engineer to carry out repair
    /// of the provided part
    /// </summary>
    /// <param name="part">The part to repair</param>
    [Command]
    public void CmdDoRepair(ComponentType part)
    {
        gameState.GetUpgradableComponent(part).Repair();
    }

    /// <summary>
    /// Call from the Command Console to add a component
    /// for upgrade
    /// </summary>
    /// <param name="part">The part to add for upgrading</param>
    [Command]
	public void CmdAddUpgrade(ComponentType part)
    {
        serverManager.NotifyEngineer(true, part);
    }

    /// <summary>
    /// Call from the Command Console to add a component
    /// for repair
    /// </summary>
    /// <param name="part">The part to add for repairing</param>
    [Command]
	public void CmdAddRepair(ComponentType part)
    {
        serverManager.NotifyEngineer(false, part);
    }

    [Command]
    public void CmdGetComponentStatus(ComponentType part)
    {
        float health = gameState.GetComponentHealth(part);
        int level = 0; //TODO: REPLACE THIS WITH AN ACTUAL CALL
        serverManager.SendComponentStatus(health, level, netId.Value);
    }

	/// <summary>
	/// Adds a job to the engineer's job queue
	/// </summary>
	/// <param name="isUpgrade">Wether the job is an upgrade or a repair</param>
	/// <param name="part">The part to upgrade/repair</param>
	public void AddJob(bool isUpgrade, ComponentType part)
    {
        // If this is somehow invoked on a client that isn't an engineer
        // or something that isn't a client at all it should be ignored
		if (role != RoleEnum.Engineer || !isLocalPlayer)
            return;

        engController.AddJob(isUpgrade, part);
    }

    /// <summary>
    /// Tells the engineer of the requested component's status.
    /// The request happens in CmdGetComponentStatus
    /// </summary>
    /// <param name="health">The health of the component</param>
    /// <param name="level">The upgrade level of the component</param>
    public void UpdateComponentStatus(float health, int level)
    {
        // If this is somehow invoked on a client that isn't an engineer
        // or something that isn't a client at all it should be ignored
        if (role != RoleEnum.Engineer || !isLocalPlayer)
            return;

        engController.SetComponentStatus(health, level);
    }

    /// <summary>
    /// Notify the CommandConsole that an upgrade has finished
    /// </summary>
    /// <param name="component"></param>
    public void FinishUpgrade(ComponentType component)
    {
        commandConsoleState.ConfirmUpgrade(component);
    }

    [ClientRpc]
    public void RpcStartMission(string title, string description)
    {
        if (commandConsoleState != null)
        {
            commandConsoleState.ShowMissionPopup(title, description);
        }
    }

    [ClientRpc]
    public void RpcCompleteMission(string description)
    {
        if (commandConsoleState != null)
        {
            commandConsoleState.ShowMissionPopup("MISSION COMPLETE", description);
        }
    }
}
