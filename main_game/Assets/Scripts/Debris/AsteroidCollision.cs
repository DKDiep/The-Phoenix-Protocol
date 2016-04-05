/*
    Handles the detection of collisions between an asteroid and another object
*/

using UnityEngine;
using System.Collections;

public class AsteroidCollision : MonoBehaviour 
{
	private float collisionDamage;
    private AsteroidLogic myLogic;

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
		if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			GameObject hitObject        = col.gameObject;
			ShipMovement movementScript = hitObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>();

			if (movementScript != null)
				movementScript.collision(collisionDamage, 0f, hitObject.name.GetComponentType());
            if(myLogic != null)
				myLogic.collision(1000f);
		}
		else if(col.gameObject.tag.Equals ("EnemyShip"))
		{
			EnemyLogic logicScript = col.gameObject.GetComponentInChildren<EnemyLogic>();
			if(logicScript != null)
				logicScript.collision(collisionDamage, -1);
            if(myLogic != null)
				myLogic.collision(1000f);
		}
        else if(col.gameObject.tag.Equals ("SmartBomb"))
        {
            if(myLogic != null)
                myLogic.collision(1000f);
        }
	}
}
