/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Instantiates a number of objects at startup on both the Server and Clients
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

            //spawn.transform.parent = this.transform;
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

        return null; // Return null GameObject if an active object cannot be found
    }

    public void RemoveObject(string objName)
    {
        int id = int.Parse(objName);
        pool[id].transform.parent = null;
        pool[id].SetActive(false);
    }

}
