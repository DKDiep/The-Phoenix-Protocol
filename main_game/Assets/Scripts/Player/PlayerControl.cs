using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerControl : NetworkBehaviour {

    // Sync vars are synchronised for all connected instances of scripts
    [SyncVar]
    public float shipX;

    [SyncVar]
    public float shipZ;

    // Private to each instance of script
    private GameObject ship;

	void Start ()
    {
        // Look for gameObject called "PlayerShip", returns null if not found. MainScene will find, TestNetworkScene won't.
        ship = GameObject.Find("PlayerShip");
	}

    // OnGUI draws to screen and is called every few frames
    void OnGUI()
    {
        if (ship != null)
        {
            GUI.Label(new Rect(10, 200, 200, 20), "Ship identified");
            //Set the sync vars, this is set on MainScene, and then TestNetworkScene will be synced.
            shipX = ship.transform.position.x;
            shipZ = ship.transform.position.z;
        }
        else
        {
            GUI.Label(new Rect(10, 200, 200, 20), "Ship unidentified");
        }
        GUI.Label(new Rect(10, 220, 200, 20), "ShipX: " + shipX);
        GUI.Label(new Rect(10, 240, 200, 20), "ShipZ: " + shipZ);
    }
}