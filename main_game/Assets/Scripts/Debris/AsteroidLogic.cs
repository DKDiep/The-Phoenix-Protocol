/*
    Sets a unique scale and rotation. Handles destruction effects.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsteroidLogic : MonoBehaviour, IDestructibleObject
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private int maxDroppedResources; // The maximum number of resources that can be dropped by an asteroid. 

	private float health;
	private int droppedResources;                 // The amount of resources dropped by the asteroid

    private GameState gameState;

    private ObjectPoolManager explosionManager;
    private ObjectPoolManager logicManager;
    private ObjectPoolManager asteroidManager;

	private List<IDestructionListener> destructionListeners;

	void Start()
	{
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        
		LoadSettings();

		destructionListeners = new List<IDestructionListener>();
        StartCoroutine(SyncRotation());
	}

    IEnumerator SyncRotation()
    {
        yield return new WaitForSeconds(Random.Range(5f,10f));
		asteroidManager.SyncAsteroidRotation(transform.parent.gameObject.name, transform.parent.rotation);
        StartCoroutine(SyncRotation());
    }


	private void LoadSettings()
	{
		maxDroppedResources = settings.AsteroidMaxDroppedResources;
	}

	void OnEnable() 
    {
		droppedResources = System.Convert.ToInt32(Random.Range (0, maxDroppedResources));
	}
		
    // Initialise player, size, and speed
	public void SetPlayer(GameObject temp, float var, int rnd, ObjectPoolManager cachedManager, ObjectPoolManager cachedLogic, ObjectPoolManager cachedAsteroid)
	{
        asteroidManager  = cachedAsteroid;
        explosionManager = cachedManager;
        logicManager     = cachedLogic;

        float random = Random.Range(5f, var);
		transform.parent.localScale = new Vector3(random + Random.Range (0, 15), random + Random.Range (0, 15),random + Random.Range (0, 15));
        transform.parent.gameObject.GetComponent<AsteroidCollision>().SetCollisionDamage(random);
        health = (transform.parent.localScale.x + transform.parent.localScale.y + transform.parent.localScale.z) * 2f;
		transform.parent.rotation = Random.rotation;
	}

	public void SetStateReference(GameState state)
	{
		gameState = state;
	}

	// Allows asteroid to take damage and spawns destroy effect
	public void collision (float damage)
	{
        health -= damage;

		if (health <= 0 && transform.parent != null) // The null check prevents trying to destroy an object again while it's already being destroyed
        {
			// Ship automatically collects resources from destroyed asteroids. 
			gameState.AddShipResources(droppedResources);

            GameObject temp = explosionManager.RequestObject();
            temp.transform.position = transform.position;
            explosionManager.EnableClientObject(temp.name, temp.transform.position, temp.transform.rotation, temp.transform.localScale);

			Despawn();
        }
	}

	/// <summary>
	/// Despawns this asteroid.
	/// </summary>
	public void Despawn()
	{
		NotifyDestructionListeners(); // Notify registered listeners that this object has been destroyed

		gameState.RemoveAsteroid(transform.parent.gameObject);

		string removeName = transform.parent.gameObject.name;
		transform.parent  = null;
		asteroidManager.DisableClientObject(removeName);
		asteroidManager.RemoveObject(removeName);
		logicManager.RemoveObject(gameObject.name);
	}

	/// <summary>
	/// Registers a listener to be notified when this object is destroyed.
	/// </summary>
	/// <param name="listener">The listener.</param>
	public void RegisterDestructionListener(IDestructionListener listener)
	{
		destructionListeners.Add(listener);
	}

	/// <summary>
	/// Notifies the registered destruction listeners and clears the list.
	/// </summary>
	private void NotifyDestructionListeners()
	{
		foreach (IDestructionListener listener in destructionListeners)
			listener.OnObjectDestructed(transform.parent.gameObject);

		destructionListeners.Clear();
	}
}
