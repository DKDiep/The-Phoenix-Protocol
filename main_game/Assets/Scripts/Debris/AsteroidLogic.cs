using UnityEngine;
using System.Collections;

public class AsteroidLogic : MonoBehaviour 
{
	GameObject player;

	void Start () 
	{
		player = GameObject.Find ("PlayerShip");
		StartCoroutine ("DestroyZ");
	}
	
	// Automatically destroy if 100 units behind the player
	IEnumerator DestroyZ()
	{
		yield return new WaitForSeconds(1f);
		if(transform.position.z < player.transform.position.z - 100f)
		{
			AsteroidSpawner.numAsteroids -= 1;
			Destroy (this.gameObject);
		}
		StartCoroutine ("DestroyZ");
	}
}
