using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RpcManager : NetworkBehaviour
{
    [ClientRpc]
    void RpcSend(string type)
    {
        Debug.Log("RpcSend:" + type);
    }

    public void CallRpcSend(string type)
    {
        Debug.Log("In Call RPC");
        RpcSend(type);
    }

    void OnGUI()
    {
		
    }
}
