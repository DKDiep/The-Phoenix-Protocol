/*
    Client side bullet movement
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BulletMove : NetworkBehaviour, IDestructionListener
{
	private GameObject target = null;
    [SerializeField] float forceSpeed;

	public float Speed { get; set; }


    public void ForceRotation(Vector3 lookPos)
    {
        RpcForceRotation(lookPos);
    }

    [ClientRpc]
    void RpcForceRotation(Vector3 lookPos)
    {
        transform.LookAt(lookPos);
    }

	void Start()
	{
		if(forceSpeed != 0)
            Speed = forceSpeed;
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
