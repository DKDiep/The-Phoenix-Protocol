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

    public GameObject GetControlledObject()
    {
        return controlledObject;
    }

    public void SetControlledObject(GameObject newControlledObject)
    {   
        controlledObject = newControlledObject;
        Transform cameraManager = newControlledObject.transform.Find("CameraManager");
    }

    public void SetCamera()
    {
        if (isLocalPlayer)
        {
        }
    }

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        //Debug.Log("don'tdestroy");
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            Debug.Log("Local Player");
        }
        else
        {
            Debug.Log("Non-Local Player");
        }
    }
}
