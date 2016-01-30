/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Spawns each prefab once to cache it in memory
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PrefetchPrefabs : MonoBehaviour 
{
    NetworkManager networkManager;

	// Use this for initialization
	void Start () 
    {
        networkManager = GetComponent<NetworkManager>();
        List<GameObject> spawnList = new List<GameObject>(); 
        spawnList = networkManager.spawnPrefabs;

        for(int i = 0; i < spawnList.Count; i++)
        {
            if(spawnList[i] != null)
            {
                GameObject temp = Instantiate(spawnList[i], new Vector3(1000,1000,1000), Quaternion.identity) as GameObject;
                Destroy(temp);
            } 

        }
	}
}
