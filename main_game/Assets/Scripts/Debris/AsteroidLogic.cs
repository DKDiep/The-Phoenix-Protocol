using UnityEngine;
using System.Collections;

public class AsteroidLogic : MonoBehaviour 
{
	public GameObject player;
	float maxVariation; // Percentage variation in size
	[SerializeField] float health;
	[SerializeField] float minSpeed;
	[SerializeField] float maxSpeed;
	[SerializeField] GameObject destroyed1;
	[SerializeField] GameObject destroyed2;
	[SerializeField] GameObject destroyed3;
	float speed;
	int type;
    private GameState gameState;
	
	public void SetPlayer(GameObject temp, float var, int rnd)
	{
		player = temp;
		type = rnd;
		maxVariation = var;
		transform.parent.localScale = new Vector3(10f + Random.Range (-var, var), 10f + Random.Range (-var, var),10f + Random.Range (-var, var));
		transform.parent.rotation = Random.rotation;
		speed = Random.Range(minSpeed,maxSpeed);
	}

	void Update()
	{
		transform.parent.Rotate(transform.parent.forward * speed * Time.deltaTime);
	}

    public void SetStateReference(GameState state)
    {
        gameState = state;
    }

    public void collision (float damage)
	{
		health -= damage;
        if (health <= 0)
        {
        	//SpawnDebris();
			gameState.RemoveAsteroid(transform.parent.gameObject);
        Destroy(transform.parent.gameObject);	

        }
	}

	void SpawnDebris()
	{
		GameObject obj;
		if(type == 0) obj = destroyed1;
		else if(type == 1) obj = destroyed2;
		else obj = destroyed3;

		GameObject debris = Instantiate(obj, transform.position, transform.rotation) as GameObject;
		gameState.RemoveAsteroid(transform.parent.gameObject);
        Destroy(transform.parent.gameObject);	
	}
}
