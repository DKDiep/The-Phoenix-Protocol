/*
    Handles bullet properties and destruction
*/

using UnityEngine;
using System.Collections;

public class BulletLogic : MonoBehaviour 
{
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
    private bool playerShooting;

	private ObjectPoolManager bulletManager;
	private ObjectPoolManager logicManager;
	private ObjectPoolManager impactManager;
    private ShipMovement shipMovement;

    void Update()
    {
        if(enableSound && !playerShooting)
        {
            distance = Vector3.Distance(transform.position, destination);
            if(distance < 150)
            {
                mySrc.PlayOneShot(sound);
                enableSound = false;
            }
        }
    }

    public void SetParameters(float t_accuracy, float t_damage)
    {
        accuracy = t_accuracy;
        damage = t_damage;
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
        playerShooting = isPlayer;
      
		if(isPlayer) 
			obj.GetComponent<BulletCollision>().playerId = playerId;
        else 
        {
            enableSound = true;
            mySrc = GetComponent<AudioSource>();
		}

		//obj.transform.localScale = new Vector3(xScale, yScale, zScale);
		obj.transform.LookAt (destination);
		obj.transform.Rotate (Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy));

		StartCoroutine(DestroyObject());
	}

	public void SetID(PlayerShooting playerObj, int id)
	{
		playerId = id;
		player = playerObj;
	}

    // Control logic when a collision is detected
	public void collision(Collider col, int bulletPlayerId)
	{
		string hitObjectTag = col.gameObject.tag;

		// Despawn the bulllet
		Despawn();

		// If it's a player bullet, show a hit marker
		if(playerShooting) 
            player.HitMarker();

		// Apply the collision logic
		if (hitObjectTag.Equals("Debris"))
			col.gameObject.GetComponentInChildren<AsteroidLogic>().collision(damage);
		else if (hitObjectTag.Equals("EnemyShip"))
		{
			EnemyLogic logic = col.gameObject.GetComponentInChildren<EnemyLogic>();
			if (logic != null)
				logic.collision(damage, bulletPlayerId);
		}
		else if (hitObjectTag.Equals("Player"))
		{
            if(shipMovement == null)
                shipMovement = playerObj.GetComponentInChildren<ShipMovement>();
			shipMovement.collision(damage, transform.eulerAngles.y, col.gameObject.name.GetComponentType());
		}
        else if (hitObjectTag.Equals("GlomMothership"))
            col.gameObject.GetComponentInChildren<MothershipLogic>().collision(damage, bulletPlayerId);

        // If in range of the player, show an impact effect
		if(Vector3.Distance(transform.position, playerObj.transform.position) < 200)
        {
            GameObject impactTemp = impactManager.RequestObject();
            impactTemp.transform.position = col.transform.position;
            impactManager.EnableClientObject(impactTemp.name, impactTemp.transform.position, impactTemp.transform.rotation, impactTemp.transform.localScale);
        }
    }

    // Automatically destroy bullet after 4 seconds
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(4f);

		Despawn();
    }

	/// <summary>
	/// Despawns this bullet.
	/// </summary>
	private void Despawn()
	{
        //player = null;
		RemoveFollowTarget();
		bulletManager.DisableClientObject(obj.name);
		bulletManager.RemoveObject(obj.name);
		logicManager.RemoveObject(gameObject.name);
	}

	/// <summary>
	/// Removes the target this bullet is following, if any .
	/// 
	/// This should be called whenever the bullet is destroyed. If it is not, and object pooling later gives
	/// this object to a shot not aimed at an enemy, the old target will be used.
	/// </summary>
	private void RemoveFollowTarget()
	{
		obj.GetComponent<BulletMove>().SetTarget(null);
	}
}
