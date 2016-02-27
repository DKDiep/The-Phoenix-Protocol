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
    private EnemyLogic myLogic;

    void Start()
    {
        if(GetComponentInChildren<EnemyLogic>() != null)
			myLogic = GetComponentInChildren<EnemyLogic>();
    }

    void OnTriggerEnter (Collider col)
    {
    	if(col.gameObject.tag.Equals ("Player"))
    	{
            GameObject hitObject = col.gameObject;
			ShipMovement movementObject = hitObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>();
            if(movementObject != null)
                movementObject.collision(collisionDamage, 0f, hitObject.name.GetComponentType());
            
			if(myLogic != null) myLogic.collision(1000f, -1);
    	}
        else if(col.gameObject.tag.Equals ("Debris"))
        {
			AsteroidLogic asteroidLogic = col.gameObject.GetComponentInChildren<AsteroidLogic>();
			if(asteroidLogic != null )
				asteroidLogic.collision(1000f);
            
			if(myLogic != null)
				myLogic.collision(collisionDamage, -1);
        }
    }
}
