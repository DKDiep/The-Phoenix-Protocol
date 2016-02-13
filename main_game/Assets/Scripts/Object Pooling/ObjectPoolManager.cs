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
    private int size = 200; // Number of bullets to spawn

    [SerializeField] GameObject obj; // Object to spawn

	// Use this for initialization
	void Start () 
    {
        pool = new GameObject[size];

	    for(int i = 0; i < size; ++i)
        {
            GameObject spawn = Instantiate (obj, Vector3.zero, Quaternion.identity) as GameObject;
            spawn.SetActive(false);
            pool[i] = spawn;
        }
	}

}
