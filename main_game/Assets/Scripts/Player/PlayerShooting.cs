using UnityEngine;
using System.Collections;

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
		if(Input.GetMouseButton (0) && canShoot)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit) && !hit.transform.gameObject.tag.Equals("Player"))
        {
        	target.transform.position = hit.transform.position;
        }
        else
        {
			    target.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(CrosshairMovement.crosshairPosition.x, CrosshairMovement.crosshairPosition.y, 1000));
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

		if(alpha > 0)
		{
			alpha -= 5f * Time.deltaTime;
		}
	}

	void OnGUI()
	{
		GUI.color = new Color(1,1,1,alpha);
		if(showMarker) GUI.DrawTexture(new Rect(Input.mousePosition.x - 32, Screen.height - Input.mousePosition.y - 32, 64, 64), hitmarker, ScaleMode.ScaleToFit, true, 0);
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
