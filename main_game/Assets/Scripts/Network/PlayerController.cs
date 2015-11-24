using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

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
        cameraComponent = gameObject.GetComponent<Camera>();
        if (!isLocalPlayer)
        {
            cameraComponent.enabled = false;
        }
    }
}
