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
	
	void Start()
	{
		obj = transform.parent.gameObject;
	}
	
	public void SetPlayer(GameObject temp)
	{
		player = temp;
		obj.transform.localScale = new Vector3(xScale, yScale, zScale);
		obj.transform.LookAt (player.transform.position); // Set to the correct rotation. Needs position predication
		obj.transform.Rotate (Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy), Random.Range (-accuracy, accuracy));
		Renderer rend = obj.GetComponent<Renderer>();
		rend.material.SetColor("_EmissionColor", bulletColor);;
		StartCoroutine ("DestroyZ");
	}
	
	// Update is called once per frame
	void Update () 
	{
		obj.transform.position += obj.transform.forward * Time.deltaTime * speed;
	}
	
	// Destroy if 100 units behind player
	IEnumerator DestroyZ()
	{
		yield return new WaitForSeconds(1f);
		if(obj.transform.position.z < player.transform.position.z - 100f)
		{
			Destroy (obj);
		}
		StartCoroutine ("DestroyZ");
	}
}
