using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerControl : NetworkBehaviour {

    [SyncVar]
    public float shipX;

    [SyncVar]
    public float shipZ;

    private GameObject ship;

	void Start ()
    {
        ship = GameObject.Find("PlayerShip");
	}

	void Update ()
    {
	    if (ship != null)
        {
            //Debug.Log("Ship identified");
        }
	}

    void OnGUI()
    {
        if (ship != null)
        {
            GUI.Label(new Rect(10, 200, 200, 20), "Ship identified");
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

/*
public class PlayerControl : NetworkBehaviour
{
	static int s_playerId;

	[SyncVar]
	public int playerId;

	[SyncVar]
	public int score;

	[Command]
	void CmdScore(int amount)
	{
		score += amount;
	}

	void Awake()
	{
		playerId = s_playerId++;
	}

	void OnGUI()
	{
		if (isLocalPlayer)
		{
			if (GUI.Button(new Rect(50, 200 + playerId * 30, 200, 20), "Your Score:" + score))
			{
				CmdScore(10);
			}
		}
		else
		{
			GUI.Label(new Rect(50, 200 + playerId * 30, 200, 20), "Other Score:" + score);
		}
	}

}
*/