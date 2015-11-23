using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientController : NetworkBehaviour {

    private string debugString = "No Info";
    public static NetworkIdentity networkIdentity;

    [ClientRpc]
    void RpcSpawn(string type)
    {
        Debug.Log("Spawn:" + type);
        debugString = type;
    }

    void Start()
    {
        networkIdentity = gameObject.GetComponent<NetworkIdentity>();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), debugString);
    }
}
