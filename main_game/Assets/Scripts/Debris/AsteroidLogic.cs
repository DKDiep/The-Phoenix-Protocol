/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Sets a unique scale and rotation. Handles destruction effects.
*/

using UnityEngine;
using System.Collections;

public class AsteroidLogic : MonoBehaviour 
{
    public GameObject player;
    public float speed;

    int type; // Defines which of the 3 asteroid prefabs is used
    float maxVariation; // Percentage variation in size

    [SerializeField] float health;
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] GameObject destroyEffect; // The prefab to spawn when destroyed

    private GameState gameState;

    // Initialise player, size, and speed
	public void SetPlayer(GameObject temp, float var, int rnd)
	{
		player = temp;
		type = rnd;
		maxVariation = var;
		transform.parent.localScale = new Vector3(10f + Random.Range (-var, var), 10f + Random.Range (-var, var),10f + Random.Range (-var, var));
		transform.parent.rotation = Random.rotation;
		speed = Random.Range(minSpeed,maxSpeed);
	}

  public void SetStateReference(GameState state)
  {
    gameState = state;
  }

  // Allows asteroid to take damage and spawns destroy effect
  public void collision (float damage)
  {
        health -= damage;

        if (health <= 0)
        {
            GameObject temp = Instantiate(destroyEffect, transform.position, transform.rotation) as GameObject;
            ServerManager.NetworkSpawn(temp);
            gameState.RemoveAsteroid(transform.parent.gameObject);
            Destroy(transform.parent.gameObject);	
        }
   }
}
