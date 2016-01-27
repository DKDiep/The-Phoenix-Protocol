/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Detect bullet collisions
*/

using UnityEngine;
using System.Collections;

public class BulletCollision : MonoBehaviour 
{
	void OnTriggerEnter (Collider col)
	{
		GetComponentInChildren<BulletLogic>().collision (col);
	}
}
