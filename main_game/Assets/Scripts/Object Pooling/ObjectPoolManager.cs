/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Instantiates a number of objects at startup on both the Server and Clients
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ObjectPoolManager : NetworkBehaviour 
{
    private GameObject[] pool;

    [SerializeField] GameObject[] obj; // Object to spawn
    [SerializeField] int size; // Number of objects to spawn
    [SerializeField] bool serverOnly;

	// Use this for initialization
	public void SpawnObjects () 
    {
        pool = new GameObject[size];

	    for(int i = 0; i < size; ++i)
        {
            int rnd = Random.Range(0,obj.Length);
            GameObject spawn = Instantiate (obj[rnd], Vector3.zero, Quaternion.identity) as GameObject;
            spawn.SetActive(false);
            spawn.name = i.ToString();
            if(!serverOnly) ServerManager.NetworkSpawn(spawn);
            if(spawn.GetComponent<Collider>() != null) spawn.GetComponent<Collider>().enabled = true;
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

        Debug.LogError("Object pool " + gameObject.name + " does not have any available objects");
        return null; // Return null GameObject if an active object cannot be found
    }

    public void RemoveObject(string objName)
    {
        int id = int.Parse(objName);
        pool[id].transform.parent = null;
        pool[id].SetActive(false);
    }

    public void EnableClientObject(string name, Vector3 position, Quaternion rotation, Vector3 scale)
    {
       int id = int.Parse(name);
       RpcEnableObject(id, position, rotation, scale);
    }

    public void DisableClientObject(string name)
    {
       int id = int.Parse(name);
       RpcDisableObject(id);
    }

    public void UpdateTransform(Vector3 position, Quaternion rotation, string name)
    {
        RpcUpdateTransform(position, rotation, int.Parse(name));
    }

    [ClientRpc]
    void RpcUpdateTransform(Vector3 position, Quaternion rotation, int id)
    {
        pool[id].transform.position = position;
        pool[id].transform.rotation = rotation;
    }

    [ClientRpc]
    void RpcEnableObject(int id, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        pool[id].SetActive(true);
        pool[id].transform.position = position;
        pool[id].transform.rotation = rotation;
        pool[id].transform.localScale = scale;
    }

    [ClientRpc]
    void RpcDisableObject(int id)
    {
        pool[id].transform.parent = null;
        pool[id].SetActive(false);
    }

}
