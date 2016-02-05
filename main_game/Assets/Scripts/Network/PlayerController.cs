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
    private int shieldsLevel = 0;
    private int gunsLevel = 0;
    private int enginesLevel = 0;
    ShipMovement shipMovement;
    public void test() { print("gotplayer"); }

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
        print("inside rpcRoleInit");
        if (ClientScene.localPlayers[0].IsValid)
            localController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        playerCamera = GameObject.Find("CameraManager(Clone)");
        if (localController.role == "camera")
        {
            Transform shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
            playerCamera.transform.parent = shipTransform;
        }
        else if (localController.role == "engineer")
        {
            // Set the camera's parent as the engineer instance
            playerCamera.transform.localPosition = new Vector3(0f, 0.8f, 0f);  // May need to be changed/removed
            playerCamera.transform.parent = localController.controlledObject.transform;

            // Set the controlled object for the server side PlayerController
            controlledObject = localController.controlledObject;
            engController = controlledObject.GetComponent<EngineerController>();

            // Set values for the client side PlayerController
            localController.engController = localController.controlledObject.GetComponent<EngineerController>();
            localController.engController.Initialize(playerCamera);
        }
        else if(localController.role == "commander")
        {
            commandConsoleGameObject = Instantiate(Resources.Load("Prefabs/CommanderManager", typeof(GameObject))) as GameObject;
            commandConsoleState = commandConsoleGameObject.GetComponent<CommandConsoleState>();
            commandConsoleState.test();
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
        switch (where)
        {
            case 0:
                shieldsLevel++;
                break;
            case 1:
                gunsLevel++;
                break;
            case 2:
                enginesLevel++;
                //shipMovement.speed += 15; <-- commented upon merge as speed is no longer accessible
                break;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if (role == "engineer")
            engController.EngUpdate();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
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

    // OnGUI draws to screen and is called every few frames
    void OnGUI()
    {
        //GUI.Label(new Rect(50, 250, 200, 20), "Shield Level: " + shieldsLevel);
    }
}