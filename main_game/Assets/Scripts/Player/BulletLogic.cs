using UnityEngine;
using System.Collections;

public class BulletLogic : MonoBehaviour 
{

	[SerializeField] float speed = 100f;
	[SerializeField] float accuracy; // 0 = perfectly accurate, 1 = very inaccurate
	[SerializeField] float damage; 
	[SerializeField] Color bulletColor;
	[SerializeField] float xScale;
	[SerializeField] float yScale;
	[SerializeField] float zScale;
	GameObject player;
	GameObject obj;
	GameObject destination;
	bool started = false;

	public void SetDestination(Vector3 destination)
	{
		obj = transform.parent.gameObject;
		Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.isKinematic = true;
		SphereCollider sphere = obj.AddComponent<SphereCollider>();
		sphere.isTrigger = true;
		obj.AddComponent<BulletCollision>();
		obj.transform.localScale = new Vector3(xScale, yScale, zScale);
		obj.transform.LookAt (destination);
		obj.transform.Rotate (Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy));
		Renderer rend = obj.GetComponent<Renderer>();
		rend.material.SetColor("_EmissionColor", bulletColor);;
		StartCoroutine ("DestroyZ");
		started = true;
	}
	
	public void collision(Collider col)
	{

		if(col.gameObject.tag.Equals("Debris"))
		{
			//Debug.Log ("A bullet has hit asteroid");
			col.gameObject.GetComponentInChildren<AsteroidLogic>().collision(damage);
		}
		else if(col.gameObject.tag.Equals("EnemyShip"))
		{
			//Debug.Log ("A bullet has hit an enemy");
			col.gameObject.GetComponentInChildren<EnemyLogic>().collision(damage);
		}
		else if(col.gameObject.tag.Equals("Player"))
		{
			//Debug.Log ("A bullet has hit the player");
			col.gameObject.transform.parent.transform.parent.transform.parent.GetComponentInChildren<ShipMovement>().collision(damage, transform.eulerAngles.y);
		}
		Destroy (obj);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(started) obj.transform.position += obj.transform.forward * Time.deltaTime * speed;
	}
	
	// Destroy if 100 units behind player
	IEnumerator DestroyZ()
	{
		yield return new WaitForSeconds(10f);
		Destroy (obj);
	}
}
