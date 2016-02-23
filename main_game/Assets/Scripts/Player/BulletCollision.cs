/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Detect bullet collisions
*/

using UnityEngine;
using System.Collections;

public class BulletCollision : MonoBehaviour 
{
	// The id of the player who shot the bullet. 
	public int playerId = -1;

	void OnTriggerEnter (Collider col)
	{
        if(GetComponentInChildren<BulletLogic>() != null) GetComponentInChildren<BulletLogic>().collision (col, playerId);
	}
}
