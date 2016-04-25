using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OutpostCollision : NetworkBehaviour 
{
	private OutpostLogic outpostLogic;

	private const int PLAYER_COLLISION_DAMAGE = 125; // This is currently half of the ship's health

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] Material helpMat;
    [SerializeField] Material savedMat;
	#pragma warning restore 0649 

    private bool client = false;
    private bool switchedMaterial = false;
    private bool isDestroyed = false;

	void Start()
	{
		outpostLogic = GetComponentInChildren<OutpostLogic>();
        if(outpostLogic == null)
            client = true;
        else
            StartCoroutine(CheckUpdate());
	}

    private IEnumerator CheckUpdate()
    {
        yield return new WaitForSeconds(1f);
        if(outpostLogic.discovered && !switchedMaterial)
        {
            switchedMaterial = true;
            SwitchMaterial(0);
        }
        else
            StartCoroutine(CheckUpdate());
    }
		
    public void SwitchMaterial(int id)
    {
        RpcSwitchMaterial(id);
        GameObject light = null;

        foreach(Transform child in transform)
        {
            if(child.gameObject.name.Equals("TopCap"))
            {
                light = child.gameObject;
                break;
            }
        }

        if(light == null)
            Debug.LogError("TopCap game object in outpost could not be found");

        if(id == 0)
            light.GetComponent<Renderer>().material = helpMat;
        else
            light.GetComponent<Renderer>().material = savedMat;
    }

    [ClientRpc]
    private void RpcSwitchMaterial(int id)
    {
        GameObject light = null;

        foreach(Transform child in transform)
        {
            if(child.gameObject.name.Equals("TopCap"))
            {
                light = child.gameObject;
                break;
            }
        }

        if(light == null)
            Debug.LogError("TopCap game object in outpost could not be found");

        if(id == 0)
            light.GetComponent<Renderer>().material = helpMat;
        else
            light.GetComponent<Renderer>().material = savedMat;
    }


	// Cause damage if collided with
	void OnTriggerEnter (Collider col)
	{
        if(!client && !isDestroyed)
        {
            if(col.gameObject.tag.Equals ("Player"))
            {
                // Resources are now collected through the separate OutpostResources object
                // If the player crashes into the outpost, the outpost is destroyed and the player takes massive damage
                Debug.Log("Outpost destroyed");

                outpostLogic.PlayerCollision(PLAYER_COLLISION_DAMAGE);
                
                GameObject explosion = Instantiate(Resources.Load("Prefabs/OutpostExplode", typeof(GameObject))) as GameObject;
                explosion.transform.position = col.transform.position;
                explosion.SetActive(true);
                ServerManager.NetworkSpawn(explosion);

                isDestroyed = true;
                Destroy(this.gameObject);
            }
            else if(col.gameObject.tag.Equals ("EnemyShip"))
            {
                EnemyLogic enemyLogic = col.gameObject.GetComponentInChildren<EnemyLogic>();
                if(enemyLogic != null)
                    enemyLogic.collision(1000, -1);
            }
        }
	}
}
