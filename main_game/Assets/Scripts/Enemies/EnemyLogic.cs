using UnityEngine;
using System.Collections;

public class EnemyLogic : MonoBehaviour 
{

	[SerializeField] float speed = 10f;
	[SerializeField] float shotsPerSec = 1f;
	[SerializeField] GameObject bullet;
	GameObject player;

	void Start () 
	{
		player = GameObject.Find ("PlayerShip");
		StartCoroutine ("DestroyZ");
		StartCoroutine ("Shoot");
	}
	
	void Update () 
	{
		transform.Translate (transform.forward*Time.deltaTime * speed);
	}
	
	// Automatically destroy if 100 units behind player
	IEnumerator DestroyZ()
	{
		yield return new WaitForSeconds(1f);
		if(transform.position.z < player.transform.position.z - 100f)
		{
			EnemySpawner.numEnemies -= 1;
			Destroy (this.gameObject);
		}
		StartCoroutine ("DestroyZ");
	}
	
	IEnumerator Shoot()
	{
		yield return new WaitForSeconds(1f/ shotsPerSec);
		Instantiate (bullet, transform.position, Quaternion.identity);
		StartCoroutine ("Shoot");
	}
}
