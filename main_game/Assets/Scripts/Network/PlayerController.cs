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
    private GameObject playerCamera;
    private GameObject multiCamera;

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
        playerCamera = Instantiate(Resources.Load("Prefabs/CameraManager", typeof(GameObject))) as GameObject;
        if (role == "camera")
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
    }

    public void CallSetCamera()
    {
        RpcSetCamera();
    }

    public void SetRole(string newRole)
    {
        role = newRole;
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
