﻿/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Handles the detection of collisions between an asteroid and another object
*/

using UnityEngine;
using System.Collections;

public class AsteroidCollision : MonoBehaviour 
{
	float collisionDamage;
    AsteroidLogic myLogic;

    void Start()
    {
        if(GetComponentInChildren<AsteroidLogic>() != null ) myLogic = GetComponentInChildren<AsteroidLogic>();
    }

    public void SetCollisionDamage(float dmg)
    {
        collisionDamage = dmg;
    }

    // Cause damage if collided with
	void OnTriggerEnter (Collider col)
	{
		if(col.gameObject.tag.Equals ("Player"))
		{
			col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(collisionDamage, 0f);
            if(myLogic != null) myLogic.collision(1000f);
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
            if(col.gameObject.GetComponentInChildren<EnemyLogic>() != null) col.gameObject.GetComponentInChildren<EnemyLogic>().collision(collisionDamage, -1);
            if(myLogic != null)myLogic.collision(1000f);
		}
	}
}
