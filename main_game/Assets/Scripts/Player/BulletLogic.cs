﻿/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Handles bullet properties and destruction
*/

using UnityEngine;
using System.Collections;

public class BulletLogic : MonoBehaviour 
{
	public float speed = 100f; // Bullet speed
	[SerializeField] float accuracy; // 0 = perfectly accurate, 1 = very inaccurate
	[SerializeField] float damage; 
	[SerializeField] Color bulletColor;
    [SerializeField] GameObject impact;
	[SerializeField] float xScale;
	[SerializeField] float yScale;
	[SerializeField] float zScale;

	GameObject obj;
	GameObject playerObj;
    Vector3 destination;
	PlayerShooting player;
    bool enableSound = false;
	int playerId;
    float distance;
    AudioSource mySrc;
    [SerializeField] AudioClip sound;

    ObjectPoolManager bulletManager;
    ObjectPoolManager logicManager;
    ObjectPoolManager impactManager;

    // Initialise object when spawned
	public void SetDestination(Vector3 dest, bool isPlayer, GameObject playerObj2, ObjectPoolManager cachedBullet, ObjectPoolManager cachedLogic, ObjectPoolManager cachedImpact)
	{
        bulletManager = cachedBullet;
        logicManager = cachedLogic;
        impactManager = cachedImpact;
        destination = dest;
        playerObj = playerObj2;
		obj = transform.parent.gameObject;
      
		if(isPlayer) 
		{
			obj.GetComponent<BulletCollision>().playerId = playerId;
		} 
        else 
        {
            enableSound = true;
            mySrc = GetComponent<AudioSource>();
		}

		obj.transform.localScale = new Vector3(xScale, yScale, zScale);
		obj.transform.LookAt (destination);
		obj.transform.Rotate (Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy));
    	//obj.GetComponent<BulletMove>().SetColor(bulletColor);
		StartCoroutine ("DestroyObject");
	}

    void Update()
    {
        if(enableSound)
        {
            distance = Vector3.Distance(transform.position, destination);
            if(distance < 150)
            {
                mySrc.PlayOneShot(sound);
                enableSound = false;
            }
        }
    }

	public void SetID(PlayerShooting playerObj, int id)
	{
		playerId = id;
		player = playerObj;
	}

    // Control logic when a collision is detected
	public void collision(Collider col, int bulletPlayerId)
	{
		if(player != null) 
		{
            player.HitMarker();
		}

		string hitObjectTag = col.gameObject.tag;
		if (hitObjectTag.Equals("Debris"))
		{
			col.gameObject.GetComponentInChildren<AsteroidLogic>().collision(damage);
		}
		else if (hitObjectTag.Equals("EnemyShip"))
		{
			//Debug.Log ("A bullet has hit an enemy");
			col.gameObject.GetComponentInChildren<EnemyLogic>().collision(damage, bulletPlayerId);
		}
		else if (hitObjectTag.Equals("Player"))
		{
			//Debug.Log ("A bullet has hit the player");
			GameObject hitObject = col.gameObject;
			hitObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>()
                .collision(damage, transform.eulerAngles.y, hitObject.name.GetComponentType());
		}
		else if (hitObjectTag.Equals("Resources"))
			return;

        if(Vector3.Distance(transform.position, playerObj.transform.position) < 200)
        {
            GameObject impactTemp = impactManager.RequestObject();
            impactTemp.transform.position = col.transform.position;
            impactManager.EnableClientObject(impactTemp.name, impactTemp.transform.position, impactTemp.transform.rotation, impactTemp.transform.localScale);
        }
        bulletManager.DisableClientObject(gameObject.name);
        bulletManager.RemoveObject(gameObject.name);
        logicManager.RemoveObject(gameObject.name);
    }

    // Automatically destroy bullet after 4 seconds
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(4f);
        bulletManager.DisableClientObject(gameObject.name);
        bulletManager.RemoveObject(gameObject.name);
        logicManager.RemoveObject(gameObject.name);
    }
}
