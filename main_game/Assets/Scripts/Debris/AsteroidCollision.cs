/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Handles the detection of collisions between an asteroid and another object
*/

using UnityEngine;
using System.Collections;

public class AsteroidCollision : MonoBehaviour 
{
	float collisionDamage = 10f;
    AsteroidLogic myLogic;

    void Start()
    {
        myLogic = GetComponentInChildren<AsteroidLogic>();
    }

    // Cause damage if collided with
	void OnTriggerEnter (Collider col)
	{
		if(col.gameObject.tag.Equals ("Player"))
		{
			col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(collisionDamage, 0f);
            myLogic.collision(1000f);
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
			col.gameObject.GetComponentInChildren<EnemyLogic>().collision(collisionDamage, -1);
            myLogic.collision(1000f);
		}
	}
}
