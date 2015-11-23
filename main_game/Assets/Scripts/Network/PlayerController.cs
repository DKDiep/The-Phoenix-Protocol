using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    private GameObject character;

    private Camera cameraComponent;

    public GameObject GetControlledPawn()
    {
        return character;
    }

    public void SetControlledCharacter(GameObject characterObject)
    {
        character = characterObject;
        Transform cameraManager = characterObject.transform.Find("CameraManager");
        if (cameraManager)
            cameraComponent = cameraManager.GetComponent<Camera>();
    }

    void Start()
    {
        cameraComponent = gameObject.GetComponent<Camera>();
    }
}
