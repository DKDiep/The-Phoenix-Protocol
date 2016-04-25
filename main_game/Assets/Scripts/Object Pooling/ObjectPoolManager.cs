/*
    Instantiates a number of objects at startup on both the Server and Clients
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
    [SerializeField] Material hackedMaterial;
    [SerializeField] Material targetMaterial;
    [SerializeField] Material hackedTargetMaterial;
    [SerializeField] Material[] enemyGlows;
	#pragma warning restore 0649

    private Vector3[] newPositions;
    private bool spawned = false;
    public bool isCommander = false;
    private Quaternion[] newRotations;
    private bool amServer = false;
    [SerializeField] Material[] asteroidMaterials;

    [Server]
    private void CheckServer()
    {
        amServer = true;
    }

    private void FixedUpdate()
    {
        if(!amServer && useInterpolation && spawned)
        {
            for(int i = 0; i < size; ++i)
            {
                if(pool[i] != null && pool[i].activeInHierarchy)
                {
                    pool[i].transform.position = Vector3.Lerp(pool[i].transform.position, newPositions[i], Time.fixedDeltaTime * 10f);
                    pool[i].transform.rotation = Quaternion.Lerp(pool[i].transform.rotation, newRotations[i], Time.fixedDeltaTime * 10f);
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

        pool = new GameObject[size];

        if(useInterpolation && !amServer)
        {
            newPositions = new Vector3[size];
            newRotations = new Quaternion[size];
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
        spawned = true;
	}

    [ClientRpc]
    public void RpcSetHackedGlow(string name, string hackedName)
    {
        if(!isCommander)
        {
            int id = int.Parse(name);
            GameObject lights = pool[id].transform.Find("pattern").gameObject;
            pool[id].transform.Find("Target").GetComponent<Renderer>().material = hackedTargetMaterial;
            pool[id].transform.Find("Target").GetComponentInChildren<TextMesh>().text = hackedName;
            lights.GetComponent<Renderer>().material = hackedMaterial;
        }


    }

    [ClientRpc]
    public void RpcResetHackedGlow(string name)
    {
        if(!isCommander)
        {
            int id = int.Parse(name);
            GameObject lights = pool[id].transform.Find("pattern").gameObject;
            pool[id].transform.Find("Target").GetComponentInChildren<TextMesh>().text = "";
            pool[id].transform.Find("Target").gameObject.GetComponent<Renderer>().material = targetMaterial;
            if(this.gameObject.name.Contains("Gnat"))
                lights.GetComponent<Renderer>().material = enemyGlows[0];
            else if(this.gameObject.name.Contains("Firefly"))
                lights.GetComponent<Renderer>().material = enemyGlows[1];
            else if(this.gameObject.name.Contains("Termite") || this.gameObject.name.Contains("Hornet"))
                lights.GetComponent<Renderer>().material = enemyGlows[2];
            else if(this.gameObject.name.Contains("Lightning"))
                lights.GetComponent<Renderer>().material = enemyGlows[3];
            else if(this.gameObject.name.Contains("BlackWidow"))
                lights.GetComponent<Renderer>().material = enemyGlows[4];
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
        if (id >= pool.Length)
            Debug.Log("OUT OF RANGE HERE - id = "+id+", pool name = "+gameObject.name);
        pool[id].transform.parent = null;
        pool[id].SetActive(false);
    }

    public void EnableClientObject(string name, Vector3 position, Quaternion rotation, Vector3 scale)
    {
       int id = int.Parse(name);
       if(!serverOnly)
            RpcEnableObject(id, position, rotation, scale);
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

    public void SetAsteroidTexture(string name, int material)
    {
        int id = int.Parse(name);
        RpcSetAsteroidTexture(id, material);
     }

    public void SyncAsteroidRotation(string name, Quaternion rotation)
    {
        int id = int.Parse(name);
        RpcSyncRotation(id, rotation);
    }

    [ClientRpc]
    public void RpcSyncRotation(int id, Quaternion rotation)
    {
        if(!isCommander)
            pool[id].transform.rotation = rotation;
    }

    [ClientRpc]
    public void RpcSetAsteroidTexture(int id, int material)
    {
        if(!isCommander)
            pool[id].GetComponent<Renderer>().material = asteroidMaterials[material];
    }

    [ClientRpc]
    void RpcUpdateTransform(Vector3 position, Quaternion rotation, int id)
    {
        if(!isCommander && !amServer)
        {
            newPositions[id] = position;
            newRotations[id] = rotation;
        }
    }

    [ClientRpc]
    void RpcEnableObject(int id, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if(!isCommander)
        {
            pool[id].SetActive(true);
            pool[id].transform.position = position;
            pool[id].transform.rotation = rotation;
            pool[id].transform.localScale = scale;
        }
     }

    [ClientRpc]
    void RpcDisableObject(int id)
    {
        if(!isCommander)
        {
            pool[id].transform.parent = null;
            pool[id].SetActive(false);
        }

    }

	/// <summary>
	/// Disables all of this manager's objects.
	/// </summary>
	public void DisableAll()
	{
		for (int i = 0; i < size; i++)
		{
			string name = i.ToString();
			DisableClientObject(name);
			RemoveObject(name);
		}
	}

	/// <summary>
	/// Returns true if this manager owns the specified object.
	/// </summary>
	/// <param name="obj">The object.</param>
	/// <returns>True if <c>obj</c> is owned by this manager.</returns>
	public bool Owns(GameObject obj)
	{
		int id = int.Parse(obj.name);

		return pool[id] == obj;
		
	}

	////////////////////////////////////////////////////////////////////
	/// Type-specific methods
	/// aka "Team Pyrolite doesn't give a shit about object orientation"
	////////////////////////////////////////////////////////////////////

	public void SetAsteroidSpeed(string name, float speed)
	{
		int id = int.Parse(name);
		RpcSetAsteroidSpeed(id, speed);
	}

	[ClientRpc]
	public void RpcSetAsteroidSpeed(int id, float speed)
	{
        if(!isCommander)
		    pool[id].GetComponent<AsteroidRotation>().SetClientSpeed(speed);
	}

	public void SetBulletSpeed(string name, float speed)
	{
		int id = int.Parse(name);
		RpcSetBulletSpeed(id, speed);
	}

	[ClientRpc]
	public void RpcSetBulletSpeed(int id, float speed)
	{
        if(!isCommander)
		    pool[id].GetComponent<BulletMove>().Speed = speed;
	}
}
