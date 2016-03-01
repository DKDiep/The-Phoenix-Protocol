/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Client side bullet movement
*/

using UnityEngine;
using System.Collections;

public class BulletMove : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float speed;

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
		transform.position += transform.forward * Time.deltaTime * speed;
	}
}
