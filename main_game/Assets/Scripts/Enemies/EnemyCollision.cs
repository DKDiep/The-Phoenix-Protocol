using UnityEngine;
using System.Collections;

public class EnemyCollision : MonoBehaviour 
{

	public float collisionDamage;

	void OnTriggerEnter (Collider col)
	{
		if(col.gameObject.tag.Equals ("Player"))
		{
			col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(collisionDamage, 0f);
			Destroy (this.gameObject);
		}
	}
}
