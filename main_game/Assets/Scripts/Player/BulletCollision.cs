using UnityEngine;
using System.Collections;

public class BulletCollision : MonoBehaviour 
{
	void OnTriggerEnter (Collider col)
	{
		GetComponentInChildren<BulletLogic>().collision (col);
	}
}
