using UnityEngine;
using System.Collections;

public class HostController : MonoBehaviour {

    private GameObject pawn;

    GameObject GetControlledPawn()
    {
        return pawn;
    }

    void SetControlledPawn(GameObject pawnObject)
    {
        pawn = pawnObject;
    }
}
