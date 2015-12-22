/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru
    Description: The game state resides solely on the server, holding a collection of data that allows clients to replicate
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour {

    public enum Status { Setup, Started };

    private Status status = Status.Setup;
    private List<GameObject> asteroidList;
    private List<GameObject> enemyList;
    private GameObject playerShip;
    
    void Update()
    {
    	if(Input.GetKeyDown (KeyCode.Escape))
    	{
    		Application.Quit ();
    	}
    }

    public Status GetStatus()
    {
        return status;
    }

    public void SetStatus(Status newStatus)
    {
        status = newStatus;
    }

    // Asteroid list getters and setters
    public List<GameObject> GetAsteroidList()
    {
        return asteroidList;
    }

    public int GetAsteroidListCount()
    {
        return asteroidList.Count;
    }

    public void AddAsteroidList(GameObject asteroidObject)
    {
        asteroidList.Add(asteroidObject);
    }

    public void RemoveAsteroidAt(int i)
    {
        asteroidList.RemoveAt(i);
    }

    public GameObject GetAsteroidAt(int i)
    {
        return asteroidList[i];
    }

    // Enemy list getters and setters
    public List<GameObject> GetEnemyList()
    {
        return enemyList;
    }

    public int GetEnemyListCount()
    {
        return enemyList.Count;
    }

    public void AddEnemyList(GameObject enemyObject)
    {
        enemyList.Add(enemyObject);
    }

    public void RemoveEnemyAt(int i)
    {
        enemyList.RemoveAt(i);
    }

    public GameObject GetEnemyAt(int i)
    {
        return enemyList[i];
    }

    public GameObject GetPlayerShip()
    {
        return playerShip;
    }

    public void SetPlayerShip(GameObject newPlayerShip)
    {
        playerShip = newPlayerShip;
    }

    private void InitializeVariables()
    {
        asteroidList = new List<GameObject>();
        enemyList = new List<GameObject>();
    }

    public void Setup()
    {
        InitializeVariables();
    }
}
