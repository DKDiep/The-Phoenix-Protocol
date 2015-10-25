using UnityEngine;
using System.Collections;

public class BulletLogic : MonoBehaviour 
{

	[SerializeField] float speed = 100f;
	GameObject player;

	void Start () 
	{
		player = GameObject.Find ("PlayerShip");
		transform.LookAt (player.transform.position); // Set to the correct rotation. Needs position predication
		StartCoroutine ("DestroyZ");
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position += transform.forward * Time.deltaTime * speed;
	}
	
	// Destroy if 100 units behind player
	IEnumerator DestroyZ()
	{
		yield return new WaitForSeconds(1f);
		if(transform.position.z < player.transform.position.z - 100f)
		{
			Destroy (this.gameObject);
		}
		StartCoroutine ("DestroyZ");
	}
}
