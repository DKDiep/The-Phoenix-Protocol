using UnityEngine;
using System.Collections;

public class AsteroidLogic : MonoBehaviour 
{
	public GameObject player;
	float maxVariation; // Percentage variation in size
	[SerializeField] float health;
	
	public void SetPlayer(GameObject temp, float var)
	{
		player = temp;
		maxVariation = var;
		transform.localScale = new Vector3(10f + Random.Range (-var, var), 10f + Random.Range (-var, var),10f + Random.Range (-var, var));
		transform.rotation = Random.rotation;
	}
	
	public void collision (float damage)
	{
		health -= damage;
		if(health <= 0) Destroy (this.gameObject);
	}
}
