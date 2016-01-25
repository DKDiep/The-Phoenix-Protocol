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

public class PlayerController : NetworkBehaviour {

    private GameObject controlledObject;
    private string role = "camera";
    private int orientation = 0;
    private GameObject playerCamera;
    private GameObject multiCamera;
    private EngineerController engController;
    private PlayerController localController = null;

    public GameObject GetControlledObject()
    {
        return controlledObject;
    }

    public void SetControlledObject(GameObject newControlledObject)
    {   
        controlledObject = newControlledObject;
    }

    [ClientRpc]
    public void RpcSetCamera()
    {
        if (ClientScene.localPlayers[0].IsValid)
            localController = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();

        playerCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
        if (localController.role == "camera")
        {
            Transform shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
            if (Network.isClient)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            playerCamera.transform.parent = shipTransform;

            
            // **** Temporary duplicate camera to compute multi-screen rotation ****
            multiCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
            multiCamera.transform.parent = shipTransform;
            multiCamera.gameObject.name = "MultiCam";
            // Get camera frustum planes
            Camera cam = multiCamera.GetComponent<Camera>();
            // Calculate frustum height at far clipping plane using field of view
            float frustumHeight = 2.0f * cam.farClipPlane * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            // Calculate frustum width using height and camera aspect
            float frustumWidth = frustumHeight * cam.aspect;
            // Calculate left and right vectors of frustum
            Vector3 of = (multiCamera.transform.localRotation * Vector3.forward * cam.farClipPlane) - multiCamera.transform.localPosition;
            Vector3 ofr = of + (multiCamera.transform.localRotation * Vector3.right * frustumWidth / 2.0f);
            Vector3 ofl = of + (multiCamera.transform.localRotation * Vector3.left * frustumWidth / 2.0f);
            Quaternion q = Quaternion.FromToRotation(ofl, ofr);
            Vector3 r = q.eulerAngles;
            Debug.Log(r);
            multiCamera.transform.localRotation = q * multiCamera.transform.localRotation;
            multiCamera.SetActive(false);
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

    public void CallSetCamera()
    {
        RpcSetCamera();
    }

    public void SetRole(string newRole)
    {
        this.role = newRole;
    }

    public void SetOrientation(int newOrientation)
    {
        orientation = newOrientation;
    }

    void Start()
    { 
        // Get lobby script
        Debug.Log("Player Controller created");
        GameObject lobbyObject = GameObject.Find("ServerLobby(Clone)");
        ServerLobby serverLobby;
        if (lobbyObject != null)
        {
            serverLobby = GameObject.Find("ServerLobby(Clone)").GetComponent<ServerLobby>();
            if (serverLobby != null)
            {
                // Notify manager and lobby of joining
                serverLobby.playerJoin(gameObject);
            }
        }
        else
        {
            Debug.Log("Server lobby not found, cannot set up.");
        }
    }
}