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
    private GameObject playerCamera;
    private GameObject multiCamera;
    private EngineerController engController;
    private PlayerController localController = null;

    public GameObject thing;
    // Private to each instance of script
    private GameObject ship;
    private CommandConsoleState thingscript;
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
            // align
            //Vector3 v = ofr - ofl;
            //multiCamera.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, v);
            Quaternion q = Quaternion.FromToRotation(ofr, ofl);
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
                shipMovement.speed += 15;
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

    public void CallSetCamera()
    {
        RpcSetCamera();
    }

    public void SetRole(string newRole)
    {
        this.role = newRole;
    }

    void Start()
    {
        if (isServer)
        {
            ship = GameObject.Find("PlayerController");
            shipMovement = ship.GetComponent<ShipMovement>();
        }
        // Look for gameObject called "PlayerShip", returns null if not found. MainScene will find, TestNetworkScene won't.
        print("player appears");
        if (isClient)
        {
            thing = GameObject.Find("StuffManager");
            thingscript = thing.GetComponent<CommandConsoleState>();
            //thingscript.test();
            thingscript.gimme(this);
        }

        if (isLocalPlayer)
        {
            Debug.Log("Local Player");
        }
        else
        {
            Debug.Log("Non-Local Player");
        }
    }
    // OnGUI draws to screen and is called every few frames
    void OnGUI()
    {
        GUI.Label(new Rect(50, 250, 200, 20), "Shield Level: " + shieldsLevel);
    }
}

/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {


    // Sync vars are synchronised for all connected instances of scripts

    public GameObject thing;
    // Private to each instance of script
    private GameObject ship;
    private CommandConsoleState thingscript;
    private int shieldsLevel = 0;
    private int gunsLevel = 0;
    private int enginesLevel = 0;
    ShipMovement shipMovement;
    public void test() { print("gotplayer"); }

    private GameObject controlledObject;

    private Camera cameraComponent;

    public GameObject GetControlledObject()
    {
        return controlledObject;
    }

    public void SetControlledObject(GameObject newControlledObject)
    {   
        controlledObject = newControlledObject;
        Transform cameraManager = newControlledObject.transform.Find("CameraManager");
        if (cameraManager)
            cameraComponent = cameraManager.GetComponent<Camera>();
    }

    void Start()
    {
        if (isServer)
        {
            ship = GameObject.Find("PlayerController");
            shipMovement = ship.GetComponent<ShipMovement>();
        }
        // Look for gameObject called "PlayerShip", returns null if not found. MainScene will find, TestNetworkScene won't.
        print("player appears");
        if (isClient)
        {
            thing = GameObject.Find("StuffManager");
            thingscript = thing.GetComponent<CommandConsoleState>();
            //thingscript.test();
            thingscript.gimme(this);
        }
        cameraComponent = gameObject.GetComponent<Camera>();
        if (!isLocalPlayer)
        {
            cameraComponent.enabled = false;
        }
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
                shipMovement.speed += 15;
                break;
        }
    }
    // OnGUI draws to screen and is called every few frames
    void OnGUI()
    {
        GUI.Label(new Rect(50, 250, 200, 20), "Shield Level: " + shieldsLevel);
    }

}
*/