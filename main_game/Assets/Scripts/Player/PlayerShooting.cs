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

	[SerializeField] GameObject bullet;
	[SerializeField] GameObject bulletLogic;
	[SerializeField] float xOffset, yOffset, zOffset, rateOfFire;
	[SerializeField] Texture2D hitmarker;
    [SerializeField] AudioClip fireSnd;
    AudioSource mySrc;

	GameObject bulletAnchor;
	GameObject target;
	bool canShoot, showMarker;
	float alpha;
	// Wii remote initialise
	private bool init = true;

	// Which player are we controlling via the mouse. (For debugging different players)
	private int currentPlayerId = 0;

	// Use this for initialization
	void Start () 
	{
        mySrc = GetComponent<AudioSource>();
        mySrc.clip = fireSnd;
		canShoot = true;
		showMarker = false;
		alpha = 0;
		target = new GameObject();
		transform.localPosition = new Vector3(0,0,0);
		foreach(Transform child in transform.parent)
		{
			if(child.name.Equals ("BulletAnchor"))
			{
				bulletAnchor = child.gameObject;
			}
		}
	}
	
	// Update is called once per frame
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
				
			Wiimote remote;
			try 
			{
				// Only for the one remote at the moment.
				remote = WiimoteManager.Wiimotes [0];

				// Setup remote at beginning or when it disconnects.
				if (this.init) 
				{
					remote.SendPlayerLED (true, false, false, false);
					remote.SetupIRCamera (IRDataType.BASIC);
					this.init = false;
				}

				// if finished reading data from remote
				if (remote.ReadWiimoteData () > 0) 
				{
					if (remote.Button.b && canShoot) 
					{
						// Shoot bullet for player 0
						ShootBullet (0);
					}
				}
			} 
			catch (Exception e) 
			{
				WiimoteManager.FindWiimotes ();
				this.init = true;
			}  
		}
			
		if(alpha > 0)
		{
			alpha -= 5f * Time.deltaTime;
		}
	}

	// Shoot a bullet for a specific player
	void ShootBullet(int playerId) 
	{
		Vector3 crosshairPosition = GameObject.Find("CrosshairImage" + playerId).transform.position;
        mySrc.Play();

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

		GameObject obj = Instantiate (bullet, bulletAnchor.transform.position, Quaternion.identity) as GameObject;
		GameObject logic = Instantiate (bulletLogic, bulletAnchor.transform.position, Quaternion.identity) as GameObject;
		logic.GetComponent<BulletLogic>().SetID(this, playerId);
		logic.transform.parent = obj.transform;
		logic.GetComponent<BulletLogic>().SetDestination (target.transform.position, true);
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
		Vector3 crosshairPosition = GameObject.Find("CrosshairImage" + currentPlayerId).transform.position;
		GUI.color = new Color(1,1,1,alpha);
		if(showMarker) GUI.DrawTexture(new Rect(crosshairPosition.x - 32, Screen.height - crosshairPosition.y - 32, 64, 64), hitmarker, ScaleMode.ScaleToFit, true, 0);
	}

	public void HitMarker()
	{
		showMarker = true;
		alpha = 1f;
		StartCoroutine("HideMarker");
	}

	IEnumerator HideMarker()
	{
		yield return new WaitForSeconds(2f);
		showMarker = false;
	}

	IEnumerator Delay()
	{
		yield return new WaitForSeconds(rateOfFire);
		canShoot = true;
	}
}
