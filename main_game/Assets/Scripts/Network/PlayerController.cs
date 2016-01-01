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
        /*GameObject playerCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
        if (role == "camera")
        {
            Transform shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
            if (Network.isClient)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(0, 180, 0);
                Debug.Log("ye");
            }
            playerCamera.transform.parent = shipTransform;
        }
        Debug.Log(this.netId);*/
    }

    public void CallSetCamera()
    {
        RpcSetCamera();
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            Debug.Log("Local Player");
            GameObject playerCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
        }
        else
        {
            Debug.Log("Non-Local Player");
        }
    }
}
