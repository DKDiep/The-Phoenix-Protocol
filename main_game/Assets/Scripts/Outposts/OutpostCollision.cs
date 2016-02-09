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
		if(col.gameObject.tag.Equals ("Player"))
		{
			outpostLogic.collision();
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
			// What happens if a enemy ship collides with an outpost?
		}
	}
}
