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
            myLogic.collision(1000f);
    	}
        else if(col.gameObject.tag.Equals ("Debris"))
        {
            col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(collisionDamage, 0f);
            myLogic.collision(collisionDamage);
        }
    }
}
