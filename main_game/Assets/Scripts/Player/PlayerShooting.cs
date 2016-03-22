/*
    Handles targeting and spawning of player bullets
*/

using UnityEngine;
using System.Collections;
using WiimoteApi;
using System;

public class PlayerShooting : MonoBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private Texture2D hitmarker; // Hitmarker texture
	private AudioClip fireSound; // Sound to make when firing
	private bool randomPitch;

	private AudioSource fireSoundAudioSource;
	private GameObject[] bulletAnchor;
	private GameObject target;
	private bool canShoot, showMarker;
	private float alpha;
	private Vector3 crosshairPosition;
	private GameObject[] crosshairs;

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
		LoadSettings();
        
        gameState = GameObject.Find("GameManager").GetComponent<GameState>();
    }

	private void LoadSettings()
	{
		hitmarker   = settings.PlayerHitmarker;
		fireSound   = settings.PlayerFireSound;
		randomPitch = settings.PlayerFireSoundRandomPitch;
	}
    
	public void Setup () 
	{
		fireSoundAudioSource = GetComponent<AudioSource>();
		fireSoundAudioSource.clip = fireSound;
		canShoot = true;
		showMarker = false;
		alpha = 0;
		target = new GameObject();
		transform.localPosition = new Vector3(0,0,0);
        //GameObject crosshairContainer = GameObject.Find("Crosshairs");
        // Get screen with index 0
        GameObject crosshairContainer = GameObject.Find("GameManager").GetComponent<ServerManager>().GetCrosshairObject(0).transform.Find("Crosshairs").gameObject;


        crosshairs = new GameObject[4];

        // Find crosshair images
        for(int i = 0; i < 4; ++i)
            crosshairs[i] = crosshairContainer.transform.GetChild(i).gameObject;

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

		// Use mouse if there are no wii remotes.
		if (WiimoteManager.Wiimotes.Count < 1) 
		{
			if(Input.GetMouseButton (0) && canShoot)
			{
				ShootBullet (currentPlayerId);
			}
		} 
		else 
		{
			int remoteId = 0;

			// Loop through all connected wii remotes
			foreach(Wiimote remote in WiimoteManager.Wiimotes) 
			{
				try 
				{
					// if finished reading data from remote
					if (remote.ReadWiimoteData () > 0) 
					{
						if (remote.Button.b && canShoot) 
						{
							// Shoot bullet for the player associated with the remote
							ShootBullet (remoteId);
						}
					}
				} 
				catch (Exception) 
				{
					WiimoteManager.FindWiimotes ();
				}  
				remoteId++;
			}
		}

        // Control alpha of hitmarker
		if(alpha > 0)
		{
			alpha -= 5f * Time.deltaTime;
		}
	}

	// Shoot a bullet for a specific player
	void ShootBullet(int playerId) 
	{
        if (crosshairs != null)
        {
            Vector3 crosshairPosition = crosshairs[playerId].transform.position;
            target.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshairPosition.x, crosshairPosition.y, 1000));

            if (randomPitch) fireSoundAudioSource.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
            fireSoundAudioSource.PlayOneShot(fireSound);

            GameObject obj = bulletManager.RequestObject();
            obj.transform.position = bulletAnchor[playerId].transform.position;

            GameObject logic = logicManager.RequestObject();
            BulletLogic logicComponent = logic.GetComponent<BulletLogic>();
			logicComponent.SetParameters(0.1f, gameState.GetBulletDamage(), 800f);
            logicComponent.SetID(this, playerId);

            logic.transform.parent = obj.transform;
            logicComponent.SetDestination(target.transform.position, true, this.gameObject, bulletManager, logicManager, impactManager);

            bulletManager.EnableClientObject(obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);

            GameObject muzzle = muzzleFlashManager.RequestObject();
            muzzle.transform.position = bulletAnchor[playerId].transform.position;
            muzzle.transform.rotation = bulletAnchor[playerId].transform.rotation;
            muzzle.transform.parent = bulletAnchor[playerId].transform.parent;

            canShoot = false;
            StartCoroutine("Delay");
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
		StartCoroutine("HideMarker");
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
}
