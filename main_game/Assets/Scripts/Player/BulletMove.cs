/*
    Client side bullet movement
*/

using UnityEngine;
using System.Collections;

public class BulletMove : MonoBehaviour, IDestructionListener
{
	private GameObject target = null;

	public float Speed { get; set; }

	void Start()
	{
		
	}

	void Update () 
	{
		if (target != null)
			transform.LookAt(target.transform);
		transform.position += transform.forward * Time.deltaTime * Speed;
	}

	/// <summary>
	/// Sets a target that this bullet will follow.
	/// </summary>
	/// <param name="targetObject">The target object.</param>
	public void SetTarget(GameObject targetObject)
	{
		target = targetObject;

		// Register to receive a notification when the object is destroyed
		if (target != null)
		{
			// The target could be an enemy...
			EnemyLogic targetEnemyLogic = target.GetComponentInChildren<EnemyLogic>();
			if (targetEnemyLogic != null)
				targetEnemyLogic.RegisterDestructionListener(this);
			else
			{
				// ... or an asteroid
				AsteroidLogic targetAsteroidLogic = target.GetComponentInChildren<AsteroidLogic>();
				if (targetAsteroidLogic != null)
					targetAsteroidLogic.RegisterDestructionListener(this);
			}
		}
	}

	/// <summary>
	/// Receives a notification that an object has been destroyed.
	/// </summary>
	/// <param name="destructed">The destructed object.</param>
	public void OnObjectDestructed(GameObject destructed)
	{
		if (destructed == target)
			target = null;
	}
}
