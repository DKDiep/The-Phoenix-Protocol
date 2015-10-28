using UnityEngine;
using System.Collections;

public class EnemyLogic : MonoBehaviour 
{

	[SerializeField] float speed = 10f;
	[SerializeField] float health;
	[SerializeField] float maxRange; // Must be at least this close to shoot at player
	[SerializeField] float shotsPerSec = 1f;
	[SerializeField] int shootChance; // Every .1 seconds a random int between 1 and this value is selected. Increase to reduce
									  // likelihood of firing and vice versa
	[SerializeField] float shootPeriod; // How long in seconds the enemy should shoot for when it fires
	[SerializeField] int percentageVariation; // Percentage variation +/- in the length of the shooting period
	[SerializeField] float maxShield; // Max recharging shield level. Set to 0 to disable shields
	[SerializeField] float shieldDelay; // Delay in seconds to wait before recharging shield
	[SerializeField] float shieldRechargeRate; // Units of shield to increase per second
	[SerializeField] bool isSuicidal; // Attempt to crash into player?
	[SerializeField] GameObject bullet;

	GameObject player;
	bool shoot = false;
	bool rechargeShield;
	float shield;
	float lastShieldCheck; // Temp variable allows us to see whether I've taken damage since last checking
	
	// This function is run when the object is spawned
	public void SetPlayer(GameObject temp)
	{
		player = temp;
		if(maxShield > 0)
		{
			shield = maxShield;
			lastShieldCheck = shield;
			StartCoroutine ("Recharge Shields");
		}
		
		StartCoroutine ("DestroyZ");
		StartCoroutine ("ShootManager");
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
	
	IEnumerator ShootManager()
	{
		if(!shoot)
		{
			yield return new WaitForSeconds(0.1f);
			if(Random.Range (1, shootChance) == 1)
			{
				shoot = true;
				StartCoroutine ("Shoot");
				StartCoroutine ("ShootManager");
			}
		}
		else
		{
			yield return new WaitForSeconds(shootPeriod * (Random.Range (100-percentageVariation, 100+percentageVariation) / 100f));
			shoot = false;
			StartCoroutine ("ShootManager");
		}
	}
	
	IEnumerator Shoot()
	{
		yield return new WaitForSeconds((1f/ shotsPerSec) + Random.Range (0.01f, 0.1f/shotsPerSec));
		GameObject temp = Instantiate (bullet, transform.position, Quaternion.identity) as GameObject;
		temp.GetComponent<BulletLogic>().SetPlayer (player);
		if(shoot) StartCoroutine ("Shoot");
	}
	
	IEnumerator RechargeShields()
	{
		if(lastShieldCheck == shield)
		{
			shield += shieldRechargeRate / 10f;
			lastShieldCheck = shield;
			yield return new WaitForSeconds(0.1f);
			StartCoroutine ("RechargeShields");
		}
		else
		{
			yield return new WaitForSeconds(shieldDelay);
			lastShieldCheck = shield;
			StartCoroutine ("RechargeShields");
		}
	}
	
	// If I hit something, check what it is and react accordingly
	void OnTriggerEnter (Collider col)
	{
		if(col.gameObject.tag == "Debris")
		{
			Debug.Log ("A Glom Fighter hit some debris");
		}
		else if(col.gameObject.tag == "PlayerBullet")
		{
			Debug.Log ("Glom Fighter was shot by the player");
		}
		if(health == 0) StartCoroutine ("DestroyEnemy"); // Initiate Destroy sequence
	}
	
	IEnumerator DestroyEnemy()
	{
		yield return new WaitForSeconds(0.1f);
		Destroy (this.gameObject);
	}
}
