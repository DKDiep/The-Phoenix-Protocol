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
	[SerializeField] float collisionDamage;
	[SerializeField] float shootPeriod; // How long in seconds the enemy should shoot for when it fires
	[SerializeField] int percentageVariation; // Percentage variation +/- in the length of the shooting period
	[SerializeField] float maxShield; // Max recharging shield level. Set to 0 to disable shields
	[SerializeField] float shieldDelay; // Delay in seconds to wait before recharging shield
	[SerializeField] float shieldRechargeRate; // Units of shield to increase per second
	[SerializeField] bool isSuicidal; // Attempt to crash into player?
	[SerializeField] GameObject bullet;
	[SerializeField] GameObject bulletLogic;

	public GameObject player;
	bool shoot = false;
	bool rechargeShield;
	float shield;
	float lastShieldCheck; // Temp variable allows us to see whether I've taken damage since last checking

	Vector3 prevPos, currentPos;

    private GameObject controlObject;

    public void SetControlObject(GameObject newControlObject)
    {
        controlObject = newControlObject;
        transform.parent.gameObject.GetComponent<EnemyCollision>().collisionDamage = collisionDamage;
    }

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
		
		StartCoroutine ("ShootManager");
	}
	
	void Update () 
	{
		controlObject.transform.Translate (transform.forward*Time.deltaTime * speed);
		prevPos = currentPos;
		currentPos = player.transform.position;
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
		GameObject obj = Instantiate (bullet, transform.position, Quaternion.identity) as GameObject;
		GameObject logic = Instantiate (bulletLogic, transform.position, Quaternion.identity) as GameObject;
		logic.transform.parent = obj.transform;
		float distance = Vector3.Distance(transform.position, player.transform.position);

		Vector3 destination = player.transform.position + ((currentPos - prevPos) * (distance / 10f));

		logic.GetComponent<BulletLogic>().SetDestination (destination);
		ServerManager.NetworkSpawn(obj);
		
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
	
	IEnumerator DestroyEnemy()
	{
		yield return new WaitForSeconds(0.1f);
		Destroy (this.gameObject);
	}
	
	public void collision(float damage)
	{
		if (shield > damage)
		{
			shield -= damage;
		}
		else if (shield > 0)
		{
			float remDamage = damage - shield;
			shield = 0;
			
			health -= remDamage;
		}
		else if(health > damage)
		{
			health -= damage;
		}
		else
		{
			Destroy(transform.parent.gameObject);
		}
		//Debug.Log ("Glom fighter was hit, has " + shield + " shield and " + health + " health");
	}

	
	
}
