using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class URotationSync : NetworkBehaviour
{
    [SyncVar]
    Quaternion rotation;

    void Update()
    {
        if (isServer)
        {
            //Debug.Log("server");
            rotation = gameObject.transform.rotation;
        }
        else if (isClient)
        {
            //Debug.Log("client");
            gameObject.transform.rotation = rotation;
        }
    }
}
