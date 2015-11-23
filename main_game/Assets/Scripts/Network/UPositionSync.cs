using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class UPositionSync : NetworkBehaviour
{
    [SyncVar]
    Vector3 position;

	void Start ()
    {
	    
	}

	void Update ()
    {
	    if(isServer)
        {
            //Debug.Log("server");
            position = gameObject.transform.position;
        }
        else if (isClient)
        {
            //Debug.Log("client");
            gameObject.transform.position = position;
        }
	}
}
