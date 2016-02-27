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
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private float speed;
	#pragma warning restore 0649

	void Update () 
	{
		transform.position += transform.forward * Time.deltaTime * speed;
	}
}
