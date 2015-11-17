using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    public List<GameObject> asteroidList;
    public List<GameObject> enemyShipList;
    public GameObject playerShip;

	// Use this for initialization
	void Start () {
        playerShip = GameObject.Find("PlayerShip");
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(playerShip.transform.position.x);
        //Debug.Log(asteroidList.Count);
        //Debug.Log(enemyShipList.Count);
    }
}
