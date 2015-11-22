/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru
    Description: The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    private List<GameObject> asteroidList;
    private List<GameObject> enemyList;
    private GameObject playerShip;

    // Asteroid list getters and setters
    public List<GameObject> getAsteroidList()
    {
        return asteroidList;
    }

    public int getAsteroidListCount()
    {
        return asteroidList.Count;
    }

    public void addAsteroidList(GameObject asteroidObject)
    {
        asteroidList.Add(asteroidObject);
    }

    public void removeAsteroidAt(int i)
    {
        asteroidList.RemoveAt(i);
    }

    public GameObject getAsteroidAt(int i)
    {
        return asteroidList[i];
    }

    // Enemy list getters and setters
    public List<GameObject> getEnemyList()
    {
        return enemyList;
    }

    public int getEnemyListCount()
    {
        return enemyList.Count;
    }

    public void addEnemyList(GameObject enemyObject)
    {
        enemyList.Add(enemyObject);
    }

    public void removeEnemyAt(int i)
    {
        enemyList.RemoveAt(i);
    }

    public GameObject getEnemyAt(int i)
    {
        return enemyList[i];
    }

    public GameObject getPlayerShip()
    {
        return playerShip;
    }

    void InitializeVariables()
    {
        asteroidList = new List<GameObject>();
        enemyList = new List<GameObject>();
    }

    void SceneSetup()
    {
        playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
    }

    void Start ()
    {
        InitializeVariables();

        SceneSetup();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(playerShip.transform.position.x);
        //Debug.Log(asteroidList.Count);
        //Debug.Log(enemyShipList.Count);
    }
}
