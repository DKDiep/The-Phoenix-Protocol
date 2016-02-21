/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Instantiates a number of objects at startup on both the Server and Clients
*/

/*
1. Spawn 200 bullets at startup, deactivated, store in array
2. When attempting to fire, send request for bullet. Return array index.
3. Move bullet to right position and direction, activate
4. Send RPC to clients requesting a bullet to be activated, pass in position, direction, array index as arguments
4. When bullet needs to be destroyed, deactivate it and free it in the array. Send RPC to clients to disable that particular bullet.

On client:

1. Spawn 200 bullets at startup, deactivated, store in array
2. RPC commands will do the rest

*/

using UnityEngine;
using System.Collections;

public class ObjectPoolManager : MonoBehaviour 
{
    private GameObject[] pool;

    [SerializeField] GameObject obj; // Object to spawn
    [SerializeField] int size; // Number of bullets to spawn

	// Use this for initialization
	void Start () 
    {
        pool = new GameObject[size];

	    for(int i = 0; i < size; ++i)
        {
            GameObject spawn = Instantiate (obj, Vector3.zero, Quaternion.identity) as GameObject;
            spawn.SetActive(false);
            spawn.name = i.ToString();
            pool[i] = spawn;
        }
	}

    public GameObject RequestObject()
    {
        for(int i = 0; i < size; ++i)
        {
            if(!pool[i].activeInHierarchy) 
            {
                pool[i].SetActive(true);
                return pool[i];
            }
                
        }

        Debug.LogError("Object pool does not have any available objects");
        return null; // Return null GameObject if an active object cannot be found
    }

    public void RemoveObject(string objName)
    {
        int id = int.Parse(objName);
        pool[id].transform.parent = null;
        pool[id].SetActive(false);
    }

}
