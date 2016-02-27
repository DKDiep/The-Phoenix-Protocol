using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientController : NetworkBehaviour {

    public static NetworkIdentity networkIdentity;

    [ClientRpc]
    void RpcSend(string type)
    {
        Debug.Log("RPCSend Client:" + type);
    }

    void Start()
    {
        networkIdentity = gameObject.GetComponent<NetworkIdentity>();
    }

    void OnGUI()
    {
		
    }
}
