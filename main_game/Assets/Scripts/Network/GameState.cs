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

    public Status getStatus()
    {
        return status;
    }

    public void setStatus(Status newStatus)
    {
        status = newStatus;
    }

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

    private void InitializeVariables()
    {
        asteroidList = new List<GameObject>();
        enemyList = new List<GameObject>();
    }

    private void SceneSetup()
    {
        playerShip = Instantiate(Resources.Load("Prefabs/PlayerShip", typeof(GameObject))) as GameObject;
    }

    public void Setup()
    {
        InitializeVariables();
        SceneSetup();
    }
}
