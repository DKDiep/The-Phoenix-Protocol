﻿/*
    Handles targeting and spawning of player bullets
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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
	private float ammoRechargeInterval, ammoRechargeDelay;

	private AudioSource fireSoundAudioSource;
	private GameObject bulletAnchorLeft, bulletAnchorRight;
	private Vector3 targetPos;
	private bool canShoot, showMarker;
	private float alpha;
	private Vector3 crosshairPosition;
	private GameObject crosshair;
    private GameObject crosshairContainer;
	private CrosshairAutoaimAssist autoaimScript;
	private GameObject playerShip;

	private ObjectPoolManager bulletManager;
	private ObjectPoolManager logicManager;
	private ObjectPoolManager muzzleFlashManager;
	private ObjectPoolManager impactManager;

    private GameState gameState;
    private ServerManager serverManager;
	private Dictionary<uint, Officer> officerMap;

	private bool ammoRecharging;
	private Coroutine ammoRechargeCoroutine;
    private bool shootButtonPressed;
	private float timeSinceLastShot;

    private int screenId;

    // Which player are we controlling via the mouse. (For debugging different players)
    private int currentPlayerId = 0;
	public int PlayerId { private get; set; }

    private PlayerController playerController;

	void Start()
	{
		string name = this.gameObject.name;
		currentPlayerId = Int32.Parse(name.Substring(name.Length - 1));

		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        Reset();
        
        gameState  = GameObject.Find("GameManager").GetComponent<GameState>();
		officerMap = gameState.GetOfficerMap();

		playerShip = transform.parent.gameObject;

        // Pick a random controller for RPC
        playerController = GameObject.Find("PlayerController(Clone)").GetComponent<PlayerController>();
    }

    public void Reset()
    {
        LoadSettings();
        shootButtonPressed = false;
		timeSinceLastShot  = 0f;

		// Disable any active bullets, but not the first time when starting the game
		if (bulletManager != null)
			bulletManager.DisableAll();
		if (logicManager != null)
			logicManager.DisableAll();
    }

	private void LoadSettings()
	{
		hitmarker   	     = settings.PlayerHitmarker;
		fireSound   	     = settings.PlayerFireSound;
		randomPitch 	     = settings.PlayerFireSoundRandomPitch;
		accuracy    	     = settings.PlayerBulletAccuracy;
		speed 			     = settings.PlayerBulletSpeed;
		ammo = maxAmmo	     = settings.PlayerMaxAmmo;
		shootAmmoCost        = settings.PlayerShootingAmmoCost;
		ammoRechargeValue    = settings.PlayerAmmoRechargeValue;
		ammoRechargeInterval = settings.PlayerAmmoRechargeInterval;
		ammoRechargeDelay    = settings.PlayerAmmoRehargeDelay;
	}
    
	public void Setup () 
	{
		fireSoundAudioSource 	  = GetComponent<AudioSource>();
		fireSoundAudioSource.clip = fireSound;
		showMarker 				  = false;
		alpha 					  = 0;
		transform.localPosition   = Vector3.zero;

        serverManager = GameObject.Find("GameManager").GetComponent<ServerManager>();
        
		// Get screen with index 0
        crosshairContainer = serverManager.GetCrosshairObject(0).transform.Find("Crosshairs").gameObject;

        bulletManager      = GameObject.Find("PlayerBulletManager").GetComponent<ObjectPoolManager>();
        logicManager       = GameObject.Find("PlayerBulletLogicManager").GetComponent<ObjectPoolManager>();
        muzzleFlashManager = GameObject.Find("PlayerMuzzleFlashManager").GetComponent<ObjectPoolManager>();
        impactManager      = GameObject.Find("BulletImpactManager").GetComponent<ObjectPoolManager>();
	}

	void Update () 
	{
        if (gameState.Status != GameState.GameStatus.Started)
            return;

		if (Input.GetMouseButton(0) && currentPlayerId == 0)
			OnShootButtonPressed(currentPlayerId);
       
        ShootUpdate(currentPlayerId);

		/*if (currentPlayerId == 0)
			Debug.Log("Ammo: " + ammo);*/
	}

    /// <summary>
    /// Registers that the shoot button has been pressed.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
	public void OnShootButtonPressed(int playerId)
    {
        currentPlayerId    = playerId;
        shootButtonPressed = true;
    }

	/// <summary>
	/// Updates the shooting status.
	/// 
	/// This shoots if the button is pressed and enough ammo is available, and updates the ammo.
	/// </summary>
	/// <param name="playerId">The shooter's ID.</param>
	private void ShootUpdate(int playerId)
	{
		if (shootButtonPressed)
		{
			// Stop ammo recharging when the player fires
			if (ammoRechargeCoroutine != null)
			{
				StopCoroutine(ammoRechargeCoroutine);
				ammoRecharging = false;
				// Debug.Log("Stop recharging");
			}

			if (canShoot && ammo >= shootAmmoCost)
			{
				ShootBullet(playerId);

				Officer officer;
				if (officerMap.TryGetValue((uint)playerId, out officer))
					officer.Ammo = ammo;
			}

			timeSinceLastShot = 0f;
		}
		else if (!ammoRecharging)
		{
			// Wait a while after the player releases the fire button before starting to recharge ammo
			if(timeSinceLastShot >= ammoRechargeDelay)
			{
				ammoRechargeCoroutine = StartCoroutine(RechargeAmmo());
				ammoRecharging = true;
				// Debug.Log("Recharging");
			}
			else
			{
				timeSinceLastShot += Time.deltaTime;
				// Debug.Log("Delay " + timeSinceLastShot);
			}
		}

		shootButtonPressed = false;
	}

	/// <summary>
	/// Shoots a bullet for a specific player.
	/// </summary>
	/// <param name="playerId">The player ID.</param>
	private void ShootBullet(int playerId) 
	{
        
        if (crosshair == null)
        {
            crosshair     = crosshairContainer.transform.GetChild(playerId).gameObject;
            autoaimScript = crosshair.GetComponent<CrosshairAutoaimAssist>();

			// Find the tip of the turret (where bullets are shot from)
			string anchorName = "BulletAnchor" + playerId;
			bulletAnchorLeft  = GameObject.Find(anchorName + "L");
			bulletAnchorRight = GameObject.Find(anchorName + "R");
        } 
		else
		{    
			// Get the aiming position
            GameObject crosshairObject = serverManager.GetCrosshairObject(screenId);
            Vector3[] targets 		   = serverManager.GetTargetPositions(crosshairObject).targets;
            GameObject[] targetObjects = serverManager.GetTargetPositions(crosshairObject).targetObjects;
            targetPos				   = targets[playerId];
            autoaimScript.Target       = targetObjects[playerId];

			// Figure out which side of ship we need shoot from based on the target position
			Vector3 targetRelativeToPlayer = playerShip.transform.InverseTransformPoint(targetPos);
			GameObject bulletAnchor        = targetRelativeToPlayer.x <= 0 ? bulletAnchorLeft : bulletAnchorRight;

            if (randomPitch)
				fireSoundAudioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            fireSoundAudioSource.PlayOneShot(fireSound);

			GameObject bullet = bulletManager.RequestObject();
			GameObject logic  = logicManager.RequestObject();
			if (bullet != null && logic != null)
			{
				bullet.transform.position = bulletAnchor.transform.position;

				BulletMove moveComponent = bullet.GetComponent<BulletMove>();
				moveComponent.Speed      = speed;
				bulletManager.SetBulletSpeed(bullet.name, speed);

				logic.transform.parent = bullet.transform;

				// Suggest add a BulletMove method for the speed and remove it from the logic
				BulletLogic logicComponent = logic.GetComponent<BulletLogic>();
				logicComponent.SetParameters(1 - accuracy, gameState.GetBulletDamage());
				logicComponent.SetID(this, playerId);
				logicComponent.SetDestination(targetPos, true, this.gameObject, bulletManager, logicManager, impactManager);

				// If this bullet was shot at a target, make it follow that target if it passes an accuracy check
				if (autoaimScript.Target != null && UnityEngine.Random.value < accuracy)
					moveComponent.SetTarget(autoaimScript.Target);

				bulletManager.EnableClientObject(bullet.name, bullet.transform.position, bullet.transform.rotation,
					bullet.transform.localScale);

				GameObject muzzle = muzzleFlashManager.RequestObject();
				muzzle.transform.position = bulletAnchor.transform.position;
				muzzle.transform.rotation = bulletAnchor.transform.rotation;
				muzzle.transform.parent = bulletAnchor.transform.parent;

				serverManager.UpdateLastShotTime(playerId, Time.time);

				ammo     -= shootAmmoCost;
			}

			canShoot  = false;
            StartCoroutine(Delay());
        }
	}

    public void SetScreenId(int newScreenId)
    {
        screenId = newScreenId;
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
		yield return new WaitForSeconds(ammoRechargeInterval);

		ammo += ammoRechargeValue;
		if (ammo > maxAmmo)
			ammo = maxAmmo;

		// Debug.Log("Recharged to " + ammo);
        
		Officer officer;
		if (officerMap.TryGetValue((uint)PlayerId, out officer))
			officer.Ammo = ammo;

		if (ammo != maxAmmo)
			ammoRechargeCoroutine = StartCoroutine(RechargeAmmo());
	}
}
