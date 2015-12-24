﻿using UnityEngine;
using System.Collections;

public class AsteroidCollision : MonoBehaviour {

	float collisionDamage = 10f;

	void OnTriggerEnter (Collider col)
	{
		if(col.gameObject.tag.Equals ("Player"))
		{
			col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(collisionDamage, 0f);
			Destroy (this.gameObject);
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
			col.gameObject.GetComponentInChildren<EnemyLogic>().collision(collisionDamage);
			Destroy (this.gameObject);
		}
	}
}