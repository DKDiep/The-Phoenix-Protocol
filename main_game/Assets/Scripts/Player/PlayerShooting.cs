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

	private AudioSource fireSoundAudioSource;
	private GameObject[] bulletAnchor;
	private GameObject target;
	private bool canShoot, showMarker;
	private float alpha;
	private Vector3 crosshairPosition;
	private GameObject[] crosshairs;
	private CrosshairAutoaimAssist[] autoaimScripts;

	private ObjectPoolManager bulletManager;
	private ObjectPoolManager logicManager;
	private ObjectPoolManager muzzleFlashManager;
	private ObjectPoolManager impactManager;

    private GameState gameState;
    private ServerManager serverManager;

    // Which player are we controlling via the mouse. (For debugging different players)
    private int currentPlayerId = 0;

	void Start()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        Reset();
        
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
    }

    public void Reset()
    {
        LoadSettings();
    }

	private void LoadSettings()
	{
		hitmarker   = settings.PlayerHitmarker;
		fireSound   = settings.PlayerFireSound;
		randomPitch = settings.PlayerFireSoundRandomPitch;
		accuracy    = settings.PlayerBulletAccuracy;
		speed 		= settings.PlayerBulletSpeed;
	}
    
	public void Setup () 
	{
		fireSoundAudioSource = GetComponent<AudioSource>();
		fireSoundAudioSource.clip = fireSound;
		showMarker = false;
		alpha = 0;
		target = new GameObject();
		transform.localPosition = new Vector3(0,0,0);
        //GameObject crosshairContainer = GameObject.Find("Crosshairs");
        // Get screen with index 0
        GameObject crosshairContainer = GameObject.Find("GameManager").GetComponent<ServerManager>().GetCrosshairObject(0).transform.Find("Crosshairs").gameObject;


        crosshairs 	   = new GameObject[4];
		autoaimScripts = new CrosshairAutoaimAssist[4];

        // Find crosshairs
		for (int i = 0; i < 4; ++i)
		{
			crosshairs[i]     = crosshairContainer.transform.GetChild(i).gameObject;
			autoaimScripts[i] = crosshairs[i].GetComponent<CrosshairAutoaimAssist>();
		}

        bulletAnchor = new GameObject[4];

        // Find crosshair images
        for(int i = 1; i <= 4; ++i)
            bulletAnchor[i-1] = GameObject.Find("BulletAnchor" + i.ToString());

        bulletManager      = GameObject.Find("PlayerBulletManager").GetComponent<ObjectPoolManager>();
        logicManager       = GameObject.Find("PlayerBulletLogicManager").GetComponent<ObjectPoolManager>();
        muzzleFlashManager = GameObject.Find("PlayerMuzzleFlashManager").GetComponent<ObjectPoolManager>();
        impactManager      = GameObject.Find("BulletImpactManager").GetComponent<ObjectPoolManager>();
	}

	void Update () 
	{
        if (gameState.Status != GameState.GameStatus.Started)
            return;

        SwitchPlayers();


		if(Input.GetMouseButton (0) && canShoot)
		{
			ShootBullet (currentPlayerId);
		}

        // Control alpha of hitmarker
		if(alpha > 0)
		{
			alpha -= 5f * Time.deltaTime;
		}
	}

	// Shoot a bullet for a specific player
	public void ShootBullet(int playerId) 
	{
        if (crosshairs != null)
        {
            Vector3 crosshairPosition = crosshairs[playerId].transform.position;
            target.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshairPosition.x, crosshairPosition.y, 1000));

            if (randomPitch) fireSoundAudioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            fireSoundAudioSource.PlayOneShot(fireSound);

            GameObject obj = bulletManager.RequestObject();
            obj.transform.position = bulletAnchor[playerId].transform.position;

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
            logicComponent.SetDestination(target.transform.position, true, this.gameObject, bulletManager, logicManager, impactManager);

			// If this bullet was shot at a target, make it follow that target if it passes an accuracy check
			if (autoaimScripts[playerId].Target != null && UnityEngine.Random.value < accuracy)
				moveComponent.SetTarget(autoaimScripts[playerId].Target);

            bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);

            GameObject muzzle = muzzleFlashManager.RequestObject();
            muzzle.transform.position = bulletAnchor[playerId].transform.position;
            muzzle.transform.rotation = bulletAnchor[playerId].transform.rotation;
            muzzle.transform.parent = bulletAnchor[playerId].transform.parent;

            canShoot = false;
            StartCoroutine(Delay());
        }
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
        if (crosshairs != null)
        {
            GUI.color = new Color(1, 1, 1, alpha);
            crosshairPosition = crosshairs[currentPlayerId].transform.position;
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
		canShoot = enabled;
	}
}
