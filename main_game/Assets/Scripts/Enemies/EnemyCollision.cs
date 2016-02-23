/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Damage the player upon collision with an enemy
*/

using UnityEngine;
using System.Collections;

public class EnemyCollision : MonoBehaviour 
{

    public float collisionDamage;
    EnemyLogic myLogic;

    void Start()
    {
        myLogic = GetComponentInChildren<EnemyLogic>();
    }

    void OnTriggerEnter (Collider col)
    {
    	if(col.gameObject.tag.Equals ("Player"))
    	{
    		col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(collisionDamage, 0f);
            myLogic.collision(1000f, -1);
    	}
        else if(col.gameObject.tag.Equals ("Debris"))
        {
            if(col.gameObject.GetComponentInChildren<AsteroidLogic>() != null ) col.gameObject.GetComponentInChildren<AsteroidLogic>().collision(1000f);
            myLogic.collision(collisionDamage, -1);
        }
    }
}
