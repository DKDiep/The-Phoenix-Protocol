/*
    Client side bullet movement
*/

using UnityEngine;
using System.Collections;

public class BulletMove : MonoBehaviour 
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
	}
}
