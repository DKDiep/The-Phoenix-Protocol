/*
    Networked player entity, input management and RPCs
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
            gameState.ResetOfficerList();
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
            //Debug.Log("setting yRotate: " + yRotate);
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
    public void RpcOutpostNotification(GameObject outpost, int id, int difficulty)
    {
        if (commandConsoleState != null)
            commandConsoleState.FoundOutpost(outpost, id, difficulty);
    }

    [ClientRpc]
    public void RpcPortalInit(GameObject portal)
    {
        if (commandConsoleState != null)
            commandConsoleState.PortalInit(portal);
    }

    /// <summary>
    /// Notifies command console of outpost visit
    /// </summary>
    /// <param name="resources">Resources.</param>
    /// <param name="civilians">Civilians.</param>
    [ClientRpc]
    public void RpcNotifyOutpostVisit(int resources, int civilians, int id)
    {
        if (commandConsoleState != null)
            commandConsoleState.OutpostVisitNotify(resources, civilians, id);
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
        gameState.RemoveNotification(true, part);
        serverManager.SendJobFinished(true, part);
    }

    /// <summary>
    /// Call from the engineer to carry out repair
    /// of the provided part
    /// </summary>
    /// <param name="part">The part to repair</param>
    [Command]
    public void CmdDoRepair(ComponentType part)
    {
		gameState.RepairPart(part);
        gameState.RemoveNotification(false, part);
        serverManager.SendJobFinished(false, part);
    }

    private bool ShouldHaveNotification(ComponentType part)
    {
        return part == ComponentType.ShieldGenerator ||
               part == ComponentType.Turret ||
               part == ComponentType.Engine ||
               part == ComponentType.Hull;
    }

    /// <summary>
    /// Call from the Command Console to add a component
    /// for upgrade
    /// </summary>
    /// <param name="part">The part to add for upgrading</param>
    [Command]
	public void CmdAddUpgrade(ComponentType part)
    {
        if (ShouldHaveNotification(part))
            gameState.AddNotification(true, part);

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
        if (ShouldHaveNotification(part))
            gameState.AddNotification(false, part);

        serverManager.NotifyEngineer(false, part);
    }

    /// <summary>
    /// Requests the list of officers for the current game
    /// </summary>
    [Command]
    public void CmdRequestOfficerList()
    {
        serverManager.SendOfficers();
    }

    /// <summary>
    /// Sends the data, packed as a string to the specified officer
    /// </summary>
    /// <param name="officerName">The username of the officer to send to</param>
    /// <param name="officerId">The user ID of the officer to send to</param>
    /// <param name="data">The data to send</param>
    [Command]
    public void CmdSendToOfficer(string officerName, uint officerId, string data)
    {
        serverManager.SendToOfficer(officerName, officerId, data);
    }

    /// <summary>
    /// Broadcasts data to all officers
    /// </summary>
    /// <param name="data">The data to broadcast</param>
    [Command]
    public void CmdBroadcastToOfficers(string data)
    {
        serverManager.BroadcastToOfficers(data);
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
    /// Notify the CommandConsole that an upgrade has finished
    /// </summary>
    /// <param name="component"></param>
    public void FinishUpgrade(ComponentType component)
    {
        commandConsoleState.ConfirmUpgrade(component);
    }

    /// <summary>
    /// Notify the CommandConsole that a repair has finished
    /// </summary>
    /// <param name="component"></param>
    public void FinishRepair(ComponentType component)
    {
        commandConsoleState.ConfirmRepair(component);
    }

    /// <summary>
    /// Updates the officer data in the command console
    /// </summary>
    /// <param name="officerData">The new officer data</param>
    public void UpdateOfficerList(string officerData)
    {
        if (this.role == RoleEnum.Commander)
            gameState.UpdateOfficerList(officerData);
    }

    [ClientRpc]
    public void RpcStartMission(string title, string description, int[] missionCompletion, int[] missionValue)
    {
        print("inside RpcStartMission");
        if (commandConsoleState != null)
        {
            commandConsoleState.ShowMissionPopup(title, description);
            for (int i = 0; i < missionCompletion.Length; i++) {
                if ((CompletionType)missionCompletion[i] == CompletionType.Outpost)
                    commandConsoleState.ShowObjectiveOnMap(missionValue[i]);
                if ((CompletionType)missionCompletion[i] == CompletionType.Upgrade || (CompletionType)missionCompletion[i] == CompletionType.Repair)
                    commandConsoleState.ShowUpgradeObjective((UpgradableComponentIndex)missionValue[i]);
            }
        }
    }

    [ClientRpc]
    public void RpcCompleteMission(string description, int[] missionCompletion, int[] missionValue)
    {
        if (commandConsoleState != null)
        {
            commandConsoleState.ShowMissionPopup("MISSION COMPLETE", description);
            for (int i = 0; i < missionCompletion.Length; i++)
            {
                if ((CompletionType)missionCompletion[i] == CompletionType.Outpost)
                    commandConsoleState.RemoveObjectiveFromMap(missionValue[i]);
                if ((CompletionType)missionCompletion[i] == CompletionType.Upgrade || (CompletionType)missionCompletion[i] == CompletionType.Repair)
                    commandConsoleState.RemoveUpgradeObjective((UpgradableComponentIndex)missionValue[i]);
            }
        }
    }
}
