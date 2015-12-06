using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
public class NetworkManagerPlus : NetworkManager {

    public static GameObject player;

	// Use this for initialization
	void Start () {
	
	}

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        print("NETWORKPMANUAAGAERPLUSe\n\n\n\n\n");

        player = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        print("done");
    }

    // Update is called once per frame
    void Update () {
	
	}
}
