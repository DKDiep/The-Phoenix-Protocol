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

	GameObject bulletAnchor;
	GameObject target;
	bool canShoot, showMarker;
	float alpha;
	// Wii remote initialise
	private bool init = true;

	// Use this for initialization
	void Start () 
	{
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
		// Use mouse if there are no wii remotes.
		if (WiimoteManager.Wiimotes.Count < 1) 
		{
			if(Input.GetMouseButton (0) && canShoot)
			{
				ShootBullet ();
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
					if (remote.Button.b) 
					{
						ShootBullet ();
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

	void ShootBullet() 
	{
		// Currently only works for first player
		Vector3 crosshairPosition = GameObject.Find("CrosshairImage0").transform.position;

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
		logic.GetComponent<BulletLogic>().SetID(this, 1);
		logic.transform.parent = obj.transform;
		logic.GetComponent<BulletLogic>().SetDestination (target.transform.position);
		ServerManager.NetworkSpawn(obj);
		canShoot = false;
		StartCoroutine("Delay");
	}
	void OnGUI()
	{
		Vector3 crosshairPosition = GameObject.Find("CrosshairImage0").transform.position;
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
