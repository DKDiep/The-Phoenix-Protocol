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

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] private GameObject[] obj; // Object to spawn
    [SerializeField] private int size; // Number of objects to spawn
    [SerializeField] private bool serverOnly;

    [SerializeField] private bool useInterpolation;
    private Vector3[] newPositions;
    private Quaternion[] newRotations;
    private float[] times;
    private bool amServer = false;

    //private bool isServer;
	#pragma warning restore 0649

    [Server]
    private void CheckServer()
    {
        amServer = true;
    }

    private void FixedUpdate()
    {
        if(!amServer && useInterpolation)
        {
            for(int i = 0; i < size; ++i)
            {
                if(pool[i].transform.position != null)
                {
                    pool[i].transform.position = Vector3.Lerp(pool[i].transform.position, newPositions[i], Time.deltaTime * 5f);
                    pool[i].transform.rotation = Quaternion.Lerp(pool[i].transform.rotation, newRotations[i], Time.deltaTime * 5f);
                }
            }
        }
    }


	// Use this for initialization
	public void SpawnObjects () 
    {
        CheckServer();

        if(!amServer && serverOnly)
            Destroy(this);

        newPositions = new Vector3[size];
        newRotations = new Quaternion[size];
        times = new float[size];

        pool = new GameObject[size];

        if(useInterpolation && !amServer)
        {
            newPositions = new Vector3[size];
            newRotations = new Quaternion[size];
            times = new float[size];
        }

	    for(int i = 0; i < size; ++i)
        {
            int rnd = Random.Range(0,obj.Length);
            GameObject spawn = Instantiate (obj[rnd], Vector3.zero, Quaternion.identity) as GameObject;
            spawn.SetActive(false);
            spawn.name = i.ToString();

            if(spawn.GetComponent<Collider>() != null)
				spawn.GetComponent<Collider>().enabled = true;
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
       if(!serverOnly)
            RpcEnableObject(id, position, rotation, scale);
    }

    public void SetAsteroidSpeed(string name, float speed)
    {
        int id = int.Parse(name);
        RpcSetAsteroidSpeed(id, speed);
    }

    [ClientRpc]
    public void RpcSetAsteroidSpeed(int id, float speed)
    {
        pool[id].GetComponent<AsteroidRotation>().SetClientSpeed(speed);
    }

    public void DisableClientObject(string name)
    {
       int id = int.Parse(name);
       if(!serverOnly)
            RpcDisableObject(id);
    }

    public void UpdateTransform(Vector3 position, Quaternion rotation, string name)
    {
        RpcUpdateTransform(position, rotation, int.Parse(name));
    }

    [ClientRpc]
    void RpcUpdateTransform(Vector3 position, Quaternion rotation, int id)
    {
        newPositions[id] = position;
        newRotations[id] = rotation;
        //pool[id].transform.position = position;
        //pool[id].transform.rotation = rotation;
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
