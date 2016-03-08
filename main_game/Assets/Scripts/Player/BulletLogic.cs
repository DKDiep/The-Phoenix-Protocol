/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Handles bullet properties and destruction
*/

using UnityEngine;
using System.Collections;

public class BulletLogic : MonoBehaviour 
{
	public float speed; // Bullet speed

	// These cannot be easily moved to GameSettings because they are set to different values based on the object they are attached to
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	private float accuracy; // 0 = perfectly accurate, 1 = very inaccurate
	private float damage; 
	[SerializeField] private Color bulletColor;
	[SerializeField] private GameObject impact;
	[SerializeField] private float xScale;
	[SerializeField] private float yScale;
	[SerializeField] private float zScale;
	[SerializeField] private AudioClip sound;
	#pragma warning restore 0649

	private GameObject obj;
	private GameObject playerObj;
	private Vector3 destination;
	private PlayerShooting player;
	private bool enableSound = false;
	private int playerId;
	private float distance;
	private AudioSource mySrc;

	private ObjectPoolManager bulletManager;
	private ObjectPoolManager logicManager;
	private ObjectPoolManager impactManager;

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

    public void SetParameters(float t_accuracy, float t_damage, float t_speed)
    {
        accuracy = t_accuracy;
        damage = t_damage;
        speed = t_speed;
    }

    // Initialise object when spawned
	public void SetDestination(Vector3 dest, bool isPlayer, GameObject playerObj2, ObjectPoolManager cachedBullet, ObjectPoolManager cachedLogic, ObjectPoolManager cachedImpact)
	{
        bulletManager = cachedBullet;
        logicManager  = cachedLogic;
        impactManager = cachedImpact;
        destination   = dest;
        playerObj     = playerObj2;
		obj           = transform.parent.gameObject;
      
		if(isPlayer) 
			obj.GetComponent<BulletCollision>().playerId = playerId;
        else 
        {
            enableSound = true;
            mySrc = GetComponent<AudioSource>();
		}

		obj.transform.localScale = new Vector3(xScale, yScale, zScale);
		obj.transform.LookAt (destination);
		obj.transform.Rotate (Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy));

		StartCoroutine ("DestroyObject");
	}

	public void SetID(PlayerShooting playerObj, int id)
	{
		playerId = id;
		player = playerObj;
	}

    // Control logic when a collision is detected
	public void collision(Collider col, int bulletPlayerId)
	{
		if(player != null && !col.CompareTag("Resources")) 
            player.HitMarker();

		string hitObjectTag = col.gameObject.tag;
		if (hitObjectTag.Equals("Debris"))
			col.gameObject.GetComponentInChildren<AsteroidLogic>().collision(damage);
		else if (hitObjectTag.Equals("EnemyShip"))
			col.gameObject.GetComponentInChildren<EnemyLogic>().collision(damage, bulletPlayerId);
		else if (hitObjectTag.Equals("Player"))
		{
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
