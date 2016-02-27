using UnityEngine;
using System.Collections;

public class OutpostCollision : MonoBehaviour 
{
	OutpostLogic outpostLogic;

	private const int PLAYER_COLLISION_DAMAGE = 50; // This is currently half of the ship's health

	void Start()
	{
		outpostLogic = GetComponentInChildren<OutpostLogic>();
	}


	// Cause damage if collided with
	void OnTriggerEnter (Collider col)
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
            if(col.gameObject.GetComponentInChildren<EnemyLogic>() != null) col.gameObject.GetComponentInChildren<EnemyLogic>().collision(1000, -1);
		}
	}
}
