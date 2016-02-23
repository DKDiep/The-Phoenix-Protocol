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

	// Defines which of the 3 asteroid prefabs is used
    int type; 
	// Percentage variation in size
    float maxVariation; 

	// The amount of resources dropped by the asteroid
	private int droppedResources;
	// The maximum number of resources that can be dropped by an asteroid. 
	private const int MAX_DROPPED_RESOURCES = 20;


    [SerializeField] float health;
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
	// The prefab to spawn when destroyed
    [SerializeField] GameObject destroyEffect;

    private GameState gameState;
    private ObjectPoolManager explosionManager;
    private ObjectPoolManager logicManager;
    private ObjectPoolManager asteroidManager;


	void OnEnable() 
    {
		droppedResources = System.Convert.ToInt32(Random.Range (0, MAX_DROPPED_RESOURCES));
	}


    // Initialise player, size, and speed
	public void SetPlayer(GameObject temp, float var, int rnd, ObjectPoolManager cachedManager, ObjectPoolManager cachedLogic, ObjectPoolManager cachedAsteroid)
	{
        asteroidManager = cachedAsteroid;
        explosionManager = cachedManager;
        logicManager = cachedLogic;
		player = temp;
		type = rnd;
		maxVariation = var;
        float random = Random.Range(5f, var);
		transform.parent.localScale = new Vector3(random + Random.Range (0, 15), random + Random.Range (0, 15),random + Random.Range (0, 15));
        transform.parent.gameObject.GetComponent<AsteroidCollision>().SetCollisionDamage(random);
        health = (transform.parent.localScale.x + transform.parent.localScale.y + transform.parent.localScale.z) * 2f;
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
			// Ship automatically collects resources from destroyed asteroids. 
			gameState.AddShipResources(droppedResources);
		
            //GameObject temp = Instantiate(destroyEffect, transform.position, transform.rotation) as GameObject;
            GameObject temp = explosionManager.RequestObject();
            temp.transform.position = transform.position;
            explosionManager.EnableClientObject(temp.name, temp.transform.position, temp.transform.rotation);
            gameState.RemoveAsteroid(transform.parent.gameObject);
            string removeName = transform.parent.gameObject.name;
            transform.parent = null;
            asteroidManager.DisableClientObject(removeName);
            asteroidManager.RemoveObject(removeName);
            logicManager.RemoveObject(gameObject.name);
        }
   }
}
