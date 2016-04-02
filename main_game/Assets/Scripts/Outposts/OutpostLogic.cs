using UnityEngine;
using System.Collections;

public class OutpostLogic : MonoBehaviour {

	private GameState gameState;

	private const int MIN_OUTPOST_RESOURCES = 100;
	private const int MAX_OUTPOST_RESOURCES = 300;
	private const int MIN_OUTPOST_CIVILIANS = 100;
	private const int MAX_OUTPOST_CIVILIANS = 300;

	public bool resourcesCollected = false;
    public bool civiliansCollected = false;
    public bool discovered = false; 
    public int id;

	private int numberOfResources;
	private int numberOfCivilians;
    private int difficulty;

    private OutpostCollision collision;
    private PlayerController playerController;
    void Start()
    {
        GameObject playerControllerObject = GameObject.Find("PlayerController(Clone)");
        playerController = playerControllerObject.GetComponent<PlayerController>();
    }

    public void SetDifficulty(int diff, int multiplier)
    {
        difficulty = diff;
        numberOfResources = Random.Range(MIN_OUTPOST_RESOURCES, MAX_OUTPOST_RESOURCES);
        numberOfCivilians = Random.Range(MIN_OUTPOST_CIVILIANS, MAX_OUTPOST_CIVILIANS);
        numberOfCivilians *= multiplier;
        numberOfResources *= multiplier;
    }

    public int GetDifficulty()
    {
        return difficulty;
    }

    /// <summary>
    /// Handle the player coming into resource collision range.
    /// This is used for collecting civilians as well. 
    /// </summary>
    public void ResourceCollision ()
	{
        if(collision == null)
            collision = transform.parent.GetComponent<OutpostCollision>();
		if(!resourcesCollected) 
		{
			CollectResources();
			resourcesCollected = true;
            collision.SwitchMaterial(1);
            playerController.RpcNotifyOutpostVisit(numberOfResources, numberOfCivilians, id);
		}
		if(!civiliansCollected) 
		{
			CollectCivilians();
			civiliansCollected = true;
            collision.SwitchMaterial(1);
		}

	}

	/// <summary>
	/// Handle the player colliding into the outpost.
	/// </summary>
	/// <param name="damage">The damage to inflict to the player.</param>
	public void PlayerCollision(int damage)
	{
		// This isn't blaster damage, so it goes straight to the hull.
		gameState.DamageShip(damage);

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
		Debug.Log("Collected " + numberOfResources + " Resources from outpost with difficulty " + difficulty);
	}
		
	public void CollectCivilians() 
    {
		// Add ship resources to the game state
		gameState.AddCivilians(numberOfCivilians);
		Debug.Log("Collected " + numberOfCivilians + " Civilians from outpost");
	}
		
}


