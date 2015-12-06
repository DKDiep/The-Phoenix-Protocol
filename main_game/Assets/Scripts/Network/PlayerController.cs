using UnityEngine;
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


