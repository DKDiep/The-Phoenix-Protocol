using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

// Clients send this message to the server to pick their roles
public class PickRoleMessage : MessageBase {
    public int role;
}
