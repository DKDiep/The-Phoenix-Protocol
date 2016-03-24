/*
    Client side bullet movement
*/

using UnityEngine;
using System.Collections;

public class BulletMove : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float speed;

	private GameObject target = null;

	void Start()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();
	}

	private void LoadSettings()
	{
		speed = settings.BulletSpeed;
	}

	void Update () 
	{
		if (target != null)
			transform.LookAt(target.transform);
		transform.position += transform.forward * Time.deltaTime * speed;
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
