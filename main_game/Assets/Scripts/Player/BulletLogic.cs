using UnityEngine;
using System.Collections;

public class BulletLogic : MonoBehaviour 
{

	[SerializeField] float speed = 100f;
	[SerializeField] float accuracy; // 0 = perfectly accurate, 1 = very inaccurate
	[SerializeField] Color bulletColor;
	[SerializeField] float xScale;
	[SerializeField] float yScale;
	[SerializeField] float zScale;
	GameObject player;
	GameObject obj;
	GameObject destination;
	bool started = false;

	public void SetDestination(GameObject destination)
	{
		obj = transform.parent.gameObject;
		obj.transform.localScale = new Vector3(xScale, yScale, zScale);
		obj.transform.LookAt (destination.transform.position); // Set to the correct rotation. Needs position predication
		obj.transform.Rotate (Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy));
		Renderer rend = obj.GetComponent<Renderer>();
		rend.material.SetColor("_EmissionColor", bulletColor);;
		StartCoroutine ("DestroyZ");
		started = true;
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
		Destroy (this.gameObject);
	}
}
