using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour 
{

	[SerializeField] GameObject bullet;
	[SerializeField] GameObject bulletLogic;
	[SerializeField] float xOffset, yOffset, zOffset;
	GameObject bulletAnchor;
	GameObject target;

	// Use this for initialization
	void Start () 
	{
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
		if(Input.GetMouseButtonDown (0))
		{
			target.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(CrosshairMovement.crosshairPosition.x, CrosshairMovement.crosshairPosition.y, 1000));
			target.transform.Translate (transform.forward * (-10f));
			GameObject obj = Instantiate (bullet, bulletAnchor.transform.position, Quaternion.identity) as GameObject;
			GameObject logic = Instantiate (bulletLogic, bulletAnchor.transform.position, Quaternion.identity) as GameObject;
			logic.transform.parent = obj.transform;
			logic.GetComponent<BulletLogic>().SetDestination (target);
			ServerManager.NetworkSpawn(obj);
		}
	}
}
