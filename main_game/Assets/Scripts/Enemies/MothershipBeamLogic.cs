using UnityEngine;
using System.Collections;

public class MothershipBeamLogic : MonoBehaviour {

    public GameObject player;
    private ShipMovement shipMovement;


    public void collision(Collider col)
    {
        string hitObjectTag = col.gameObject.tag;
        // Apply the collision logic
        if (hitObjectTag.Equals("Debris"))
            col.gameObject.GetComponentInChildren<AsteroidLogic>().collision(1000);
        else if (hitObjectTag.Equals("EnemyShip"))
        {
            EnemyLogic logic = col.gameObject.GetComponentInChildren<EnemyLogic>();
            if (logic != null)
                logic.collision(1000, -1);
        }
        else if (hitObjectTag.Equals("Player"))
        {
            if(shipMovement == null)
                shipMovement = player.GetComponentInChildren<ShipMovement>();
            shipMovement.collision(10f, transform.eulerAngles.y, col.gameObject.name.GetComponentType());
        }

        transform.parent.GetComponent<Collider>().enabled = false;
    }



}
