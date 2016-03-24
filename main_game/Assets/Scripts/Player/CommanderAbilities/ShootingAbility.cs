using UnityEngine;
using System.Collections;

public class ShootingAbility : CommanderAbility {

    private GameObject shootAnchor;
    private ObjectPoolManager bulletManager;
    private ObjectPoolManager logicManager;
    private ObjectPoolManager impactManager;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] GameObject enemyFinderObject;
	#pragma warning restore 0649

	// Use this for initialization
	private void Awake () 
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        state = GameObject.Find("GameManager").GetComponent<GameState>();
        cooldown = settings.shootCooldown;
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            UseAbility();
        }
	}

    private IEnumerator FireMissiles(EnemyFinder enemyFinder)
    {
        yield return new WaitForSeconds(0.1f);
        if(enemyFinder.searchCompleted)
        {
            for(int i = 0; i < settings.projectileCount; i++)
            {
                if(enemyFinder.enemyList[i] != null)
                {
                    GameObject obj = bulletManager.RequestObject();
                    obj.transform.position = shootAnchor.transform.position;
                    obj.transform.localScale = new Vector3(5f,5f,5f);

                    GameObject logic = logicManager.RequestObject();
                    BulletLogic logicComponent = logic.GetComponent<BulletLogic>();
                    logicComponent.SetParameters(0.1f, 250f);

					float speed = 5f;
					obj.GetComponent<BulletMove>().Speed = speed;
					bulletManager.SetBulletSpeed(obj.name, speed);

                    logic.transform.parent = obj.transform;

                    logicComponent.SetDestination(enemyFinder.enemyList[i].transform.position, true, this.gameObject, bulletManager, logicManager, impactManager);

                    bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
                }
            }

            Destroy(enemyFinder.gameObject);
        }
        else
        {
            StartCoroutine(FireMissiles(enemyFinder));
        }

    }

    internal override void ActivateAbility()
    {
        if(shootAnchor == null)
                shootAnchor = GameObject.Find("CommanderShootAnchor");

        if(bulletManager == null)
            bulletManager      = GameObject.Find("CommanderRocketManager").GetComponent<ObjectPoolManager>();

        if(logicManager == null)
            logicManager       = GameObject.Find("PlayerBulletLogicManager").GetComponent<ObjectPoolManager>();

        if(impactManager == null)
            impactManager      = GameObject.Find("AsteroidExplosionManager").GetComponent<ObjectPoolManager>();

        GameObject temp = Instantiate(enemyFinderObject, state.PlayerShip.transform.position, Quaternion.identity) as GameObject;
        EnemyFinder enemyFinder = temp.GetComponent<EnemyFinder>();

        StartCoroutine(FireMissiles(enemyFinder));
     }

    internal override void DeactivateAbility()
    {
        // No operation
    }


}
