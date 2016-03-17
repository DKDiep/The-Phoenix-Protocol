using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OutpostCollision : NetworkBehaviour 
{
	private OutpostLogic outpostLogic;

	private const int PLAYER_COLLISION_DAMAGE = 50; // This is currently half of the ship's health

    [SerializeField] Material helpMat;
    [SerializeField] Material savedMat;
    bool client = false;

	void Start()
	{
		outpostLogic = GetComponentInChildren<OutpostLogic>();
        if(outpostLogic == null)
            client = true;
	}


    public void SwitchMaterial()
    {
        RpcSwitchMaterial();
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

        light.GetComponent<Renderer>().material = savedMat;
    }

    [ClientRpc]
    private void RpcSwitchMaterial()
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

        light.GetComponent<Renderer>().material = savedMat;
    }


	// Cause damage if collided with
	void OnTriggerEnter (Collider col)
	{
        if(!client)
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
