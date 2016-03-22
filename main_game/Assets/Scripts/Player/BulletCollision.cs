/*
    Detect bullet collisions
*/

using UnityEngine;
using System.Collections;

public class BulletCollision : MonoBehaviour 
{
	// The id of the player who shot the bullet. 
	public int playerId = -1;

	void OnTriggerEnter (Collider col)
	{
		BulletLogic logicScript = GetComponentInChildren<BulletLogic>();
		if(logicScript != null)
			logicScript.collision (col, playerId);
	}
}
