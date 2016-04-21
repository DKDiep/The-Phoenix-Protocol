/*
    Damage the player upon collision with an enemy
*/

using UnityEngine;
using System.Collections;

public class EnemyCollision : MonoBehaviour 
{
    public float collisionDamage;
    private EnemyLogic myLogic;
    private ShipMovement shipMovement;

    void OnTriggerEnter (Collider col)
    {
        myLogic = GetComponentInChildren<EnemyLogic>();

    	if(col.gameObject.tag.Equals ("Player"))
    	{
			if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				GameObject hitObject = col.gameObject;
				if (shipMovement == null)
				{
					try
					{
						shipMovement = hitObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>();
					}
					catch(System.NullReferenceException e)
					{
						Debug.LogError("Collision exception on " + hitObject.name);
					}
				}

				if (shipMovement != null)
					shipMovement.collision(collisionDamage, 0f, hitObject.name.GetComponentType());
			}
            
			if(myLogic != null)
            {
                myLogic.collision(1000f, -1);
                if(myLogic.type == EnemyType.LightningBug)
                {
                    shipMovement.LightningBugEffect();
                }
            }
                
    	}
        else if(col.gameObject.tag.Equals ("Debris"))
        {
			AsteroidLogic asteroidLogic = col.gameObject.GetComponentInChildren<AsteroidLogic>();
			if(asteroidLogic != null )
				asteroidLogic.collision(1000f);
            
			if(myLogic != null)
				myLogic.collision(collisionDamage, -1);
        }
        else if(col.gameObject.tag.Equals ("SmartBomb"))
        {
            if(myLogic != null)
                myLogic.collision(1000f, -1);
        }
        else if(col.gameObject.tag.Equals ("EMP"))
        {
            if(myLogic != null)
                StartCoroutine(myLogic.EMPEffect());
        }
    }
}
