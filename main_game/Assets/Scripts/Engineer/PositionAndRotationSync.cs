using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PositionAndRotationSync : NetworkBehaviour
{
    [SyncVar]
    Quaternion rotation;

    [SyncVar]
    Vector3 position;

    void Update()
    {
        if (isServer)
        {
            //Debug.Log("server");
            rotation = gameObject.transform.rotation;
            position = gameObject.transform.position;
        }
        else if (isClient)
        {
            //Debug.Log("client");
            gameObject.transform.rotation = rotation;
            gameObject.transform.position = position;
        }
    }
}
