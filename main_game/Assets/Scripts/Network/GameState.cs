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
    private List<GameObject> newAsteroids;
    private List<uint> removedAsteroids;
    private List<GameObject> enemyList;
    private List<GameObject> engineerList;
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
        newAsteroids.Add(asteroidObject);
    }

    public void RemoveAsteroid(GameObject removeObject)
    {        
        bool wasDeleted = newAsteroids.Remove(removeObject);
        if (!wasDeleted) removedAsteroids.Add((uint)removeObject.GetInstanceID());
        asteroidList.Remove(removeObject);
        AsteroidSpawner.numAsteroids--;
    }

    public void RemoveAsteroidAt(int i)
    {
        bool wasDeleted = newAsteroids.Remove(asteroidList[i]);
        if (!wasDeleted) removedAsteroids.Add((uint)asteroidList[i].GetInstanceID());
        asteroidList.RemoveAt(i);
        AsteroidSpawner.numAsteroids--;
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

    // Engineer list getters and setters
    public List<GameObject> GetEngineerList()
    {
        return engineerList;
    }

    public int GetEngineerCount()
    {
        return engineerList.Count;
    }

    public void AddEngineerList(GameObject engineerObject)
    {
        engineerList.Add(engineerObject);
    }

    public void RemoveEngineerAt(int i)
    {
        engineerList.RemoveAt(i);
    }

    public GameObject GetEngineerAt(int i)
    {
        return engineerList[i];
    }

    public GameObject GetPlayerShip()
    {
        return playerShip;
    }

    public void SetPlayerShip(GameObject newPlayerShip)
    {
        playerShip = newPlayerShip;
    }

    public List<GameObject> GetNewAsteroids()
    {
        return newAsteroids;
    }

    public void ClearNewAsteroids()
    {
        newAsteroids = new List<GameObject>();
    }

    public List<uint> GetRemovedAsteroids()
    {
        return removedAsteroids;
    }

    public void ClearRemovedAsteroids()
    {
        removedAsteroids = new List<uint>();
    }

    private void InitializeVariables()
    {
        asteroidList = new List<GameObject>();
        enemyList = new List<GameObject>();
        newAsteroids = new List<GameObject>();
        removedAsteroids = new List<uint>();
        engineerList = new List<GameObject>();
    }

    public void Setup()
    {
        InitializeVariables();
    }
}
