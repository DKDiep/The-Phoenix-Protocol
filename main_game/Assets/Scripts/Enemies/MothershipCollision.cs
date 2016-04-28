/*
    Damage the player upon collision with an enemy
*/

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class MothershipCollision : MonoBehaviour 
{
    private MothershipLogic myLogic;
    private ShipMovement shipMovement;

    private readonly Regex turretRegex = new Regex("Turret[012][LR]");

    void OnTriggerEnter (Collider col)
    {
        myLogic = GetComponentInChildren<MothershipLogic>();

		if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
    	{
            GameObject hitObject = col.gameObject;
            if(shipMovement == null)
            {
                if (turretRegex.IsMatch(hitObject.name))
                        shipMovement = hitObject.transform.parent.GetComponentInChildren<ShipMovement>();
                else
                        shipMovement = hitObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>();
            }

            if (shipMovement != null)
                shipMovement.collision(float.MaxValue, 0f, hitObject.name.GetComponentType());
                
    	}
        else if(col.gameObject.tag.Equals ("Debris"))
        {
			AsteroidLogic asteroidLogic = col.gameObject.GetComponentInChildren<AsteroidLogic>();
			if(asteroidLogic != null )
				asteroidLogic.collision(1000f);
        }
		else if (col.gameObject.CompareTag("EnemyShip"))
		{
			EnemyLogic enemyLogic = col.gameObject.GetComponentInChildren<EnemyLogic>();
			if (enemyLogic != null)
				enemyLogic.collision(1000f, -1);
		}
    }
}
