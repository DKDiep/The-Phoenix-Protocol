using UnityEngine;
using System.Collections;

public class OutpostLogic : MonoBehaviour {

	private GameState gameState;

	private const int MIN_OUTPOST_RESOURCES = 100;
	private const int MAX_OUTPOST_RESOURCES = 500;
	private const int MIN_OUTPOST_CIVILIANS = 100;
	private const int MAX_OUTPOST_CIVILIANS = 500;

	private bool resourcesCollected = false;
	private bool civiliansCollected = false;

	private int numberOfResources;
	private int numberOfCivilians; 

	// Use this for initialization
	void Start () 
    {
		// Set the number of resources for this outpost to be between the min and max value.
		numberOfResources = Random.Range(MIN_OUTPOST_RESOURCES, MAX_OUTPOST_RESOURCES);
		numberOfCivilians = Random.Range(MIN_OUTPOST_CIVILIANS, MAX_OUTPOST_CIVILIANS);
        transform.parent.eulerAngles = new Vector3(Random.Range(-30, 10), Random.Range(0, 359), Random.Range(-20, 20));
	}
		
	/// <summary>
	/// Handle the player coming into resource collision range.
	/// This is used for collecting civilians as well. 
	/// </summary>
	public void ResourceCollision ()
	{
		if(!resourcesCollected) 
		{
			CollectResources();
			resourcesCollected = true;
		}
		if(!civiliansCollected) 
		{
			CollectCivilians();
			civiliansCollected = true;
		}
	}

	/// <summary>
	/// Handle the player colliding into the outpost.
	/// </summary>
	/// <param name="damage">The damage to inflict to the player.</param>
	public void PlayerCollision(int damage)
	{
		// This isn't blaster damage, so it goes straight to the hull.
		gameState.DamageShip(false, damage);

		// The outpost is destroyed when coliding with a player, so update the game state
		gameState.RemoveOutpost(transform.parent.gameObject);
	}

	public void SetStateReference(GameState state)
	{
		gameState = state;
	}

	public void CollectResources() 
    {
		// Add ship resources to the game state
		gameState.AddShipResources(numberOfResources);
		Debug.Log("Collected Resources from outpost");
	}
		
	public void CollectCivilians() 
    {
		// Add ship resources to the game state
		gameState.AddCivilians(numberOfCivilians);
		Debug.Log("Collected Civilians from outpost");
	}
		
}


