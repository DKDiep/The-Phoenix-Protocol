using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

// Message from server to client to let the client know what object it controls
public class ControlledObjectMessage : MessageBase {
    public GameObject controlledObject;
}
