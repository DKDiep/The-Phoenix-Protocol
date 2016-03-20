using UnityEngine;
using System.Collections;

public class ShootingAbility : CommanderAbility {

    private GameObject shootAnchor;
    private ObjectPoolManager bulletManager;
    private ObjectPoolManager logicManager;
    private ObjectPoolManager impactManager;
    private GameObject target;

	// Use this for initialization
	private void Awake () 
    {
        target = new GameObject();
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        cooldown = settings.shootCooldown;
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetMouseButtonDown(0))
        {
            if(shootAnchor == null)
                shootAnchor = GameObject.Find("CommanderShootAnchor");
            UseAbility();
        }
	}

    internal override void AbilityEffect()
    {
        if(bulletManager == null)
            bulletManager      = GameObject.Find("CommanderRocketManager").GetComponent<ObjectPoolManager>();

        if(logicManager == null)
            logicManager       = GameObject.Find("PlayerBulletLogicManager").GetComponent<ObjectPoolManager>();

        if(impactManager == null)
            impactManager      = GameObject.Find("AsteroidExplosionManager").GetComponent<ObjectPoolManager>();

        GameObject obj = bulletManager.RequestObject();
        obj.transform.position = shootAnchor.transform.position;
        obj.transform.localScale = new Vector3(5f,5f,5f);

        GameObject logic = logicManager.RequestObject();
        BulletLogic logicComponent = logic.GetComponent<BulletLogic>();
        logicComponent.SetParameters(0.1f, 250f, 5f);

        logic.transform.parent = obj.transform;
        target.transform.position = shootAnchor.transform.position;
        target.transform.Translate(transform.forward * 1000f);

        logicComponent.SetDestination(target.transform.position, true, this.gameObject, bulletManager, logicManager, impactManager);

        bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);

    }


}
