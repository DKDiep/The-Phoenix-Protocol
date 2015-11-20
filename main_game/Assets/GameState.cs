/*
    2015-2016 Team Pyrolite, University of Bristol.
    Authors: Dillon Keith Diep, Andrei Poenaru
    Description: The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    private List<GameObject> asteroidList;
    private List<GameObject> enemyShipList;
    private GameObject playerShip;

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

    public GameObject getPlayerShip()
    {
        return playerShip;
    }

    void InitializeVariables()
    {
        asteroidList = new List<GameObject>();
        enemyShipList = new List<GameObject>();
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
