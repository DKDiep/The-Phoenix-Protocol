/*
    Handles targeting and spawning of player bullets
*/

using UnityEngine;
using System.Collections;
using System;

public class PlayerShooting : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private Texture2D hitmarker; // Hitmarker texture
	private AudioClip fireSound; // Sound to make when firing
	private bool randomPitch;
	private float accuracy;
	private float speed;
	private int ammo, maxAmmo, shootAmmoCost, ammoRechargeValue;
	private float ammoRechargeDelay;

	private AudioSource fireSoundAudioSource;
	private GameObject bulletAnchor;
	private Vector3 targetPos;
	private bool canShoot, showMarker;
	private float alpha;
	private Vector3 crosshairPosition;
	private GameObject crosshair;
	private CrosshairAutoaimAssist autoaimScript;
	private Camera mainCamera;

	private ObjectPoolManager bulletManager;
	private ObjectPoolManager logicManager;
	private ObjectPoolManager muzzleFlashManager;
	private ObjectPoolManager impactManager;

    private GameState gameState;
    private ServerManager serverManager;

	private bool ammoRecharging;
	private Coroutine ammoRechargeCoroutine;

    private int screenId;

    // Which player are we controlling via the mouse. (For debugging different players)
    private int currentPlayerId = 0;
    public int playerId;

	void Start()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        Reset();
        
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();

		mainCamera = Camera.main;
    }

    public void Reset()
    {
        LoadSettings();
    }

	private void LoadSettings()
	{
		hitmarker   	  = settings.PlayerHitmarker;
		fireSound   	  = settings.PlayerFireSound;
		randomPitch 	  = settings.PlayerFireSoundRandomPitch;
		accuracy    	  = settings.PlayerBulletAccuracy;
		speed 			  = settings.PlayerBulletSpeed;
		ammo = maxAmmo	  = settings.PlayerMaxAmmo;
		shootAmmoCost     = settings.PlayerShootingAmmoCost;
		ammoRechargeValue = settings.PlayerAmmoRechargeValue;
		ammoRechargeDelay = settings.PlayerAmmoRechargeInterval;
	}
    
	public void Setup () 
	{
		fireSoundAudioSource = GetComponent<AudioSource>();
		fireSoundAudioSource.clip = fireSound;
		showMarker = false;
		alpha = 0;
		transform.localPosition = new Vector3(0,0,0);

        serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();
        // Get screen with index 0
        GameObject crosshairContainer = serverManager.GetCrosshairObject(0).transform.Find("Crosshairs").gameObject;

        // Find crosshair
		crosshair     = crosshairContainer.transform.GetChild(playerId).gameObject;
        autoaimScript = crosshair.GetComponent<CrosshairAutoaimAssist>();

        bulletAnchor = new GameObject();

        // Find crosshair images
        bulletAnchor = GameObject.Find("BulletAnchor" + (playerId+1).ToString());

        bulletManager      = GameObject.Find("PlayerBulletManager").GetComponent<ObjectPoolManager>();
        logicManager       = GameObject.Find("PlayerBulletLogicManager").GetComponent<ObjectPoolManager>();
        muzzleFlashManager = GameObject.Find("PlayerMuzzleFlashManager").GetComponent<ObjectPoolManager>();
        impactManager      = GameObject.Find("BulletImpactManager").GetComponent<ObjectPoolManager>();
	}

	void Update () 
	{
        if (gameState.Status != GameState.GameStatus.Started)
            return;

		// TODO: For debugging: see if the player has been switched using the keyboard
        SwitchPlayers();

        if(playerId == currentPlayerId)
		    TryShoot(currentPlayerId, false);

        // Control alpha of hitmarker
		if(alpha > 0)
		{
			alpha -= 5f * Time.deltaTime;
		}
	}

	/// <summary>
	/// Shoots a bullet if enough ammo is available.
	/// </summary>
	/// <param name="playerId">The shooter's ID.</param>
	public void TryShoot(int playerId, bool remote)
	{
		bool shootButtonPressed = false;
        if(remote)
            shootButtonPressed = true;
        else 
            shootButtonPressed = Input.GetMouseButton(0);

		if (shootButtonPressed && canShoot && ammo >= shootAmmoCost)
		{
			// Stop ammo recharging when the player fires
			if (ammoRechargeCoroutine != null)
			{
				StopCoroutine(ammoRechargeCoroutine);
				ammoRecharging = false;
			}

			ShootBullet(playerId);
			ammo -= shootAmmoCost;
		}
		else if (!shootButtonPressed && !ammoRecharging)
		{
			// Wait for the player to release the fire button before startin to recharge ammo
			ammoRechargeCoroutine = StartCoroutine(RechargeAmmo());
			ammoRecharging = true;
		}
	}

	/// <summary>
	/// Shoots a bullet for a specific player.
	/// </summary>
	/// <param name="playerId">The player ID.</param>
	private void ShootBullet(int playerId) 
	{
        if (crosshair != null)
        {
            Vector3 crosshairPosition = crosshair.transform.position;
            
			// Get correct crosshair object's ScreenToWorld results
            GameObject crosshairObject = serverManager.GetCrosshairObject(screenId);
            Vector3[] targets 		   = serverManager.GetTargetPositions(crosshairObject).targets;
            targetPos				   = targets[0];

            if (randomPitch) fireSoundAudioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            fireSoundAudioSource.PlayOneShot(fireSound);

            GameObject obj = bulletManager.RequestObject();
            obj.transform.position = bulletAnchor.transform.position;

			BulletMove moveComponent = obj.GetComponent<BulletMove>();
			moveComponent.Speed = speed;
			bulletManager.SetBulletSpeed(obj.name, speed);

            GameObject logic = logicManager.RequestObject();
			logic.transform.parent = obj.transform;

            // TODO: this is now broken because the logic is only spawned on the server, so the bullets don't move on the clients
			// Suggest add a BulletMove method for the speed and remove it from the logic
			BulletLogic logicComponent = logic.GetComponent<BulletLogic>();
			logicComponent.SetParameters(1-accuracy, gameState.GetBulletDamage());
            logicComponent.SetID(this, playerId);
            logicComponent.SetDestination(targetPos, true, this.gameObject, bulletManager, logicManager, impactManager);

			// If this bullet was shot at a target, make it follow that target if it passes an accuracy check
			if (autoaimScript.Target != null && UnityEngine.Random.value < accuracy)
				moveComponent.SetTarget(autoaimScript.Target);

            bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);

            GameObject muzzle = muzzleFlashManager.RequestObject();
            muzzle.transform.position = bulletAnchor.transform.position;
            muzzle.transform.rotation = bulletAnchor.transform.rotation;
            muzzle.transform.parent = bulletAnchor.transform.parent;

            canShoot = false;
            StartCoroutine(Delay());
        }
	}

    public void SetScreenId(int newScreenId)
    {
        screenId = newScreenId;
    }

	// Switch between players using keys 4-7, for debugging different player shooting.
	void SwitchPlayers() 
	{
		// Loop through 4 players
		for (int i = 4; i <= 7; i++) 
		{
			if (Input.GetKeyDown (i.ToString ())) 
			{
				currentPlayerId = i-4;
			}
		}
	}

	void OnGUI()
	{
        if (crosshair != null)
        {
            GUI.color = new Color(1, 1, 1, alpha);
            crosshairPosition = crosshair.transform.position;
            if (showMarker) GUI.DrawTexture(new Rect(crosshairPosition.x - 32, Screen.height - crosshairPosition.y - 32, 64, 64), hitmarker, ScaleMode.ScaleToFit, true, 0);
        }
	}

    // Show hitmarker when an enemy is hit
	public void HitMarker()
	{
		showMarker = true;
		alpha = 1f;
		StartCoroutine(HideMarker());
	}

    // Stop drawing hitmarker after certain time limit
	IEnumerator HideMarker()
	{
		yield return new WaitForSeconds(2f);
		showMarker = false;
	}

    // Delay before player can shoot again
	IEnumerator Delay()
	{
		yield return new WaitForSeconds(gameState.GetFiringDelay());
		canShoot = true;
	}

	/// <summary>
	/// Enables or disables shooting.
	/// </summary>
	/// <param name="enabled">If set to <c>true</c>, shooting is enabled.</param>
	public void SetShootingEnabled(bool enabled)
	{
		canShoot = ammoRecharging = enabled;

		if (enabled)
			ammoRechargeCoroutine = StartCoroutine(RechargeAmmo());
		else if (ammoRechargeCoroutine != null)
			StopCoroutine(ammoRechargeCoroutine);
	}

	/// <summary>
	/// Recharges the ammo.
	/// </summary>
	IEnumerator RechargeAmmo()
	{
		yield return new WaitForSeconds(ammoRechargeDelay);

		ammo += ammoRechargeValue;
		if (ammo > maxAmmo)
			ammo = maxAmmo;

		ammoRechargeCoroutine = StartCoroutine(RechargeAmmo());
	}
}
