using UnityEngine;
using System.Collections;

public class AsteroidLogic : MonoBehaviour 
{
	public GameObject player;
	float maxVariation; // Percentage variation in size
	
	public void SetPlayer(GameObject temp, float var)
	{
		player = temp;
		maxVariation = var;
		transform.localScale = new Vector3(10f + Random.Range (-var, var), 10f + Random.Range (-var, var),10f + Random.Range (-var, var));
		transform.rotation = Random.rotation;
		StartCoroutine ("DestroyZ");
	}
	
	// Automatically destroy if 100 units behind the player
	IEnumerator DestroyZ()
	{
		yield return new WaitForSeconds(1f);
		if(transform.position.z < player.transform.position.z - 100f)
		{
			AsteroidSpawner.numAsteroids -= 1;
			Destroy (this.gameObject);
		}
		StartCoroutine ("DestroyZ");
	}
}
