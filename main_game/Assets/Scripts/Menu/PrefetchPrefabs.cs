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
	
	// Update is called once per frame
	void Update () {
	
	}
}
