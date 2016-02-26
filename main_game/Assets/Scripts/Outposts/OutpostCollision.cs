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
			Destroy(this.gameObject); // TODO: Does this work fine with the network?
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
			// If an enemy crashes into an outpost, nothing happens to the outpost
		}
	}
}
