/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene, Andrei Poenaru
    Description: Handles targeting and spawning of player bullets
*/

using UnityEngine;
using System.Collections;
using WiimoteApi;
using System;

public class PlayerShooting : MonoBehaviour 
{
	[SerializeField] GameObject bullet; // Bullet prefab to use
	[SerializeField] GameObject bulletLogic;
	[SerializeField] float xOffset, yOffset, zOffset, rateOfFire;
	[SerializeField] Texture2D hitmarker; // Hitmarker texture
    [SerializeField] AudioClip fireSnd; // Sound to make when firing
    AudioSource mySrc;
    [SerializeField] bool randomPitch;
    [SerializeField] GameObject muzzleFlash;

	GameObject[] bulletAnchor;
	GameObject target;
	bool canShoot, showMarker;
	float alpha;
    Vector3 crosshairPosition;
    GameObject[] crosshairs;

    ObjectPoolManager bulletManager;
    ObjectPoolManager logicManager;

	// Which player are we controlling via the mouse. (For debugging different players)
	private int currentPlayerId = 0;
    
	void Start () 
	{
        mySrc = GetComponent<AudioSource>();
        mySrc.clip = fireSnd;
		canShoot = true;
		showMarker = false;
		alpha = 0;
		target = new GameObject();
		transform.localPosition = new Vector3(0,0,0);
        GameObject crosshairContainer = GameObject.Find("Crosshairs");

        crosshairs = new GameObject[4];

        // Find crosshair images
        for(int i = 0; i < 4; ++i)
        {
            crosshairs[i] = crosshairContainer.transform.GetChild(i).gameObject;
        }

        bulletAnchor = new GameObject[4];

        // Find crosshair images
        for(int i = 1; i <= 4; ++i)
        {
            bulletAnchor[i-1] = GameObject.Find("BulletAnchor" + i.ToString());
        }

        bulletManager = GameObject.Find("PlayerBulletManager").GetComponent<ObjectPoolManager>();
        logicManager = GameObject.Find("PlayerBulletLogicManager").GetComponent<ObjectPoolManager>();
	}

	void Update () 
	{
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
				catch (Exception e) 
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
		Vector3 crosshairPosition = crosshairs[playerId].transform.position;

        if(randomPitch) mySrc.pitch = UnityEngine.Random.Range(0.7f, 1.3f);
        mySrc.PlayOneShot(fireSnd);

		Ray ray = Camera.main.ScreenPointToRay(crosshairPosition);
		RaycastHit hit;

		if(Physics.Raycast(ray,out hit) && !hit.transform.gameObject.tag.Equals("Player"))
		{
			target.transform.position = hit.transform.position;
		}
		else
		{
			target.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(crosshairPosition.x, crosshairPosition.y, 1000));
			target.transform.Translate (transform.forward * (-10f));
		}
        
        GameObject obj = bulletManager.RequestObject();
        obj.transform.position = bulletAnchor[playerId].transform.position;

        GameObject logic = logicManager.RequestObject();
        BulletLogic logicComponent = logic.GetComponent<BulletLogic>();
		logicComponent.SetID(this, playerId);
		logic.transform.parent = obj.transform;
		logicComponent.SetDestination (target.transform.position, true, this.gameObject, bulletManager, logicManager);

        GameObject muzzle = Instantiate (muzzleFlash, bulletAnchor[playerId].transform.position, bulletAnchor[playerId].transform.rotation) as GameObject;
        muzzle.transform.parent = bulletAnchor[playerId].transform.parent;

        ServerManager.NetworkSpawn(obj);
		canShoot = false;
		StartCoroutine("Delay");
	}

	// Switch between players using keys 1-4, for debugging different player shooting.
	void SwitchPlayers() 
	{
		// Loop through 4 players
		for (int i = 1; i <= 4; i++) 
		{
			if (Input.GetKeyDown (i.ToString ())) 
			{
				currentPlayerId = i-1;
			}
		}
	}

	void OnGUI()
	{
		GUI.color = new Color(1,1,1,alpha);
        crosshairPosition = crosshairs[currentPlayerId].transform.position;
		if(showMarker) GUI.DrawTexture(new Rect(crosshairPosition.x - 32, Screen.height - crosshairPosition.y - 32, 64, 64), hitmarker, ScaleMode.ScaleToFit, true, 0);
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
		yield return new WaitForSeconds(rateOfFire);
		canShoot = true;
	}
}
