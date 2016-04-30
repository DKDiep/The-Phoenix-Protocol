/*
    Damage the player upon collision with an enemy
*/

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class EnemyCollision : MonoBehaviour 
{
    public float collisionDamage;
    private EnemyLogic myLogic;
    private ShipMovement shipMovement;

	private readonly Regex turretRegex = new Regex("Turret[012][LR]");

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
					if (turretRegex.IsMatch(hitObject.name))
						shipMovement = hitObject.transform.parent.GetComponentInChildren<ShipMovement>();
					else
						shipMovement = hitObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>();
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
