using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class HostController : NetworkBehaviour {

    private GameObject pawn;
    private int clientId = 0;
    private List<int> clientsIds;

    GameObject GetControlledPawn()
    {
        return pawn;
    }

    void SetControlledPawn(GameObject pawnObject)
    {
        pawn = pawnObject;
    }

    void Start()
    {
        clientsIds = new List<int>();
        // Host is client Id #0
        clientsIds.Add(0);
        clientId = 0;
    }
}
