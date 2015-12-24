using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EngineerJoinHandler : NetworkBehaviour {

    private GameState gameState;

    void Start()
    {
        gameState = gameObject.GetComponent<GameState>();
    }

    [Command]
    void CmdEngineerJoin()
    {

    }

    [ClientRpc]
    void RpcJoinResponse(GameObject engineerInstance)
    {

    }
}
