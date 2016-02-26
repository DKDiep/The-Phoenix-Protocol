using UnityEngine;
using System.Collections;

public class OutpostCollision : MonoBehaviour 
{
	OutpostLogic outpostLogic;

	void Start()
	{
		outpostLogic = GetComponentInChildren<OutpostLogic>();
	}


	// Cause damage if collided with
	void OnTriggerEnter (Collider col)
	{
		// TODO: this should damage the player and outpost.
		if(col.gameObject.tag.Equals ("Player"))
		{
			// Resources are now collected through the separate OutpostResources object
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
			// What happens if a enemy ship collides with an outpost?
		}
	}
}
