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
            rotation = gameObject.transform.rotation;
            position = gameObject.transform.position;
        }
        else if (isClient)
        {
            gameObject.transform.rotation = rotation;
            gameObject.transform.position = position;
        }
    }
}
