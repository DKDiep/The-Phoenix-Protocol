using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    private GameObject pawn;

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
        
    }
}
