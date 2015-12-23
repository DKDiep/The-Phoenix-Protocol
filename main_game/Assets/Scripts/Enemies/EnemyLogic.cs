﻿using UnityEngine;
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
	[SerializeField] Texture2D target;

	public GameObject player;
	bool shoot = false;
	bool rechargeShield;
	float shield, distance;
	float lastShieldCheck; // Temp variable allows us to see whether I've taken damage since last checking
	bool draw = false;

	int state; // 0 = fly towards player, 1 = avoid object, 2 = cooldown
	GameObject shootAnchor;

	Vector3 prevPos, currentPos;
	Renderer myRender;

    private GameObject controlObject;

    public void SetControlObject(GameObject newControlObject)
    {
        controlObject = newControlObject;
        transform.parent.gameObject.GetComponent<EnemyCollision>().collisionDamage = collisionDamage;
    }

    void OnGUI()
    {
		Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.parent.position);
		screenPos = GUIUtility.ScreenToGUIPoint(screenPos);
		float size = Mathf.Clamp(128f / (distance / 100f),0,128);

		if(distance < 750 && draw && myRender.isVisible) GUI.DrawTexture(new Rect(screenPos.x - (size/2), Screen.height - screenPos.y - (size/2), size, size), target, ScaleMode.ScaleToFit, true, 0);
    }

    // This function is run when the object is spawned
    public void SetPlayer(GameObject temp)
	{
		player = temp;
		myRender = transform.parent.gameObject.GetComponent<Renderer>();
		if(maxShield > 0)
		{
			shield = maxShield;
			lastShieldCheck = shield;
			StartCoroutine ("Recharge Shields");
		}

		foreach(Transform child in this.transform.parent)
		{
			if(child.gameObject.name.Equals("ShootAnchor")) shootAnchor = child.gameObject;
		}
		
		StartCoroutine ("ShootManager");
		StartCoroutine("DrawDelay");
	}

	IEnumerator DrawDelay()
	{
		yield return new WaitForSeconds(1f);
		draw = true;
	}
	
	void Update () 
	{
		controlObject.transform.Translate (controlObject.transform.forward*Time.deltaTime * speed);
		prevPos = currentPos;
		currentPos = player.transform.position;
		distance = Vector3.Distance(transform.position, player.transform.position);


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
			}
			StartCoroutine ("ShootManager");

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
		GameObject obj = Instantiate (bullet, shootAnchor.transform.position, Quaternion.identity) as GameObject;
		GameObject logic = Instantiate (bulletLogic, shootAnchor.transform.position, Quaternion.identity) as GameObject;
		logic.transform.parent = obj.transform;
		logic.transform.localPosition = Vector3.zero;

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
			lastShieldCheck = shield;
			yield return new WaitForSeconds(shieldDelay);
			StartCoroutine ("RechargeShields");
		}
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
