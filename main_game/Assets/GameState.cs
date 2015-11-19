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
        asteroidList = new List<GameObject>(); // TODO: this should use AsteroidSpawner.maxAsteroids
        /* TODO: Doing this causes an UnassignedReferenceException on playerShip. Maybe Dillon or Marc can look at it?
        EnemySpawner es = this.GetComponent<EnemySpawner>();
        Debug.Log(es.maxEnemies); */
        enemyShipList = new List<GameObject>(); // TODO: this should use EnemySpawner.maxEnemies
        playerShip = GameObject.Find("PlayerShip");
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(playerShip.transform.position.x);
        //Debug.Log(asteroidList.Count);
        //Debug.Log(enemyShipList.Count);
    }
}
