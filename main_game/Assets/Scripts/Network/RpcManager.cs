using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RpcManager : NetworkBehaviour
{

    private string debugString = "No Info";

    [ClientRpc]
    void RpcSend(string type)
    {
        Debug.Log("RpcSend:" + type);
        debugString = type;
    }

    public void CallRpcSend(string type)
    {
        Debug.Log("In Call RPC");
        RpcSend(type);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 500, 100, 20), debugString);
    }
}
