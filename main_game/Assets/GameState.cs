/*
    Copyright 2015-2016 Team Pyrolite, University of Bristol.
    Authors: Dillon Keith Diep, Andrei Poenaru
    Description: The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

    private List<Transform> asteroidList;
    private List<GameObject> enemyShipList;
    private GameObject playerShip;

    List<Transform> getAsteroidList()
    {
        return asteroidList;
    }

    void InitializeVariables()
    {
        asteroidList = new List<Transform>();
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
